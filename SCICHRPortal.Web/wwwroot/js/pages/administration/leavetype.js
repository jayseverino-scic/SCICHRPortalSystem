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
        $('#leavetype-form').on('submit', onFormSubmit);
    };

    let onClickAddModal = function () {
        $('#leavetype-form')[0].reset();
        $('#leavetype-form').find(':submit').text('Add');
        $('#leavetype-modal').modal('show');
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
            data.IsPaid = $("#isPaid").is(':checked');
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/LeaveType',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/LeaveType',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#leavetype-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#leavetype-modal').modal('hide');
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
        $(form).find('#isPaid').prop("checked", data.isPaid);
        _formHelper.populateForm(form, data);
    }

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#leavetype-grid').DataTable({
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
                let gridInfo = $('#leavetype-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/LeaveType/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });


                    $('#leavetype-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#leavetype-form');
                        populateForm(form, data);
                        $('#leavetype-modal').modal('show');
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, data.typeDescription, SYSTEM);
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
                data: "leaveTypeId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Leave Type Description',
                data: "leaveDescription",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Allowed Days",
                data: "allowedDays",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Is Paid?",
                data: "isPaid",
                class: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isPaid" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false
            },

        ];
        let lastColumn = {
            data: "leaveTypeId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.leaveTypeId + '" data-endpoint="Authenticated/LeaveType" data-table="leavetype-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.leaveTypeId + '" data-endpoint="Authenticated/LeaveType" data-table="leavetype-grid"><i class="fas fa-trash border-icon"></i></a>';

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
        $('#leavetype-modal').modal({ backdrop: 'static', keyboard: false });
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