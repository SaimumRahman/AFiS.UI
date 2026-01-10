using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Designations;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.UnitOfWork
{
    public class ServiceUnitOfWork : IServiceUnitOfWork
    {
        private readonly IRepositoryUnitOfWork _repoUow;
        public IApprovalLevelService ApprovalLevelService { get; }
        public IApprovalWorkflowService ApprovalWorkflowService { get; }
        public IApprovalLevelApproverService ApprovalLevelApproverService { get; }
        public IPendingApprovalService PendingApprovalService { get; }
        public IDesignationService DesignationService { get; }

        public ServiceUnitOfWork(IRepositoryUnitOfWork repoUow)
        {
            _repoUow = repoUow;
           
            ApprovalLevelService = new ApprovalLevelService(_repoUow);
            ApprovalWorkflowService = new ApprovalWorkflowService(_repoUow);
            ApprovalLevelApproverService = new ApprovalLevelApproverService(_repoUow);
            ApprovalLevelApproverService = new ApprovalLevelApproverService(_repoUow);
            PendingApprovalService = new PendingApprovalService(_repoUow);
        }

        public void Dispose()
        {
            _repoUow.Dispose();
        }
    }


}
