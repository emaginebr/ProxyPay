using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Invoice;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            // Entity <-> Model
            CreateMap<Invoice, InvoiceModel>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (InvoiceStatusEnum)s.Status));
            CreateMap<InvoiceModel, Invoice>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status))
                .ForMember(d => d.InvoiceItems, opt => opt.Ignore())
                .ForMember(d => d.Customer, opt => opt.Ignore())
                .ForMember(d => d.Store, opt => opt.Ignore());

            // Entity <-> Model
            CreateMap<InvoiceItem, InvoiceItemModel>();
            CreateMap<InvoiceItemModel, InvoiceItem>()
                .ForMember(d => d.Invoice, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<InvoiceInsertInfo, InvoiceModel>()
                .ForMember(d => d.InvoiceId, opt => opt.Ignore())
                .ForMember(d => d.CustomerId, opt => opt.Ignore())
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.InvoiceNumber, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.PaidAt, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
            CreateMap<InvoiceItemInsertInfo, InvoiceItemModel>()
                .ForMember(d => d.InvoiceItemId, opt => opt.Ignore())
                .ForMember(d => d.InvoiceId, opt => opt.Ignore())
                .ForMember(d => d.Total, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<InvoiceModel, InvoiceInfo>();
            CreateMap<InvoiceItemModel, InvoiceItemInfo>();
        }
    }
}
