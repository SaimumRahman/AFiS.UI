
using JM.UI.Entities.Model.Users;


namespace JM.UI.Service.Users
{
    public interface IRoleService
    {
        void SaveRoles(Role roleservice);
        //void UpdateBuyerDetailsByID(BuyerDetailsDAO buyerDetailsDAO);
        //Task<BuyerDetailsDAO> GetBuyerDetailsById(int buyerId);
        Task<IEnumerable<Role>> GetRoleDetails();
    }
}