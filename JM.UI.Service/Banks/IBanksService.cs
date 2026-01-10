using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;

namespace JM.UI.Service.Banks;

public interface IBanksService
{
    Task<IEnumerable<BanksDTO>> GetBankss();
    Task<BanksDTO?> GetBanksById(int id);
    Task<ResponseResult> SaveUpdateBanks(BanksDTO Banks);
    Task<ResponseResult> DeleteBanks(int id);
    Task<(bool IsValid, string ErrorMessage)> ValidateBanks(BanksDTO Banks);
    string Truncate(string? value, int maxChars);
}
