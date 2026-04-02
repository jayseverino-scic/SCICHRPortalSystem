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
        $('#gender-form').on('submit', onFormSubmit);
        $('#add-button').on(CLICK_EVENT, onClickAddModal);

    };

    let onClickAddModal = function () {
        $('#gender-form')[0].reset();
        $('#gender-form').find(':submit').text('Add');
        $('#gender-modal').modal('show');
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
            let currentTabTitle = 'pending user';
            data.isApproved = true;
            if (button == 'add') {
                response = await _apiHelper.post({
                    url: 'Lookups/Gender',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            } else {
                response = await _apiHelper.put({
                    url: 'Lookups/Gender',
                    data: data,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });
                status = 'Created!';
            }

            if (response.ok) {
                $('#gender-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                $('#gender-modal').modal('hide');
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
        let table = $('#pending-user-grid').DataTable({
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
            //buttons: initializeButton("Pending User"),
            ajax: async function (params, success, settings) {
                let gridInfo = $('#pending-user-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/User/Unapproved/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
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
                        let message = `${data.lastName}, ${data.firstName}, ${data.middleName}`;
                        _formHelper.deleteRecord(e, message, SYSTEM);
                    });

                    $('#pending-user-grid tbody').on('click', '.approve-btn', onApproveUser);
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
                data: "userId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },

            {
                title: "Last Name",
                data: "lastName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "First Name",
                data: "firstName",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "User Name",
                data: "username",
                render: (data, type, row) => {
                    return data
                },
            },
            {
                title: "Email",
                data: "email",
                render: (data, type, row) => {
                    return data
                },
            },
            //{
            //    title: "Contact Number",
            //    data: "contactNumber",
            //    render: (data, type, row) => {
            //        return data
            //    },
            //},

        ];
        let lastColumn = {
            data: "moduleId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<button class="btn btn-primary mr-4 approve-btn" data-id="' + full.userId + '" data-endpoint="Authenticated/User/Approved" data-table="pending-user-grid" >Approve</button>';
                buttons += '<button class="btn btn-danger m-1 delete-btn" data-id="' + full.userId + '" data-endpoint="Authenticated/User" data-table="pending-user-grid">Delete</button>';

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

    let onApproveUser = async (e) => {
        e.preventDefault();
        $('#busy-indicator-container').removeClass('d-none');
        let taget = $(e.currentTarget);
        let id = taget.data('id');
        let endpoint = taget.data('endpoint');
        let gridId = taget.data('table');
        let currentTabTitle = 'pending user';

        Swal.fire({
            title: 'Are you sure?',
            text: "You want to approve this user?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, approve it!'
        }).then(async (result) => {
            if (result.value) {
                let response = await _apiHelper.put({
                    url: `${endpoint}/${id}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM
                });

                if (response.ok) {
                    $('#busy-indicator-container').addClass('d-none');
                    let table = $('#' + gridId).DataTable();
                    table.draw();

                    Swal.fire(
                        'Approved!',
                        'User Aprrove',
                        'success'
                    );
                }

            }
        });

        $('#busy-indicator-container').addClass('d-none');

    }

    let initializeModals = e => {
        $('.nav-link').removeClass('d-none');
        $('.nav-link.pending-user').addClass('active');
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