(function ($) {
    'use strict';

    const api = new ApiHelper();
    const endpoint = 'Authenticated/Leave';
    const modal = $('#leave-modal');
    const modalDialog = bootstrap.Modal.getOrCreateInstance(modal[0], { backdrop: 'static', keyboard: false });
    const form = $('#leave-form');
    const searchVersions = { EmployeeNo: 0, EmployeeName: 0 };
    let table;
    let selectedEmployee = null;
    let editingId = 0;
    let modalVersion = 0;
    let pageVersion = 0;
    let activeSearch = null;
    let detailsReady = false;
    let typesReady = false;
    let loadingTypes = false;
    let leaveTypes = [];
    let editingRecord = null;
    let loading = false;
    let submitting = false;
    const deleting = new Set();

    const attachEvents = () => {
        $(document).on('leave:imported', reloadGrid);
        $('#leave-add').on('click', onAddButtonClicked);
        $('#leave-types-retry').on('click', onLeaveTypesRetryClicked);
        $('#leave-retry').on('click', reloadGrid);
        $('#leave-grid').on('click', '.leave-edit', onEditButtonClicked);
        $('#leave-grid').on('click', '.leave-delete', onDeleteButtonClicked);

        form.on('submit', onFormSubmit);
        modal.on('hide.bs.modal', onFormModalHiding);
        modal.on('shown.bs.modal', onFormModalShown);
        modal.on('hidden.bs.modal', onFormModalHidden);

        $(document).on('input.leave', '.select2-search__field', onEmployeeSearchInput);
    };

    const setFormStatus = message => $('#leave-form-status').text(message || '');
    const setPageStatus = message => $('#leave-page-status').text(message || '');
    const updateControls = () => {
        form.find('input, select').prop('disabled', loading || submitting);
        $('#LeaveTypeId').prop('disabled', loading || submitting || !typesReady);
        $('#leave-save').prop('disabled', loading || submitting || !detailsReady || !typesReady)
            .text(submitting ? 'Saving...' : editingId ? 'Save' : 'Add');
        $('#leave-cancel').prop('disabled', submitting);
        $('#leave-types-retry').prop('disabled', loading || submitting || loadingTypes);
    };

    const responseError = response => api.readErrorMessage(response, {
        fallback: 'The request could not be completed.',
        forbidden: 'Administrator access is required.',
        serverError: 'The server could not complete the request. Please try again.'
    });

    const getJson = async url => {
        const response = await api.get({ url });
        if (!response.ok) throw new Error(await responseError(response));
        return response.json();
    };

    const renderLeaveTypes = selectedId => {
        const select = $('#LeaveTypeId').empty().append(new Option('', ''));
        for (const type of leaveTypes) select.append(new Option(type.description || '(No description)', String(type.leaveTypeId)));
        if (editingRecord && !leaveTypes.some(type => type.leaveTypeId === editingRecord.leaveTypeId)) {
            select.append(new Option((editingRecord.leaveType || '(No description)') +
                (editingRecord.leaveTypeDeleted ? ' (deleted)' : ''), String(editingRecord.leaveTypeId)));
        }
        select.val(selectedId || '').trigger('change.select2');
    };

    const loadLeaveTypes = async version => {
        typesReady = false;
        loadingTypes = true;
        $('#leave-types-status').text('Loading leave types...');
        $('#leave-types-retry').prop('hidden', true);
        updateControls();
        try {
            const result = await getJson(`${endpoint}/LeaveTypes`);
            if (version !== modalVersion) return;
            leaveTypes = result;
            const selectedId = $('#LeaveTypeId').val();
            renderLeaveTypes(selectedId);
            typesReady = true;
            $('#leave-types-status').text(result.length ? '' : 'No active leave types. Add one in Leave Type master data before creating a record.');
        } catch (error) {
            if (version === modalVersion) {
                $('#leave-types-status').text(error.message || 'Leave types could not be loaded. Please retry.');
                $('#leave-types-retry').prop('hidden', false);
            }
        } finally {
            if (version === modalVersion) {
                loadingTypes = false;
                updateControls();
            }
        }
    };

    const setEmployee = employee => {
        selectedEmployee = employee;
        searchVersions.EmployeeNo++;
        searchVersions.EmployeeName++;
        for (const [id, label] of [['EmployeeNo', employee && employee.employeeNo], ['EmployeeName', employee && employee.employeeName]]) {
            const select = $(`#${id}`).empty().append(new Option('', ''));
            if (employee) select.append(new Option(label || '(Not provided)', String(employee.employeeId), true, true));
            select.val(employee ? String(employee.employeeId) : '').trigger('change.select2');
        }
    };

    const initializeEmployeeLookup = id => {
        $(`#${id}`).select2({
            theme: 'bootstrap', width: '100%', dropdownParent: modal, allowClear: true,
            placeholder: id === 'EmployeeNo' ? 'Search employee number' : 'Search employee name',
            minimumInputLength: 2, maximumInputLength: 100,
            ajax: {
                delay: 250,
                data: parameters => ({ term: parameters.term, page: parameters.page || 1 }),
                transport: (parameters, success, failure) => {
                    let cancelled = false;
                    const version = searchVersions[id];
                    const currentModal = modalVersion;
                    const query = new URLSearchParams(parameters.data);
                    getJson(`${endpoint}/EmployeeLookup?${query}`).then(result => {
                        if (!cancelled && currentModal === modalVersion && version === searchVersions[id]) success(result);
                    }).catch(error => {
                        if (!cancelled && currentModal === modalVersion && version === searchVersions[id]) {
                            $('#leave-lookup-status').text('Employee search failed. Type again to retry.');
                            failure(error);
                        }
                    });
                    return { abort: () => { cancelled = true; } };
                },
                processResults: result => ({
                    results: result.data.map(employee => ({
                        id: String(employee.employeeId),
                        text: id === 'EmployeeNo' ? employee.employeeNo || '(No employee number)' : employee.employeeName || '(No employee name)',
                        employee
                    })),
                    pagination: { more: result.more }
                })
            }
        }).on('select2:open', () => { activeSearch = id; })
            .on('select2:close', () => { activeSearch = null; })
            .on('select2:select', event => {
                setEmployee(event.params.data.employee);
                $('#leave-lookup-status').text('');
            }).on('select2:clear', () => setEmployee(null));
    };

    const openForm = async id => {
        if (submitting) return;
        const version = ++modalVersion;
        editingId = id || 0;
        detailsReady = !id;
        typesReady = false;
        leaveTypes = [];
        editingRecord = null;
        loading = true;
        form[0].reset();
        if (form.data('validator')) form.validate().resetForm();
        setEmployee(null);
        renderLeaveTypes(null);
        setFormStatus('');
        $('#leave-lookup-status').text(id ? 'Loading form...' : '');
        $('#leave-modal-title').text(id ? 'Edit Leave' : 'Add Leave');
        updateControls();
        modalDialog.show();
        const typeRequest = loadLeaveTypes(version);
        try {
            if (id) {
                const record = await getJson(`${endpoint}/${encodeURIComponent(id)}`);
                if (version !== modalVersion) return;
                editingRecord = record;
                setEmployee(record);
                renderLeaveTypes(record.leaveTypeId);
                // Business timestamps have no offset. Slicing avoids browser timezone conversion.
                $('#FromDate').val(record.from.slice(0, 10));
                $('#FromTime').val(record.from.slice(11, 16));
                $('#ToDate').val(record.to.slice(0, 10));
                $('#ToTime').val(record.to.slice(11, 16));
                detailsReady = true;
                $('#leave-lookup-status').text('');
            }
        } catch (error) {
            if (version === modalVersion) {
                setFormStatus(error.message || 'The record could not be loaded. Close the form and try again.');
                $('#leave-lookup-status').text('Close the form and select Edit again to retry.');
            }
        } finally {
            await typeRequest;
            if (version === modalVersion) {
                loading = false;
                updateControls();
            }
        }
    };

    const getFormValues = () => {
        return {
            employeeId: selectedEmployee ? selectedEmployee.employeeId : null,
            leaveTypeId: Number($('#LeaveTypeId').val()) || null,
            fromDate: $('#FromDate').val(),
            fromTime: $('#FromTime').val(),
            toDate: $('#ToDate').val(),
            toTime: $('#ToTime').val()
        };
    };

    const validateForm = values => {
        const hasRequiredValues = values.employeeId && values.leaveTypeId &&
            values.fromDate && values.fromTime && values.toDate && values.toTime;
        if (!hasRequiredValues || !form.valid()) {
            setFormStatus('Select an employee and Type of Leave, and enter all From and To dates and times.');
            return false;
        }

        const from = `${values.fromDate}T${values.fromTime}`;
        const to = `${values.toDate}T${values.toTime}`;
        if (to <= from) {
            setFormStatus('The end date and time must be later than the start date and time.');
            return false;
        }

        return true;
    };

    const saveLeave = async (id, data) => {
        const options = {
            url: id ? `${endpoint}/${encodeURIComponent(id)}` : endpoint,
            data,
            requesterName: $('#current-user').text(),
            requestOrigin: 'Leave',
            requestSystem: 'SCIC HR Portal'
        };
        const response = await (id ? api.put(options) : api.post(options));
        if (!response.ok) {
            throw new Error(await responseError(response));
        }
    };

    const onFormSubmit = async event => {
        event.preventDefault();
        if (submitting || loading || !detailsReady || !typesReady) return;

        setFormStatus('');
        const values = getFormValues();
        if (!validateForm(values)) return;

        const id = editingId;
        submitting = true;
        updateControls();

        try {
            await saveLeave(id, values);
            toastr.success(id ? 'Leave updated.' : 'Leave created.');
            submitting = false;
            modalDialog.hide();
            reloadGrid();
        } catch (error) {
            setFormStatus(error.message || 'Leave could not be saved. Please try again.');
        } finally {
            submitting = false;
            updateControls();
        }
    };

    const onDelete = async record => {
        if (deleting.has(record.leaveId)) return;
        deleting.add(record.leaveId);
        try {
            const confirmation = await Swal.fire({
                title: 'Delete Leave?', text: 'This removes the record from the active list.',
                icon: 'warning', showCancelButton: true, confirmButtonText: 'Delete', confirmButtonColor: '#b42318',
                customClass: { container: 'leave-confirmation' }
            });
            if (!confirmation.isConfirmed) return;
            const response = await api.delete({
                url: `${endpoint}/${encodeURIComponent(record.leaveId)}`,
                requesterName: $('#current-user').text(), requestOrigin: 'Leave', requestSystem: 'SCIC HR Portal'
            });
            if (!response.ok) throw new Error(await responseError(response));
            toastr.success('Leave deleted.');
            table.ajax.reload(null, false);
        } catch (error) {
            setPageStatus(error.message || 'Leave could not be deleted. Please try again.');
        } finally {
            deleting.delete(record.leaveId);
        }
    };

    const requestGridPage = async (request, callback) => {
        const version = ++pageVersion;
        const order = request.order[0] || { column: 2, dir: 'desc' };
        const sortColumns = ['employeeNo', 'employeeName', 'from', 'from', 'to', 'to', 'leaveType'];
        const query = new URLSearchParams({
            draw: request.draw,
            start: request.start,
            length: request.length,
            search: request.search.value,
            sortColumn: sortColumns[order.column] || 'from',
            sortDirection: order.dir
        });

        setPageStatus('Loading records...');
        $('#leave-retry').prop('hidden', true);

        try {
            const result = await getJson(`${endpoint}/Page?${query}`);
            if (version !== pageVersion) return;

            setPageStatus('');
            callback(result);
        } catch (error) {
            if (version !== pageVersion) return;

            callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
            setPageStatus(error.message || 'Records could not be loaded. Please retry.');
            $('#leave-retry').prop('hidden', false);
        }
    };

    function onGridDraw() {
        const grid = this.api();
        const info = grid.page.info();
        if (info.recordsDisplay > 0 && info.start >= info.recordsDisplay && info.page > 0) {
            grid.page('previous').draw('page');
        }
    }

    const initializeGrid = () => {
        const text = $.fn.dataTable.render.text();
        // Interpret offset-free Manila wall times without applying the browser's DST rules.
        const date = (value, type) => type === 'display' ? moment.utc(value).format('MMM DD, YYYY') : value;
        const time = (value, type) => type === 'display' ? moment.utc(value).format('h:mm A') : value;
        table = $('#leave-grid').DataTable({
            serverSide: true,
            processing: true,
            searching: true,
            searchDelay: 300,
            scrollX: true,
            dom: '<"leave-table-toolbar"lf>rtip',
            pageLength: 10,
            lengthMenu: [10, 20, 40, 80],
            order: [[2, 'desc']],
            language: {
                emptyTable: 'No Leave records.',
                zeroRecords: 'No matching Leave records.'
            },
            ajax: requestGridPage,
            columns: [
                { data: 'employeeNo', render: text, defaultContent: '' },
                { data: 'employeeName', render: text, defaultContent: '' },
                { data: 'from', render: date },
                { data: 'from', render: time },
                { data: 'to', render: date },
                { data: 'to', render: time },
                { data: 'leaveType', render: text, defaultContent: '' },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    className: 'leave-actions',
                    defaultContent: '<button type="button" class="leave-edit" title="Edit" aria-label="Edit Leave"><i class="fas fa-edit" aria-hidden="true"></i></button>' +
                        '<button type="button" class="leave-delete" title="Delete" aria-label="Delete Leave"><i class="fas fa-trash border-icon" aria-hidden="true"></i></button>'
                }
            ],
            drawCallback: onGridDraw
        });
        $('#leave-grid_filter input').attr('maxlength', 200);
    };

    const reloadGrid = () => table.ajax.reload(null, false);

    const onAddButtonClicked = () => openForm(0);

    const onEditButtonClicked = event => {
        const record = table.row($(event.currentTarget).closest('tr')).data();
        if (record && !deleting.has(record.leaveId)) {
            openForm(record.leaveId);
        }
    };

    const onDeleteButtonClicked = event => {
        const record = table.row($(event.currentTarget).closest('tr')).data();
        if (record) {
            onDelete(record);
        }
    };

    const onLeaveTypesRetryClicked = () => loadLeaveTypes(modalVersion);

    const onFormModalHiding = event => {
        if (submitting) {
            event.preventDefault();
            return;
        }

        modalVersion++;
        searchVersions.EmployeeNo++;
        searchVersions.EmployeeName++;
    };

    const onFormModalShown = () => {
        $('.modal-backdrop').last().addClass('leave-backdrop');
        $('#EmployeeNo').next('.select2').find('.select2-selection').trigger('focus');
    };

    const onFormModalHidden = () => $('#leave-add').trigger('focus');

    const onEmployeeSearchInput = () => {
        if (activeSearch) {
            searchVersions[activeSearch]++;
        }
    };

    const initialize = () => {
        initializeEmployeeLookup('EmployeeNo');
        initializeEmployeeLookup('EmployeeName');
        $('#LeaveTypeId').select2({
            theme: 'bootstrap',
            width: '100%',
            dropdownParent: modal,
            placeholder: 'Select Type of Leave',
            allowClear: true
        });
        attachEvents();
        initializeGrid();
    };

    $(initialize);
})(jQuery);
