using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Validators;
using VerichatCRM.Core.Models.Inquiry;
using Xunit;

namespace VerichatCRM.Tests;

/// <summary>
/// The enquiry score is a starting position for a queue that staff re-rank by
/// hand, so what matters is that the bands are ordered, that nothing stated
/// still lands in the middle, and that the total cannot run past 100.
/// </summary>
public class InquiryScoringTests
{
    [Fact]
    public void EveryFactorAtItsBestIsExactlyOneHundred()
    {
        Assert.Equal(100, InquiryScoring.Calculate("Clinical", "Active", 5_000_000m, "Immediate"));
    }

    [Fact]
    public void NothingStatedScoresMidQueueRatherThanLast()
    {
        // A blank enquiry is not a cold one: sector 15, contract 10, value 11,
        // urgency 10.
        Assert.Equal(46, InquiryScoring.Calculate(null, null, null, null));
    }

    [Fact]
    public void LargerEnquiriesOutrankSmallerOnes()
    {
        var scores = new[] { 5_000_000m, 200_000m, 80_000m, 20_000m, 5_000m }
            .Select(value => InquiryScoring.Calculate(null, null, value, null))
            .ToArray();

        for (var i = 1; i < scores.Length; i++)
            Assert.True(scores[i - 1] > scores[i], $"band {i} should outrank band {i + 1}");
    }

    [Fact]
    public void MoreUrgentOutrankLessUrgent()
    {
        var immediate = InquiryScoring.Calculate(null, null, null, "Immediate");
        var high = InquiryScoring.Calculate(null, null, null, "High");
        var standard = InquiryScoring.Calculate(null, null, null, "Standard");
        var flexible = InquiryScoring.Calculate(null, null, null, "Flexible");

        Assert.True(immediate > high);
        Assert.True(high > standard);
        Assert.True(standard > flexible);
    }

    [Fact]
    public void AnUnrecognisedUrgencyScoresAsNotStated()
    {
        Assert.Equal(
            InquiryScoring.Calculate(null, null, null, null),
            InquiryScoring.Calculate(null, null, null, "yesterday"));
    }

    [Fact]
    public void ContractBeatsProspectBeatsInactive()
    {
        var active = InquiryScoring.Calculate(null, "Active", null, null);
        var prospect = InquiryScoring.Calculate(null, "Prospect", null, null);
        var inactive = InquiryScoring.Calculate(null, "Inactive", null, null);

        Assert.True(active > prospect);
        Assert.True(prospect > inactive);
    }

    [Fact]
    public void SectorRankingUsesTheCaseOfTheStoredValueNotItsSpelling()
    {
        Assert.Equal(
            InquiryScoring.Calculate("clinical", null, null, null),
            InquiryScoring.Calculate(" CLINICAL ", null, null, null));
    }

    [Fact]
    public void ASectorNobodyHasHeardOfIsNotTreatedAsWorthless()
    {
        var unknown = InquiryScoring.Calculate("Space Mining", null, null, null);
        var education = InquiryScoring.Calculate("Education", null, null, null);

        Assert.True(unknown > education);
    }
}

/// <summary>
/// decimal(5,4) overflows at 10, and every consumer reads VatRate as a
/// fraction, so a caller sending "15" for 15% must be refused by the binder
/// rather than reaching SQL Server as an unhandled HTTP 500.
/// </summary>
public class CreateQuotationRequestValidatorTests
{
    private static CreateQuotationRequest Valid(decimal vatRate, string terms = "Net 30.") => new()
    {
        ClientId = "client-1",
        VatRate = vatRate,
        Terms = terms,
        LineItems = new List<CreateQuotationLineItemRequest>
        {
            new() { SKU = "PPE-MASK-IIR", ProductName = "Surgical Mask - Type IIR", Quantity = 50, UnitPrice = 2.5m }
        }
    };

    [Theory]
    [InlineData(0.15)]
    [InlineData(0)]
    [InlineData(0.25)]
    public void FractionRatesAreAccepted(double rate)
    {
        Assert.True(new CreateQuotationRequestValidator().Validate(Valid((decimal)rate)).IsValid);
    }

    [Fact]
    public void PercentageRateIsRejectedWithTheMessageThatFixesIt()
    {
        var result = new CreateQuotationRequestValidator().Validate(Valid(15m));

        Assert.False(result.IsValid);
        Assert.Contains("0.15 for 15%", result.Errors.Single(e => e.PropertyName == "VatRate").ErrorMessage);
    }

    [Fact]
    public void NegativeRateIsRejected()
    {
        Assert.False(new CreateQuotationRequestValidator().Validate(Valid(-0.15m)).IsValid);
    }

    [Fact]
    public void TermsLongerThanTheColumnAreRejectedBeforeTheInsert()
    {
        var result = new CreateQuotationRequestValidator().Validate(Valid(0.15m, new string('x', 4001)));

        Assert.False(result.IsValid);
        Assert.Equal("Terms", result.Errors.Single().PropertyName);
    }

    [Fact]
    public void AbsentTermsAreRejectedBecauseTheColumnIsNotNull()
    {
        var request = Valid(0.15m);
        request.Terms = null!;

        Assert.False(new CreateQuotationRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void AQuotationLineNeedsASkuAndSomethingToChargeFor()
    {
        var request = Valid(0.15m);
        request.LineItems[0].SKU = "";

        Assert.False(new CreateQuotationRequestValidator().Validate(request).IsValid);

        request.LineItems[0].SKU = "PPE-MASK-IIR";
        request.LineItems[0].Quantity = 0;
        Assert.False(new CreateQuotationRequestValidator().Validate(request).IsValid);

        // A complimentary line is quoted at zero, not rejected.
        request.LineItems[0].Quantity = 1;
        request.LineItems[0].UnitPrice = 0m;
        Assert.True(new CreateQuotationRequestValidator().Validate(request).IsValid);
    }
}
