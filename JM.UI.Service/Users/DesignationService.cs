
using JM.UI.DataService.DAL.Users;
using JM.UI.Entities.Model.Users;

namespace JM.UI.Service.Users
{
    public class DesignationService : IDesignationService
    {
        readonly IDesignationRepository _designationRepository;

        public DesignationService(IDesignationRepository designationRepository)
        {
            _designationRepository = designationRepository;
        }

        public async Task<IEnumerable<Designation>> GetDesignations()
        {
            return await _designationRepository.GetDesignations();
        }


        public void SaveDesignation(Designation designationService)
        {
            Designation designationObj = new Designation()
            {
                DesignationName = designationService.DesignationName,
                isActive=1,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };

            _designationRepository.SaveDesignation(designationObj);
        }

   
    }
}