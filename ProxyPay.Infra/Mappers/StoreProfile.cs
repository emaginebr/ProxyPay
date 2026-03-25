using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Store;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public class StoreProfile : Profile
    {
        public StoreProfile()
        {
            // Entity <-> Model
            CreateMap<Store, StoreModel>();
            CreateMap<StoreModel, Store>()
                .ForMember(d => d.Invoices, opt => opt.Ignore())
                .ForMember(d => d.Transactions, opt => opt.Ignore())
                .ForMember(d => d.Customers, opt => opt.Ignore())
                .ForMember(d => d.Billings, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<StoreInsertInfo, StoreModel>()
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.ClientId, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
            CreateMap<StoreUpdateInfo, StoreModel>()
                .ForMember(d => d.ClientId, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<StoreModel, StoreInfo>();
        }
    }
}
