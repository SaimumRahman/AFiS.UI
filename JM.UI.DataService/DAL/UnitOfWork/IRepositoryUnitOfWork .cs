using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.Actions;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Employees;
using JM.UI.DataService.DAL.GroupActionPermission;
using JM.UI.DataService.DAL.GroupRole;
using JM.UI.DataService.DAL.GroupRoutePermissions;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Routes;
using JM.UI.DataService.DAL.Shift;
using JM.UI.DataService.DAL.Sizes;
using JM.UI.DataService.DAL.Stores;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.UserGroup;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.Vouchers;
using System;
using System.Collections.Generic;
using System.Text;

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
        IColorsRepository ColorsRepository { get; }
        ISizesRepository SizesRepository { get; }
        IBarcodeRepository BarcodeRepository { get; }
        IVoucherDetailsRepository VoucherDetailsRepository { get; }
        IVoucherRepository VoucherRepository { get; }
        IAccountsGroupsRepository AccountsGroupsRepository { get; }
        IAccountsRepository AccountsRepository { get; }
        ISupplierRepository SupplierRepository { get; }
        IPurchaseOrderRepository PurchaseOrderRepository { get; }
        IPurchaseRepository PurchaseRepository { get; }
        IPurchaseReturnRepository PurchaseReturnRepository { get; }
        IGroupRepository GroupRepository { get; }
        IActionRepository ActionRepository { get; }
        IRouteRepository RouteRepository { get; }
        IGroupRoutePermissionRepository GroupRoutePermissionRepository { get; }
        IGroupActionPermissionRepository GroupActionPermissionRepository { get; }
    }

}
