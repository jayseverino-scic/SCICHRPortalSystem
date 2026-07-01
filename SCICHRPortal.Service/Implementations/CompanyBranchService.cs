using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SCICHRPortal.Service.Implementations
{
    public class CompanyBranchService : ICompanyBranchService
    {
        private ICompanyBranchRepository CompanyBranchRepository { get; }

        public CompanyBranchService(ICompanyBranchRepository companyBranchRepository)
        {
            CompanyBranchRepository = companyBranchRepository;
        }

        public Task<IEnumerable<XCompany_Branch>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<XCompany_Branch>> GetAllAsync()
        {
            return await CompanyBranchRepository.GetAllAsync();
        }

        public async Task<XCompany_Branch> GetAsync(int id)
        {
            return await CompanyBranchRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<XCompany_Branch>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await CompanyBranchRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<XCompany_Branch>> GetBranches()
        {
            return await CompanyBranchRepository.GetBranches();
        }

    }
}
