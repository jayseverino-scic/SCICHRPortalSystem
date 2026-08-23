using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class ProjectRepository : Repository, IProjectRepository
    {
        public ProjectRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<bool> DeleteAsync(int projectId)
        {
            var project = await Context.Project!.SingleOrDefaultAsync(s => s.Id == projectId && !s.Deleted);
            if (project == null)
                return false;

            project.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Project>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var projects = Context.Project!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                projects = projects
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = projects.Count();

            projects = projects
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Project>, int>(await projects.ToListAsync(), total);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            var projects = await Context.Project!.Where(s => !s.Deleted).ToListAsync();
            return projects;
        }

        public async Task<Project> GetAsync(int id)
        {
            var project = await Context.Project!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.Deleted);
            return project!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Project project)
        {
            DuplicateMessage message = new();
            var title = project.Name!.ToLower().StringSplitThenJoin();
            var announcementMessage = project.Name!.ToLower().StringSplitThenJoin();
            var projects = await Context.Project!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = projects.Any(t => t.Name!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = projects.Any(t => announcementMessage.ToLower() == t.Name!.ToLower().StringSplitThenJoin());
            var duplicatedDate = projects.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Project Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Project Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Project entity)
        {
            await Context.Project!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            var record = Context.Update(project);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
        public async Task<Project> GetProjectCodeAsync(string projectCode)
        {
            var project = await Context.Project!.SingleOrDefaultAsync(p => p.Code!.ToUpper() == projectCode.ToUpper());
            return project!;
        }
        public async Task<Project> GetProjectNameAsync(string projectName)
        { 
            var project = await Context.Project!.SingleOrDefaultAsync(p => p.Name!.ToUpper() == projectName.ToUpper());
            return project!;
        }
    }
}
