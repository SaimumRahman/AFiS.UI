using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Bankss;
using JM.UI.Service.Company;
using JM.UI.Service.Designations;
using JM.UI.Service.Employee;
using JM.UI.Service.GroupRole;
using JM.UI.Service.Shift;
using JM.UI.Service.Sizes;
using JM.UI.Service.Stores;
using JM.UI.Service.UserGroup;
using JM.UI.Service.Barcodes;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.Vouchers;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.Accounts;
using JM.UI.Service.Suppliers;
using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.Service.Colors;
using JM.UI.Service.AccountsGroups;

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

        public IColorsService ColorsService { get; }


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
            ColorsService = new ColorsService(_repoUow);
        }

        public void Dispose()
        {
            _repoUow.Dispose();
        }
    }


}
