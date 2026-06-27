using FluentValidation;
using ProxyPay.DTO.Invoice;

namespace ProxyPay.Domain.Validators
{
    public class InvoiceItemRequestValidator : AbstractValidator<InvoiceItemRequest>
    {
        public InvoiceItemRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Item id is required");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Item description is required")
                .MaximumLength(500).WithMessage("Item description must not exceed 500 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be greater than zero");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Item unit price must be greater than zero");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Item discount cannot be negative");
        }
    }
}
