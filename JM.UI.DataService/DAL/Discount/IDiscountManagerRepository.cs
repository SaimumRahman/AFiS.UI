using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Discount;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Discount
{
    public interface IDiscountManagerRepository
    {
        Task<IEnumerable<DiscountManagerDTO>> GetAll();
        Task<DiscountManagerDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(DiscountManagerDTO discountManager);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<DiscountTypeDTO>> GetDiscountTypes();
    }
}
