
using JM.UI.Entities.Model.Users;


namespace JM.UI.Service.Users
{
    public interface IDesignationService
    {
        void SaveDesignation(Designation designationService);
        //void UpdateBuyerDetailsByID(BuyerDetailsDAO buyerDetailsDAO);
        //Task<BuyerDetailsDAO> GetBuyerDetailsById(int buyerId);
        Task<IEnumerable<Designation>> GetDesignations();
    }
}