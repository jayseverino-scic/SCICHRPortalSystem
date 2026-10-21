# Same-schedule assignment checks

These package-free executable checks call the real service and repository with
PostgreSQL. Start an isolated, disposable PostgreSQL server at
`127.0.0.1:55485` with a local test-only `postgres` role that can create databases.
Do not use the application's database server or configuration.

Run from the repository root:

```powershell
dotnet run --project tests/SCICHRPortal.EmployeeShift.SameSchedule.Checks/SCICHRPortal.EmployeeShift.SameSchedule.Checks.csproj
node tests/employee-shift-same-schedule-ui.cjs
dotnet run --project tests/SCICHRPortal.EmployeeShift.SameSchedule.Checks/SCICHRPortal.EmployeeShift.SameSchedule.Checks.csproj -- --history-deletion
node tests/employee-shift-history-deletion-ui.cjs
```

The database checks create a uniquely named `scic_same_schedule_check_*`
database and delete it in `finally`, including when an assertion fails. All data
is synthetic. They cover matching schedules at effective boundaries, ignored
time/flag differences, temporary periods, mixed bulk assignments, flag-only
changes, soft deletion, query counts and rollback across batches.

The Node checks execute the actual page save handler with controlled API
responses. They cover the no-change toast, successful cleanup/reload, and
retaining selections and dates after a conflict. They do not replace signed-in
browser testing.

History-deletion checks use another uniquely named disposable database on that
same isolated server. They verify exact-row deletion, latest future/restoration
rows, gaps without neighbor changes, full-filter selection with exclusions,
unchanged saved time logs/attendance, and rollback after an injected second-batch
serialization failure. The UI checks exercise confirmation, cancellation,
selected-only targeting, bulk exclusions, unrelated edit retention and failures.
