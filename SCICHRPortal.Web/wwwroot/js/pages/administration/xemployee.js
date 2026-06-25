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
    const SYSTEM = 'scicportal';

    let _department = [];
    let _position = [];

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
                    url: `Authenticated/XEmployee/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
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
                data: "id",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Employee No',
                data: "employee_code",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Last Name",
                data: "last_Name",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "first_Name",
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
                data: "mobile",
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
                data: "department.name",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
        ];
        return columns;
    };


    let initializeGrids = e => {
        initializeGrid();
    }

    let renderDropDowns = async () => {
        await getDropdownData();
        _formHelper.renderDropdown({ name: 'employee-form #Id', valueName: 'id', data: _department, text: 'name', placeHolder: '-' });
        _formHelper.renderDropdown({ name: 'employee-form #PositionId', valueName: 'positionId', data: _position, text: 'positionName', placeHolder: '-' });
    };

    let getDropdownData = async () => {
        let [departmentResp, positionResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/XDepartment`
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
                id : s.id,
                name: s.name
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
        renderDropDowns();
        initializeGrids();
    });

})(jQuery);