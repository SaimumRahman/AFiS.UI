using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Action
{
    public class ActionService : IActionService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ActionService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ActionDTO>> GetAllActions()
        {
            return await _repositoryUnitOfWork.ActionRepository.GetAllActions();
        }

        public async Task<ActionDTO?> GetActionById(int actionId)
        {
            if (actionId <= 0)
                throw new ArgumentException("Invalid action ID", nameof(actionId));

            return await _repositoryUnitOfWork.ActionRepository.GetActionById(actionId);
        }

        public async Task<ResponseResult> CreateAction(ActionDTO action)
        {
            if (string.IsNullOrWhiteSpace(action.ActionKey))
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Action Key is required"
                };
            }

            return await _repositoryUnitOfWork.ActionRepository.CreateAction(action);
        }

        public async Task<ResponseResult> UpdateAction(ActionDTO action)
        {
            if (action.ActionId <= 0)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Invalid action ID"
                };
            }

            if (string.IsNullOrWhiteSpace(action.ActionKey))
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Action Key is required"
                };
            }

            return await _repositoryUnitOfWork.ActionRepository.UpdateAction(action);
        }

        public async Task<ResponseResult> DeleteAction(int actionId)
        {
            if (actionId <= 0)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Invalid action ID"
                };
            }

            return await _repositoryUnitOfWork.ActionRepository.DeleteAction(actionId);
        }
    }

}
