using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MembershipType;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.MembershipType
{
    public interface IMembershipTypeRepository
    {
        Task<IEnumerable<MembershipTypeDTO>> GetAll();
        Task<MembershipTypeDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(MembershipTypeDTO membershipType);
        Task<ResponseResult> Delete(int id);
    }
}
