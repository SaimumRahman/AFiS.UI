using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Approval.Aprrover;
using JM.UI.DataService.DAL.UnitOfWork;
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
          
            services.AddScoped<IApprovalLevelRepository, ApprovalLevelRepository>();
            services.AddScoped<IApprovalLevelApproverRepository, ApprovalLevelApproverRepository>();
            services.AddScoped<IPendingApprovalRepository, PendingApprovalRepository>();
            #endregion


        }

    }
}