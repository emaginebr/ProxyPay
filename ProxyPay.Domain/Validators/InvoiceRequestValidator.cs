using FluentValidation;
using ProxyPay.DTO.Invoice;

namespace ProxyPay.Domain.Validators
{
    public class InvoiceRequestValidator : AbstractValidator<InvoiceRequest>
    {
        public InvoiceRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId is required");

            RuleFor(x => x.Customer)
                .NotNull().WithMessage("Customer is required")
                .SetValidator(new CustomerInsertInfoValidator());

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.Now).WithMessage("Due date must be in the future");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items are required")
                .NotEmpty().WithMessage("Invoice must have at least one item");

            RuleForEach(x => x.Items)
                .SetValidator(new InvoiceItemRequestValidator());
        }
    }
}
