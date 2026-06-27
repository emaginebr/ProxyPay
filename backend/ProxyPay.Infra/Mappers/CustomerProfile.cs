using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Customer;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            // Entity <-> Model
            CreateMap<Customer, CustomerModel>();
            CreateMap<CustomerModel, Customer>()
                .ForMember(d => d.Invoices, opt => opt.Ignore())
                .ForMember(d => d.Billings, opt => opt.Ignore())
                .ForMember(d => d.Store, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<CustomerInsertInfo, CustomerModel>()
                .ForMember(d => d.CustomerId, opt => opt.Ignore())
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
            CreateMap<CustomerUpdateInfo, CustomerModel>()
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<CustomerModel, CustomerInfo>();
        }
    }
}
