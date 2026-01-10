using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;

namespace JM.UI.DataService.DAL.Banks;

public interface IBanksRepository
{
    Task<IEnumerable<BanksDTO>> GetBankss();
    Task<BanksDTO?> GetBanksById(int id);
    Task<ResponseResult> SaveUpdateBanks(BanksDTO Banks);
    Task DeleteBanks(int id);
}
