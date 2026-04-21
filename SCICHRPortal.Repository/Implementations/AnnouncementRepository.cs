using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class AnnouncementRepository : Repository, IAnnouncementRepository
    {

        public AnnouncementRepository(ApplicationContext context)
            : base(context)
        {
        }

        public async Task<bool> DeleteAsync(int announcementId)
        {
            var announcement = await Context.Announcement!
                        .SingleOrDefaultAsync(s => s.AnnouncementId == announcementId && !s.Deleted);
            if (announcement == null)
                return false;

            announcement.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Announcement>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var announcements = Context.Announcement!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                announcements = announcements
                    .Where(e =>
                        e.Title!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = announcements.Count();

            announcements = announcements
                .OrderByDescending(e => e.AnnouncementId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Announcement>, int>(await announcements.ToListAsync(), total);
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            var announcements = await Context.Announcement!.Where(s => !s.Deleted)
              .ToListAsync();
            return announcements;
        }

        public async Task<Announcement> GetAsync(int id)
        {
            var announcement = await Context.Announcement!
                    .SingleOrDefaultAsync(s => s.AnnouncementId == id && !s.Deleted);
            return announcement!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Announcement announcement)
        {
            DuplicateMessage message = new();
            var title = announcement.Title!.ToLower().StringSplitThenJoin();
            var announcementMessage = announcement.AnnouncementForm!.ToLower().StringSplitThenJoin();
            var announcements = await Context.Announcement!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = announcements.Any(t => t.Title!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = announcements.Any(t => announcementMessage.ToLower() == t.AnnouncementForm!.ToLower().StringSplitThenJoin());
            var duplicatedDate = announcements.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Title Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Announcement Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        //    public async Task<List<AnnouncementRecipient>> GetAllEmailByRoleIdsAsync(List<int> roleIds, bool notifyStudents, List<int> sectionIds)
        //{
        //    var filteredRoleIds = roleIds
        //        .Where(id => id > 0)
        //        .Distinct()
        //        .ToList();
        //    var filteredSectionIds = sectionIds
        //        .Where(id => id > 0)
        //        .Distinct()
        //        .ToList();

        //    var roleBasedRecipients = new List<AnnouncementRecipient>();

            //if (filteredRoleIds.Any())
            //{
            //    var userIds = await Context.UserRole!
            //        .AsNoTracking()
            //        .Where(ur => !ur.Deleted && filteredRoleIds.Contains(ur.RoleId))
            //        .Select(ur => ur.UserId)
            //        .Distinct()
            //        .ToListAsync();

            //    if (userIds.Any())
            //    {
            //        var userRecipients = await Context.User!
            //            .AsNoTracking()
            //            .Where(u =>
            //                !u.Deleted &&
            //                userIds.Contains(u.UserId) &&
            //                !string.IsNullOrWhiteSpace(u.Email))
            //            .Select(u => new AnnouncementRecipient
            //            {
            //                Email = u.Email!,
            //                Name = !string.IsNullOrWhiteSpace(u.FirstName) ? u.FirstName! :
            //                    (!string.IsNullOrWhiteSpace(u.Username) ? u.Username! : "User")
            //            })
            //            .ToListAsync();

            //        var registrationRecipients = await Context.Registration!
            //            .AsNoTracking()
            //            .Where(r =>
            //                !r.Deleted &&
            //                r.UserId.HasValue &&
            //                userIds.Contains(r.UserId.Value) &&
            //                !string.IsNullOrWhiteSpace(r.Email))
            //            .Select(r => new AnnouncementRecipient
            //            {
            //                Email = r.Email!,
            //                Name = !string.IsNullOrWhiteSpace(r.FirstName) ? r.FirstName! : "User"
            //            })
            //            .ToListAsync();

            //        roleBasedRecipients = userRecipients.Concat(registrationRecipients).ToList();
            //    }
            //}

            
            //return roleBasedRecipients
            //    .Concat(studentRecipients)
            //    .Where(r => !string.IsNullOrWhiteSpace(r.Email))
            //    .Select(r => new AnnouncementRecipient
            //    {
            //        Email = r.Email.Trim(),
            //        Name = string.IsNullOrWhiteSpace(r.Name) ? "User" : r.Name
            //    })
            //    .GroupBy(r => r.Email, StringComparer.OrdinalIgnoreCase)
            //    .Select(group => group.First())
            //    .ToList();
        //}

        public async Task InsertAsync(Announcement entity)
        {
            await Context.Announcement!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Announcement announcement)
        {
            var record = Context.Update(announcement);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
