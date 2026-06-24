using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.Service.Coupon
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponDTO>> GetAll();
        Task<CouponDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(CouponDTO coupon);
        Task<ResponseResult> Delete(int id);
        Task<(bool IsValid, string ErrorMessage)> Validate(CouponDTO coupon);
        CouponDTO CreateNew();
    }
}
