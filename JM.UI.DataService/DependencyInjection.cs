using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.Banks;
using JM.UI.DataService.DAL.Company;
using JM.UI.DataService.DAL.Designations;
using JM.UI.DataService.DAL.Employees;
using JM.UI.DataService.DAL.GroupRole;
using JM.UI.DataService.DAL.Stores;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.DataService.DAL.Users;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            #endregion


        }

    }
}