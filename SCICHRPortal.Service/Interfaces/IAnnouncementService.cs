using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IAnnouncementService :
         IScopedService,
         IInserter<Announcement>,
         IRetriever<Announcement, int>,
         IListRetriever<Announcement>
    {
        Task<bool> DeleteAsync(int announcementId);
        Task<bool> UpdateAsync(Announcement announcement);
        Task<Tuple<IEnumerable<Announcement>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Announcement announcement);
        //Task<List<AnnouncementRecipient>> GetAllEmailByRoleIdsAsync(List<int> roleIds, bool notifyStudents, List<int> sectionIds);
    }
}
