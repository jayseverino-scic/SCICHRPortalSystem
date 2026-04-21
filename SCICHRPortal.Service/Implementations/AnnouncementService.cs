using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        public IAnnouncementRepository AnnouncementRepository { get; }

        public AnnouncementService(IAnnouncementRepository announcementRepository)
        {
            AnnouncementRepository = announcementRepository;
        }

        public async Task<bool> DeleteAsync(int announcementId)
        {
            return await AnnouncementRepository.DeleteAsync(announcementId);
        }

        public async Task<bool> UpdateAsync(Announcement announcement)
        {
            return await AnnouncementRepository.UpdateAsync(announcement);
        }

        public async Task<Tuple<IEnumerable<Announcement>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await AnnouncementRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task InsertAsync(Announcement entity)
        {
            await AnnouncementRepository.InsertAsync(entity);
        }

        public async Task<Announcement> GetAsync(int id)
        {
            return await AnnouncementRepository.GetAsync(id);
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await AnnouncementRepository.GetAllAsync();
        }

        public async Task<DuplicateMessage> HasDuplicateName(Announcement announcement)
        {
            return await AnnouncementRepository.HasDuplicateName(announcement);
        }

        //public async Task<List<AnnouncementRecipient>> GetAllEmailByRoleIdsAsync(List<int> roleIds, bool notifyStudents, List<int> sectionIds)
        //{
        //    return await AnnouncementRepository.GetAllEmailByRoleIdsAsync(roleIds, notifyStudents, sectionIds);
        //}
    }
}
