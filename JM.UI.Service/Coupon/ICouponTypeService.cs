using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.Service.Coupon
{
    public interface ICouponTypeService
    {
        Task<IEnumerable<CouponTypeDTO>> GetAll();
        Task<CouponTypeDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(CouponTypeDTO couponType);
        Task<ResponseResult> Delete(int id);
        Task<(bool IsValid, string ErrorMessage)> Validate(CouponTypeDTO couponType);
        CouponTypeDTO CreateNew();
    }
}
