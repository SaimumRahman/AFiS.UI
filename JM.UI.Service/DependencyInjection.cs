using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Actions;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Routes;
using JM.UI.Service.Accounts;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.AccountsGroups;
using JM.UI.Service.Action;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Bankss;
using JM.UI.Service.Barcodes;
using JM.UI.Service.Colors;
using JM.UI.Service.Designations;
using JM.UI.Service.Employee;
using JM.UI.Service.GroupRole;
using JM.UI.Service.GroupRoutePermission;
using JM.UI.Service.Groups;
using JM.UI.Service.PurchaseOrders;
using JM.UI.Service.PurchaseReturns;
using JM.UI.Service.Purchases;
using JM.UI.Service.Routes;
using JM.UI.Service.Shift;
using JM.UI.Service.Sizes;
using JM.UI.Service.Stores;
using JM.UI.Service.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.UserGroup;
using JM.UI.Service.Users;
using JM.UI.Service.VoucherDetails;
using JM.UI.Service.Vouchers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Service
{
    public static class DependencyInjection
    {
        public static void AddService(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
            services.AddScoped<IServiceUnitOfWork, ServiceUnitOfWork>();

    
            services.AddScoped<IApprovalLevelService, ApprovalLevelService>();
            services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
            services.AddScoped<IApprovalLevelApproverService, ApprovalLevelApproverService>();
            services.AddScoped<IPendingApprovalService, PendingApprovalService>();
            services.AddScoped<IUserAuthService, UserAuthService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IBanksService, BanksService>();
            services.AddScoped<IGroupRoleService, GroupRoleService>();
            services.AddScoped<IUserGroupService, UserGroupService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IColorsService, ColorsService>();
            services.AddScoped<ISizesService, Sizeservice>();
            services.AddScoped<IBarcodeService, BarcodeService>();
            services.AddScoped<IVoucherDetailsService, VoucherDetailsService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IAccountsGroupsService, AccountsGroupsService>();
            services.AddScoped<IAccountsService, AccountsService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddTransient<IPurchaseService, PurchaseService>();
            services.AddTransient<IPurchaseReturnService, PurchaseReturnService>();
            services.AddTransient<IGroupService, GroupService>();
            services.AddTransient<IColorsService, ColorsService>();
            services.AddTransient<IActionService, ActionService>();
            services.AddTransient<IRouteService, RouteService>();
            services.AddTransient<IGroupRoutePermissionService, GroupRoutePermissionService>();
        }
    }
}