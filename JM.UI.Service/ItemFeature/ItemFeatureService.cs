using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.ItemFeatures;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.ItemFeature
{
    public class ItemFeatureService : IItemFeatureService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ItemFeatureService(IRepositoryUnitOfWork repositoryUnitOfWork)
        {
            _repositoryUnitOfWork = repositoryUnitOfWork;
        }

        public async Task<IEnumerable<ItemFeatureDTO>> GetItemFeatures()
        {
            return await _repositoryUnitOfWork.ItemFeatureRepository.GetItemFeatures();
        }

        public async Task<ResponseResult> SaveItemFeature(ItemFeatureDTO feature)
        {
            if (string.IsNullOrWhiteSpace(feature.FeatureName))
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Feature name is required."
                };
            }

            return await _repositoryUnitOfWork.ItemFeatureRepository.SaveItemFeature(feature);
        }
    }
}
