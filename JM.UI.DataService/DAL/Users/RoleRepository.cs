using JM.Infrastructure.Base;
using JM.Infrastructure.Common;

using JM.UI.DataService.DAL.Users;


using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Users
{

    public class RoleRepository : IRoleRepository
    {
        public readonly ILogger<RoleRepository> _logger;
        public RoleRepository(ILogger logger) : base()
        {

        }

        public async Task<IEnumerable<Role>> GetRoles()
        {
          //  return await base.Query<Role>($@"select * from Role where isActive=1");
            return null;
        }

        public async Task<int>  SaveRole(Role role)
        {

            try
            {
                string sql = $@"INSERT into Role (RoleName,UserId,isActive,CreatedDate,CreatedBy) values (@RoleName,@UserId,@isActive,@CreatedDate,@CreatedBy)";
                // return  await base.ExecuteIdentityAsync(sql, role);
                return 1;
            }
            catch (Exception e)
            {
                e.ToString();
                throw;
            }

          
        }
    }
}