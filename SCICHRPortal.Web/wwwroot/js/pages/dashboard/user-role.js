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
    const SYSTEM = 'enrolment';
    let attachEvents = () => {
        $('#user-role-form').on('submit', onFormSubmit);
        $('#add-button').on(CLICK_EVENT, onClickAddModal);

    };

    let onClickAddModal = function () {
        $('#user-role-form')[0].reset();
        $('#user-role-form').find(':submit').text('Add');
        $('#user-role-modal').modal('show');
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
            let currentTabTitle = 'User Role';
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Authenticated/Role',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Authenticated/Role',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#user-role-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#user-role-modal').modal('hide');
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

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#user-role-grid').DataTable({
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
            "createdRow": function (row, data, dataIndex) {
                if (data.isTodayAnnouncement) {
                    $(row).addClass('red-background');
                }
            },
            ajax: async function (params, success, settings) {
                let gridInfo = $('#user-role-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Role/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
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
                        _formHelper.deleteRecord(e, data.title, SYSTEM);
                    });

                    $('#user-role-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#user-role-form');
                        populateForm(form, data);
                        $('#user-role-modal').modal('show');
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

    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);   
    }


    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "roleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'No.',
                data: "roleId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Name",
                data: "name",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return data
                },
            },

        ];
        let lastColumn = {
            data: "userRoleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.roleId + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-eye"></i></a>';
                buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.roleId + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-trash border-icon"></i></a>';

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
        $('#user-role-modal').modal({ backdrop: 'static', keyboard: false });
        $('#master-data-menu').removeClass('d-none');
        $('#master-data-menu .announcement').addClass('active');

        $('#announcement-form #createdAt').attr('value', moment().format('yyyy-MM-DD'));
    }


    let initializeGrids = e => {
        initializeGrid();
    }


    $(document).ready(function () {
        initializeModals();
        initializeGrids();
        attachEvents();
    });

})(jQuery);