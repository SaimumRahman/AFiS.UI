using JM.Infrastructure.Base;
using JM.UI.DataService.DAL.Customer;
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
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IUserAuthRepository, UsersAuthRepository>();

            #region Customer
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            #endregion


        }

    }
}