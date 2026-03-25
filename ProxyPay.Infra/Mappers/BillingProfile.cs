using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Billing;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public class BillingProfile : Profile
    {
        public BillingProfile()
        {
            // Entity <-> Model (enum <-> int)
            CreateMap<Billing, BillingModel>()
                .ForMember(d => d.Frequency, opt => opt.MapFrom(s => (BillingFrequencyEnum)s.Frequency))
                .ForMember(d => d.BillingStrategy, opt => opt.MapFrom(s => (BillingStrategyEnum)s.BillingStrategy))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (BillingStatusEnum)s.Status));
            CreateMap<BillingModel, Billing>()
                .ForMember(d => d.Frequency, opt => opt.MapFrom(s => (int)s.Frequency))
                .ForMember(d => d.BillingStrategy, opt => opt.MapFrom(s => (int)s.BillingStrategy))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status))
                .ForMember(d => d.Store, opt => opt.Ignore())
                .ForMember(d => d.Customer, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<BillingInsertInfo, BillingModel>()
                .ForMember(d => d.BillingId, opt => opt.Ignore())
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.CustomerId, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<BillingModel, BillingInfo>()
                .ForMember(d => d.Customer, opt => opt.Ignore());
        }
    }
}
