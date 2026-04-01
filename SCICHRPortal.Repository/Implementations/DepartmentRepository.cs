using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class DepartmentRepository : Repository, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationContext context)
    : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int departmentId)
        {
            var department = await Context.Department!
                        .SingleOrDefaultAsync(s => s.DepartmentId == departmentId && !s.Deleted);
            if (department == null)
                return false;

            department.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Department>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var departments = Context.Department!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                departments = departments
                    .Where(e =>
                        e.DepartmentName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = departments.Count();

            departments = departments
                .OrderByDescending(e => e.DepartmentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Department>, int>(await departments.ToListAsync(), total);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            var departments = await Context.Department!.Where(s => !s.Deleted)
              .ToListAsync();
            return departments;
        }

        public async Task<Department> GetAsync(int id)
        {
            var department = await Context.Department!
                    .SingleOrDefaultAsync(s => s.DepartmentId == id && !s.Deleted);
            return department!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Department department)
        {
            DuplicateMessage message = new();
            var title = department.DepartmentName!.ToLower().StringSplitThenJoin();
            var announcementMessage = department.DepartmentName!.ToLower().StringSplitThenJoin();
            var departments = await Context.Department!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = departments.Any(t => t.DepartmentName!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = departments.Any(t => announcementMessage.ToLower() == t.DepartmentName!.ToLower().StringSplitThenJoin());
            var duplicatedDate = departments.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Department Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Department Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Department entity)
        {
            await Context.Department!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Department department)
        {
            var record = Context.Update(department);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
