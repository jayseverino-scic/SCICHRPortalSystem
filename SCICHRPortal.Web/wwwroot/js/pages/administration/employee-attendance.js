(function ($) {
    const apiHelper = new ApiHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const $form = $('#attendance-filter');
    const $project = $('#project');
    const $date = $('#inputdate');
    const $compute = $('#compute');
    const $reimport = $('#attendance-reimport');
    const $grid = $('#employee-attendance-grid');
    const $gridStatus = $('#attendance-grid-status');
    const $result = $('#attendance-result');
    const manilaDate = new Intl.DateTimeFormat('en', {
        timeZone: 'Asia/Manila', year: 'numeric', month: '2-digit', day: '2-digit'
    });

    let dataTable;
    let loadedFilter = null;
    let requestVersion = 0;
    let loading = false;
    let saving = false;
    let hasApprovalEdits = false;

    function todayInManila() {
        const parts = Object.fromEntries(manilaDate.formatToParts().map(part => [part.type, part.value]));
        return [parts.year, parts.month, parts.day].join('-');
    }

    function readFilter() {
        return { projectId: Number($project.val()), attendanceDate: $date.val() };
    }

    function filtersMatch(first, second) {
        return first?.projectId === second.projectId && first?.attendanceDate === second.attendanceDate;
    }

    function canEditGrid() {
        return !loading && !saving && filtersMatch(loadedFilter, readFilter());
    }

    function canReimport(filter) {
        return canEditGrid() && !hasApprovalEdits && filter.projectId > 0 &&
            filter.attendanceDate <= todayInManila();
    }

    function updateControls() {
        const editable = canEditGrid();
        const filter = readFilter();
        const hasRows = dataTable.rows().count() > 0;
        $compute.prop('disabled', !editable || !hasRows);
        $reimport.prop('disabled', !canReimport(filter));
        $form.find('input, select, button').prop('disabled', saving);
        $(dataTable.table().container()).find('.select-all, .approval-check').prop('disabled', !editable || !hasRows);

        const help = hasApprovalEdits ? 'Compute or discard your approval edits before reimporting.'
            : filter.projectId <= 0 ? 'Select one project to Reimport.'
            : filter.attendanceDate > todayInManila() ? 'Select today or a past date to Reimport.'
            : 'Reimport refreshes the cutoff results for the selected project and date.';
        $('#attendance-reimport-help').text(help);
    }

    function showGridStatus(message) {
        $gridStatus.text(message).prop('hidden', !message);
    }

    function showResult(message, level) {
        $result.removeClass('alert alert-info alert-success alert-warning alert-danger').text(message);
        if (message) $result.addClass('alert alert-' + level);
    }

    async function loadProjects() {
        try {
            const response = await apiHelper.get({ url: 'Authenticated/Project' });
            if (!response.ok) throw new Error();
            for (const project of await response.json()) {
                $('<option>').val(project.id).text(project.name).appendTo($project);
            }
        } catch (_) {
            $('#attendance-project-status')
                .text('Could not load projects. You can still view All Projects.')
                .prop('hidden', false);
        }
    }

    async function loadAttendance(filter) {
        const version = ++requestVersion;
        loading = true;
        updateControls();
        showGridStatus('Loading saved attendance…');

        try {
            const rows = [];
            const length = 100;
            let total;
            do {
                const query = new URLSearchParams({ ...filter, start: rows.length, length });
                const response = await apiHelper.get({ url: 'Authenticated/EmployeeAttendance/SavedAttendance?' + query });
                if (!response.ok) throw new Error();
                const page = await response.json();
                if (version !== requestVersion || !filtersMatch(filter, readFilter())) return;

                rows.push(...page.data);
                total = page.recordsFiltered;
                if (page.data.length === 0) break;
            } while (rows.length < total);

            loadedFilter = filter;
            hasApprovalEdits = false;
            dataTable.clear().rows.add(rows).draw();
            showGridStatus('');
        } catch (_) {
            if (version !== requestVersion) return;
            showGridStatus('Could not load saved attendance. Click Get Attendance to try again.');
        } finally {
            if (version === requestVersion) {
                loading = false;
                updateControls();
            }
        }
    }


    function approvalColumn(title, field) {
        return {
            title: `<input type="checkbox" class="select-all" data-column="${field}" aria-label="Select all for ${title}"> ${title}`,
            data: field,
            width: '50px',
            className: 'dt-center',
            render: (checked, type) => type === 'display'
                ? `<input type="checkbox" class="approval-check" data-column="${field}" aria-label="Approve ${title}"${checked ? ' checked' : ''}>`
                : checked,
            orderable: false
        };
    }

    function getEmployeeAttendanceColumns() {
        return [
            {
                title: "Employee",
                data: "employeeName",
                className: 'dt-center',
                render: $.fn.dataTable.render.text()
            },
            {
                title: "Time Log Id",
                data: "timeLogId",
                className: 'dt-center',
                visible: false

            },
            {
                title: "Time In",
                data: "timeIn",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Time Out",
                data: "timeOut",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Shift Start",
                data: "shiftStart",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: 'Shift End',
                data: "shiftEnd",
                className: 'dt-center',
                render: (data) => {
                    if (!data || _dateHelper.formatShortLocalDate(data) === '01/01/0001') return '-';
                    return _dateHelper.formatShortLocalDate(data) + ' ' + _dateHelper.formatLocalShortTime(data);
                },
                orderable: false
            },
            {
                title: "Flexible Shift",
                data: "isFlexibleShift",
                className: 'noVis dt-center',
                render: function (data) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" ${data ? 'checked' : ''} disabled>`;
                },
                orderable: false
            },
            {
                title: "No Shift",
                data: "isNoShift",
                className: 'noVis dt-center',
                render: function (data) {
                    return `<input type="checkbox" class="row-check" data-column="isNoShift" ${data ? 'checked' : ''} disabled>`;
                },
                orderable: false
            },
            {
                title: "No Break",
                data: "isNoBreak",
                className: 'noVis dt-center',
                render: function (data) {
                    return `<input type="checkbox" class="row-check" data-column="isNoBreak" ${data ? 'checked' : ''} disabled>`;
                },
                orderable: false
            },
            {
                title: "Shift Hours",
                data: "shiftHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Regular Hours",
                data: "regularHour",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Total Worked Hours",
                data: "totalLoggedHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Late",
                data: "shiftLate",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Undertime",
                data: "shiftUndertime",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            approvalColumn("Overtime", "approvedOT"),
            {
                title: "Overtime Hours",
                data: "otHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            {
                title: "Night Differential Hours",
                data: "ndHours",
                className: 'dt-center',
                orderable: false,
                render: (data, type, row) => {
                    if (data === 0) {
                        return '0';
                    }
                    return _numberHelper.formatCommaSeperator(data);
                }
            },
            approvalColumn("Holiday", "approvedHoliday"),
            approvalColumn("Holiday Overtime", "approvedHolidayOT"),
            approvalColumn("Special Holiday", "approvedSPHoliday"),
            approvalColumn("Special Holiday Overtime", "approvedSPHolidayOT"),
            approvalColumn("Rest Day", "approvedRestDay"),
            approvalColumn("Rest Day Overtime", "approvedRestDayOT"),
            {
                title: "Project",
                data: "projectName",
                className: 'dt-center',
                render: $.fn.dataTable.render.text(),
                orderable: true
            },
            {
                title: "Employee Attendance Id",
                data: "employeeAttendanceId",
                visible: false
            },
            {
                title: "Employee Id",
                data: "employeeId",
                visible: false
            },
        ];
    }

    async function readActionError(response, fallback) {
        if (response.status === 403) return 'You do not have access to perform this action.';
        if (![400, 404, 409].includes(response.status)) return fallback;

        const body = await response.text();
        try {
            const error = JSON.parse(body);
            return error.message || error.detail || Object.values(error.errors ?? {}).flat()[0] || fallback;
        } catch (_) {
            return response.status === 409 && body ? body : fallback;
        }
    }

    async function onCompute() {
        if (!canEditGrid() || dataTable.rows().count() === 0) return;

        const filter = loadedFilter;
        saving = true;
        updateControls();
        showResult('Computing attendance…', 'info');
        const failure = 'Could not compute attendance. Try again.';

        try {
            const response = await apiHelper.post({
                url: 'Authenticated/EmployeeAttendance/Compute',
                data: dataTable.rows().data().toArray()
            });
            if (!response.ok) {
                showResult(await readActionError(response, failure), 'danger');
                return;
            }

            hasApprovalEdits = false;
            loadedFilter = null;
            showResult('Attendance computed. Run Reimport for each applicable project to refresh cutoff results.', 'success');
            await loadAttendance(filter);
        } catch (_) {
            showResult(failure, 'danger');
        } finally {
            saving = false;
            updateControls();
        }
    }

    async function onReimport() {
        const filter = readFilter();
        if (!canReimport(filter)) return;

        saving = true;
        updateControls();
        showResult('Reimporting attendance…', 'info');
        const failure = 'Could not reimport attendance. Try again.';

        try {
            const response = await apiHelper.post({
                url: 'Authenticated/EmployeeAttendance/ReimportSavedAttendance',
                data: filter
            });
            if (!response.ok) {
                showResult(await readActionError(response, failure), 'danger');
                return;
            }

            const result = await response.json();
            showResult('Saved ' + result.savedCount + ' attendance records. Skipped ' + result.skippedCount + ' time logs.',
                result.skippedCount > 0 ? 'warning' : 'success');
            loadedFilter = null;
            await loadAttendance(filter);
        } catch (_) {
            showResult(failure, 'danger');
        } finally {
            saving = false;
            updateControls();
        }
    }

    function initializeGrid() {
        dataTable = $grid.DataTable({
            dom: '<"attendance-table-toolbar"lf>rtip',
            pageLength: 10,
            lengthMenu: [[5, 10, 20, 40, 80, -1], [5, 10, 20, 40, 80, 'All']],
            autoWidth: false,
            scrollX: true,
            data: [],
            columns: getEmployeeAttendanceColumns(),
            language: { emptyTable: 'No saved attendance to display. Select one project and run Reimport.' }
        });

        const $container = $(dataTable.table().container());
        const $headers = $container.find('.dataTables_scrollHead .select-all');

        function updateApprovalHeaders() {
            $headers.each(function () {
                const column = this.dataset.column;
                const rows = dataTable.rows().data().toArray();
                const checked = rows.filter(row => row[column]).length;
                this.checked = rows.length > 0 && checked === rows.length;
                this.indeterminate = checked > 0 && checked < rows.length;
            });
        }

        $grid.on('draw.dt', () => {
            updateApprovalHeaders();
            updateControls();
        });

        $container.on('change', '.approval-check', function () {
            if (!canEditGrid()) return;
            const cell = this.closest('td');
            const row = dataTable.row(this.closest('tr'));
            row.data()[this.dataset.column] = this.checked;
            row.invalidate();
            cell.querySelector('.approval-check')?.focus();
            hasApprovalEdits = true;
            updateApprovalHeaders();
            updateControls();
        });

        $container.on('change', '.dataTables_scrollHead .select-all', function () {
            if (!canEditGrid()) return;
            const column = this.dataset.column;
            const checked = this.checked;
            dataTable.rows().every(function () {
                this.data()[column] = checked;
                this.invalidate();
            });
            hasApprovalEdits = true;
            dataTable.draw(false);
        });
    }

    function initialize() {
        $date.val(todayInManila());
        initializeGrid();
        $form.on('submit', event => {
            event.preventDefault();
            if (saving || !$form[0].reportValidity()) return;
            if (hasApprovalEdits && !window.confirm('Discard unsaved approval edits and get attendance?')) return;

            showResult('');
            loadAttendance(readFilter());
        });

        $project.add($date).on('change', () => {
            ++requestVersion;
            loading = false;
            showResult('');
            showGridStatus(filtersMatch(loadedFilter, readFilter()) ? '' : 'Click Get Attendance to load the selected filters.');
            updateControls();
        });

        $compute.on('click', onCompute);
        $reimport.on('click', onReimport);
        loadProjects();
        loadAttendance(readFilter());
    }

    $(initialize);
})(jQuery);
