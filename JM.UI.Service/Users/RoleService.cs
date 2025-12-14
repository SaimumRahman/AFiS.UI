
using JM.UI.DataService.DAL.Users;
using JM.UI.Entities.Model.Users;

namespace JM.UI.Service.Users
{
    public class RoleService : IRoleService
    {
        readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task<IEnumerable<Role>> GetRoleDetails()
        {
          return await _roleRepository.GetRoles();
        }

        public void SaveRoles(Role roleservice)
        {
            Role roleObj = new Role()
            {
                RoleName = roleservice.RoleName,
                UserId =1,
                isActive = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };
           
            _roleRepository.SaveRole(roleObj);
        }
    }
}