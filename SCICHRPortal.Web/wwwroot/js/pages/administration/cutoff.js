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
    const SYSTEM = 'library';

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#cutoff-form').on('submit', onFormSubmit);
    };

    let onClickAddModal = function () {
        $('#cutoff-form')[0].reset();
        $('#cutoff-form').find(':submit').text('Add');
        $('#cutoff-modal').modal('show');
    }

    let onFormSubmit = async event => {
        event.preventDefault();

        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            let currentTabTitle = $('.tab-pane.active .title').text();
            data.IsActive = $('#cutoff-form #IsActive').is(':checked');
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/CutOff',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/CutOff',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#cutoff-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#cutoff-modal').modal('hide');
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
        let startDate = moment(data.startDate).format('YYYY-MM-DD');
        $(form).find('#StartDate').val(startDate);
        let endDate = moment(data.endDate).format('YYYY-MM-DD');
        $(form).find('#EndDate').val(endDate);
    }

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#cutoff-grid').DataTable({
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
                let gridInfo = $('#cutoff-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/CutOff/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });


                    $('#cutoff-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#cutoff-form');
                        populateForm(form, data);
                        $('#cutoff-modal').modal('show');
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, data.startDate + ' ' + data.endDate,SYSTEM);
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
                data: "cutOffId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Start Date',
                data: "startDate",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    if (_dateHelper.formatShortLocalDate(data) == '01/01/0001')
                        return '-';
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "End Date",
                data: "endDate",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    if (_dateHelper.formatShortLocalDate(data) == '01/01/0001')
                        return '-';
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Is Active?",
                data: "isActive",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "cutOffId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.cutOffId + '" data-endpoint="Authenticated/CutOff" data-table="cutoff-grid"><i class="fas fa-edit"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.cutOffId + '" data-endpoint="Authenticated/CutOff" data-table="cutoff-grid"><i class="fas fa-trash border-icon"></i></a>';
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
        $('#cutoff-form #StartDate').attr('value', moment().format('yyyy-MM-DD'));
        $('#cutoff-form #EndDate').attr('value', moment().format('yyyy-MM-DD'));
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