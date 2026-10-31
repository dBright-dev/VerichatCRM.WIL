using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.Models.Client;
using Xunit;

namespace VerichatCRM.Tests;

public class PermissionTests
{
    private static readonly string[] AdminAndSales = { Roles.SystemAdministrator, Roles.SalesRepresentative };
    private static readonly string[] OtherRoles =
    {
        Roles.ProcurementManager, Roles.FieldSupportEngineer, Roles.Logistics, Roles.ClientUser, Roles.MedicalStudent
    };

    [Theory]
    [InlineData(Perm.SalesLeads)]
    [InlineData(Perm.SalesMap)]
    [InlineData(Perm.DashboardAnalytics)]
    public void SalesScreens_AreGrantedToAdminAndSalesOnly(string permission)
    {
        foreach (var role in AdminAndSales)
            Assert.True(Perm.Has(role, permission), $"{role} should have {permission}");
        foreach (var role in OtherRoles)
            Assert.False(Perm.Has(role, permission), $"{role} should not have {permission}");
    }

    [Fact]
    public void ClientAssign_IsAdminOnly()
    {
        Assert.True(Perm.Has(Roles.SystemAdministrator, Perm.ClientAssign));
        Assert.False(Perm.Has(Roles.SalesRepresentative, Perm.ClientAssign));
        foreach (var role in OtherRoles)
            Assert.False(Perm.Has(role, Perm.ClientAssign), $"{role} should not have ClientAssign");
    }

    [Fact]
    public void AdminAnalytics_RedirectPermission_StaysAdminOnly()
    {
        Assert.True(Perm.Has(Roles.SystemAdministrator, Perm.AdminAnalytics));
        Assert.False(Perm.Has(Roles.SalesRepresentative, Perm.AdminAnalytics));
    }

    [Fact]
    public void UnknownOrMissingRole_HasNoPermissions()
    {
        Assert.False(Perm.Has("Intern", Perm.SalesLeads));
        Assert.False(Perm.Has(null!, Perm.SalesLeads));
    }
}

public class FollowUpHelperTests
{
    [Fact]
    public void Describe_NoDate_SaysNoContactLogged() =>
        Assert.Equal("No contact logged", FollowUpHelper.Describe(null));

    [Fact]
    public void Describe_Now_IsToday() =>
        Assert.Equal("Today", FollowUpHelper.Describe(DateTime.UtcNow));

    [Fact]
    public void Describe_OneDayAgo_IsYesterday() =>
        Assert.Equal("Yesterday", FollowUpHelper.Describe(DateTime.UtcNow.AddDays(-1)));

    [Fact]
    public void Describe_FiveDaysAgo_SaysFiveDaysAgo() =>
        Assert.Equal("5 days ago", FollowUpHelper.Describe(DateTime.UtcNow.AddDays(-5)));

    [Fact]
    public void Describe_FutureDate_ClampsToToday() =>
        Assert.Equal("Today", FollowUpHelper.Describe(DateTime.UtcNow.AddDays(2)));

    [Theory]
    [InlineData(null, false)]   // no contact logged is not flagged overdue
    [InlineData(0, false)]
    [InlineData(7, false)]      // exactly at the rule is still fine
    [InlineData(8, true)]
    public void IsStale_UsesTheSevenDayRule(int? daysAgo, bool expected)
    {
        DateTime? date = daysAgo is null ? null : DateTime.UtcNow.AddDays(-daysAgo.Value);
        Assert.Equal(expected, FollowUpHelper.IsStale(date));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void IsFollowUpDue_TodayOrEarlier(int? dayOffset, bool expected)
    {
        DateTime? date = dayOffset is null ? null : DateTime.UtcNow.AddDays(dayOffset.Value);
        Assert.Equal(expected, FollowUpHelper.IsFollowUpDue(date));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    public void IsFollowUpOverdue_OnlyBeforeToday(int? dayOffset, bool expected)
    {
        DateTime? date = dayOffset is null ? null : DateTime.UtcNow.AddDays(dayOffset.Value);
        Assert.Equal(expected, FollowUpHelper.IsFollowUpOverdue(date));
    }
}

public class MapSectorTests
{
    [Theory]
    [InlineData("Mining", "Mine")]
    [InlineData("mine", "Mine")]
    [InlineData("Clinical", "Clinic")]
    [InlineData("Day Clinic", "Clinic")]
    [InlineData("Hospital", "Hospital")]
    [InlineData("Private hospital clinic", "Hospital")]
    [InlineData("University", "Education")]
    [InlineData("School", "Education")]
    [InlineData("Manufacturing", "Other")]
    [InlineData("", "Other")]
    [InlineData(null, "Other")]
    public void Normalize_MapsFreeTextToPinTypes(string? raw, string expected) =>
        Assert.Equal(expected, MapSector.Normalize(raw));
}

public class StatusPageViewModelTests
{
    [Fact]
    public void NotFound_SignedOut_SendsVisitorToLogin()
    {
        var vm = StatusPageViewModel.For(404, null, false);
        Assert.Equal("/account/login", vm.PrimaryUrl);
        Assert.Equal("Sign in", vm.PrimaryLabel);
    }

    [Theory]
    [InlineData(Roles.SystemAdministrator, "/admin/dashboard")]
    [InlineData(Roles.SalesRepresentative, "/pipeline")]
    [InlineData(Roles.ProcurementManager, "/procurement/dashboard")]
    [InlineData(Roles.FieldSupportEngineer, "/support/dashboard")]
    [InlineData(Roles.Logistics, "/support/dashboard")]
    [InlineData(Roles.ClientUser, "/")]
    public void NotFound_SignedIn_LinksToTheRolesHome(string role, string expectedUrl)
    {
        var vm = StatusPageViewModel.For(404, role, true);
        Assert.Equal(expectedUrl, vm.PrimaryUrl);
        Assert.Equal(expectedUrl, RoleHome.UrlFor(role));
    }

    [Fact]
    public void Forbidden_ClientUser_ExplainsTheMobileApp()
    {
        var vm = StatusPageViewModel.For(403, Roles.ClientUser, true);
        Assert.Contains("mobile app", vm.Message);
        Assert.Equal("Go to home", vm.PrimaryLabel);
    }

    [Fact]
    public void Forbidden_StaffRole_NamesTheRole()
    {
        var vm = StatusPageViewModel.For(403, Roles.SalesRepresentative, true);
        Assert.Contains("Sales Representative", vm.Message);
        Assert.Equal("Back to your dashboard", vm.PrimaryLabel);
    }

    [Fact]
    public void ServerError_ShowsTheRequestId()
    {
        var vm = StatusPageViewModel.For(500, Roles.SalesRepresentative, true, "req-1");
        Assert.True(vm.ShowRequestId);
        Assert.Equal("req-1", vm.RequestId);
    }
}