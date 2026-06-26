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

            model.ValidateOwnership(userId);
            return model;
        }

        public async Task<StoreModel> GetByClientIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new Exception("ClientId is required");

            var model = await _storeRepository.GetByClientIdAsync(clientId);
            if (model == null)
                throw new Exception("Store not found for the provided ClientId");

            return model;
        }

        public async Task<StoreModel> InsertAsync(StoreInsertInfo store, long userId)
        {
            var existing = await _storeRepository.ListByUserAsync(userId);
            if (existing.Any())
                throw new Exception("User already has a store");

            var model = _mapper.Map<StoreModel>(store);
            model.GenerateClientId();
            model.SetOwner(userId);
            model.MarkCreated();

            return await _storeRepository.InsertAsync(model);
        }

        public async Task<StoreModel> UpdateAsync(StoreUpdateInfo store, long userId)
        {
            var existing = await _storeRepository.GetByIdAsync(store.StoreId);
            if (existing == null)
                throw new Exception("Store not found");

            existing.ValidateOwnership(userId);
            _mapper.Map(store, existing);
            existing.MarkUpdated();

            return await _storeRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(long storeId, long userId)
        {
            var existing = await _storeRepository.GetByIdAsync(storeId);
            if (existing == null)
                throw new Exception("Store not found");

            existing.ValidateOwnership(userId);
            await _storeRepository.DeleteAsync(storeId);
        }
    }
}
