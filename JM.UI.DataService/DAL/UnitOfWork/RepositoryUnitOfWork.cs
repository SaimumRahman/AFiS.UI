using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.Actions;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Barcode;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Designs;
using JM.UI.DataService.DAL.Employees;
using JM.UI.DataService.DAL.GroupActionPermission;
using JM.UI.DataService.DAL.GroupRole;
using JM.UI.DataService.DAL.GroupRoutePermissions;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.CustomerDetails;
using JM.UI.DataService.DAL.Discount;
using JM.UI.DataService.DAL.ItemBrand;
using JM.UI.DataService.DAL.ItemCalalogue;
using JM.UI.DataService.DAL.ItemFeatures;
using JM.UI.DataService.DAL.ItemOrigin;
using JM.UI.DataService.DAL.Items;
using JM.UI.DataService.DAL.Items;
using JM.UI.DataService.DAL.MembershipType;
using JM.UI.DataService.DAL.MesurementUnits;
using JM.UI.DataService.DAL.MesurementUnits;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.PurchaseReturnItems;
using JM.UI.DataService.DAL.PurchaseReturnItems;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Routes;
using JM.UI.DataService.DAL.Shift;
using JM.UI.DataService.DAL.Sizes;
using JM.UI.DataService.DAL.Sizes;
using JM.UI.DataService.DAL.Sizes;
using JM.UI.DataService.DAL.Stock;
using JM.UI.DataService.DAL.StockOpenings;
using JM.UI.DataService.DAL.StockReport;
using JM.UI.DataService.DAL.Stores;
using JM.UI.DataService.DAL.SubGroups;
using JM.UI.DataService.DAL.SubGroups;
using JM.UI.DataService.DAL.SupplierPayments;
using JM.UI.DataService.DAL.SupplierPayments;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.Transfer;
using JM.UI.DataService.DAL.UserGroup;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.Vouchers;
using JM.UI.DataService.DAL.Vouchers;
using JM.UI.DataService.DAL.Vouchers;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.UnitOfWork
{
    public class RepositoryUnitOfWork : IRepositoryUnitOfWork
    {
        private readonly IHttpClientFactory _factory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILoggerFactory _loggerFactory;

    
        public IApprovalLevelRepository ApprovalLevelRepository { get; }
        public IApprovalWorkflowRepository ApprovalWorkflowRepository { get; }
        public IApprovalLevelApproverRepository ApprovalLevelApproverRepository { get; }
        public IPendingApprovalRepository PendingApprovalRepository { get; }
        public IDesignationRepository DesignationRepository { get; }
        public ICompanyRepository CompanyRepository { get; }
        public IBanksRepository BanksRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public IStoreRepository StoreRepository { get; }
        public IGroupRoleRepository GroupRoleRepository { get; }
        public IUserGroupRepository UserGroupRepository { get; }
        public IShiftRepository ShiftRepository { get; }
        public IColorsRepository ColorsRepository { get; }
        public ISizesRepository SizesRepository { get; }
        public IBarcodeRepository BarcodeRepository { get; }
        public IVoucherDetailsRepository VoucherDetailsRepository { get; }
        public IVoucherRepository VoucherRepository { get; }
        public IAccountsGroupsRepository AccountsGroupsRepository { get; }
        public IAccountsRepository AccountsRepository { get; }
        public ISupplierRepository SupplierRepository { get; }
        public IPurchaseOrderRepository PurchaseOrderRepository { get; }
        public IPurchaseRepository PurchaseRepository { get; }
        public IPurchaseReturnRepository PurchaseReturnRepository { get; }
        public IGroupRepository GroupRepository { get; }
        public ISubGroupRepository SubGroupRepository { get; }
        public IMesurementUnitRepository MesurementUnitRepository { get; }
        public IItemRepository ItemRepository { get; }
        public IPurchaseReturnItemRepository PurchaseReturnItemRepository { get; }
        public ISupplierPaymentRepository SupplierPaymentRepository { get; }
        public IDesignRepository DesignRepository { get; }
        
        public IActionRepository ActionRepository { get; }
        public IRouteRepository RouteRepository { get; }
        public IGroupRoutePermissionRepository GroupRoutePermissionRepository { get; }
        public IGroupActionPermissionRepository GroupActionPermissionRepository { get; }
        public IItemOriginRepository ItemOriginRepository { get; }
        public IItemBrandRepository ItemBrandRepository { get; }
        public IItemFeatureRepository ItemFeatureRepository { get; }
        public IStockOpeningRepository StockOpeningRepository { get; }
        public IItemCatalogueRepository ItemCatalogueRepository { get; }
        public ICurrentStockReportRepository CurrentStockReportRepository { get; }
        public ITransferRepository TransferRepository { get; }
        public IStockRepository StockRepository { get; }
        public IBarcodePrintConfigRepository BarcodePrintConfigRepository { get; }
        public IMembershipTypeRepository MembershipTypeRepository { get; }
        public ICustomerDetailsRepository CustomerDetailsRepository { get; }
        public IDiscountManagerRepository DiscountManagerRepository { get; }

        public RepositoryUnitOfWork(
            IHttpClientFactory factory,
            ITokenProvider tokenProvider,
            ILoggerFactory loggerFactory)
        {
            _factory = factory;
            _tokenProvider = tokenProvider;
            _loggerFactory = loggerFactory;

       
            ApprovalLevelRepository = new ApprovalLevelRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalLevelRepository>());
            ApprovalWorkflowRepository = new ApprovalWorkflowRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalWorkflowRepository>());
            ApprovalLevelApproverRepository = new ApprovalLevelApproverRepository(factory, tokenProvider, loggerFactory.CreateLogger<ApprovalLevelApproverRepository>());
            PendingApprovalRepository = new PendingApprovalRepository(factory, tokenProvider, loggerFactory.CreateLogger<PendingApprovalRepository>());
            CompanyRepository = new CompanyRepository(factory, tokenProvider, loggerFactory.CreateLogger<CompanyRepository>());
            DesignationRepository = new DesignationRepository(factory, tokenProvider, loggerFactory.CreateLogger<DesignationRepository>());
            BanksRepository = new BanksRepository(factory, tokenProvider, loggerFactory.CreateLogger<BanksRepository>());
            EmployeeRepository = new EmployeeRepository(factory, tokenProvider, loggerFactory.CreateLogger<EmployeeRepository>());
            StoreRepository = new StoreRepository(factory, tokenProvider, loggerFactory.CreateLogger<StoreRepository>());
            GroupRoleRepository = new GroupRoleRepository(factory, tokenProvider, loggerFactory.CreateLogger<GroupRoleRepository>());
            UserGroupRepository = new UserGroupRepository(factory, tokenProvider, loggerFactory.CreateLogger<UserGroupRepository>());
            ShiftRepository = new ShiftRepository(factory, tokenProvider, loggerFactory.CreateLogger<ShiftRepository>());
            SizesRepository = new SizesRepository(factory, tokenProvider, loggerFactory.CreateLogger<SizesRepository>());
            BarcodeRepository = new BarcodeRepository(factory, tokenProvider, loggerFactory.CreateLogger<BarcodeRepository>());
            VoucherDetailsRepository = new VoucherDetailsRepository(factory, tokenProvider, loggerFactory.CreateLogger<VoucherDetailsRepository>());
            VoucherRepository = new VoucherRepository(factory, tokenProvider, loggerFactory.CreateLogger<VoucherRepository>());
            AccountsGroupsRepository = new AccountsGroupsRepository(factory, tokenProvider, loggerFactory.CreateLogger<AccountsGroupsRepository>());
            AccountsRepository = new AccountsRepository(factory, tokenProvider, loggerFactory.CreateLogger<AccountsRepository>());
            SupplierRepository = new SupplierRepository(factory, tokenProvider, loggerFactory.CreateLogger<SupplierRepository>());
            PurchaseOrderRepository = new PurchaseOrderRepository(factory, tokenProvider, loggerFactory.CreateLogger<PurchaseOrderRepository>());
            PurchaseRepository = new PurchaseRepository(factory, tokenProvider, loggerFactory.CreateLogger<PurchaseRepository>());
            PurchaseReturnRepository = new PurchaseReturnRepository(factory, tokenProvider, loggerFactory.CreateLogger<PurchaseReturnRepository>());
            ColorsRepository = new ColorsRepository(factory, tokenProvider, loggerFactory.CreateLogger<ColorsRepository>());
            GroupRepository = new GroupRepository(factory, tokenProvider, loggerFactory.CreateLogger<GroupRepository>());
            ActionRepository = new ActionRepository(factory, tokenProvider, loggerFactory.CreateLogger<ActionRepository>());
            RouteRepository = new RouteRepository(factory, tokenProvider, loggerFactory.CreateLogger<RouteRepository>());
            GroupRoutePermissionRepository = new GroupRoutePermissionRepository(factory, tokenProvider, loggerFactory.CreateLogger<GroupRoutePermissionRepository>());
            GroupActionPermissionRepository = new GroupActionPermissionRepository(factory, tokenProvider, loggerFactory.CreateLogger<GroupActionPermissionRepository>());
            SubGroupRepository = new SubGroupRepository(factory, tokenProvider, loggerFactory.CreateLogger<SubGroupRepository>());
            MesurementUnitRepository = new MesurementUnitRepository(factory, tokenProvider, loggerFactory.CreateLogger<MesurementUnitRepository>());
            ItemRepository = new ItemRepository(factory, tokenProvider, loggerFactory.CreateLogger<ItemRepository>());
            PurchaseReturnItemRepository = new PurchaseReturnItemRepository(factory, tokenProvider, loggerFactory.CreateLogger<PurchaseReturnItemRepository>());
            SupplierPaymentRepository = new SupplierPaymentRepository(factory, tokenProvider, loggerFactory.CreateLogger<SupplierPaymentRepository>());
            DesignRepository = new DesignRepository(factory, tokenProvider, loggerFactory.CreateLogger<DesignRepository>());
            ItemOriginRepository = new ItemOriginRepository(factory, tokenProvider, loggerFactory.CreateLogger<ItemOriginRepository>());
            ItemBrandRepository = new ItemBrandRepository(factory, tokenProvider, loggerFactory.CreateLogger<ItemBrandRepository>());
            ItemFeatureRepository = new ItemFeatureRepository(factory, tokenProvider, loggerFactory.CreateLogger<ItemFeatureRepository>());
            StockOpeningRepository = new StockOpeningRepository(factory, tokenProvider, loggerFactory.CreateLogger<StockOpeningRepository>());
            ItemCatalogueRepository = new ItemCatalogueRepository(factory, tokenProvider, loggerFactory.CreateLogger<ItemCatalogueRepository>());
            CurrentStockReportRepository = new CurrentStockReportRepository(factory, tokenProvider, loggerFactory.CreateLogger<CurrentStockReportRepository>());
            TransferRepository = new TransferRepository(factory, tokenProvider, loggerFactory.CreateLogger<TransferRepository>());
            StockRepository = new StockRepository(factory, tokenProvider, loggerFactory.CreateLogger<StockRepository>());
            BarcodePrintConfigRepository = new BarcodePrintConfigRepository(factory, tokenProvider, loggerFactory.CreateLogger<BarcodePrintConfigRepository>());
            MembershipTypeRepository = new MembershipTypeRepository(factory, tokenProvider, loggerFactory.CreateLogger<MembershipTypeRepository>());
            CustomerDetailsRepository = new CustomerDetailsRepository(factory, tokenProvider, loggerFactory.CreateLogger<CustomerDetailsRepository>());
            DiscountManagerRepository = new DiscountManagerRepository(factory, tokenProvider, loggerFactory.CreateLogger<DiscountManagerRepository>());
        }

        public void Dispose()
        {
            // No unmanaged resources — nothing to dispose.
        }
    }
}
