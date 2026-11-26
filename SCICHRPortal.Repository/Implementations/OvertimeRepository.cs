using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public sealed class OvertimeRepository(ClaraFormsContext context, IConfiguration configuration) : IOvertimeRepository
    {
        private static readonly string[] SourceColumns = typeof(ClaraFormOTRequest)
            .GetProperties().Select(property => property.Name).ToArray();

        public async Task<OvertimePage> GetPageAsync(OvertimePageQuery query, CancellationToken cancellationToken)
        {
            var source = await GetSourceAsync(cancellationToken);
            var sqlHelper = context.GetService<ISqlGenerationHelper>();
            // Quote catalog identifiers with the provider before composing the shared read query.
            var relation = sqlHelper.DelimitIdentifier(source.Name, source.Schema);
            var columns = string.Join(", ", SourceColumns.Select(sqlHelper.DelimitIdentifier));
            var sql = $"SELECT {columns} FROM {relation}";
            var records = context.OvertimeRequests.FromSqlRaw(sql).AsNoTracking();

            var total = await records.CountAsync(cancellationToken);
            var filtered = total;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim()
                    .Replace(@"\", @"\\")
                    .Replace("%", @"\%")
                    .Replace("_", @"\_");
                var pattern = $"%{term}%";
                records = records.Where(row =>
                    EF.Functions.ILike(row.employee_overtime_summary_employee_name ?? "", pattern, @"\") ||
                    EF.Functions.ILike(row.employee_overtime_summary_employee_hris_id ?? "", pattern, @"\") ||
                    EF.Functions.ILike(row.employee_overtime_summary_request_reference ?? "", pattern, @"\"));
                filtered = await records.CountAsync(cancellationToken);
            }

            // Summary IDs can be absent. Use the visible fields to settle any remaining ties.
            var rows = await records
                .OrderBy(row => row.employee_overtime_summary_overtime_date_from == null)
                .ThenByDescending(row => row.employee_overtime_summary_overtime_date_from)
                .ThenBy(row => row.employee_overtime_summary_id)
                .ThenBy(row => row.employee_overtime_summary_request_reference)
                .ThenBy(row => row.employee_overtime_summary_employee_hris_id)
                .ThenBy(row => row.employee_overtime_summary_employee_name)
                .ThenBy(row => row.employee_overtime_summary_approved_start_time)
                .ThenBy(row => row.employee_overtime_summary_overtime_date_to)
                .ThenBy(row => row.employee_overtime_summary_approved_end_time)
                .ThenBy(row => row.employee_overtime_summary_filed_hours)
                .ThenBy(row => row.employee_overtime_summary_approved_hours)
                .ThenBy(row => row.employee_overtime_summary_closed_at)
                .ThenBy(row => row.employee_overtime_summary_date_created)
                .Skip(query.Start)
                .Take(query.Length)
                .Select(row => new OvertimeRow(
                    row.employee_overtime_summary_employee_name,
                    row.employee_overtime_summary_employee_hris_id,
                    row.employee_overtime_summary_overtime_date_from,
                    row.employee_overtime_summary_approved_start_time,
                    row.employee_overtime_summary_overtime_date_to,
                    row.employee_overtime_summary_approved_end_time,
                    row.employee_overtime_summary_filed_hours,
                    row.employee_overtime_summary_approved_hours,
                    row.employee_overtime_summary_request_reference,
                    row.employee_overtime_summary_closed_at,
                    row.employee_overtime_summary_date_created,
                    row.employee_overtime_summary_id))
                .ToListAsync(cancellationToken);

            return new OvertimePage(query.Draw, total, filtered, rows);
        }

        private async Task<SourceRelation> GetSourceAsync(CancellationToken cancellationToken)
        {
            var schema = configuration["ClaraForms:OvertimeSchema"] ?? "";
            var name = configuration["ClaraForms:OvertimeRelation"] ?? "";
            var matches = await context.Database.SqlQuery<SourceRelation>($"""
                SELECT space.nspname::text AS "Schema", source.relname::text AS "Name"
                FROM pg_catalog.pg_class AS source
                JOIN pg_catalog.pg_namespace AS space ON space.oid = source.relnamespace
                JOIN pg_catalog.pg_attribute AS column_info ON column_info.attrelid = source.oid
                WHERE source.relkind IN ('r', 'p', 'v', 'm', 'f')
                  AND pg_catalog.has_schema_privilege(space.oid, 'USAGE')
                  AND space.nspname NOT IN ('pg_catalog', 'information_schema')
                  AND column_info.attnum > 0 AND NOT column_info.attisdropped
                  AND column_info.attname::text = ANY ({SourceColumns})
                  AND pg_catalog.has_column_privilege(source.oid, column_info.attnum, 'SELECT')
                  AND ({schema} = '' OR space.nspname::text = {schema})
                  AND ({name} = '' OR source.relname::text = {name})
                GROUP BY space.nspname, source.relname
                HAVING count(*) = {SourceColumns.Length}
                LIMIT 2
                """).ToListAsync(cancellationToken);

            return matches.Count switch
            {
                1 => matches[0],
                0 => throw new InvalidOperationException("No readable Clara Forms relation contains all overtime summary columns."),
                _ => throw new InvalidOperationException("Multiple Clara Forms overtime relations match. Set ClaraForms:OvertimeSchema and ClaraForms:OvertimeRelation.")
            };
        }

        private sealed record SourceRelation(string Schema, string Name);
    }
}
