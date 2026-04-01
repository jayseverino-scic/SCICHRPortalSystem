using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class LookupsRepository : Repository, ILookupsRepository
    {
        public LookupsRepository(ApplicationContext context) : base(context)
        {
        }
        public async Task<bool> DeleteModuleTypeAsync(int moduleId)
        {
            var module = await Context.Module!
             .SingleOrDefaultAsync(c => c.ModuleId == moduleId && !c.Deleted);

            if (module == null)
                return false;

            module.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Module>, int>> FilterModuleAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var modules = Context.Module!
               .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                modules = modules
                    .Where(e =>
                        e.Description!.ToLower().Contains(searchKeyword.ToLower())
                        );
            }

            var total = modules.Count();

            modules = modules
                .OrderByDescending(e => e.ModuleId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Module>, int>(await modules.ToListAsync(), total);
        }
        public async Task<IEnumerable<T>> GetAll<T>() where T : BaseEntity
        {
            return await Context.Set<T>().Where(e => e.Deleted == false).ToListAsync();
        }

        public async Task<bool> HasDuplicateName(Module module)
        {
            return await Context.Module!
             .AnyAsync(r =>
             r.Description!.ToLower() == module.Description!.ToLower() &&
             r.Deleted == false);
        }

        public async Task InsertAsync<T>(T entity) where T : BaseEntity
        {
            await Context.Set<T>().AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync<T>(T entity) where T : BaseEntity
        {
            var record = Context.Update(entity);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
   }
}
