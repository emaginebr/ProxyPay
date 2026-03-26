using FluentValidation;
using ProxyPay.DTO.Customer;

namespace ProxyPay.Domain.Validators
{
    public class CustomerInsertInfoValidator : AbstractValidator<CustomerInsertInfo>
    {
        public CustomerInsertInfoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(240).WithMessage("Customer name must not exceed 240 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Customer email is required")
                .EmailAddress().WithMessage("Customer email is not valid");
        }
    }
}
