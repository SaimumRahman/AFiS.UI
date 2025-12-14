using JM.Infrastructure.Base;

using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Users
{
    public interface IAuthResponseRepository
    {
       Task<int> DeleteExistingUserResponse(int userID);
        // Task<BuyerDetailsDAO> GetBuyerById(int buyerId);
        // void UpdateBuyerDetailsById(BuyerDetails buyerDetails);
        Task<int> SaveUserResponse(AuthenticatedUserResponse authenticatedUser);

    }

}
