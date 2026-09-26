using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Data.Enums;

namespace SCICHRPortal.Service.Implementations
{
    public class HolidayService : IHolidayService
    {
        private IHolidayRepository HolidayRepository { get; }
        private IProjectRepository ProjectRepository { get; }

        public HolidayService(IHolidayRepository holidayRepository, IProjectRepository projectRepository)
        {
            HolidayRepository = holidayRepository;
            ProjectRepository = projectRepository;
        }

        public async Task<HolidayWriteResult> CreateAsync(HolidayCreateRequest request)
        {
            var error = ValidateCommon(request.HolidayName, request.HolidayDate, request.HolidayType);
            if (error != null) return new(HolidayWriteStatus.Invalid, error);

            var projectIds = request.ProjectIds ?? [];
            error = ValidateScope(request.AllProjects, projectIds);
            if (error != null) return new(HolidayWriteStatus.Invalid, error);

            var selectedIds = projectIds.Distinct().ToArray();
            if (!await ProjectsAreActive(selectedIds))
                return new(HolidayWriteStatus.Invalid, "One or more projects are not active.");

            var existing = await HolidayRepository.GetAllAsync();
            if (IsDuplicate(existing, request.HolidayName!, request.HolidayDate!.Value, request.HolidayType!.Value, selectedIds))
                return new(HolidayWriteStatus.Conflict, "Holiday already exists for this date, type, and project.");

            var now = DateTime.UtcNow;
            var holiday = new Holiday
            {
                HolidayName = NormalizeWhitespace(request.HolidayName!),
                HolidayDate = request.HolidayDate!.Value.Date,
                HolidayType = request.HolidayType,
                CreatedAt = now,
                CreatedBy = "manuel"
            };
            await HolidayRepository.InsertAsync(holiday, selectedIds);
            return new(HolidayWriteStatus.Success);
        }

        public async Task<HolidayWriteResult> UpdateAsync(HolidayUpdateRequest request)
        {
            if (request.HolidayId <= 0)
                return new(HolidayWriteStatus.Invalid, "Holiday ID is required.");
            var existing = await HolidayRepository.GetAsync(request.HolidayId);
            if (existing == null)
                return new(HolidayWriteStatus.NotFound, "Holiday was not found.");

            var error = ValidateCommon(request.HolidayName, request.HolidayDate, request.HolidayType);
            if (error != null) return new(HolidayWriteStatus.Invalid, error);

            var projectIds = request.ProjectIds ?? [];
            error = ValidateScope(request.AllProjects, projectIds);
            if (error != null) return new(HolidayWriteStatus.Invalid, error);
            var selectedIds = projectIds.Distinct().ToArray();
            var retainedIds = existing.Projects.Select(project => project.Id);
            if (!await ProjectsAreActive(selectedIds.Except(retainedIds)))
                return new(HolidayWriteStatus.Invalid, "One or more new projects are not active.");

            var all = await HolidayRepository.GetAllAsync();
            if (IsDuplicate(all.Where(h => h.HolidayId != request.HolidayId), request.HolidayName!, request.HolidayDate!.Value, request.HolidayType!.Value, selectedIds))
                return new(HolidayWriteStatus.Conflict, "Holiday already exists for this date, type, and project.");

            existing.HolidayName = NormalizeWhitespace(request.HolidayName!);
            existing.HolidayDate = request.HolidayDate!.Value.Date;
            existing.HolidayType = request.HolidayType;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = "manuel";
            await HolidayRepository.UpdateAsync(existing, selectedIds);
            return new(HolidayWriteStatus.Success);
        }

        private static string? ValidateCommon(string? name, DateTime? date, int? type)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Holiday name is required.";
            if (NormalizeWhitespace(name).Length > 256) return "Holiday name is too long.";
            if (date == null || date == default) return "Holiday date is required.";
            if (type is not (int)HolidayType.Regular and not (int)HolidayType.SpecialNonWorking and not (int)HolidayType.Local)
                return "Holiday type is invalid.";
            return null;
        }

        private async Task<bool> ProjectsAreActive(IEnumerable<int> selectedIds)
        {
            var ids = selectedIds.ToArray();
            if (ids.Length == 0) return true;
            var activeIds = (await ProjectRepository.GetAllAsync())
                .Where(project => !project.Deleted)
                .Select(project => project.Id).ToHashSet();
            return ids.All(id => id > 0 && activeIds.Contains(id));
        }

        private static string? ValidateScope(bool? allProjects, int[] projectIds)
        {
            if (allProjects is null) return "Project scope is required.";
            if (allProjects.Value && projectIds.Length > 0) return "All Projects cannot be combined with project IDs.";
            if (!allProjects.Value && projectIds.Length == 0) return "Select at least one project or All Projects.";
            if (projectIds.Any(id => id <= 0)) return "Project IDs must be positive.";
            return null;
        }

        private static bool IsDuplicate(IEnumerable<Holiday> existing, string name, DateTime date, int type, int[] projectIds)
        {
            var keyName = NormalizeWhitespace(name).ToUpperInvariant();
            return existing.Any(h => !h.Deleted && h.HolidayDate?.Date == date.Date &&
                h.HolidayType == type &&
                (projectIds.Length == 0
                    ? !h.Projects.Any()
                    : h.Projects.Any(project => projectIds.Contains(project.Id))) &&
                NormalizeWhitespace(h.HolidayName ?? "").ToUpperInvariant() == keyName);
        }

        private static string NormalizeWhitespace(string name) =>
            string.Join(" ", name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            return await HolidayRepository.GetAllAsync();
        }

        public async Task<Holiday> GetAsync(int id)
        {
            return await HolidayRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int holidayId)
        {
            return await HolidayRepository.DeleteAsync(holidayId);
        }

        public async Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await HolidayRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

    }
}
