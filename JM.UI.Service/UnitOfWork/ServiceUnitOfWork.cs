using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Bankss;
using JM.UI.Service.Company;
using JM.UI.Service.Designations;
using JM.UI.Service.Shift;
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
        public ICompanyService CompanyService { get; }
        public IDesignationService DesignationService { get; }
        public IBanksService BanksService { get; }
        public IShiftService ShiftService { get; }

        public ServiceUnitOfWork(IRepositoryUnitOfWork repoUow)
        {
            _repoUow = repoUow;
           
            ApprovalLevelService = new ApprovalLevelService(_repoUow);
            ApprovalWorkflowService = new ApprovalWorkflowService(_repoUow);
            ApprovalLevelApproverService = new ApprovalLevelApproverService(_repoUow);
            ApprovalLevelApproverService = new ApprovalLevelApproverService(_repoUow);
            PendingApprovalService = new PendingApprovalService(_repoUow);
            DesignationService = new DesignationService(_repoUow);
            CompanyService = new CompanyService(_repoUow);
            BanksService = new BanksService(_repoUow);
            ShiftService = new ShiftService(_repoUow);
        }

        public void Dispose()
        {
            _repoUow.Dispose();
        }
    }


}
