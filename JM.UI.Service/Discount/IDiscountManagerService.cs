using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Discount;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Discount
{
    public interface IDiscountManagerService
    {
        Task<IEnumerable<DiscountManagerDTO>> GetAll();
        Task<DiscountManagerDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(DiscountManagerDTO discountManager);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<DiscountTypeDTO>> GetDiscountTypes();
        Task<(bool IsValid, string ErrorMessage)> Validate(DiscountManagerDTO discountManager);
        DiscountManagerDTO CreateNew();
        string Truncate(string? value, int maxChars);
    }
}
