# Official Business

Administrators manage records at **Attendance > Official Business**. The MVC page
and every `api/Authenticated/OfficialBusiness` action require the existing
`Administrator` role.

Select an employee by Employee No. or Employee Name; both controls identify one
internal EmployeeId. Employee numbers remain strings, including leading zeros.
Provide From Date OB, To Date OB, From Time OB, and To Time OB.
These describe one continuous Manila-local period; overnight and multi-day
periods are supported. All fields are required, time precision is one minute,
and the end must be later than the start. Past and future periods are allowed.

An employee cannot have overlapping active Official Business periods.
Adjacent periods are allowed. Writes check overlap and
save in a PostgreSQL serializable transaction; simultaneous overlapping saves
cannot both commit. A 409 means overlap or a concurrent database change. The
form retains its input so the administrator can correct the period or reload.

Deletes are soft deletes. Creation metadata is preserved on edits/deletes;
actor fields come from authenticated claims and audit timestamps are UTC.
If a display name exceeds the audit field length, the authenticated user ID is
stored instead so valid long account names do not prevent saving.
ApiHelper request metadata continues to supply the existing audit trail headers.
Employee names come from current master data. Deleted employees
remain readable on existing records and can be retained on edits, but cannot be
newly selected.

This module does not change attendance, time logs (including IsOB), leave
balances, payroll, or the existing LeaveRequest table. Approvals, attachments,
notifications, and export are outside this version.

## Excel bulk upload

Use **Download Template**, fill the first worksheet, and choose **Upload Excel**.
The template has six columns in this order: Employee No., Employee Name,
From Date OB, From Time OB, To Date OB, To Time OB. Headers must be retained.
Employee No. identifies an undeleted employee, preserving leading zeros. Employee
Name is optional and informational; the stored employee is never renamed.
Employee-number lookup ignores surrounding spaces and case; ambiguous employee
numbers are rejected. Download a fresh template to replace earlier formats, including
the seven-column format and the grouped-date format. Official Business has no Leave Type selection.

Upload an unencrypted `.xlsx` file, up to 10 MiB, with data in rows 2 through 1001.
Blank rows are ignored. Workbook ZIP content is limited to 50 MiB expanded and
1,000 entries. Native Excel dates/times are accepted, including 1904 workbooks.
Text dates use `yyyy-MM-dd`; text times use `HH:mm` or `h:mm AM/PM`. Whole-minute
precision and the normal Manila period rules still apply. Formulas are rejected;
paste values instead. Workbook validation failures return 400 before any writes.

`GET api/Authenticated/OfficialBusiness/ImportTemplate` downloads the template.
`POST api/Authenticated/OfficialBusiness/Import` accepts multipart field `file`.
Both endpoints require Administrator. The server returns `totalRows`, `imported`,
`rejected`, and `errors` containing Excel `rowNumber`, `employeeNo`, and `message`.
Completed imports return 200 even when some or all rows are rejected.

Rows are processed top to bottom. Each valid row commits independently through
the audited, serializable manual creation path. Earlier successful rows remain
saved when a later row is rejected. Existing and earlier imported overlaps are
rejected rather than overwritten, so retrying a file does not duplicate saved OBs.
Each row rechecks its unique employee-number match inside
the save transaction. If those identifiers change during the import, affected
rows are rejected for review rather than silently assigned using stale IDs.
Correct and upload only rejected rows. If a request is interrupted, refresh the
list before retrying because some rows may already have committed. Uploads are
not retried automatically and no workbook is retained by the feature.

## Deployment

Apply Repository migrations through `20261119113815_RemoveLeaveTypeFromOfficialBusiness`
to the verified SCIC PostgreSQL ApplicationContext database before using the page.
The original `20261119102615_AddOfficialBusiness` creates the table; the new migration
removes only its LeaveType foreign key, index, and column. Existing employee references,
periods, and audit fields are preserved. LeaveType master data and other modules,
external Timekeeping, and Xscribe databases are unaffected. Business timestamps
are stored without timezone; audit timestamps use timestamptz with UTC mapping.
The global legacy Npgsql timestamp setting remains unchanged.

Generate/review the upgrade script using the preceding migration as the start:

```powershell
dotnet ef migrations script 20261118120331_AddEmployeeDailyAttendanceComputation 20261119113815_RemoveLeaveTypeFromOfficialBusiness --project SCICHRPortal.Repository --startup-project SCICHRPortal.API --context ApplicationContext
```

Verify the target before running any database update. The design-time factory
reads appsettings.json from the application base directory; environment flags
alone do not select its connection.

If the original OB migration is already applied, generate the script starting at
`20261119102615_AddOfficialBusiness` instead. Back up before upgrading: removed
leave-type assignments cannot be recovered by a down migration. Rolling back a
populated OB table requires restoring that backup; the down migration rejects
populated records instead of inventing assignments.
