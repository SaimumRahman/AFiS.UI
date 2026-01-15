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
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.DataService.DAL.Sizes;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.Vouchers;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Colors;

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
            ColorsRepository = new ColorsRepository(factory, tokenProvider, loggerFactory.CreateLogger<ColorsRepository>());

        }

        public void Dispose()
        {
            // No unmanaged resources — nothing to dispose.
        }
    }
}
