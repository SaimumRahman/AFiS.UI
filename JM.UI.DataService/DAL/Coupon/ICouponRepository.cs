using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.DataService.DAL.Coupon
{
    public interface ICouponRepository
    {
        Task<IEnumerable<CouponDTO>> GetAll();
        Task<CouponDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(CouponDTO coupon);
        Task<ResponseResult> Delete(int id);
    }
}
