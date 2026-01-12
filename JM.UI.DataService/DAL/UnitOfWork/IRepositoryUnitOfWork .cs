using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Employees;
using JM.UI.DataService.DAL.GroupRole;
using JM.UI.DataService.DAL.Stores;
using JM.UI.DataService.DAL.UserGroup;
using JM.UI.DataService.DAL.Shift;
using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.DataService.DAL.Sizes;

namespace JM.UI.DataService.DAL.UnitOfWork
{
    public interface IRepositoryUnitOfWork : IDisposable
    {
        IApprovalLevelRepository ApprovalLevelRepository { get; }
        IApprovalWorkflowRepository ApprovalWorkflowRepository { get; }
        IApprovalLevelApproverRepository ApprovalLevelApproverRepository { get; }
        IPendingApprovalRepository PendingApprovalRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        IDesignationRepository DesignationRepository { get; }
        IBanksRepository BanksRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }
        IStoreRepository StoreRepository { get; }
        IGroupRoleRepository GroupRoleRepository { get; }
        IUserGroupRepository UserGroupRepository { get; }
        IShiftRepository ShiftRepository { get; }
        ISizesRepository SizesRepository { get; }
    }

}
