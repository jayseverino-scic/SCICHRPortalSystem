(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    const SYSTEM = 'grading';

    let _department = [];
    let _position = [];

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#employee-form').on('submit', onFormSubmit);

    };

    let onClickAddModal = function () {
        $('#employee-form')[0].reset();
        $('#employee-form').find(':submit').text('Add');
        $('#employee-modal').modal('show');
    }

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
                title: "Position",
                data: "position.positionName",
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
        _formHelper.renderDropdown({ name: 'employee-form #PositionId', valueName: 'positionId', data: _position, text: 'positionName', placeHolder: '-' });
    };

    let getDropdownData = async () => {
        let [departmentResp, positionResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/Department`
            }),
            _apiHelper.get({
                url: `Authenticated/Position`
            }),
        ]);

        let [departmentComponent, positionComponent] = await Promise.all(
            [
                departmentResp.json(),
                positionResp.json(),
            ]
        );

        _department = _.map(departmentComponent, (s) => {
            return {
                departmentId : s.departmentId,
                description: s.departmentName
            }
        });
        _position = _.map(positionComponent, (s) => {
            return {
                positionId: s.positionId,
                positionName: s.positionName
            }
        });

        console.log(_department)
    }


    $(document).ready(function () {
        initializeModals();
        renderDropDowns();
        initializeGrids();
        attachEvents();
    });

})(jQuery);