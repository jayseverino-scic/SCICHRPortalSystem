(function ($) {
    const api = new ApiHelper();
    const formHelper = new FormHelper();
    const dateHelper = new DateHelper();
    const rules = HolidayFormRules;
    let activeProjects = [];
    let projectsAvailable = false;
    let projectRequestId = 0;
    let editAssignments = [];
    let editAllProjects = false;
    let grid;

    function renderProjectOptions(selected = null) {
        const select = $('#ProjectIds').empty();
        if (projectsAvailable || editAllProjects) select.append($('<option>').val('__all__').text('All Projects'));
        activeProjects.forEach(project => {
            const assigned = editAssignments.find(item => item.id === project.id);
            const inactive = assigned?.deleted === true;
            select.append($('<option>').val(String(project.id)).text(`${project.name}${inactive ? ' (inactive)' : ''}`));
        });
        editAssignments.filter(project => !activeProjects.some(active => active.id === project.id)).forEach(project => {
            select.append($('<option>').val(String(project.id)).text(`${project.name || `Project #${project.id}`} (inactive)`));
        });
        select.val(selected).trigger('change');
    }

    function configureProjectField() {
        const select = $('#ProjectIds');
        select.prop('multiple', true);
        $('#local-project-group').removeClass('d-none');
        select.prop('required', true);
        $('#project-validation').addClass('d-none');
    }

    async function loadProjects() {
        const requestId = ++projectRequestId;
        const select = $('#ProjectIds');
        const selection = select.val();
        activeProjects = [];
        projectsAvailable = false;
        select.prop('disabled', true);
        renderProjectOptions(selection);
        try {
            const response = await api.get({ url: 'Authenticated/Project' });
            if (requestId !== projectRequestId) return;
            if (!response.ok) throw new Error('Project request failed');
            const projects = await response.json();
            if (requestId !== projectRequestId) return;
            activeProjects = projects
                .filter(project => !project.deleted && Number.isInteger(Number(project.id)) && Number(project.id) > 0)
                .map(project => ({ id: Number(project.id), name: project.name }));
            projectsAvailable = true;
            const current = select.val();
            renderProjectOptions(current && current.length ? current : (Number($('#HolidayId').val()) === 0 ? ['__all__'] : []));
            select.prop('disabled', false);
        } catch (_error) {
            if (requestId !== projectRequestId) return;
            activeProjects = [];
            projectsAvailable = false;
            renderProjectOptions(Number($('#HolidayId').val()) > 0 ? select.val() : null);
            toastr.error('Could not load projects.');
        }
    }

    function openForCreate() {
        editAssignments = [];
        editAllProjects = false;
        const form = $('#holiday-form')[0];
        form.reset();
        $('#HolidayId').val(0);
        $('#HolidayType').val('1').trigger('change');
        renderProjectOptions(['__all__']);
        $('#holiday-modal-btn').text('Add');
        configureProjectField();
        $('#holiday-modal').modal('show');
        loadProjects();
    }

    function openForEdit(row) {
        editAllProjects = row.allProjects === true;
        const projectIds = Array.isArray(row.projectIds) ? row.projectIds.map(Number).filter(id => Number.isSafeInteger(id) && id > 0) : [];
        const projectDetails = Array.isArray(row.projects) ? row.projects : [];
        editAssignments = projectIds.map((id, index) => {
            const detail = projectDetails.find(project => Number(project.id) === id);
            return { id, name: detail?.name || row.projectNames?.[index] || `Project #${id}`, deleted: detail?.deleted === true };
        });
        const form = $('#holiday-form')[0];
        form.reset();
        $('#HolidayId').val(row.holidayId);
        $('#HolidayName').val(row.holidayName);
        $('#HolidayDate').val(moment(row.holidayDate).format('YYYY-MM-DD'));
        $('#HolidayType').val(String(row.holidayType)).trigger('change');
        configureProjectField();
        renderProjectOptions(editAllProjects ? ['__all__'] : projectIds.map(String));
        $('#holiday-modal-btn').text('Update');
        $('#holiday-modal').modal('show');
    }

    async function submitForm(event) {
        event.preventDefault();
        const form = $(event.currentTarget);
        if (!form.valid()) return;
        const payload = rules.buildPayload({
            holidayId: $('#HolidayId').val(),
            holidayName: $('#HolidayName').val(),
            holidayDate: $('#HolidayDate').val(),
            holidayType: $('#HolidayType').val(),
            selectedProjects: $('#ProjectIds').val()
        });
        const editing = payload.holidayId > 0;
        const validProject = payload.allProjects || payload.projectIds.length > 0;
        $('#project-validation').toggleClass('d-none', validProject);
        if (!validProject) return;

        $('#busy-indicator-container').removeClass('d-none');
        const request = {
            url: 'Authenticated/Holiday',
            data: payload,
            requestOrigin: `${$('.tab-pane.active .title').text()} Tab`,
            requesterName: $('#current-user').text()
        };
        try {
            const response = editing ? await api.put(request) : await api.post(request);
            if (response.ok) {
                grid.ajax.reload(null, false);
                toastr.success('Success');
                $('#holiday-modal').modal('hide');
            } else if (response.status === 403) {
                noAccessAlert();
            } else {
                const error = await response.json().catch(() => null);
                Swal.fire('Error!', error?.message || 'Could not save holiday.', 'error');
            }
        } catch (_error) {
            Swal.fire('Error!', 'Could not save holiday. Please try again.', 'error');
        } finally {
            $('#busy-indicator-container').addClass('d-none');
        }
    }

    function textColumn(title, field) {
        return {
            title, data: field, className: 'noVis dt-center',
            render: (data, type) => type === 'display' ? rules.escapeHtml(data) : data
        };
    }

    function initializeGrid() {
        grid = $('#holiday-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true, bInfo: true, serverSide: true, bSort: false,
            scrollY: '350px', scrollX: true, pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
            ajax: async (params, success) => {
                const page = Math.floor(params.start / params.length) + 1;
                const response = await api.get({
                    url: `Authenticated/Holiday/Filter?pageNumber=${page}&pageSize=${params.length}&searchKeyword=${encodeURIComponent(params.search.value)}`
                });
                if (!response.ok) {
                    success({ recordsFiltered: 0, recordsTotal: 0, data: [] });
                    return;
                }
                const json = await response.json();
                success({ recordsFiltered: json.total, recordsTotal: json.total, data: json.data });
            },
            columns: [
                {
                    title: 'No.', data: 'holidayId', className: 'noVis dt-center',
                    render: (_data, _type, _row, meta) => meta.settings._iDisplayStart + meta.row + 1
                },
                textColumn('Holiday Name', 'holidayName'),
                textColumn('Holiday Type', 'holidayTypeName'),
                {
                    title: 'Projects', data: null, className: 'noVis dt-center',
                    render: (_data, type, row) => type === 'display' ? rules.projectSummary(row) : row.projectNames?.join(', ') || ''
                },
                {
                    title: 'Holiday Date', data: 'holidayDate', className: 'noVis dt-center',
                    render: data => dateHelper.formatShortLocalDate(data)
                },
                {
                    data: 'holidayId', width: '3em', className: 'noVis dt-center',
                    render: (data, type) => type === 'display'
                        ? `<a href="#" class="m-1 icon-edit" data-id="${Number(data)}"><i class="fas fa-edit"></i></a>` +
                          `<a href="#" class="m-1 icon-delete" data-id="${Number(data)}" data-endpoint="Authenticated/Holiday" data-table="holiday-grid"><i class="fas fa-trash border-icon"></i></a>`
                        : ''
                }
            ]
        });
        $('#holiday-grid tbody').on('click', '.icon-edit', function (event) {
            event.preventDefault();
            openForEdit(grid.row($(this).closest('tr')).data());
        });
        $('#holiday-grid tbody').on('click', '.icon-delete', function (event) {
            const row = grid.row($(this).closest('tr')).data();
            formHelper.deleteRecord(event, row.holidayName, 'timekeeping');
        });
    }

    $(document).ready(function () {
        $('#HolidayType').select2({ theme: 'bootstrap', width: '100%', dropdownParent: $('#holiday-modal') });
        $('#ProjectIds').select2({ theme: 'bootstrap', width: '100%', dropdownParent: $('#holiday-modal') });
        $('#ProjectIds').on('select2:select', event => {
            const select = $('#ProjectIds');
            if (event.params.data.id === '__all__') {
                select.val(['__all__']).trigger('change');
            } else {
                const values = select.val() || [];
                if (values.includes('__all__')) {
                    select.val(values.filter(value => value !== '__all__')).trigger('change');
                }
            }
        });
        $('#HolidayType').on('change', configureProjectField);
        $('#add-button').on('click', openForCreate);
        $('#holiday-form').on('submit', submitForm);
        initializeGrid();
        loadProjects();
    });
})(jQuery);
