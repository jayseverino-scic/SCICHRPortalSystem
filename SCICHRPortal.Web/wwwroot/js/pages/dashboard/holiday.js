(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load';
    const SYSTEM = 'library';

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#holiday-form').on('submit', onFormSubmit);
    };

    let onClickAddModal = function () {
        $('#holiday-form')[0].reset();
        $('#holiday-form').find(':submit').text('Add');
        $('#holiday-modal').modal('show');
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
            let currentTabTitle = $('.tab-pane.active .title').text();
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/Holiday',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text()
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/Holiday',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text()
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#holiday-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#holiday-modal').modal('hide');
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
        $('#HolidayDate').val(data.holidayDate);
        _formHelper.populateForm(form, data);
    }

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#holiday-grid').DataTable({
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
                let gridInfo = $('#holiday-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Holiday/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });


                    $('#holiday-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#holiday-form');
                        populateForm(form, data);
                        let date = moment(data.holidayDate);
                        form.find('#HolidayDate').val(date.format('yyyy-MM-DD'))
                        $('#holiday-modal').modal('show');
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, data.holidayName, SYSTEM);
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
                data: "holidayId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Holiday Name',
                data: "holidayName",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Holiday Type",
                data: "holidayType",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Holiday Date",
                data: "holidayDate",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },

        ];
        let lastColumn = {
            data: "holidayId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.holidayId + '" data-endpoint="Authenticated/Holiday" data-table="holiday-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.holidayId + '" data-endpoint="Authenticated/Holiday" data-table="holiday-grid"><i class="fas fa-trash border-icon"></i></a>';

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
        $('#enrollment-modal').modal({ backdrop: 'static', keyboard: false });
        $('#reasonModal').modal({ backdrop: 'static', keyboard: false });
        $('#viewApprovalRegister').modal({ backdrop: 'static', keyboard: false });
    }

    let initializeGrids = e => {
        initializeGrid();
    }


    $(document).ready(function () {
        initializeGrids();
        attachEvents();
        initializeModals();
    });

})(jQuery);