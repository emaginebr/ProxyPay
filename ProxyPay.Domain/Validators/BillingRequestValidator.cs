using FluentValidation;
using ProxyPay.Domain.Core;
using ProxyPay.DTO.Billing;

namespace ProxyPay.Domain.Validators
{
    public class BillingRequestValidator : AbstractValidator<BillingRequest>
    {
        public BillingRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId is required");

            RuleFor(x => x.Customer)
                .NotNull().WithMessage("Customer is required");

            When(x => x.Customer != null, () =>
            {
                RuleFor(x => x.Customer.Name)
                    .NotEmpty().WithMessage("Customer name is required");

                RuleFor(x => x.Customer.Email)
                    .NotEmpty().WithMessage("Customer email is required")
                    .EmailAddress().WithMessage("Customer email is not valid");

                RuleFor(x => x.Customer.DocumentId)
                    .NotEmpty().WithMessage("Customer CPF (documentId) is required")
                    .Must(Utils.IsValidCpf).WithMessage("Customer CPF (documentId) is invalid");
            });

            RuleFor(x => x.Frequency)
                .IsInEnum().WithMessage("Invalid frequency");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method");

            RuleFor(x => x.BillingStartDate)
                .GreaterThan(System.DateTime.Now).WithMessage("Billing start date must be in the future");

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items are required")
                .NotEmpty().WithMessage("Billing must have at least one item");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.Description)
                    .NotEmpty().WithMessage("Item description is required");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Item quantity must be greater than zero");

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThan(0).WithMessage("Item unit price must be greater than zero");

                item.RuleFor(x => x.Discount)
                    .GreaterThanOrEqualTo(0).WithMessage("Item discount cannot be negative");
            });
        }
    }
}
