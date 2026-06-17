using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MembershipType;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.MembershipType
{
    public interface IMembershipTypeService
    {
        Task<IEnumerable<MembershipTypeDTO>> GetAll();
        Task<MembershipTypeDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(MembershipTypeDTO membershipType);
        Task<ResponseResult> Delete(int id);
        Task<(bool IsValid, string ErrorMessage)> Validate(MembershipTypeDTO membershipType);
        MembershipTypeDTO CreateNew();
        string Truncate(string? value, int maxChars);
    }
}
