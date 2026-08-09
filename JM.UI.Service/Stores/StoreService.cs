using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Stores
{
    public class StoreService : IStoreService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public StoreService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<StoreDTO>> GetStores()
        {
            var stores = await _repositoryUnitOfWork.StoreRepository.GetStores();
            return stores.Select(s => new StoreDTO
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Address = s.Address,
                Contact = s.Contact,
                Email = s.Email,
                VAT = s.VAT,
                TIN = s.TIN,
                LetterHeadFile = s.LetterHeadFile,
                UseLetterHead = s.UseLetterHead,
                CreatedOn = s.CreatedOn,
                CreatedBy = s.CreatedBy,
                ModifiedOn = s.ModifiedOn,
                ModifiedBy = s.ModifiedBy,
                FinancialAccountIds = s.FinancialAccountIds,
                StoreAccounts = s.StoreAccounts
            }).ToList();
        }

        public async Task<StoreDTO?> GetStoreById(int id)
        {
            return await _repositoryUnitOfWork.StoreRepository.GetStoreById(id);
        }

        public async Task<ResponseResult> SaveUpdateStore(StoreDTO store)
        {
            var validation = await ValidateStore(store);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (store.Id == 0)
            {
                store.CreatedOn = DateTime.Now;
            }
            else
            {
                store.ModifiedOn = DateTime.Now;
            }

            return await _repositoryUnitOfWork.StoreRepository.SaveUpdateStore(store);
        }

        public async Task<ResponseResult> DeleteStore(int id)
        {
            try
            {
                await _repositoryUnitOfWork.StoreRepository.DeleteStore(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Store deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete store: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateStore(StoreDTO store)
        {
            if (string.IsNullOrWhiteSpace(store.Name))
                return Task.FromResult((false, "Store name is required."));

            if (store.Name.Length > 250)
                return Task.FromResult((false, "Store name cannot exceed 250 characters."));

            if (!string.IsNullOrWhiteSpace(store.Code) && store.Code.Length > 50)
                return Task.FromResult((false, "Store code cannot exceed 50 characters."));

            if (!string.IsNullOrWhiteSpace(store.Address) && store.Address.Length > 500)
                return Task.FromResult((false, "Address cannot exceed 500 characters."));

            if (!string.IsNullOrWhiteSpace(store.Contact) && store.Contact.Length > 50)
                return Task.FromResult((false, "Contact cannot exceed 50 characters."));

            if (!string.IsNullOrWhiteSpace(store.Email))
            {
                if (store.Email.Length > 100)
                    return Task.FromResult((false, "Email cannot exceed 100 characters."));

                if (!IsValidEmail(store.Email))
                    return Task.FromResult((false, "Invalid email format."));
            }

            if (!string.IsNullOrWhiteSpace(store.VAT) && store.VAT.Length > 50)
                return Task.FromResult((false, "VAT cannot exceed 50 characters."));

            if (!string.IsNullOrWhiteSpace(store.TIN) && store.TIN.Length > 50)
                return Task.FromResult((false, "TIN cannot exceed 50 characters."));

            return Task.FromResult((true, string.Empty));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public StoreDTO CreateNewStore()
        {
            return new StoreDTO
            {
                CreatedOn = DateTime.Now,
                UseLetterHead = false
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
