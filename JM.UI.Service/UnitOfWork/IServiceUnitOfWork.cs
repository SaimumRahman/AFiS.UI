using JM.UI.DataService.DAL.StockOpenings;
using JM.UI.Service.Accounts;
using JM.UI.Service.Accounts;
using JM.UI.Service.Accounts;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.Action;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Barcodes;
using JM.UI.Service.Barcodes;
using JM.UI.Service.Barcodes;
using JM.UI.Service.Colors;
using JM.UI.Service.Colors;
using JM.UI.Service.Colors;
using JM.UI.Service.Company;
using JM.UI.Service.Designations;
using JM.UI.Service.Designs;
using JM.UI.Service.Employee;
using JM.UI.Service.GroupActionPermission;
using JM.UI.Service.GroupRole;
using JM.UI.Service.GroupRoutePermission;
using JM.UI.Service.Groups;
using JM.UI.Service.Groups;
using JM.UI.Service.Groups;
using JM.UI.Service.ItemBrand;
using JM.UI.Service.ItemCatalogue;
using JM.UI.Service.ItemFeature;
using JM.UI.Service.ItemOrigin;
using JM.UI.Service.Items;
using JM.UI.Service.MesurementUnits;
using JM.UI.Service.MesurementUnits;
using JM.UI.Service.PurchaseOrders;
using JM.UI.Service.PurchaseOrders;
using JM.UI.Service.PurchaseOrders;
using JM.UI.Service.PurchaseReturnItems;
using JM.UI.Service.PurchaseReturnItems;
using JM.UI.Service.PurchaseReturns;
using JM.UI.Service.PurchaseReturns;
using JM.UI.Service.PurchaseReturns;
using JM.UI.Service.Purchases;
using JM.UI.Service.Purchases;
using JM.UI.Service.Purchases;
using JM.UI.Service.Routes;
using JM.UI.Service.Shift;
using JM.UI.Service.Sizes;
using JM.UI.Service.Sizes;
using JM.UI.Service.Sizes;
using JM.UI.Service.Stores;
using JM.UI.Service.SubGroups;
using JM.UI.Service.SubGroups;
using JM.UI.Service.SupplierPayments;
using JM.UI.Service.SupplierPayments;
using JM.UI.Service.Suppliers;
using JM.UI.Service.Suppliers;
using JM.UI.Service.Suppliers;
using JM.UI.Service.UserGroup;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.Vouchers;
using JM.UI.Service.Vouchers;
using JM.UI.Service.Vouchers;
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
        IGroupRoleService GroupRoleService { get; }
        IUserGroupService UserGroupService { get; }
        IShiftService ShiftService { get; }

        ISizesService SizesService { get; }
        IBarcodeService BarcodeService { get; }
        IVoucherDetailsService VoucherDetailsService { get; }
        IVoucherService VoucherService { get; }
        IAccountsGroupsService AccountsGroupsService { get; }
        IAccountsService AccountsService { get; }
        ISupplierService SupplierService { get; }
        IPurchaseOrderService PurchaseOrderService { get; }
        IPurchaseService PurchaseService { get; }
        IPurchaseReturnService PurchaseReturnService { get; }
        IGroupService GroupService { get; }
        ISubGroupService SubGroupService { get; }
        IMesurementUnitService MesurementUnitService { get; }
        IColorsService ColorsService { get;}
        IActionService ActionService { get;}
        IRouteService RouteService { get;}
        IGroupRoutePermissionService GroupRoutePermissionService { get;}
        IGroupActionPermissionService GroupActionPermissionService { get;}


        IItemService ItemService { get; }
        IPurchaseReturnItemService PurchaseReturnItemService { get; }
        ISupplierPaymentService SupplierPaymentService { get; }
        IItemOriginService ItemOriginService { get; }
        IItemFeatureService ItemFeatureService { get; }
        IItemBrandService ItemBrandService { get; }
        IDesignService DesignService { get; }
        IStockOpeningRepository StockOpeningService { get; }
        IItemCatalogueService ItemCatalogueService { get; }
    }
}
