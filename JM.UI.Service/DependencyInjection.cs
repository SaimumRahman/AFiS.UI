using JM.Infrastructure.Base;
using JM.UI.Service.Customer;

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
            services.AddScoped<IRoleService,RoleService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IUserAuthService, UserAuthService>();

            services.AddScoped<ICustomerService, CustomerService>();
        }
    }
}