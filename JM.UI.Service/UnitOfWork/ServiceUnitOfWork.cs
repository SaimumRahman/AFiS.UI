using JM.UI.DataService.DAL.GroupActionPermission;
using JM.UI.DataService.DAL.StockOpenings;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Service.Accounts;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.Action;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Bankss;
using JM.UI.Service.Barcode;
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
using JM.UI.Service.Coupon;
using JM.UI.Service.CustomerDetails;
using JM.UI.Service.Discount;
using JM.UI.Service.ItemBrand;
using JM.UI.Service.ItemCatalogue;
using JM.UI.Service.ItemFeature;
using JM.UI.Service.ItemOrigin;
using JM.UI.Service.Items;
using JM.UI.Service.Items;
using JM.UI.Service.MembershipType;
using JM.UI.Service.MesurementUnits;
using JM.UI.Service.MesurementUnits;
using JM.UI.Service.PurchaseOrders;
using JM.UI.Service.PurchaseReturnItems;
using JM.UI.Service.PurchaseReturnItems;
using JM.UI.Service.PurchaseReturns;
using JM.UI.Service.Purchases;
using JM.UI.Service.Routes;
using JM.UI.Service.Shift;
using JM.UI.Service.Sizes;
using JM.UI.Service.Stock;
using JM.UI.Service.StockReport;
using JM.UI.Service.Stores;
using JM.UI.Service.SubGroups;
using JM.UI.Service.SubGroups;
using JM.UI.Service.SupplierPayments;
using JM.UI.Service.SupplierPayments;
using JM.UI.Service.Suppliers;
using JM.UI.Service.Transfer;
using JM.UI.Service.InvRequisition;
using JM.UI.Service.UserGroup;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.Vouchers;
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
        public IEmployeeService EmployeeService { get; }
        public IStoreService StoreService { get; }
        public IBanksService BanksService { get; }
        public IGroupRoleService GroupRoleService { get; }
        public IUserGroupService UserGroupService { get; }
        public IShiftService ShiftService { get; }
        public ISizesService SizesService { get; }
        public IBarcodeService BarcodeService { get; }
        public IVoucherDetailsService VoucherDetailsService { get; }
        public IVoucherService VoucherService { get; }
        public IAccountsGroupsService AccountsGroupsService { get; }
        public IAccountsService AccountsService { get; }
        public ISupplierService SupplierService { get; }
        public IPurchaseOrderService PurchaseOrderService { get; }
        public IPurchaseService PurchaseService { get; private set; }
        public IPurchaseReturnService PurchaseReturnService { get; private set; }
        public IGroupService GroupService { get; private set; }
        public ISubGroupService SubGroupService { get; private set; }
        public IMesurementUnitService MesurementUnitService { get; private set; }
        public IColorsService ColorsService { get; private set; }
        public IActionService ActionService { get; private set; }
        public IRouteService RouteService { get; private set; }
        public IGroupRoutePermissionService GroupRoutePermissionService { get; private set; }
        public IGroupActionPermissionService GroupActionPermissionService { get; private set; }
        public IItemService ItemService { get; private set; }
        public IPurchaseReturnItemService PurchaseReturnItemService { get; private set; }
        public ISupplierPaymentService SupplierPaymentService { get; private set; }
        public IDesignService DesignService { get; private set; }
        public IItemOriginService ItemOriginService { get; private set; }
        public IItemFeatureService ItemFeatureService { get; private set; }
        public IItemBrandService ItemBrandService { get; private set; }
        public IStockOpeningRepository StockOpeningService { get; private set; }
        public IItemCatalogueService ItemCatalogueService { get; private set; }
        public ICurrentStockReportService CurrentStockReportService { get; private set; }
        public ITransferService TransferService { get; private set; }
        public IStockService StockService { get; private set; }
        public IBarcodePrintConfigService BarcodePrintConfigService { get; private set; }
        public IMembershipTypeService MembershipTypeService { get; private set; }
        public ICustomerDetailsService CustomerDetailsService { get; private set; }
        public IDiscountManagerService DiscountManagerService { get; private set; }
        public IInvRequisitionService InvRequisitionService { get; private set; }
        public ICouponTypeService CouponTypeService { get; private set; }
        public ICouponService CouponService { get; private set; }

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
            EmployeeService = new EmployeeService(_repoUow);
            StoreService = new StoreService(_repoUow);
            ShiftService = new ShiftService(_repoUow);
            GroupRoleService = new GroupRoleService(_repoUow);
            UserGroupService = new UserGroupService(_repoUow);
            SizesService = new Sizeservice(_repoUow);
            BarcodeService = new BarcodeService(_repoUow);
            VoucherDetailsService = new VoucherDetailsService(_repoUow);
            VoucherService = new VoucherService(_repoUow);
            AccountsGroupsService = new AccountsGroupsService(_repoUow);
            AccountsService = new AccountsService(_repoUow);
            SupplierService = new SupplierService(_repoUow);
            PurchaseOrderService = new PurchaseOrderService(_repoUow);
            PurchaseService = new PurchaseService(_repoUow);
            PurchaseReturnService = new PurchaseReturnService(_repoUow);
            GroupService = new GroupService(_repoUow);
            SubGroupService = new SubGroupService(_repoUow);
            MesurementUnitService = new MesurementUnitService(_repoUow);
            ColorsService = new ColorsService(_repoUow);
            ItemService = new ItemService(_repoUow);
            PurchaseReturnItemService = new PurchaseReturnItemService(_repoUow);
            ActionService = new ActionService(_repoUow);
            RouteService = new RouteService(_repoUow);
            GroupRoutePermissionService = new GroupRoutePermissionService(_repoUow);
            GroupActionPermissionService = new GroupActionPermissionService(_repoUow);
            SupplierPaymentService = new SupplierPaymentService(_repoUow);
            DesignService = new DesignService(_repoUow);
            ItemOriginService = new ItemOriginService(_repoUow);
            ItemFeatureService = new ItemFeatureService(_repoUow);
            ItemBrandService = new ItemBrandService(_repoUow);
            StockOpeningService = _repoUow.StockOpeningRepository;
            ItemCatalogueService = new ItemCatalogueService(_repoUow);
            CurrentStockReportService = new CurrentStockReportService(_repoUow);
            TransferService = new TransferService(_repoUow);
            StockService = new StockService(_repoUow);
            BarcodePrintConfigService = new BarcodePrintConfigService(_repoUow);
            MembershipTypeService = new MembershipTypeService(_repoUow);
            CustomerDetailsService = new CustomerDetailsService(_repoUow);
            DiscountManagerService = new DiscountManagerService(_repoUow);
            InvRequisitionService = new InvRequisitionService(_repoUow);
            CouponTypeService = new CouponTypeService(_repoUow);
            CouponService = new CouponService(_repoUow);
        }
        public void Dispose()
        {
            _repoUow.Dispose();
        }
    }
}
