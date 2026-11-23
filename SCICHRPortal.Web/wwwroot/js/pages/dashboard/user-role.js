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
    const SYSTEM = 'HR Portal';
    let submitting = false;
    const roleModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('user-role-modal'), { backdrop: 'static', keyboard: false });
    let attachEvents = () => {
        $('#user-role-form').on('submit', onFormSubmit);
        $('#add-button').on(CLICK_EVENT, onClickAddModal);

    };

    let onClickAddModal = function () {
        if (submitting) return;
        $('#user-role-form')[0].reset();
        $('#user-role-form-status').text('');
        if ($('#user-role-form').data('validator')) $('#user-role-form').validate().resetForm();
        $('#RoleId').val('0');
        $('#user-role-modal-title').text('New Role');
        $('#user-role-form').find(':submit').text('Add');
        roleModal.show();
    }
    let onFormSubmit = async event => {
        event.preventDefault();

        const form = $(event.target);
        form.validate();
        if (submitting || !form.valid()) return;
        const button = form.find(':submit');
        const buttonText = button.text();
        const roleId = Number($('#RoleId').val());
        submitting = true;
        form.find('input, button').prop('disabled', true);
        button.text('Saving...');
        $('#user-role-form-status').text('');
        $('#busy-indicator-container').removeClass('d-none');
        try {
            const options = {
                url: 'Authenticated/Role',
                data: { roleId: roleId, name: $('#Name').val().trim() },
                requestOrigin: 'User Roles',
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            };
            const response = roleId === 0 ? await _apiHelper.post(options) : await _apiHelper.put(options);
            if (response.ok) {
                $('#user-role-grid').DataTable().ajax.reload(null, false);
                toastr.success('Role saved.');
                roleModal.hide();
            } else if (response.status === 403 || response.status === 401) {
                $('#user-role-form-status').text(response.status === 403
                    ? 'Administrator access is required. Your account may no longer have access.'
                    : 'Your session has expired. Sign in again.');
            } else {
                const body = await response.text();
                let message = 'The role could not be saved. Please try again.';
                try {
                    const error = JSON.parse(body);
                    message = error.message || error.Message || (error.errors && Object.values(error.errors).flat().join(' ')) || message;
                } catch (_) { /* Keep the safe fallback for non-JSON responses. */ }
                $('#user-role-form-status').text(message);
            }
        } catch (_) {
            $('#user-role-form-status').text('The role could not be saved. Check your connection and try again.');
        } finally {
            submitting = false;
            form.find('input, button').prop('disabled', false);
            button.text(buttonText);
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
                    url: `Authenticated/Role/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${encodeURIComponent(searchKeyword)}`,
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
            dom: '<"users-table-toolbar"lf>rtip',
        });
        $('#user-role-grid').on('click.userRoles', '.icon-delete', function (event) {
            const data = table.row($(this).closest('tr')).data();
            _formHelper.deleteRecord(event, data.name, SYSTEM);
        });
        $('#user-role-grid').on('click.userRoles', '.icon-edit', function () {
            const data = table.row($(this).closest('tr')).data();
            populateForm($('#user-role-form'), data);
            roleModal.show();
        });
    }

    let populateForm = (form, data) => {
        $('#user-role-form-status').text('');
        if (form.data('validator')) form.validate().resetForm();
        $('#user-role-modal-title').text('Edit Role');
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
                title: 'Role ID',
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
                    return $.fn.dataTable.render.text().display(data ?? '')
                },
            },
            {
                title: "Description",
                data: "description",
                render: (data, type, row) => {
                    return $.fn.dataTable.render.text().display(data ?? '')
                },
            },

        ];
        let lastColumn = {
            title: 'Actions',
            data: "userRoleId",
            width: "96px",
            render: function (data, type, full) {
                let buttons = '<button type="button" class="users-action users-edit icon-edit" title="Edit role" aria-label="Edit role" data-id="' + Number(full.roleId) + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-edit" aria-hidden="true"></i></button>';
                buttons += '<button type="button" class="users-action users-delete icon-delete" title="Delete role" aria-label="Delete role" data-id="' + Number(full.roleId) + '" data-endpoint="Authenticated/Role" data-table="user-role-grid"><i class="fas fa-trash" aria-hidden="true"></i></button>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center user-role-actions'
        };
        columns.push(lastColumn);
        return columns;
    };

    let initializeModals = e => {
        $('#user-role-modal-title').text('User Role');
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
