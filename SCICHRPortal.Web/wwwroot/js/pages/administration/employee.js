class UploadDownloadModalHelper {
    constructor(config) {
        this.config = config;
        this.init();
    }

    init() {
        this.setupModalEvents();
    }

    setupModalEvents() {
        $(document).on('click', '.modal-close, .modal-overlay', (e) => {
            this.hideAll();
        });
        $(document).on('click', '.modal-content', (e) => {
            e.stopPropagation();
        });
    }

    show(type, data = {}) {
        this.hideAll();

        switch (type) {
            case 'uploading':
                $('#uploadModal').show();
                if (data.filename) {
                    $('#uploadFileName').text(data.filename);
                }
                break;
            case 'success':
                $('#successModal').show();
                break;
            case 'failed':
                $('#errorModal').show();
                if (data.error) {
                    $('#errorMessage').text(data.error);
                }
                break;
        }
    }

    hideAll() {
        $('.modal').hide();
    }

    resetFileInput() {
        $('#upload-file').val('');
        $(".progress-bar").width('0%');
        $(".progress-bar").html('0%');
    }

    downloadFile() {
        const { xlsxDownloadUri, fileName } = this.config;
        console.log('Downloading file...');
    }
}

(function ($) {
    const _config = {
        pdfButtonEnabled: true,
        fileName: 'biometricsLog',
        xlsxDownloadUri: 'Authenticated/employee/Download',
        pdfButtonNumber: 0,
        tableId: 'biometrics-log-grid'
    };
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    const _uploadDownloadModalHelper = new UploadDownloadModalHelper(_config);
    const SYSTEM = 'grading';

    let _department = [];
    let _project = [];
    let dataTable = null;
    let employeeRequestVersion = 0;

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#employee-form').on('submit', onFormSubmit);
        $('#import').on('click', onSubmitUploadForm);
        $('#dbfilter').on(CLICK_EVENT, onClickDbFilter);
        $('#employee-project-filter').on('change', function () {
            if (dataTable) dataTable.ajax.reload(null, true);
        });
    };

    let onClickAddModal = function () {
        $('#employee-form')[0].reset();
        $('#employee-form').find(':submit').text('Add');
        $('#employee-modal').modal('show');
    }
    let onClickDbFilter = async e => {
        e.preventDefault();

        let response = await _apiHelper.get({
            url: `Authenticated/Employee/ImportDb`,
        });

        if (response.ok) {
            let json = await response.json();
            let dataRetrieved = json.data;

            // Reload existing DataTable
            if ($.fn.DataTable.isDataTable('#employee-grid')) {
                $('#employee-grid').DataTable().ajax.reload(null, false);
            } else {
                initializeGrid();
            }
        }
    };
    let onSubmitUploadForm = async e => {
        e.preventDefault();
        let fileInput = $('#upload-file');

        if (fileInput[0].files.length == 0) {
            Swal.fire(
                'No File!',
                'Please Select File',
                'error'
            );
            return;
        }

        let fileName = fileInput.val().replace(/C:\\fakepath\\/i, '');
        _uploadDownloadModalHelper.show('uploading', { filename: fileName });

        var request = _apiHelper.ajaxRequest('POST', {
            url: 'Authenticated/Employee/Import',
            data: $('#upload-file')[0].files[0],
            xhr: function () {
                let xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = ((evt.loaded / evt.total) * 100);
                        $(".progress-bar").width(percentComplete + '%');
                        $(".progress-bar").html(percentComplete + '%');
                    }
                }, false);
                return xhr;
            },
            beforeSend: function () {
                $(".progress-bar").width('0%');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                _uploadDownloadModalHelper.show('failed', { error: XMLHttpRequest.responseText });
            },
            success: function (response) {
                _uploadDownloadModalHelper.resetFileInput();
                initializeGrid(response);
                _uploadDownloadModalHelper.show('success');
            }
        });
    };
    let onFormSubmit = async event => {
        event.preventDefault();

        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();
        console.log(button);
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            data.DepartmentId = data.DepartmentId ? data.DepartmentId : null;
            data.PositionId = data.PositionId ? data.PositionId : null;
            let currentTabTitle = 'Employee';
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/Employee',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/Employee',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#employee-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#employee-modal').modal('hide');
            }
            else if (response.status == 403) {
                noAccessAlert();
            }
            else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }

            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);
    }

    let initializeGrid = async () => {
        if (dataTable) {
            dataTable.ajax.reload(null, false);
            return;
        }
        let columns = await getColumns();
        dataTable = $('#employee-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            // This page has no export configuration; do not instantiate missing default button types.
            buttons: [],
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            ajax: async function (params, success, settings) {
                const version = ++employeeRequestVersion;
                const empty = { draw: params.draw, recordsFiltered: 0, recordsTotal: 0, data: [] };
                const query = new URLSearchParams({
                    pageNumber: Math.floor(params.start / params.length) + 1,
                    pageSize: params.length,
                    searchKeyword: params.search.value || '',
                    projectId: $('#employee-project-filter').val() || '0'
                });
                $('#employee-page-status').text('Loading employees...');
                try {
                    const response = await _apiHelper.get({ url: `Authenticated/Employee/Filter?${query}` });
                    if (version !== employeeRequestVersion) return;
                    if (!response.ok) throw new Error('Employees unavailable');
                    const json = await response.json();
                    if (version !== employeeRequestVersion) return;
                    if (!json || !Array.isArray(json.data) || !Number.isInteger(json.total) || json.total < 0 ||
                        json.data.length > params.length) throw new Error('Invalid employee page');
                    if (params.start > 0 && params.start >= json.total) {
                        dataTable.page(Math.max(0, Math.ceil(json.total / params.length) - 1)).draw('page');
                        return;
                    }
                    success({
                        draw: params.draw,
                        recordsFiltered: json.total,
                        recordsTotal: json.total,
                        data: json.data
                    });
                    $('#employee-page-status').text('');
                } catch (_) {
                    if (version !== employeeRequestVersion) return;
                    success(empty);
                    $('#employee-page-status').text('Could not load employees. Change the project or search to try again.');
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
        $('#employee-grid').on('click', '.icon-delete', function (event) {
            const data = dataTable.row($(this).closest('tr')).data();
            if (data) _formHelper.deleteRecord(event, `${data.lastName}, ${data.firstName}`, SYSTEM);
        });
        $('#employee-grid').on('click', '.icon-edit', function (event) {
            event.preventDefault();
            const data = dataTable.row($(this).closest('tr')).data();
            if (!data) return;
            populateForm($('#employee-form'), data);
            $('#employee-modal').modal('show');
        });
    }

    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "employeeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Employee No',
                data: "employeeNo",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Last Name",
                data: "lastName",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Contact Number",
                data: "contactNumber",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Project",
                data: "project.name",
                defaultContent: '-',
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Department",
                data: "department.departmentName",
                defaultContent: '-',
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
        ];
        let lastColumn = {
            data: "employeeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.employeeId + '" data-endpoint="Authenticated/Employee" data-table="employee-grid"><i class="fas fa-edit"></i></a>';
                if (system.toLocaleLowerCase() == SYSTEM)
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.employeeId + '" data-endpoint="Authenticated/Employee" data-table="employee-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let initializeModals = e => {
        $('#employee-modal').modal({ backdrop: 'static', keyboard: false, show: false });
        $('#master-data-menu').removeClass('d-none');
        $('#master-data-menu .employee').addClass('active');
    }


    let initializeGrids = e => {
        initializeGrid();
    }

    let renderDropDowns = async () => {
        await getDropdownData();
        _formHelper.renderDropdown({ name: 'employee-form #DepartmentId', valueName: 'departmentId', data: _department, text: 'description', placeHolder: '-' });
        _formHelper.renderDropdown({ name: 'employee-form #ProjectId', valueName: 'id', data: _project, text: 'name', placeHolder: '-' });
    };

    let getDropdownData = async () => {
        const results = await Promise.allSettled(['Department', 'Project'].map(async name => {
            const response = await _apiHelper.get({ url: `Authenticated/${name}` });
            if (!response.ok) throw new Error('Options unavailable');
            const items = await response.json();
            if (!Array.isArray(items)) throw new Error('Invalid options');
            return items;
        }));
        if (results[0].status === 'fulfilled' && results[0].value.every(item => item &&
            Number.isInteger(item.departmentId) && typeof item.departmentName === 'string')) {
            _department = results[0].value.map(item => ({ departmentId: item.departmentId, description: item.departmentName }));
        } else {
            toastr.error('Departments could not be loaded. Refresh before editing employees.');
        }

        const filter = $('#employee-project-filter');
        const selected = filter.val() || '0';
        filter.empty().append($('<option>').val('0').text('All Projects'));
        if (results[1].status === 'fulfilled' && results[1].value.every(item => item &&
            Number.isInteger(item.id) && item.id > 0 && typeof item.name === 'string' && item.name.trim())) {
            _project = results[1].value.map(item => ({ id: item.id, name: item.name }));
            for (const project of _project) filter.append($('<option>').val(project.id).text(project.name));
            filter.val(_project.some(project => String(project.id) === selected) ? selected : '0');
            $('#employee-project-filter-status').text(_project.length ? '' : 'No projects available. All Projects is still available.');
        } else {
            $('#employee-project-filter-status').text('Projects could not be loaded. Refresh the page to try again.');
        }
        filter.prop('disabled', false).trigger('change.select2');
    }


    $(document).ready(function () {
        initializeModals();
        renderDropDowns();
        initializeGrids();
        attachEvents();
    });

})(jQuery);
