(function ($) {
    'use strict';

    const api = new ApiHelper();
    const endpoint = 'Authenticated/OfficialBusiness';
    const modal = $('#official-business-modal');
    const modalDialog = bootstrap.Modal.getOrCreateInstance(modal[0], { backdrop: 'static', keyboard: false });
    const form = $('#official-business-form');
    const searchVersions = { EmployeeNo: 0, EmployeeName: 0 };
    let table;
    let selectedEmployee = null;
    let editingId = 0;
    let modalVersion = 0;
    let pageVersion = 0;
    let activeSearch = null;
    let detailsReady = false;
    let loading = false;
    let submitting = false;
    const deleting = new Set();

    const setFormStatus = message => $('#official-business-form-status').text(message || '');
    const setPageStatus = message => $('#official-business-page-status').text(message || '');
    const updateControls = () => {
        form.find('input, select').prop('disabled', loading || submitting);
        $('#official-business-save').prop('disabled', loading || submitting || !detailsReady)
            .text(submitting ? 'Saving...' : editingId ? 'Save' : 'Add');
        $('#official-business-cancel').prop('disabled', submitting);
    };

    const responseError = async response => {
        if (response.status >= 500) return 'The server could not complete the request. Please try again.';
        if (response.status === 403) return 'Administrator access is required.';
        const body = await response.text();
        try {
            const parsed = JSON.parse(body);
            return parsed.message || parsed.Message || (parsed.errors && Object.values(parsed.errors).flat().join(' ')) ||
                'The request could not be completed.';
        } catch (_) {
            return body || 'The request could not be completed.';
        }
    };

    const getJson = async url => {
        const response = await api.get({ url });
        if (!response.ok) throw new Error(await responseError(response));
        return response.json();
    };

    const setEmployee = employee => {
        selectedEmployee = employee;
        searchVersions.EmployeeNo++;
        searchVersions.EmployeeName++;
        $('#EmployeeId').val(employee ? employee.employeeId : '');
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
                            $('#official-business-lookup-status').text('Employee search failed. Type again to retry.');
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
                $('#official-business-lookup-status').text('');
            }).on('select2:clear', () => setEmployee(null));
    };

    const openForm = async id => {
        if (submitting) return;
        const version = ++modalVersion;
        editingId = id || 0;
        detailsReady = !id;
        loading = true;
        form[0].reset();
        if (form.data('validator')) form.validate().resetForm();
        setEmployee(null);
        $('#OfficialBusinessId').val(editingId);
        setFormStatus('');
        $('#official-business-lookup-status').text(id ? 'Loading form...' : '');
        $('#official-business-modal-title').text(id ? 'Edit Official Business' : 'Add Official Business');
        updateControls();
        modalDialog.show();
        try {
            if (id) {
                const record = await getJson(`${endpoint}/${encodeURIComponent(id)}`);
                if (version !== modalVersion) return;
                setEmployee(record);
                // Business timestamps have no offset. Slicing avoids browser timezone conversion.
                $('#FromDate').val(record.from.slice(0, 10));
                $('#FromTime').val(record.from.slice(11, 16));
                $('#ToDate').val(record.to.slice(0, 10));
                $('#ToTime').val(record.to.slice(11, 16));
                detailsReady = true;
                $('#official-business-lookup-status').text('');
            }
        } catch (error) {
            if (version === modalVersion) {
                setFormStatus(error.message || 'The record could not be loaded. Close the form and try again.');
                $('#official-business-lookup-status').text('Close the form and select Edit again to retry.');
            }
        } finally {
            if (version === modalVersion) {
                loading = false;
                updateControls();
            }
        }
    };

    const onFormSubmit = async event => {
        event.preventDefault();
        if (submitting || loading || !detailsReady) return;
        setFormStatus('');
        const data = {
            employeeId: selectedEmployee ? selectedEmployee.employeeId : null,
            fromDate: $('#FromDate').val(), toDate: $('#ToDate').val(),
            fromTime: $('#FromTime').val(), toTime: $('#ToTime').val()
        };
        if (!data.employeeId || !data.fromDate || !data.toDate || !data.fromTime || !data.toTime || !form.valid()) {
            setFormStatus('Select an employee and enter all From and To OB dates and times.');
            return;
        }
        if (`${data.toDate}T${data.toTime}` <= `${data.fromDate}T${data.fromTime}`) {
            setFormStatus('The end date and time must be later than the start date and time.');
            return;
        }
        submitting = true;
        updateControls();
        try {
            const options = {
                url: editingId ? `${endpoint}/${encodeURIComponent(editingId)}` : endpoint, data,
                requesterName: $('#current-user').text(), requestOrigin: 'Official Business', requestSystem: 'SCIC HR Portal'
            };
            const response = await (editingId ? api.put(options) : api.post(options));
            if (!response.ok) throw new Error(await responseError(response));
            toastr.success(editingId ? 'Official Business updated.' : 'Official Business created.');
            submitting = false;
            modalDialog.hide();
            table.ajax.reload(null, false);
        } catch (error) {
            setFormStatus(error.message || 'Official Business could not be saved. Please try again.');
        } finally {
            submitting = false;
            updateControls();
        }
    };

    const onDelete = async record => {
        if (deleting.has(record.officialBusinessId)) return;
        deleting.add(record.officialBusinessId);
        try {
            const confirmation = await Swal.fire({
                title: 'Delete Official Business?', text: 'This removes the record from the active list.',
                icon: 'warning', showCancelButton: true, confirmButtonText: 'Delete', confirmButtonColor: '#b42318',
                customClass: { container: 'official-business-confirmation' }
            });
            if (!confirmation.isConfirmed) return;
            const response = await api.delete({
                url: `${endpoint}/${encodeURIComponent(record.officialBusinessId)}`,
                requesterName: $('#current-user').text(), requestOrigin: 'Official Business', requestSystem: 'SCIC HR Portal'
            });
            if (!response.ok) throw new Error(await responseError(response));
            toastr.success('Official Business deleted.');
            table.ajax.reload(null, false);
        } catch (error) {
            setPageStatus(error.message || 'Official Business could not be deleted. Please try again.');
        } finally {
            deleting.delete(record.officialBusinessId);
        }
    };

    const initializeGrid = () => {
        const text = $.fn.dataTable.render.text();
        // Interpret offset-free Manila wall times without applying the browser's DST rules.
        const date = (value, type) => type === 'display' ? moment.utc(value).format('MMM DD, YYYY') : value;
        const time = (value, type) => type === 'display' ? moment.utc(value).format('h:mm A') : value;
        table = $('#official-business-grid').DataTable({
            serverSide: true, processing: true, searching: true, searchDelay: 300,
            scrollX: true,
            dom: '<"official-business-table-toolbar"lf>rtip',
            pageLength: 10, lengthMenu: [10, 20, 40, 80], order: [[2, 'desc']],
            language: { emptyTable: 'No Official Business records.', zeroRecords: 'No matching Official Business records.' },
            ajax: async (request, callback) => {
                const version = ++pageVersion;
                const order = request.order[0] || { column: 2, dir: 'desc' };
                const columns = ['employeeNo', 'employeeName', 'from', 'from', 'to', 'to'];
                const query = new URLSearchParams({
                    draw: request.draw, start: request.start, length: request.length,
                    search: request.search.value, sortColumn: columns[order.column] || 'from', sortDirection: order.dir
                });
                setPageStatus('Loading records...');
                $('#official-business-retry').prop('hidden', true);
                try {
                    const result = await getJson(`${endpoint}/Page?${query}`);
                    if (version !== pageVersion) return;
                    setPageStatus('');
                    callback(result);
                } catch (error) {
                    if (version !== pageVersion) return;
                    callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    setPageStatus(error.message || 'Records could not be loaded. Please retry.');
                    $('#official-business-retry').prop('hidden', false);
                }
            },
            columns: [
                { data: 'employeeNo', render: text, defaultContent: '' },
                { data: 'employeeName', render: text, defaultContent: '' },
                { data: 'from', render: date }, { data: 'from', render: time },
                { data: 'to', render: date }, { data: 'to', render: time },
                { data: null, orderable: false, searchable: false, className: 'official-business-actions',
                    defaultContent: '<button type="button" class="official-business-edit" title="Edit" aria-label="Edit Official Business"><i class="fas fa-edit" aria-hidden="true"></i></button>' +
                        '<button type="button" class="official-business-delete" title="Delete" aria-label="Delete Official Business"><i class="fas fa-trash border-icon" aria-hidden="true"></i></button>' }
            ],
            drawCallback: function () {
                const grid = this.api();
                const info = grid.page.info();
                if (info.recordsDisplay > 0 && info.start >= info.recordsDisplay && info.page > 0) grid.page('previous').draw('page');
            }
        });
        $('#official-business-grid_filter input').attr('maxlength', 200);
    };

    const attachEvents = () => {
        $(document).on('official-business:imported', () => table.ajax.reload(null, false));
        $('#official-business-add').on('click', () => openForm(0));
        form.on('submit', onFormSubmit);
        $('#official-business-retry').on('click', () => table.ajax.reload(null, false));
        $('#official-business-grid').on('click', '.official-business-edit, .official-business-delete', function () {
            const record = table.row($(this).closest('tr')).data();
            if (!record || deleting.has(record.officialBusinessId)) return;
            if ($(this).hasClass('official-business-edit')) openForm(record.officialBusinessId);
            else onDelete(record);
        });
        modal.on('hide.bs.modal', event => {
            if (submitting) { event.preventDefault(); return; }
            modalVersion++;
            searchVersions.EmployeeNo++;
            searchVersions.EmployeeName++;
        }).on('shown.bs.modal', () => {
            $('.modal-backdrop').last().addClass('official-business-backdrop');
            $('#EmployeeNo').next('.select2').find('.select2-selection').trigger('focus');
        })
            .on('hidden.bs.modal', () => $('#official-business-add').trigger('focus'));
        $(document).on('input.officialBusiness', '.select2-search__field', () => {
            if (activeSearch) searchVersions[activeSearch]++;
        });
    };

    const initialize = () => {
        initializeEmployeeLookup('EmployeeNo');
        initializeEmployeeLookup('EmployeeName');
        attachEvents();
        initializeGrid();
    };

    $(initialize);
})(jQuery);
