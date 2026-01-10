using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Company;
using JM.UI.Service.Designations;
using JM.UI.Service.Employee;
using JM.UI.Service.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.UnitOfWork
{
    public interface IServiceUnitOfWork : IDisposable
    {
        IApprovalLevelService ApprovalLevelService { get; }
        IApprovalWorkflowService ApprovalWorkflowService { get; }
        IApprovalLevelApproverService ApprovalLevelApproverService { get; }
        IPendingApprovalService PendingApprovalService { get; }
        IDesignationService DesignationService { get; }
        ICompanyService CompanyService { get; }
        IEmployeeService EmployeeService { get; }
        IStoreService StoreService { get; }
        IBanksService BanksService { get; }
    }
}
