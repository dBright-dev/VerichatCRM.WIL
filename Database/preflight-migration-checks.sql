/* ============================================================================
  Database/preflight-migration-checks.sql

  Read-only pre-flight for 20261101151838_SyncSchemaAndAccountLifecycle and
  20261031160000_AddInquiryScoringInputs. Run this against the LIVE database
  before applying the migration chain, not after.

  Every statement below prints the rows that would abort the migration. Each
  one should return nothing. If any of them returns something, fix or trim the
  row and re-run - because the API calls MigrateAsync() at startup, a migration
  that fails half-way leaves the schema part-narrowed and the app comes up in
  degraded mode.

  Nothing here writes. It reads column lengths and duplicate keys only.

  Usage (password is prompted, never typed on the command line):
    sqlcmd -S tcp:<server>.database.windows.net -d <database> -U <login> `
           -b -i Database\preflight-migration-checks.sql
============================================================================ */

SET NOCOUNT ON;

/* -- 1. The 28 columns the migration narrows from nvarchar(max) -------------
   Generated from the migration source, so the list cannot drift from what
   actually runs. DATALENGTH counts bytes; the new maximum is characters, so
   the threshold is MaxLen * 2. Widening columns (the two GUID foreign keys,
   to nvarchar(450)) are included and simply never report anything.        */

CREATE TABLE #checks ( TableName sysname, ColumnName sysname, MaxLen int );
INSERT INTO #checks (TableName, ColumnName, MaxLen) VALUES
 ('Assets','Manufacturer',200),   ('Assets','Model',200),
 ('Assets','SerialNumber',100),   ('Assets','Status',30),
 ('Clients','Address',500),       ('Clients','ContactPerson',150),
 ('Clients','ContactPhone',30),   ('Clients','ContractStatus',50),
 ('Deals','Notes',4000),          ('Deals','Stage',50),
 ('DeliveryLogs','ClientId',450), ('DeliveryLogs','Destination',200),
 ('DeliveryLogs','Origin',200),   ('DeliveryLogs','Status',30),
 ('DeliveryLogs','TrackingNumber',30),
 ('QuotationLineItem','ProductName',200), ('QuotationLineItem','SKU',50),
 ('Quotations','QuoteReference',50), ('Quotations','Status',30),
 ('Quotations','Terms',4000),
 ('Tickets','AssignedToUserId',450), ('Tickets','Category',50),
 ('Tickets','Description',4000),  ('Tickets','Priority',20),
 ('Tickets','ResolutionNotes',4000), ('Tickets','Status',30),
 ('Tickets','Title',200),         ('Tickets','TrackingNumber',30);

CREATE TABLE #results ( TableName sysname, ColumnName sysname, MaxLen int,
                        LongestBytes int, RowsAtRisk int );

DECLARE @t sysname, @c sysname, @m int, @sql nvarchar(max);

DECLARE check_cursor CURSOR LOCAL FAST_FORWARD FOR
  SELECT TableName, ColumnName, MaxLen FROM #checks;
OPEN check_cursor;
FETCH NEXT FROM check_cursor INTO @t, @c, @m;
WHILE @@FETCH_STATUS = 0
BEGIN
  SET @sql = N'INSERT INTO #results '
           + N'SELECT @t, @c, @m, MAX(DATALENGTH([' + @c + N'])), COUNT(*) '
           + N'FROM [' + @t + N'] WHERE DATALENGTH([' + @c + N']) > @m * 2 '
           + N'HAVING COUNT(*) > 0;';
  EXEC sp_executesql @sql, N'@t sysname, @c sysname, @m int', @t, @c, @m;
  FETCH NEXT FROM check_cursor INTO @t, @c, @m;
END
CLOSE check_cursor; DEALLOCATE check_cursor;

PRINT '1. Over-length text that would fail the narrowing (expect no rows):';
SELECT TableName, ColumnName, MaxLen AS NewMaxChars,
       LongestBytes / 2 AS LongestCharsStored, RowsAtRisk
FROM #results ORDER BY TableName, ColumnName;

/* -- 2. The two decimal re-types -------------------------------------------
   VatRate goes 18,2 -> 5,4, so only 0.9999 and below fit. A rate stored as 15
   (meaning 15%) rather than 0.15 cannot be converted. WeightKg goes 18,2 ->
   10,2, which caps at 99,999,999.99.                                     */

PRINT '2. Decimal values outside the new precision (expect no rows):';
SELECT QuotationId, VatRate FROM Quotations WHERE VatRate >= 10;
SELECT DeliveryId, WeightKg FROM DeliveryLogs WHERE WeightKg > 99999999.99;

/* -- 3. The five unique indexes --------------------------------------------
   IX_Users_Email is the only one with no filter: a second NULL, or two blank
   addresses, fails it just as surely as two real ones. The other four exclude
   the empty string, so blank values are safe and duplicates are not.      */

PRINT '3. Keys the unique indexes would reject (expect no rows):';
SELECT Email, COUNT(*) AS Copies FROM Users GROUP BY Email HAVING COUNT(*) > 1;
SELECT TrackingNumber, COUNT(*) AS Copies FROM Tickets
  WHERE TrackingNumber <> '' GROUP BY TrackingNumber HAVING COUNT(*) > 1;
SELECT TrackingNumber, COUNT(*) AS Copies FROM DeliveryLogs
  WHERE TrackingNumber <> '' GROUP BY TrackingNumber HAVING COUNT(*) > 1;
SELECT QuoteReference, COUNT(*) AS Copies FROM Quotations
  WHERE QuoteReference <> '' GROUP BY QuoteReference HAVING COUNT(*) > 1;
SELECT SerialNumber, COUNT(*) AS Copies FROM Assets
  WHERE SerialNumber <> '' GROUP BY SerialNumber HAVING COUNT(*) > 1;

/* -- 4. AddInquiryScoringInputs ------------------------------------------- */

PRINT '4. Inquiries.Status longer than 30 (expect no rows):';
SELECT InquiryId, Status, DATALENGTH(Status) / 2 AS Len
FROM Inquiries WHERE DATALENGTH(Status) > 60;

PRINT '4b. Urgency values present. Anything outside the four the scorer knows';
PRINT '    is not an error - it scores as "not stated" - but know what is in';
PRINT '    the column before training anyone on the ranking.';
SELECT Urgency, COUNT(*) AS N FROM Inquiries WHERE Urgency IS NOT NULL GROUP BY Urgency;

/* -- 5. What the two account migrations are about to do --------------------
   ReviewSelfClaimedStaffAccounts (21) sets every Sales Representative,
   Procurement Manager, Field Support Engineer and Logistics row to Pending, and
   RemoveAccountApprovalGate (22) - which is in the same chain, so on a hosted
   database the two arrive together - turns that unreviewed set into IsActive = 0
   and deletes the queue that displayed it. This is therefore the list of people
   who cannot sign in until an administrator flips their Access switch on
   /Admin/Users - see DEPLOYMENT.md section 8.4. Run it before migrating and
   write the numbers down; they are a heads-up, not a failure.

   On a database that has already run 22 there is no ApprovalStatus left to read,
   so the same section reports the rows 22 disabled instead: the same list seen
   from the other side of the migration.                               */

PRINT '5. Staff accounts the review will pend and the removal will disable (expected, but count them):';
IF COL_LENGTH(N'[Users]', N'ApprovalStatus') IS NULL
    SELECT Role, COUNT(*) AS AccountsAlreadyDisabled
    FROM Users WHERE IsActive = 0 GROUP BY Role;
ELSE
    SELECT Role, COUNT(*) AS AccountsGoingToPending
    FROM Users
    WHERE Role IN (N'Sales Representative', N'Procurement Manager',
                   N'Field Support Engineer', N'Logistics')
    GROUP BY Role;

DROP TABLE #checks, #results;
