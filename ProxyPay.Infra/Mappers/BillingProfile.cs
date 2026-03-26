using AutoMapper;
using ProxyPay.DTO;
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
                .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(s => (PaymentMethodEnum)s.PaymentMethod))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (BillingStatusEnum)s.Status))
                .ForMember(d => d.Customer, opt => opt.Ignore())
                .ForMember(d => d.Items, opt => opt.Ignore());
            CreateMap<BillingModel, Billing>()
                .ForMember(d => d.Frequency, opt => opt.MapFrom(s => (int)s.Frequency))
                .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(s => (int)s.PaymentMethod))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status))
                .ForMember(d => d.Store, opt => opt.Ignore())
                .ForMember(d => d.Customer, opt => opt.Ignore())
                .ForMember(d => d.BillingItems, opt => opt.Ignore());

            // Entity <-> Model (BillingItem)
            CreateMap<BillingItem, BillingItemModel>();
            CreateMap<BillingItemModel, BillingItem>()
                .ForMember(d => d.Billing, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<BillingRequest, BillingModel>()
                .ForSourceMember(s => s.ClientId, opt => opt.DoNotValidate())
                .ForMember(d => d.BillingId, opt => opt.Ignore())
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.CustomerId, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.Customer, opt => opt.Ignore())
                .ForMember(d => d.Items, opt => opt.Ignore());
            CreateMap<BillingItemInfo, BillingItemModel>()
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<BillingModel, BillingInfo>()
                .ForMember(d => d.Customer, opt => opt.Ignore());
            CreateMap<BillingItemModel, BillingItemInfo>();
        }
    }
}
