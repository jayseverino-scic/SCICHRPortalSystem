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
    let SYSTEM = system.toLowerCase();
    let attachEvents = () => {
        console.log(SYSTEM);
    };


    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#audit-grid').DataTable({
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
                let gridInfo = $('#audit-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Audit/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&system=${SYSTEM}`,
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
                title: 'Id',
                data: "id",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "User Name",
                data: "userName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Action",
                data: "type",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Page",
                data: "requestOrigin",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Date",
                data: "dateTime",
                render: (data, type, row) => {
                    return _dateHelper.formatLocalDateTime(data)
                },
            },

        ];
        return columns;
    };

    let initializeModals = e => {
        $('.nav-link').removeClass('d-none');
        $('.nav-link.section').addClass('active');
    }

    let renderDropDowns = async () => {
        await getDropdownData();
        _formHelper.renderDropdown({ name: 'grade', valueName: 'gradeLevelId', data: _gradeLevel, text: 'description', placeHolder: '-' });
    };

    let getDropdownData = async () => {
        let [gradeLevelResp,] = await Promise.all([
            _apiHelper.get({
                url: `Lookups/GradeLevel`
            }),

        ]);

        let [gradeLevel] = await Promise.all(
            [
                gradeLevelResp.json(),
            ]
        );
        _gradeLevel = gradeLevel;

    }
  

    let initializeGrids = e => {
        //if ($('#system-input').val())
        //    SYSTEM = $('#system-input').val();

        initializeGrid();
        renderDropDowns();
    }


    $(document).ready(function () {
     
        initializeGrids();
        initializeModals();
        attachEvents();
    });

})(jQuery);