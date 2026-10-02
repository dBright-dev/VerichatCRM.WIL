using VerichatCRM.API.DTOs.Requests;
using FluentValidation;

namespace VerichatCRM.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.")
                .Must(BeSanitized).WithMessage("Product name contains invalid characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .Must(c => new[] { "PPE", "Laboratory", "Hospital Infrastructure", "Home Care", "Infant Care" }
                    .Contains(c)).WithMessage("Invalid product category.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.InitialStockQty)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold cannot be negative.");
        }

        private static bool BeSanitized(string input)
        {
            // Reject input containing HTML tags or script fragments
            return !input.Contains("<") && !input.Contains(">") && !input.Contains("javascript:");
        }
    }

    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

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
                .NotEmpty().MaximumLength(200)
                .Must(s => !s.Contains("<") && !s.Contains(">"))
                .WithMessage("Supplier name contains invalid characters.");

            RuleFor(x => x.LineItems).NotEmpty();
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
}
