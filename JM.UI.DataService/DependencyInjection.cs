using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Accounts;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.AccountsGroups;
using JM.UI.DataService.DAL.Actions;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Barcodes;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Colors;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Employees;
using JM.UI.DataService.DAL.GroupActionPermission;
using JM.UI.DataService.DAL.GroupRole;
using JM.UI.DataService.DAL.GroupRoutePermissions;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.Groups;
using JM.UI.DataService.DAL.ItemBrand;
using JM.UI.DataService.DAL.ItemCalalogue;
using JM.UI.DataService.DAL.ItemFeatures;
using JM.UI.DataService.DAL.ItemOrigin;
using JM.UI.DataService.DAL.Items;
using JM.UI.DataService.DAL.MesurementUnits;
using JM.UI.DataService.DAL.PurchaseOrders;
using JM.UI.DataService.DAL.PurchaseReturnItems;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.PurchaseReturns;
using JM.UI.DataService.DAL.Purchases;
using JM.UI.DataService.DAL.Routes;
using JM.UI.DataService.DAL.Shift;
using JM.UI.DataService.DAL.Stock;
using JM.UI.DataService.DAL.StockReport;
using JM.UI.DataService.DAL.Stores;
using JM.UI.DataService.DAL.SubGroups;
using JM.UI.DataService.DAL.SupplierPayments;
using JM.UI.DataService.DAL.Suppliers;
using JM.UI.DataService.DAL.Transfer;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.DataService.DAL.UserGroup;
using JM.UI.DataService.DAL.Users;
using JM.UI.DataService.DAL.VoucherDetails;
using JM.UI.DataService.DAL.Vouchers;
using Microsoft.Extensions.DependencyInjection;

namespace JM.UI.DataService
{
    public static class DependencyInjection
    {
        public static void AddDataService(this IServiceCollection services)
        {

            services.AddScoped<IBaseDapperRepository, BaseDapperRepository>();
            services.AddScoped<IRepositoryUnitOfWork, RepositoryUnitOfWork>();

            #region All Repositories
          
            services.AddScoped<IUserAuthRepository, UsersAuthRepository>();
            services.AddScoped<IApprovalLevelRepository, ApprovalLevelRepository>();
            services.AddScoped<IApprovalLevelApproverRepository, ApprovalLevelApproverRepository>();
            services.AddScoped<IPendingApprovalRepository, PendingApprovalRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IBanksRepository, BanksRepository>();
            services.AddScoped<IGroupRoleRepository, GroupRoleRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IShiftRepository, ShiftRepository>();
            services.AddScoped<IBarcodeRepository, BarcodeRepository>();
            services.AddScoped<IVoucherDetailsRepository, VoucherDetailsRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IAccountsGroupsRepository, AccountsGroupsRepository>();
            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
            services.AddScoped<IColorsRepository, ColorsRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IActionRepository, ActionRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<IGroupRoutePermissionRepository, GroupRoutePermissionRepository>();
            services.AddScoped<IGroupActionPermissionRepository, GroupActionPermissionRepository>();
            services.AddScoped<ISubGroupRepository, SubGroupRepository>();
            services.AddScoped<IMesurementUnitRepository, MesurementUnitRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IPurchaseReturnItemRepository, PurchaseReturnItemRepository>();
            services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();
            services.AddScoped<IItemOriginRepository, ItemOriginRepository>();
            services.AddScoped<IItemBrandRepository, ItemBrandRepository>();
            services.AddScoped<IItemFeatureRepository, ItemFeatureRepository>();
            services.AddScoped<IItemCatalogueRepository, ItemCatalogueRepository>();
            services.AddScoped<ICurrentStockReportRepository, CurrentStockReportRepository>();
            services.AddScoped<ITransferRepository, TransferRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            #endregion
        }

    }
}