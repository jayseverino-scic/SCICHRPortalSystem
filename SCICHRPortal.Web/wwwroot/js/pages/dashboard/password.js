(function ($) {
    'use strict';
    const api = new ApiHelper();
    const cookies = new CookieHelper();
    let submitting = false;
    $(document).ready(() => {
        $('#change-password-form').on('submit.password', async event => {
            event.preventDefault();
            const form = $(event.target);
            if (submitting) return;
            $('#password-form-status').text('');
            $('.unmatched-password-validation').addClass('d-none');
            if ($('#new-password').val() !== $('#confirm-password').val()) {
                $('.unmatched-password-validation').removeClass('d-none');
                return;
            }
            if (submitting || !form.valid()) return;
            submitting = true;
            const button = form.find(':submit');
            const buttonText = button.text();
            form.find('input, button').prop('disabled', true);
            button.text('Changing password...');
            try {
                const response = await api.put({ url: 'Authenticated/User/Reset', data: {
                    currentPassword: form.find('[name="CurrentPassword"]').val(), password: $('#new-password').val()
                }, requesterName: $('#current-user').text(), requestOrigin: 'My password', requestSystem: 'HR Portal' });
                if (!response.ok) {
                    const body = await response.text();
                    let message = 'Password could not be changed. Please try again.';
                    try { const error = JSON.parse(body); message = error.message || (error.errors && Object.values(error.errors).flat().join(' ')) || message; } catch (_) { }
                    throw new Error(response.status >= 500 ? 'The server could not change your password. Please try again.' : message);
                }
                cookies.delete('jsonWebToken');
                cookies.delete('refreshToken');
                window.location = '/Login/Index';
            } catch (error) {
                const message = error.message || 'Could not connect to the server. Please try again.';
                $('#password-form-status').text(message);
                toastr.error(message);
            } finally {
                submitting = false;
                form.find('input, button').prop('disabled', false);
                button.text(buttonText);
            }
        });
    });
})(jQuery);
