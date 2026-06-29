using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Coupon;

namespace JM.UI.Service.Coupon
{
    public class CouponTypeService : ICouponTypeService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CouponTypeService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<CouponTypeDTO>> GetAll()
            => await _repositoryUnitOfWork.CouponTypeRepository.GetAll();

        public async Task<CouponTypeDTO?> GetById(int id)
            => await _repositoryUnitOfWork.CouponTypeRepository.GetById(id);

        public async Task<ResponseResult> SaveUpdate(CouponTypeDTO couponType)
        {
            var validation = await Validate(couponType);
            if (!validation.IsValid)
                return new ResponseResult { IsSuccessStatus = false, Message = validation.ErrorMessage };

            return await _repositoryUnitOfWork.CouponTypeRepository.SaveUpdate(couponType);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.CouponTypeRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = $"Failed to delete coupon type: {ex.Message}" };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> Validate(CouponTypeDTO couponType)
        {
            if (string.IsNullOrWhiteSpace(couponType.TypeName))
                return Task.FromResult((false, "Coupon type name is required."));

            if (couponType.TypeName.Length > 100)
                return Task.FromResult((false, "Coupon type name cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public CouponTypeDTO CreateNew() => new CouponTypeDTO();
    }
}
