using FluentValidation;
using ProxyPay.Domain.Core;
using ProxyPay.DTO.Invoice;
using System.Linq;

namespace ProxyPay.Domain.Validators
{
    public class QRCodeRequestValidator : AbstractValidator<QRCodeRequest>
    {
        public QRCodeRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId is required");

            RuleFor(x => x.Customer)
                .NotNull().WithMessage("Customer is required");

            When(x => x.Customer != null, () =>
            {
                RuleFor(x => x.Customer.Name)
                    .NotEmpty().WithMessage("Customer name is required")
                    .MaximumLength(240).WithMessage("Customer name must not exceed 240 characters");

                RuleFor(x => x.Customer.Email)
                    .NotEmpty().WithMessage("Customer email is required")
                    .EmailAddress().WithMessage("Customer email is not valid");

                RuleFor(x => x.Customer.DocumentId)
                    .NotEmpty().WithMessage("Customer CPF (documentId) is required")
                    .Must(Utils.IsValidCpf).WithMessage("Customer CPF (documentId) is invalid");
            });

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items are required")
                .NotEmpty().WithMessage("Invoice must have at least one item");

            RuleForEach(x => x.Items)
                .SetValidator(new InvoiceItemRequestValidator());

            RuleFor(x => x.Items)
                .Must(items => items == null || items.Sum(i => (i.Quantity * i.UnitPrice) - i.Discount) > 0)
                .WithMessage("Total amount must be greater than zero")
                .When(x => x.Items != null && x.Items.Count > 0);
        }
    }
}
