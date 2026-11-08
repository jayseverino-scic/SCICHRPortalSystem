using System.Globalization;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class TardinessService : ITardinessService
    {
        private ITardinessRepository TardinessRepository { get; }
        private IXEmployeeRepository XEmployeeRepository { get; }
        private ILogger<TardinessService> Logger { get; }

        public TardinessService(ITardinessRepository tardinessRepository, IXEmployeeRepository xEmployeeRepository, ILogger<TardinessService> logger)
        {
            TardinessRepository = tardinessRepository;
            XEmployeeRepository = xEmployeeRepository;
            Logger = logger;
        }

        public async Task<TardinessPage> GetPageAsync(TardinessPageQuery query, CancellationToken cancellationToken = default)
        {
            if (!query.IsValid()) throw new ArgumentException("Invalid tardiness page query.", nameof(query));
            var page = await TardinessRepository.GetPageAsync(query, cancellationToken);
            page.Warning = await AddPositionsAsync(page.Data, cancellationToken);
            return page;
        }

        public async Task<TardinessExport> GetExportAsync(TardinessFilterQuery query, CancellationToken cancellationToken = default)
        {
            if (!query.IsValid()) throw new ArgumentException("Invalid tardiness export query.", nameof(query));
            var rows = await TardinessRepository.GetExportAsync(query, cancellationToken);
            return new TardinessExport { Data = rows, Warning = await AddPositionsAsync(rows, cancellationToken) };
        }

        private async Task<string?> AddPositionsAsync(List<TardinessRow> rows, CancellationToken cancellationToken)
        {
            // EmployeeController.ImportDb stores the HRIS primary key in EmployeeNo.
            // A business code such as 0012 must not be guessed to mean HRIS ID 12.
            var identifiers = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row.EmployeeNo is { } value && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) &&
                    id > 0 && id.ToString(CultureInfo.InvariantCulture) == value) identifiers[value] = id;
            }
            if (identifiers.Count == 0) return null;
            try
            {
                var positions = await XEmployeeRepository.GetPositionsByIdsAsync(identifiers.Values.Distinct().ToArray(), cancellationToken);
                foreach (var row in rows)
                    if (row.EmployeeNo is { } value && identifiers.TryGetValue(value, out var id) && positions.TryGetValue(id, out var position))
                        row.Position = position;
                return null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogWarning("HRIS position lookup unavailable ({ExceptionType}).", exception.GetType().Name);
                return "HRIS positions are unavailable. Attendance values are shown without position data.";
            }
        }
    }
}
