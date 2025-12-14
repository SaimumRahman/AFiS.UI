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

    public class DesignationRepository : IDesignationRepository
    {
        public readonly ILogger<DesignationRepository> _logger;
        public DesignationRepository(ILogger logger) : base()
        {

        }

        public async Task<IEnumerable<Designation>> GetDesignations()
        {
          //  return await base.Query<Designation>($@"select * from Designation where isActive=1");
            return null;
        }

        public async Task<int> SaveDesignation(Designation designation)
        {
         // return await  base.ExecuteIdentityAsync("Insert into Designation(DesignationName,isActive,CreatedBy,CreatedDate)" +
                 //   " values (@DesignationName,@isActive,@CreatedBy,@CreatedDate)", designation);
            return 1;
        }
    }
}