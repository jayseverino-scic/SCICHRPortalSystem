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
    const SYSTEM = 'scic-portal';
    const _projects = [];
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
        return columns;
    };

    let initializeGrids = e => {
        initializeGrid();
    }

    let renderDropDowns = async () => {
        await getDropdownData();
        _formHelper.renderDropdown({ name: 'employee-form #Id', valueName: 'id', data: _projects, text: 'description', placeHolder: '-' });
    };

    let getDropdownData = async () => {
        let [projectResp] = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/SGroups`
            })
        ]);

        let [projectComponent] = await Promise.all(
            [
                projectResp.json()
            ]
        );

        _projects = _.map(projectComponent, (s) => {
            return {
                Id : s.id,
                description: s.description
            }
        });

        console.log(_projects)
    }


    $(document).ready(function () {
        initializeModals();
        renderDropDowns();
        initializeGrids();
    });

})(jQuery);