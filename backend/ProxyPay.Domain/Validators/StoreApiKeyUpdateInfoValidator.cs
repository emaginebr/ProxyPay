using FluentValidation;
using ProxyPay.DTO.Store;

namespace ProxyPay.Domain.Validators
{
    public class StoreApiKeyUpdateInfoValidator : AbstractValidator<StoreApiKeyUpdateInfo>
    {
        public StoreApiKeyUpdateInfoValidator()
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("AbacatePay API key is required")
                .MaximumLength(500).WithMessage("AbacatePay API key must not exceed 500 characters");
        }
    }
}
