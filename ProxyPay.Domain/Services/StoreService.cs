using AutoMapper;
using ProxyPay.Infra.Interfaces.Repository;
using ProxyPay.Domain.Models;
using ProxyPay.Domain.Interfaces;
using ProxyPay.DTO.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyPay.Domain.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository<StoreModel> _storeRepository;
        private readonly IMapper _mapper;

        public StoreService(
            IStoreRepository<StoreModel> storeRepository,
            IMapper mapper
        )
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<StoreModel> GetByIdAsync(long storeId, long userId)
        {
            var model = await _storeRepository.GetByIdAsync(storeId);
            if (model == null)
                return null;

            if (model.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: store does not belong to this user");

            return model;
        }

        public Task<StoreInfo> GetStoreInfoAsync(StoreModel model)
        {
            var info = _mapper.Map<StoreInfo>(model);
            return Task.FromResult(info);
        }

        public async Task<IList<StoreInfo>> ListByUserAsync(long userId)
        {
            var stores = await _storeRepository.ListByUserAsync(userId);
            return stores.Select(s => _mapper.Map<StoreInfo>(s)).ToList();
        }

        public async Task<StoreModel> InsertAsync(StoreInsertInfo store, long userId)
        {
            var model = _mapper.Map<StoreModel>(store);
            model.UserId = userId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            return await _storeRepository.InsertAsync(model);
        }

        public async Task<StoreModel> UpdateAsync(StoreUpdateInfo store, long userId)
        {
            var existing = await _storeRepository.GetByIdAsync(store.StoreId);
            if (existing == null)
                throw new Exception("Store not found");

            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: store does not belong to this user");

            _mapper.Map(store, existing);
            existing.UpdatedAt = DateTime.Now;

            return await _storeRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(long storeId, long userId)
        {
            var existing = await _storeRepository.GetByIdAsync(storeId);
            if (existing == null)
                throw new Exception("Store not found");

            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("Access denied: store does not belong to this user");

            await _storeRepository.DeleteAsync(storeId);
        }
    }
}
