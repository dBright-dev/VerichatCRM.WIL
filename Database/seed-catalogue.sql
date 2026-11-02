/* ============================================================================
   Verichat CRM - public catalogue seed
   ----------------------------------------------------------------------------
   WHAT THIS IS
     Every product the public gallery (/gallery), the client portal catalogue
     and the procurement list can show. 60 rows across the eight categories in
     ProductCategories. Re-runnable: it upserts by SKU.

   READ THIS BEFORE RUNNING IT ON ANYTHING THAT MATTERS
     These rows were written from the sectors the company publishes - clinical
     and hospital, mining and industrial, education and research - not from a
     stock count. Nobody in the sales desk has signed them off yet, so treat
     them as the structure to approve and prune, not as a list of what is in
     the Avondale warehouse today. Two consequences of that:

       - Price is indicative. There is no currency column on Product, and the
         staff views print "R" while this list is costed as a plain number.
         Correcting that is a code decision, not a data one; until it is made,
         do not publish these prices to a client.
       - StockQty is a plausible opening quantity, not a count. Rows are
         deliberately left below their threshold (see the verification block)
         because the low-stock alert needs something to fire on in development.

   IDEMPOTENCY, AND WHAT A RE-RUN WILL NOT DO
     When the SKU already exists, this script updates ProductName and Category
     only. It never overwrites Price, StockQty or LowStockThreshold on an
     existing row, because those columns are live data the moment the
     warehouse starts transacting: re-seeding after a month of deliveries
     would silently reprice stock and reset counts. Insert sets them.

     So: run it freely to add or rename lines, and change prices in the product
     form rather than here.

   IMAGES ARE NOT IN THIS SCRIPT
     ImageUrl is inserted as NULL on purpose. Product.ImageUrl is the override
     for a photograph an administrator uploads, and the API only accepts an
     absolute http(s) URL there because the mobile app loads it directly - a
     relative path baked into the database would be a lie on whichever host did
     not serve it. Catalogue photography is instead resolved by filename:
     wwwroot/images/catalogue/products/<product name, slugged>.jpg, with the
     category photograph standing in when no such file exists (CatalogueArt).
     Drop the files in and the gallery picks them up; no data write involved.

   HOW TO RUN - LOCAL ONLY
     sqlcmd -S "(localdb)\MSSQLLocalDB" -d VerichatCRM -b -i Database\seed-catalogue.sql

     -b makes sqlcmd return non-zero on the raised errors below, so a CI step or
     a careful pair of eyes notices a failure instead of scrolling past it.
     Run the migrations first (DEPLOYMENT.md 8); the script stops if dbo.Products
     is not there.

   NOT YET THE HOSTED DATABASE
     The hosted schema is behind the local one - the account-lifecycle and
     approval migrations (47 to 50) are still pending there, see task 32. Apply
     this to a local database, and apply it to the hosted one only once those
     migrations are in and the sales desk has been through these rows.
   ========================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Products does not exist. Apply the EF migrations first (DEPLOYMENT.md section 8), then re-run this script.', 16, 1);
END
GO

BEGIN TRANSACTION;
GO

/* ---------------------------------------------------------------------------
   1. Re-file the categories that predate the validated list.

   CreateProductRequestValidator rejects anything outside ProductCategories,
   which means a row sitting on the legacy free-text value "Clinical" cannot be
   saved again from the product form without first changing its category - the
   admin edits a price, gets "Invalid product category", and concludes the form
   is broken. This moves those rows onto real categories.

   Deliberately narrow: only the three literals the old DataSeeder wrote are
   touched, and the precise per-SKU decisions happen in the upsert below.
   --------------------------------------------------------------------------- */
UPDATE dbo.Products
SET Category = CASE LOWER(LTRIM(RTRIM(Category)))
                   WHEN 'clinical' THEN N'Clinical Consumables'
                   WHEN 'rehab'    THEN N'Home Care'
                   WHEN 'education' THEN N'Teaching & Simulation'
               END,
    UpdatedAt = SYSUTCDATETIME()
WHERE LOWER(LTRIM(RTRIM(Category))) IN ('clinical', 'rehab', 'education');

PRINT 'Legacy categories re-filed: ' + CAST(@@ROWCOUNT AS nvarchar(20));
GO

/* ---------------------------------------------------------------------------
   2. The catalogue.

   Column order in the VALUES block: SKU, ProductName, Category, Price,
   StockQty, LowStockThreshold.
   --------------------------------------------------------------------------- */
MERGE dbo.Products AS target
USING (VALUES
    /* ---- Clinical consumables: the reorder cycle, ward and theatre ---- */
    (N'VC-TB-005',  N'IV Giving Set - Sterile',                        N'Clinical Consumables',     28.00,  3500,  2000),
    (N'VC-SM-013',  N'Surgical Mask - Type IIR Box 50',                N'Clinical Consumables',    180.00,   180,   500),
    (N'VC-CN-101',  N'Syringe - 5ml Luer Slip, Box 100',               N'Clinical Consumables',     62.00,  4200,  1500),
    (N'VC-CN-102',  N'Hypodermic Needle - 23G, Box 200',               N'Clinical Consumables',     48.00,  3100,  1200),
    (N'VC-CN-103',  N'IV Cannula - 20G, Box 100',                      N'Clinical Consumables',     75.00,  2400,   900),
    (N'VC-GL-104',  N'Examination Glove - Nitrile Blue, Box 100',      N'Clinical Consumables',    140.00,  1800,   600),
    (N'VC-DR-105',  N'Sterile Wound Dressing, Pack 25',                N'Clinical Consumables',     95.00,  1500,   500),
    (N'VC-DR-106',  N'Gauze Roll - 10cm Sterile, Pack 10',             N'Clinical Consumables',     55.00,  2200,   800),
    (N'VC-UC-107',  N'Foley Catheter - 16CH, Box 10',                  N'Clinical Consumables',    245.00,   500,   180),
    (N'VC-HS-108',  N'Hand Sanitiser - 70 Percent Alcohol 500ml',      N'Clinical Consumables',     65.00,  1200,   400),

    /* ---- PPE: what goes on a person before they go near a patient ---- */
    (N'VC-LB-006',  N'Lab Coat - White Unisex',                        N'PPE',                     380.00,    24,    15),
    (N'VC-PR-111',  N'N95 Respirator - Box of 20',                     N'PPE',                     380.00,   700,   250),
    (N'VC-PC-112',  N'Disposable Coverall - SMS, Box 25',              N'PPE',                     420.00,   240,    80),
    (N'VC-PF-113',  N'Face Shield - Reusable Full Visor',              N'PPE',                      85.00,   600,   200),
    (N'VC-PG-114',  N'Safety Goggles - Anti-Fog Indirect Vent',        N'PPE',                      95.00,   500,   180),
    (N'VC-PV-115',  N'Hi-Vis Reflective Vest',                         N'PPE',                      75.00,   900,   250),
    (N'VC-PH-116',  N'Hard Hat - Ventilated Industrial',               N'PPE',                     145.00,   400,   120),

    /* ---- Hospital infrastructure: the equipment that gets installed ---- */
    (N'VC-HB-121',  N'Hospital Bed - Manual Three Function',           N'Hospital Infrastructure', 14500.00,   24,     8),
    (N'VC-HB-122',  N'Overhead Bed Cradle - With Mattress',            N'Hospital Infrastructure',  1250.00,   45,    15),
    (N'VC-HV-123',  N'IV Stand - Mobile Five Hook',                    N'Hospital Infrastructure',   680.00,   90,    30),
    (N'VC-OT-124',  N'Operating Table - Hydraulically Adjustable',     N'Hospital Infrastructure', 88000.00,    4,     2),
    (N'VC-ST-125',  N'Steam Autoclave - 23 Litre',                     N'Hospital Infrastructure', 34500.00,    6,     2),
    (N'VC-TR-126',  N'Resuscitation Trolley - Stainless Steel',        N'Hospital Infrastructure',  7800.00,   18,     6),
    (N'VC-SP-127',  N'Suction Unit - Portable Electric',               N'Hospital Infrastructure', 12400.00,   12,     5),
    (N'VC-PM-128',  N'Patient Monitor - Six Parameter',                N'Hospital Infrastructure', 24500.00,    9,     3),
    (N'VC-OC-129',  N'Oxygen Concentrator - 5 Litre',                  N'Hospital Infrastructure', 18900.00,   11,     4),
    (N'VC-EC-130',  N'Examination Couch - Three Seater',               N'Hospital Infrastructure',  5600.00,   20,     6),

    /* ---- Infant care: the maternity and neonatal corner ---- */
    (N'VC-IW-131',  N'Infant Radiant Warmer',                          N'Infant Care',             39500.00,    5,     2),
    (N'VC-IW-132',  N'Phototherapy Unit - LED Biliblanket',            N'Infant Care',             28500.00,    6,     2),
    (N'VC-IW-133',  N'Baby Bassinet - Tilting Frame',                  N'Infant Care',              3400.00,   18,     6),
    (N'VC-IW-134',  N'Foetal Doppler - Handheld',                      N'Infant Care',              4800.00,   22,     8),
    (N'VC-IW-135',  N'Obstetric Delivery Kit - Sterile',               N'Infant Care',               260.00,  300,   100),

    /* ---- Home care and rehabilitation: discharge and daily living ---- */
    (N'VC-WA-015',  N'Wheelchair - Lightweight Aluminium',             N'Home Care',                8900.00,   22,    25),
    (N'VC-CM-003',  N'Crutch - Adjustable Forearm',                    N'Home Care',                1100.00,    8,    10),
    (N'VC-WF-141',  N'Walking Frame - Folding Aluminium',              N'Home Care',                1350.00,   40,    15),
    (N'VC-WC-142',  N'Bedside Commode - With Pan',                     N'Home Care',                1650.00,   35,    12),
    (N'VC-WC-143',  N'Shower Trolley - Height Adjustable',             N'Home Care',                3800.00,   16,     6),
    (N'VC-WB-144',  N'Hospital Bed - Semi-Electric Two Function',      N'Home Care',               22500.00,   12,     4),
    (N'VC-WM-145',  N'Alternating Pressure Air Mattress',              N'Home Care',                4200.00,   25,     8),
    (N'VC-WN-146',  N'Nebuliser - Home Compressor',                    N'Home Care',                2900.00,   30,    10),

    /* ---- Mining and industrial: rescue stations and site clinics ---- */
    (N'VC-MR-151',  N'Self-Rescuer - 30 Minute Carbon Monoxide',       N'Mining & Industrial',       3200.00,  220,    80),
    (N'VC-MD-152',  N'Gas Detector - Four Channel Portable',           N'Mining & Industrial',      14500.00,   18,     6),
    (N'VC-MD-153',  N'Gas Calibration Kit - Six Cylinder',             N'Mining & Industrial',       8900.00,   10,     4),
    (N'VC-MR-154',  N'Rescue Stretcher - Folding Scoop',               N'Mining & Industrial',       5400.00,   20,     6),
    (N'VC-MP-155',  N'Cap Lamp - Rechargeable LED Mine',               N'Mining & Industrial',       1850.00,  140,    50),
    (N'VC-MH-156',  N'Body Harness - Full Body Double Lanyard',        N'Mining & Industrial',       2150.00,   70,    25),
    (N'VC-MF-157',  N'Mine Site First Aid Box - 40 Person',            N'Mining & Industrial',       1450.00,   60,    20),

    /* ---- Teaching and simulation: what a training laboratory buys ---- */
    (N'VC-TS-161',  N'Adult CPR Manikin - With Tracer',                N'Teaching & Simulation',    18500.00,    8,     3),
    (N'VC-TS-162',  N'Child Resuscitation Dummy',                      N'Teaching & Simulation',    12400.00,   10,     3),
    (N'VC-TS-163',  N'Birth Simulator - Delivery Trainer',             N'Teaching & Simulation',    26500.00,    4,     2),
    (N'VC-TS-164',  N'Anatomy Skeleton - Life Size Articulated',       N'Teaching & Simulation',     9800.00,   14,     4),
    (N'VC-TS-165',  N'Simulation IV Arm - Paediatric',                 N'Teaching & Simulation',     3400.00,   25,     8),

    /* ---- Laboratory: benches, instruments and the glass that goes with them ---- */
    (N'VC-LM-171',  N'Microscope - Binocular Student',                 N'Laboratory',                6800.00,   26,     8),
    (N'VC-LC-172',  N'Benchtop Centrifuge - Six Place',                N'Laboratory',               15800.00,    8,     3),
    (N'VC-LC-173',  N'Laboratory Incubator - 25 Litre',                N'Laboratory',               19500.00,    6,     2),
    (N'VC-LB-174',  N'Digital Water Bath - Two Tank',                  N'Laboratory',                8400.00,    9,     3),
    (N'VC-LP-175',  N'Micropipette Set - 20 to 200 Microlitre',        N'Laboratory',                2400.00,   40,    12),
    (N'VC-LG-176',  N'Borosilicate Test Tube 15ml, Pack 100',          N'Laboratory',                 380.00,  420,   150),
    (N'VC-LF-177',  N'Lab Bench - Steel Frame Four Seat',              N'Laboratory',               12800.00,   10,     3),
    (N'VC-LF-178',  N'Fume Hood - Benchtop 1200mm',                    N'Laboratory',               38500.00,    4,     1)
) AS source (SKU, ProductName, Category, Price, StockQty, LowStockThreshold)
ON target.SKU = source.SKU
WHEN MATCHED AND (
        target.ProductName <> source.ProductName
     OR target.Category     <> source.Category
    ) THEN UPDATE SET
        ProductName = source.ProductName,
        Category    = source.Category,
        UpdatedAt   = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (SKU, ProductName, Category, Price, StockQty, LowStockThreshold, CreatedAt, ImageUrl)
    VALUES
    (source.SKU, source.ProductName, source.Category, source.Price,
     source.StockQty, source.LowStockThreshold, SYSUTCDATETIME(), NULL);

PRINT 'Catalogue upserted: ' + CAST(@@ROWCOUNT AS nvarchar(20)) + ' rows touched.';
GO

/* ---------------------------------------------------------------------------
   3. Guard: nothing may be left outside the validated category list.

   If this returns any row, the API will refuse to save it from the product
   form. Either the row came from an old script or somebody typed a new sector
   into the database - add it to ProductCategories, or re-file the row here.
   --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM dbo.Products
           WHERE Category NOT IN (N'PPE', N'Laboratory', N'Hospital Infrastructure',
                                  N'Home Care', N'Infant Care', N'Clinical Consumables',
                                  N'Mining & Industrial', N'Teaching & Simulation'))
BEGIN
    SELECT SKU, ProductName, Category AS UnvalidatedCategory
    FROM dbo.Products
    WHERE Category NOT IN (N'PPE', N'Laboratory', N'Hospital Infrastructure',
                           N'Home Care', N'Infant Care', N'Clinical Consumables',
                           N'Mining & Industrial', N'Teaching & Simulation');

    RAISERROR('Rows are filed under a category the API will reject. See the list above, then fix the data or ProductCategories.', 16, 1);
END
GO

/* ---------------------------------------------------------------------------
   4. What the public gallery will show, and what will fire a warehouse alert.

   The first block is the gallery's grouping: the page buckets by category
   keyword into clinical / mining / education, so this is the count a visitor
   sees per sector. The second is the low-stock list the alert job reads -
   several rows are intentionally below threshold in development.
   --------------------------------------------------------------------------- */
SELECT CASE
           WHEN Category LIKE N'%mine%' OR Category LIKE N'%industrial%' THEN N'Mining & industrial'
           WHEN Category LIKE N'%educat%' OR Category LIKE N'%teach%'
             OR Category LIKE N'%laborator%' OR Category LIKE N'%research%' THEN N'Education & research'
           ELSE N'Clinical & hospital'
       END AS GallerySector,
       COUNT(*) AS LinesShown
FROM dbo.Products
GROUP BY CASE
           WHEN Category LIKE N'%mine%' OR Category LIKE N'%industrial%' THEN N'Mining & industrial'
           WHEN Category LIKE N'%educat%' OR Category LIKE N'%teach%'
             OR Category LIKE N'%laborator%' OR Category LIKE N'%research%' THEN N'Education & research'
           ELSE N'Clinical & hospital'
       END
ORDER BY GallerySector;

SELECT Category, COUNT(*) AS Products, SUM(CASE WHEN StockQty < LowStockThreshold THEN 1 ELSE 0 END) AS BelowThreshold
FROM dbo.Products
GROUP BY Category
ORDER BY Category;
GO

COMMIT TRANSACTION;
GO
