using AutoMapper;
using ProxyPay.Domain.Models;
using ProxyPay.DTO.Transaction;
using ProxyPay.Infra.Context;

namespace ProxyPay.Infra.Mappers
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            // Entity <-> Model
            CreateMap<Transaction, TransactionModel>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => (TransactionTypeEnum)s.Type))
                .ForMember(d => d.Category, opt => opt.MapFrom(s => (TransactionCategoryEnum)s.Category));
            CreateMap<TransactionModel, Transaction>()
                .ForMember(d => d.Type, opt => opt.MapFrom(s => (int)s.Type))
                .ForMember(d => d.Category, opt => opt.MapFrom(s => (int)s.Category))
                .ForMember(d => d.Invoice, opt => opt.Ignore())
                .ForMember(d => d.Store, opt => opt.Ignore());

            // DTO -> Model
            CreateMap<TransactionInsertInfo, TransactionModel>()
                .ForMember(d => d.TransactionId, opt => opt.Ignore())
                .ForMember(d => d.StoreId, opt => opt.Ignore())
                .ForMember(d => d.Balance, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            // Model -> DTO
            CreateMap<TransactionModel, TransactionInfo>();
        }
    }
}
