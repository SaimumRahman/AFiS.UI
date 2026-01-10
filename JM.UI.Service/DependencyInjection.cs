using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.Service.Approval;
using JM.UI.Service.Approval.Approver;
using JM.UI.Service.Banks;
using JM.UI.Service.Bankss;
using JM.UI.Service.Designations;
using JM.UI.Service.Employee;
using JM.UI.Service.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.Users;
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
        }
    }
}