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

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#employee-form').on('submit', onFormSubmit);
        $('#import').on('click', onSubmitUploadForm);
        $('#dbfilter').on(CLICK_EVENT, onClickDbFilter);
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
        let columns = await getColumns();
        let table = $('#employee-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            ajax: async function (params, success, settings) {
                let gridInfo = $('#employee-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Employee/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, `${data.lastName}, ${data.firstName}`, SYSTEM);

                    });
                    $('#employee-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#employee-form');
                        console.log(data);
                        populateForm(form, data);
                        $('#employee-modal').modal('show');
                    });


                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
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
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Department",
                data: "department.departmentName",
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
        $('#employee-modal').modal({ backdrop: 'static', keyboard: false });
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
        let [departmentResp, projectResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/Department`
            }),
            _apiHelper.get({
                url: `Authenticated/Project`
            }),
        ]);

        let [departmentComponent, projectComponent] = await Promise.all(
            [
                departmentResp.json(),
                projectResp.json(),
            ]
        );

        _department = _.map(departmentComponent, (s) => {
            return {
                departmentId : s.departmentId,
                description: s.departmentName
            }
        });
        _project = _.map(projectComponent, (s) => {
            return {
                id: s.id,
                name: s.name
            }
        });

        console.log(_project)
    }


    $(document).ready(function () {
        initializeModals();
        renderDropDowns();
        initializeGrids();
        attachEvents();
    });

})(jQuery);