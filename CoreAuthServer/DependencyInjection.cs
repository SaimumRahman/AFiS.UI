using CoreAuthServer.Models;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace InvPos.CoreAuth
{
    public static class DependencyInjection
    {
        public static void AddAuthService(this IServiceCollection services,IConfiguration configuration)
        {
            Configuration.Bind()



        }
    }
}
