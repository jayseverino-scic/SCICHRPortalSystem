using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class XCompanyPositionService : IXCompanyPositionService
    {
        private IXCompanyPositionRepository CompanyPositionRepository{ get; }

        public XCompanyPositionService(IXCompanyPositionRepository companyPositionRepository)
        {
            CompanyPositionRepository = companyPositionRepository;
        }

        public Task<IEnumerable<XCompany_Position>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<XCompany_Position>> GetAllAsync()
        {
            return await CompanyPositionRepository.GetAllAsync();
        }

        public async Task<XCompany_Position> GetAsync(int id)
        {
            return await CompanyPositionRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<XCompany_Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await CompanyPositionRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }
    }
}
