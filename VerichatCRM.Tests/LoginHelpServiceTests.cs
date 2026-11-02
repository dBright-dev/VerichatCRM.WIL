using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using Xunit;

namespace VerichatCRM.Tests;

/// <summary>
/// The note somebody sends when none of the recovery flows apply. Most of what is
/// proved here is about the note surviving: it is written before anything is mailed,
/// it survives a mail server that is down, it survives having no administrator to
/// notify, and a second copy of it does not multiply the first.
/// </summary>
public class LoginHelpServiceTests
{
    [Fact]
    public async Task Submit_WritesTheNoteBeforeAnythingIsMailed()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        var outcome = await service.SubmitAsync(Ask("Ruth Nakamura", "ruth@clinic.test", "Client User", "Signed up Tuesday, still says waiting."));

        Assert.True(outcome.Success);
        var note = Assert.Single(db.LoginHelpRequests);
        Assert.Equal("Ruth Nakamura", note.Name);
        Assert.Equal("ruth@clinic.test", note.Email);
        Assert.Equal("Client User", note.ExpectedRole);
        Assert.Equal(LoginHelpStatus.Open, note.Status);

        var mail = Assert.Single(email.Sent);
        Assert.Equal("admin@verichat.test", mail.To);
        Assert.Equal("Someone cannot sign in to Verichat", mail.Subject);
        Assert.Contains("Signed up Tuesday", mail.Body);
    }

    [Fact]
    public async Task Submit_TellsOnlyTheAdministratorsWhoCouldActOnIt()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "live@verichat.test");
        // A deactivated administrator cannot open the queue or fix anything, so
        // mailing them reads as a handover to nobody.
        await AddAdministratorAsync(db, "gone@verichat.test", isActive: false);
        await AddUserAsync(db, "sales@verichat.test", UserRoles.SalesRepresentative);

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));

        Assert.Equal(new[] { "live@verichat.test" }, email.Sent.Select(s => s.To));
    }

    [Fact]
    public async Task Submit_MailServerDown_KeepsTheNoteAndStillSucceeds()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");
        email.FailOnSend = true;

        var outcome = await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));

        // The screen promised the person their note was kept. Losing it because SMTP
        // was down would make that sentence a lie, and would leave them with nothing.
        Assert.True(outcome.Success);
        Assert.Single(db.LoginHelpRequests);
    }

    [Fact]
    public async Task Submit_WithNoAdministratorToTell_StillKeepsTheNote()
    {
        var (db, email, service) = Create();

        var outcome = await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));

        Assert.True(outcome.Success);
        Assert.Single(db.LoginHelpRequests);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Submit_SecondNoteFromTheSameAddress_IsFoldedIntoTheFirst()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", message: "The portal will not open."));
        var again = await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", message: "Also still waiting."));

        Assert.True(again.Success);
        Assert.Contains("still open", again.Message.ToLowerInvariant());
        Assert.Single(db.LoginHelpRequests);
        Assert.Single(email.Sent);
    }

    [Fact]
    public async Task Submit_OnceTheFirstNoteIsOld_TheSecondOneStandsOnItsOwn()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));
        db.LoginHelpRequests.Single().CreatedAt = DateTime.UtcNow.AddHours(-3);
        await db.SaveChangesAsync();

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", message: "A week on and nobody has answered."));

        Assert.Equal(2, db.LoginHelpRequests.Count());
        Assert.Equal(2, email.Sent.Count);
    }

    [Fact]
    public async Task Submit_OnceTheFirstNoteIsClosed_TheNextOneIsANewProblem()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");
        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));
        var note = db.LoginHelpRequests.Single();

        await service.ResolveAsync(note.Id, "admin-id");
        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", message: "It has come back again."));

        Assert.Equal(2, db.LoginHelpRequests.Count());
        Assert.Equal(2, email.Sent.Count);
    }

    [Fact]
    public async Task Submit_SendersTextIsEncodedBeforeItBecomesAnEmail()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        await service.SubmitAsync(Ask(
            "Ruth <script>alert(1)</script>",
            "ruth@clinic.test",
            message: "Line one.\nLine two."));

        var body = email.Sent[0].Body;
        Assert.DoesNotContain("<script>", body);
        Assert.Contains("&lt;script&gt;", body);
        // Typed as prose, and an email that swallows the line breaks is harder to read
        // than the form it came from.
        Assert.Contains("Line one.<br />Line two.", body);
    }

    [Fact]
    public async Task Submit_WithoutARoleClaimed_DoesNotInventOne()
    {
        var (db, email, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", expectedRole: "   "));

        Assert.Null(db.LoginHelpRequests.Single().ExpectedRole);
        Assert.DoesNotContain("They signed up as", email.Sent[0].Body);
    }

    [Theory]
    [InlineData("Mobile", "Mobile")]
    [InlineData("mobile", "Mobile")]
    [InlineData("  Mobile  ", "Mobile")]
    [InlineData("iOS", "Web")]
    [InlineData("", "Web")]
    [InlineData(null, "Web")]
    public async Task Submit_ChannelIsOneOfTwoWords(string? sent, string expected)
    {
        var (db, _, service) = Create();

        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test", channel: sent));

        // The column only ever means "a browser" or "a phone", so a hand-built request
        // cannot put a fourth spelling of the same thing in front of a reviewer.
        Assert.Equal(expected, db.LoginHelpRequests.Single().Channel);
    }

    [Fact]
    public async Task GetOpen_LongestWaitingFirstAndNothingAlreadyClosed()
    {
        var (db, _, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");
        await service.SubmitAsync(Ask("First", "a@clinic.test"));
        db.LoginHelpRequests.OrderBy(h => h.CreatedAt).First().CreatedAt = DateTime.UtcNow.AddMinutes(-30);
        await service.SubmitAsync(Ask("Second", "b@clinic.test"));
        await db.SaveChangesAsync();
        var older = db.LoginHelpRequests.Single(h => h.Name == "First");
        await service.ResolveAsync(older.Id, "admin-id");

        var open = await service.GetOpenAsync();

        var note = Assert.Single(open);
        Assert.Equal("Second", note.Name);
        Assert.Equal("Web", note.Channel);
    }

    [Fact]
    public async Task Resolve_RecordsWhoClosedItAndWhen()
    {
        var (db, _, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");
        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));
        var note = db.LoginHelpRequests.Single();

        var outcome = await service.ResolveAsync(note.Id, "admin-id");

        Assert.True(outcome.Success);
        Assert.Equal(LoginHelpStatus.Resolved, note.Status);
        Assert.Equal("admin-id", note.ResolvedByUserId);
        Assert.NotNull(note.ResolvedAt);
    }

    [Fact]
    public async Task Resolve_SomethingAlreadyClosed_SaysSoRatherThanSilentlyPassing()
    {
        var (db, _, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");
        await service.SubmitAsync(Ask("Ruth", "ruth@clinic.test"));
        var note = db.LoginHelpRequests.Single();
        await service.ResolveAsync(note.Id, "admin-id");

        var again = await service.ResolveAsync(note.Id, "other-admin-id");

        // Two reviewers reach this screen at once more than anything else does, and the
        // second one needs to know the note is answered rather than that the click failed.
        Assert.False(again.Success);
        Assert.Contains("already closed", again.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("admin-id", note.ResolvedByUserId);
    }

    [Fact]
    public async Task Resolve_IdThatWasNeverSent_IsRefused()
    {
        var (db, _, service) = Create();
        await AddAdministratorAsync(db, "admin@verichat.test");

        var outcome = await service.ResolveAsync("00000000-0000-0000-0000-000000000000", "admin-id");

        Assert.False(outcome.Success);
        Assert.False(db.LoginHelpRequests.Any());
    }

    // ---------------------------------------------------------------- helpers

    private static AskForHelpRequest Ask(
        string name,
        string email,
        string? expectedRole = null,
        string message = "The sign-in screen refuses me.",
        string? channel = null) => new()
    {
        Name = name,
        Email = email,
        ExpectedRole = expectedRole,
        Message = message,
        Channel = channel
    };

    /// <summary>
    /// The tuple shape the recovery tests use, but with the database in front of the
    /// repository: this service reads and writes two tables, and the administrator
    /// query is the part worth testing rather than stubbing.
    /// </summary>
    private static (AppDbContext Db, FakeEmailService Email, LoginHelpService Service) Create()
    {
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var email = new FakeEmailService();
        return (db, email, new LoginHelpService(db, email, NullLogger<LoginHelpService>.Instance));
    }

    private static async Task AddAdministratorAsync(
        AppDbContext db, string email, bool isActive = true)
    {
        db.Users.Add(new User
        {
            DisplayName = "Admin " + email.Split('@')[0],
            Email = email,
            PasswordHash = "pbkdf2-sha256$210000$c2FsdA==$aGFzaA==",
            Role = UserRoles.SystemAdministrator,
            IsActive = isActive
        });
        await db.SaveChangesAsync();
    }

    private static async Task AddUserAsync(AppDbContext db, string email, string role)
    {
        db.Users.Add(new User
        {
            DisplayName = role,
            Email = email,
            PasswordHash = "pbkdf2-sha256$210000$c2FsdA==$aGFzaA==",
            Role = role
        });
        await db.SaveChangesAsync();
    }
}
