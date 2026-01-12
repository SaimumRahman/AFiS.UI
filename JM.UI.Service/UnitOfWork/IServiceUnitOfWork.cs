using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Company;
using JM.UI.Service.Designations;
using JM.UI.Service.Shift;
using JM.UI.Service.Employee;
using JM.UI.Service.GroupRole;
using JM.UI.Service.Stores;
using JM.UI.Service.UserGroup;
using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.Service.Sizes;
using JM.UI.Service.Barcodes;

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
        IGroupRoleService GroupRoleService { get; }
        IUserGroupService UserGroupService { get; }
        IShiftService ShiftService { get; }

        ISizesService SizesService { get; }
        IBarcodeService BarcodeService { get; }
    }
}
