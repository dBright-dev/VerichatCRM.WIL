using VerichatCRM.Web.Authorization;
using Xunit;

namespace VerichatCRM.Tests;

/// <summary>
/// Badge.For and Badge.Tone replaced eleven separate switches that had already
/// drifted apart. These are the collisions that drift produced, pinned so a new
/// screen cannot quietly re-split one of them, plus the invariants that keep the
/// one mapping honest.
/// </summary>
public class BadgeTests
{
    [Theory]
    // The tracker plotted "In Transit" as info and the ticket queue as warning for
    // the same fact on the same day.
    [InlineData("In Transit", "badge-info")]
    // Only the portal's switch knew this word; the staff side fell through to grey.
    [InlineData("Expired", "badge-neutral")]
    // Warehouse alerts used to rename the feed's "Warning" to "Low", which is a
    // priority word and colouring grey.
    [InlineData("Warning", "badge-warning")]
    [InlineData("Low", "badge-neutral")]
    [InlineData("Under Review", "badge-warning")]
    [InlineData("Critical", "badge-critical")]
    [InlineData("Delivered", "badge-success")]
    public void For_GivesEachWord_OneMeaning(string value, string expected)
        => Assert.Equal(expected, Badge.For(value));

    [Fact]
    public void For_Always_Returns_A_Chip_Class()
    {
        Assert.StartsWith("badge-", Badge.For("Something the product has not thought about yet"));
        Assert.StartsWith("badge-", Badge.For(null));
        Assert.StartsWith("badge-", Badge.For(""));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(4, true)]
    [InlineData(40, false)]
    public void StockLabel_NamesEveryShelfState_WithItsOwnColour(int qty, bool low)
    {
        var label = Badge.StockLabel(qty, low);
        // The fallback chip is grey; a stock word landing on it means the label was
        // reworded here without rewording the mapping with it.
        Assert.NotEqual("badge-neutral", Badge.For(label));
    }

    [Fact]
    public void StockLabel_SeparatesEmptyFromScarce()
    {
        Assert.Equal("Out of stock", Badge.StockLabel(0, true));
        Assert.Equal("Low stock", Badge.StockLabel(3, true));
        Assert.Equal("In stock", Badge.StockLabel(3, false));
    }

    [Theory]
    [InlineData("Resolved", "status-ok")]
    [InlineData("Escalated", "status-critical")]
    [InlineData("Under Review", "status-warning")]
    [InlineData("Open", "")]
    [InlineData("In Progress", "")]
    public void Tone_UsesOnly_ClassesTheThemeDefines(string status, string expected)
        => Assert.Equal(expected, Badge.Tone(status));
}
