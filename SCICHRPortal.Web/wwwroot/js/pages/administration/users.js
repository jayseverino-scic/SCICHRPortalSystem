(function ($) {
    'use strict';
    const api = new ApiHelper();
    const endpoint = 'Authenticated/User';
    const form = $('#user-form');
    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('user-modal'), { backdrop: 'static', keyboard: false });
    const busyUsers = new Set();
    let table;
    let editingId = 0;
    let modalVersion = 0;
    let pageVersion = 0;
    let submitting = false;
    let loading = false;
    let ready = false;

    const metadata = () => ({ requesterName: $('#current-user').text(), requestOrigin: 'Users', requestSystem: 'HR Portal' });
    const errorMessage = async response => {
        if (response.status === 403) return 'Administrator access is required. Your account may no longer have access.';
        if (response.status === 401) return 'Your session has expired. Sign in again.';
        if (response.status >= 500) return 'The server could not complete the request. Please try again.';
        const body = await response.text();
        try {
            const parsed = JSON.parse(body);
            return parsed.message || parsed.Message || (parsed.errors && Object.values(parsed.errors).flat().join(' ')) || 'The request could not be completed.';
        } catch (_) { return body || 'The request could not be completed.'; }
    };
    const read = async url => {
        const response = await api.get({ url });
        if (!response.ok) throw new Error(await errorMessage(response));
        return response.json();
    };
    const updateControls = () => {
        form.find('input, select').prop('disabled', loading || submitting);
        $('#user-save').prop('disabled', loading || submitting || !ready).text(submitting ? 'Saving...' : editingId ? 'Save changes' : 'Create user');
        $('#user-cancel').prop('disabled', submitting);
    };
    const showResult = result => {
        if (result.notificationSent === false) toastr.warning(result.message);
        else toastr.success(result.notificationSent ? 'Account saved. The password email was sent.' : 'Account updated.');
    };
    const reload = () => table.ajax.reload(() => {
        if (table.page.info().page > 0 && table.rows({ page: 'current' }).count() === 0) table.page('previous').draw('page');
    }, false);

    const openForm = async id => {
        if (submitting || busyUsers.has(id)) return;
        const version = ++modalVersion;
        editingId = id || 0;
        loading = true;
        ready = false;
        form[0].reset();
        if (form.data('validator')) form.validate().resetForm();
        $('#UserId').val(editingId);
        $('#RoleIds').empty();
        $('#user-form-status').text('Loading form...');
        $('#user-modal-title').text(id ? 'Edit user' : 'New user');
        $('#users-create-help').prop('hidden', !!id);
        updateControls();
        modal.show();
        try {
            const [roles, user] = await Promise.all([read(`${endpoint}/Roles`), id ? read(`${endpoint}/${id}`) : Promise.resolve(null)]);
            if (version !== modalVersion) return;
            for (const role of roles) $('#RoleIds').append(new Option(role.name, String(role.roleId)));
            if (user) {
                for (const field of ['FirstName', 'MiddleName', 'LastName', 'Email', 'ContactNumber']) {
                    $('#' + field).val(user[field[0].toLowerCase() + field.slice(1)] || '');
                }
                $('#RoleIds').val(user.roles.map(role => String(role.roleId)));
            }
            if (!roles.length) throw new Error('No roles are available. Configure a role before creating an account.');
            ready = true;
            $('#user-form-status').text('');
        } catch (error) {
            if (version === modalVersion) $('#user-form-status').text(error.message || 'The form could not be loaded. Close it and try again.');
        } finally {
            if (version === modalVersion) {
                loading = false;
                updateControls();
                if (ready) $('#FirstName').trigger('focus');
            }
        }
    };

    const onFormSubmit = async event => {
        event.preventDefault();
        if (submitting || loading || !ready || !form.valid()) return;
        const roleIds = ($('#RoleIds').val() || []).map(Number);
        if (!roleIds.length) { $('#user-form-status').text('Select at least one role.'); return; }
        const data = {
            firstName: $('#FirstName').val().trim(), middleName: $('#MiddleName').val().trim() || null,
            lastName: $('#LastName').val().trim(), email: $('#Email').val().trim(),
            contactNumber: $('#ContactNumber').val().trim() || null, roleIds
        };
        submitting = true;
        updateControls();
        $('#user-form-status').text('');
        try {
            const options = { url: editingId ? `${endpoint}/${editingId}` : `${endpoint}/Register`, data, ...metadata() };
            const response = editingId ? await api.put(options) : await api.post(options);
            if (!response.ok) throw new Error(await errorMessage(response));
            showResult(await response.json());
            submitting = false;
            modal.hide();
            reload();
        } catch (error) { $('#user-form-status').text(error.message || 'The account could not be saved. Please try again.'); }
        finally { submitting = false; updateControls(); }
    };

    const onAction = async event => {
        const button = $(event.currentTarget);
        const id = Number(button.data('id'));
        const action = button.data('action');
        if (busyUsers.has(id) || submitting) return;
        if (action === 'edit') { openForm(id); return; }
        const descriptions = { approve: 'Approve this account and email a new password?', activate: 'Activate this account? Its password will stay the same.',
            deactivate: 'Deactivate this account?', reset: 'Generate and email a new password? Its current password will stop working.', delete: 'Delete this account? The employee record will remain.' };
        busyUsers.add(id);
        button.prop('disabled', true);
        try {
            const confirmation = await Swal.fire({ title: 'Confirm action', text: descriptions[action], icon: 'warning', showCancelButton: true, confirmButtonText: 'Continue' });
            if (!(confirmation.isConfirmed || confirmation.value === true)) return;
            let url = `${endpoint}/${id}`;
            if (action === 'approve') url = `${endpoint}/Approved/${id}`;
            if (action === 'activate' || action === 'deactivate') url += `/Status/${action === 'activate'}`;
            if (action === 'reset') url += '/ResetPassword';
            const options = { url, ...metadata() };
            const response = action === 'delete' ? await api.delete(options) : action === 'reset' ? await api.post(options) : await api.put(options);
            if (!response.ok) throw new Error(await errorMessage(response));
            showResult(await response.json());
            reload();
        } catch (error) { $('#users-status').text(error.message || 'The account could not be updated. Please try again.'); }
        finally { busyUsers.delete(id); button.prop('disabled', false); }
    };

    const initializeGrid = () => {
        const text = $.fn.dataTable.render.text();
        table = $('#users-grid').DataTable({
            serverSide: true, processing: true, ordering: false, searchDelay: 350, pageLength: 10,
            lengthMenu: [5, 10, 20, 40, 80],
            scrollX: true,
            dom: '<"users-table-toolbar"lf>rtip',
            language: { emptyTable: 'No users found.', zeroRecords: 'No users match these filters.' },
            ajax: async (parameters, success) => {
                const version = ++pageVersion;
                $('#users-status').text('Loading users...');
                $('#users-retry').prop('hidden', true);
                try {
                    const query = new URLSearchParams({ draw: parameters.draw, start: parameters.start, length: parameters.length,
                        search: parameters.search.value, approval: $('#users-approval').val(), state: $('#users-state').val() });
                    const result = await read(`${endpoint}/Page?${query}`);
                    if (version !== pageVersion) return;
                    success(result);
                    $('#users-status').text('');
                } catch (error) {
                    if (version !== pageVersion) return;
                    success({ draw: parameters.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    $('#users-status').text(error.message || 'Users could not be loaded. Please try again.');
                    $('#users-retry').prop('hidden', false);
                }
            },
            columns: [
                { data: null, render: (_, type, user) => text.display([user.lastName, user.firstName, user.middleName].filter(Boolean).join(', ')) },
                { data: 'email', render: text }, { data: 'contactNumber', defaultContent: '', render: text },
                { data: 'roles', render: roles => text.display(roles.map(role => role.name).join(', ')) },
                { data: 'isApproved', render: (value, type) => type === 'display'
                    ? `<span class="users-badge ${value ? 'users-badge-success' : 'users-badge-pending'}">${value ? 'Approved' : 'Pending'}</span>`
                    : value ? 'Approved' : 'Pending' },
                { data: null, render: (_, type, user) => {
                    const label = user.locked ? 'Locked' : user.active ? 'Active' : 'Inactive';
                    return type === 'display' ? `<span class="users-badge ${user.locked ? 'users-badge-danger' : user.active ? 'users-badge-success' : 'users-badge-muted'}">${label}</span>` : label;
                } },
                { data: null, searchable: false, render: (_, type, user) => {
                    if (type !== 'display') return '';
                    const id = Number(user.userId);
                    if (!Number.isSafeInteger(id) || id <= 0) return '';
                    const icons = { edit: 'fa-edit', approve: 'fa-check', activate: 'fa-toggle-on', deactivate: 'fa-toggle-off', reset: 'fa-key', delete: 'fa-trash' };
                    const button = (action, label) => `<button type="button" class="users-action users-${action}" data-action="${action}" data-id="${id}" title="${label}" aria-label="${label}"><i class="fas ${icons[action]}" aria-hidden="true"></i></button>`;
                    return '<div class="users-actions">' + button('edit', 'Edit') + (!user.isApproved ? button('approve', 'Approve') : '') +
                        button(user.active ? 'deactivate' : 'activate', user.active ? 'Deactivate' : 'Activate') +
                        (user.isApproved && user.active ? button('reset', 'Reset password') : '') + button('delete', 'Delete') + '</div>';
                } }
            ]
        });
    };

    $(document).ready(() => {
        const approval = new URLSearchParams(window.location.search).get('approval');
        if (approval === 'pending' || approval === 'approved') $('#users-approval').val(approval);
        initializeGrid();
        $('#users-add').on('click.users', () => openForm(0));
        form.on('submit.users', onFormSubmit);
        $('#users-grid').on('click.users', 'button[data-action]', onAction);
        $('#users-approval, #users-state').on('change.users', () => table.ajax.reload(null, true));
        $('#users-retry').on('click.users', () => table.ajax.reload(null, false));
        $('#users-reset-filters').on('click.users', () => { $('#users-approval, #users-state').val(''); table.search('').ajax.reload(null, true); });
        $('#user-modal').on('hide.bs.modal', event => { if (submitting) event.preventDefault(); else { modalVersion++; ready = false; } });
    });
})(jQuery);
