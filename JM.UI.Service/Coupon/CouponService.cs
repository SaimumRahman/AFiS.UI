using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.Service.Coupon
{
    public class CouponService : ICouponService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CouponService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<CouponDTO>> GetAll()
            => await _repositoryUnitOfWork.CouponRepository.GetAll();

        public async Task<CouponDTO?> GetById(int id)
            => await _repositoryUnitOfWork.CouponRepository.GetById(id);

        public async Task<ResponseResult> SaveUpdate(CouponDTO coupon)
        {
            var validation = await Validate(coupon);
            if (!validation.IsValid)
                return new ResponseResult { IsSuccessStatus = false, Message = validation.ErrorMessage };

            if (coupon.Id == 0)
                coupon.CreatedDate = DateTime.Now;
            else
                coupon.ModifiedDate = DateTime.Now;

            return await _repositoryUnitOfWork.CouponRepository.SaveUpdate(coupon);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.CouponRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = $"Failed to delete coupon: {ex.Message}" };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> Validate(CouponDTO coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.CouponCode))
                return Task.FromResult((false, "Coupon code is required."));

            if (coupon.CouponTypeId <= 0)
                return Task.FromResult((false, "Coupon type is required."));

            if (coupon.DiscountValue <= 0)
                return Task.FromResult((false, "Discount value must be greater than 0."));

            if (coupon.StartDate == default)
                return Task.FromResult((false, "Start date is required."));

            if (coupon.EndDate == default)
                return Task.FromResult((false, "End date is required."));

            if (coupon.EndDate <= coupon.StartDate)
                return Task.FromResult((false, "End date must be after start date."));

            return Task.FromResult((true, string.Empty));
        }

        public CouponDTO CreateNew()
        {
            return new CouponDTO
            {
                IsActive = true,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                CreatedDate = DateTime.Now,
                ApplicableToAll = true
            };
        }
    }
}
