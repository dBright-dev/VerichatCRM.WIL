using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.Core.Entities;
using FluentValidation;

namespace VerichatCRM.API.Validators
{
    // The angle-bracket "is it sanitised" rules that used to live here have been
    // removed. RequestBodySanitisationMiddleware strips markup from the JSON body
    // before binding, so by the time these rules run a value is already clean -
    // a surviving "<" would be a literal comparison in prose ("2 < 3"), not an
    // injection attempt, and rejecting it was a false positive.
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .Must(ProductCategories.IsValid).WithMessage("Invalid product category.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.InitialStockQty)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold cannot be negative.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .Must(BeHttpUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("Image URL must be an absolute http(s) address.");
        }

        private static bool BeHttpUrl(string? url)
            => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
               && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Every field is optional and omitted values are left untouched, so the
    /// rules are skipped rather than failed when a property arrives as null.
    /// </summary>
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name cannot be cleared.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.")
                .When(x => x.ProductName != null);

            RuleFor(x => x.Category)
                .Must(ProductCategories.IsValid).WithMessage("Invalid product category.")
                .When(x => x.Category != null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold cannot be negative.")
                .When(x => x.LowStockThreshold.HasValue);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .Must(BeHttpUrl).WithMessage("Image URL must be an absolute http(s) address.")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }

        private static bool BeHttpUrl(string? url)
            => Uri.TryCreate(url, UriKind.Absolute, out var parsed)
               && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }

    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            // ClientId is not validated here. A client user's order has no
            // ClientId in the body - OrderController assigns it from the JWT
            // after model binding, so a NotEmpty rule at binding stage would
            // reject every legitimate client-user order before that happened.
            RuleFor(x => x.LineItems)
                .NotEmpty().WithMessage("Order must contain at least one line item.");

            RuleForEach(x => x.LineItems).SetValidator(new OrderLineItemRequestValidator());
        }
    }

    public class OrderLineItemRequestValidator : AbstractValidator<OrderLineItemRequest>
    {
        public OrderLineItemRequestValidator()
        {
            RuleFor(x => x.SKU).NotEmpty().WithMessage("SKU is required.");
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }

    public class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
    {
        public CreatePurchaseOrderRequestValidator()
        {
            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters.");

            RuleFor(x => x.LineItems).NotEmpty().WithMessage("Purchase order must contain at least one line item.");
            RuleForEach(x => x.LineItems).SetValidator(new PurchaseOrderLineItemRequestValidator());
        }
    }

    public class PurchaseOrderLineItemRequestValidator : AbstractValidator<PurchaseOrderLineItemRequest>
    {
        public PurchaseOrderLineItemRequestValidator()
        {
            RuleFor(x => x.SKU).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitCost).GreaterThan(0);
        }
    }

    public class CreateQuotationRequestValidator : AbstractValidator<CreateQuotationRequest>
    {
        public CreateQuotationRequestValidator()
        {
            // ClientId and "at least one line item" are deliberately not repeated
            // here. QuotationController.Create answers those with "A quotation
            // needs a client." / "A quotation needs at least one line item.", and
            // a validator would pre-empt the friendlier wording with a field error.

            // Every consumer treats this as a fraction - QuotationService
            // multiplies the subtotal by it, PdfGenerator prints "VAT (" + rate *
            // 100 + "%)". A caller who sends 15 meaning 15% does not get a 150%
            // quote: decimal(5,4) overflows above 9.9999 and the insert failed as
            // an unhandled "Parameter value '15,0000' is out of range" HTTP 500.
            RuleFor(x => x.VatRate)
                .GreaterThanOrEqualTo(0).WithMessage("VAT rate cannot be negative.")
                .LessThan(1).WithMessage("VAT rate is a fraction - send 0.15 for 15%, not 15.");

            RuleFor(x => x.Terms)
                .NotNull().WithMessage("Terms cannot be null.")
                .MaximumLength(4000).WithMessage("Terms cannot exceed 4000 characters.");

            RuleForEach(x => x.LineItems).SetValidator(new CreateQuotationLineItemRequestValidator());
        }
    }

    public class CreateQuotationLineItemRequestValidator : AbstractValidator<CreateQuotationLineItemRequest>
    {
        public CreateQuotationLineItemRequestValidator()
        {
            RuleFor(x => x.SKU).NotEmpty().WithMessage("Every quotation line needs a SKU.");
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            // Zero is allowed: a complimentary or bundled line is quoted at R0.00.
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        }
    }
}
