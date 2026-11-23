(function ($) {
    'use strict';
    const api = new ApiHelper();
    const form = $('#user-detail-form');
    let submitting = false;
    let ready = false;
    let savedProfile = null;
    const fields = ['FirstName', 'MiddleName', 'LastName', 'Email', 'ContactNumber'];
    const populate = user => fields.forEach(field => form.find('#' + field).val(user[field[0].toLowerCase() + field.slice(1)] || ''));
    const setEditing = editing => {
        form.find('input').prop('disabled', !editing);
        $('#edit-user-detail').prop('disabled', editing || !ready);
        $('#button-container').toggleClass('d-none', !editing);
    };
    const errorMessage = async response => {
        if (response.status === 403) return 'You can only update your own profile.';
        if (response.status >= 500) return 'The server could not save your profile. Please try again.';
        const body = await response.text();
        try { const error = JSON.parse(body); return error.message || (error.errors && Object.values(error.errors).flat().join(' ')) || 'Profile update failed.'; }
        catch (_) { return body || 'Profile update failed.'; }
    };
    const loadProfile = async () => {
        ready = false;
        setEditing(false);
        $('#profile-status').text('Loading profile...');
        $('#profile-retry').prop('hidden', true);
        $('#busy-indicator-container').removeClass('d-none');
        try {
            const response = await api.get({ url: 'Authenticated/User/GetAuthenticatedAsync' });
            if (!response.ok) throw new Error('Your profile could not be loaded. Please try again.');
            savedProfile = await response.json();
            populate(savedProfile);
            ready = true;
            $('#edit-user-detail').prop('disabled', false);
            $('#profile-status').text('');
        } catch (error) {
            $('#profile-status').text(error.message || 'Your profile could not be loaded. Please try again.');
            $('#profile-retry').prop('hidden', false);
        } finally { $('#busy-indicator-container').addClass('d-none'); }
    };
    $(document).ready(() => {
        $('#edit-user-detail').on('click.profile', function () {
            if (!ready || submitting) return;
            $('#profile-form-status').text('');
            setEditing(true);
            $('#FirstName').trigger('focus');
        });
        $('#profile-retry').on('click.profile', loadProfile);
        $('#profile-cancel').on('click.profile', () => {
            if (submitting || !savedProfile) return;
            populate(savedProfile);
            $('#profile-form-status').text('');
            if (form.data('validator')) form.validate().resetForm();
            setEditing(false);
            $('#edit-user-detail').trigger('focus');
        });
        form.on('submit.profile', async event => {
            event.preventDefault();
            if (!ready || submitting || !form.valid()) return;
            submitting = true;
            const button = form.find(':submit');
            const buttonText = button.text();
            form.find('button').prop('disabled', true);
            button.text('Saving...');
            $('#profile-form-status').text('');
            try {
                const data = {};
                for (const field of fields) data[field] = form.find('#' + field).val().trim() || null;
                form.find('input').prop('disabled', true);
                const response = await api.put({ url: 'Authenticated/User/Profile', data,
                    requesterName: $('#current-user').text(), requestOrigin: 'My profile', requestSystem: 'HR Portal' });
                if (!response.ok) throw new Error(await errorMessage(response));
                savedProfile = Object.fromEntries(Object.entries(data).map(([key, value]) => [key[0].toLowerCase() + key.slice(1), value]));
                populate(savedProfile);
                toastr.success('Profile updated.');
                setEditing(false);
                $('#edit-user-detail').trigger('focus');
            } catch (error) {
                const message = error.message || 'Could not connect to the server. Please try again.';
                $('#profile-form-status').text(message);
                toastr.error(message);
                setEditing(true);
            } finally {
                submitting = false;
                form.find('button').prop('disabled', false);
                button.text(buttonText);
            }
        });
        loadProfile();
    });
})(jQuery);
