# Leave

Administrators manage records at **Attendance > Leave**. The MVC page and all
`api/Authenticated/Leave` endpoints require the existing `Administrator` role.

The form, list, and Excel template use this order:
Employee No., Employee Name, From Date, From Time, To Date, To Time, Type of Leave.
Employee No. and Employee Name select one internal employee ID. Leading zeros in
employee numbers are preserved; names reflect current employee master data.

Employee, Type of Leave, both dates, and both times are required. Choose an active
type from the existing Leave Type master data. Dates and times represent one
continuous Manila-local period, with whole-minute precision and an end later than
the start. Past, future, overnight, and multi-day periods are supported.

Overlapping undeleted Leave records for the same employee are rejected across all
leave types; adjacent periods are allowed. The overlap check and audited save share
a PostgreSQL serializable transaction. Overlap, serialization, and deadlock conflicts
return 409. The form preserves entered values on failures.

Records are soft-deleted. Authenticated claims supply actor fields; audit timestamps
are UTC. ApiHelper supplies existing audit metadata headers. Previously selected
deleted employees/types remain readable and may be retained on edits, but cannot
be newly selected. New writes accept only employee/type IDs and date/time values.

This is a separate module/table from the older `LeaveRequest` entity and Official
Business. It does not change approvals, attendance, time logs, payroll, leave balances,
or LeaveRequest data. There is no overlap check between Leave and OB records.

## Excel upload

Download a fresh template from **Download Template**, fill its first worksheet,
and choose **Upload Excel**. Keep the seven headers and their order. Employee No.
matches an active employee; Employee Name is optional and informational. Type of
Leave matches one active description from the included **Leave Types** reference
sheet/dropdown. Matching ignores surrounding spaces and case; ambiguous employee
numbers or type descriptions are rejected. Missing types must first be created
through the existing Leave Type master page.

Use an unencrypted `.xlsx` file, at most 10 MiB, with data in rows 2–1001. Workbook
ZIP content is limited to 50 MiB expanded and 1,000 entries. Excel date/time cells
and 1904 workbooks are supported; text dates use `yyyy-MM-dd`, text times `HH:mm`
or `h:mm AM/PM`. Times must be whole minutes. Formulas in input columns are rejected;
paste values. Blank rows are ignored. Invalid workbook structure returns 400
before any rows are saved.

Valid rows are committed independently in file order. Invalid, ambiguous, and
overlapping rows are rejected and reported by Excel row number and employee number.
Earlier successful rows remain saved. Each row rechecks its employee-number and
type-description matches inside its transaction, so changes to master data cannot
silently redirect a cached selection. Existing records are never overwritten.
Correct and re-upload only rejected rows. After a network/server failure, refresh
and review the list before retrying; some rows may already have committed. Uploads
are not retried automatically and no workbook is retained.

`GET /ImportTemplate` downloads the template. `POST /Import` accepts multipart
field `file` and returns `totalRows`, `imported`, `rejected`, and `errors` containing
`rowNumber`, `employeeNo`, and `message`. Completed imports return 200 even if all
rows are rejected.

## Deployment

Apply Repository migration `20261119123049_AddLeave` to a verified SCIC PostgreSQL
ApplicationContext database before using the module. It creates only the Leave
table, foreign keys, period check, and indexes. Business timestamps use
`timestamp without time zone`; audits use UTC `timestamp with time zone`.
Existing LeaveRequest, OB, Timekeeping, and Xscribe tables are unaffected.

Generate and inspect the upgrade script before applying it:

```powershell
dotnet ef migrations script 20261119113815_RemoveLeaveTypeFromOfficialBusiness 20261119123049_AddLeave --project SCICHRPortal.Repository --startup-project SCICHRPortal.API --context ApplicationContext
```

Verify the target database: the design-time factory reads appsettings.json from
the application base directory. Environment flags alone do not select a different
connection. Rebuild/restart API and Web after deployment. Rolling back the Leave
migration drops its table and records, so retain a backup before migration changes.
