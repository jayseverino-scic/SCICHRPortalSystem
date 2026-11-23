(function ($) {
    'use strict';
    const api = new ApiHelper();
    const cookies = new CookieHelper();
    let submitting = false;
    const onLogInSubmitted = async event => {
        event.preventDefault();
        const form = $(event.target);
        if (submitting || !form.valid()) return;
        submitting = true;
        const button = form.find(':submit');
        button.prop('disabled', true).text('Logging in...');
        $('#busy-indicator-container').removeClass('d-none');
        try {
            const response = await api.post({ url: 'Authenticate/User', data: {
                email: form.find('[name="Email"]').val().trim(), password: form.find('[name="Password"]').val(), IsLogin: true
            }, requestOrigin: 'Login', requestSystem: 'HR Portal' });
            if (response.ok) {
                const result = await response.json();
                cookies.set('jsonWebToken', result.accessToken.jsonWebToken);
                cookies.set('refreshToken', result.accessToken.refreshToken);
                window.location = '/Home/Index';
            } else if (response.status === 401) toastr.error('Invalid email address or password.');
            else if (response.status === 403) toastr.error('Your account is inactive or awaiting approval. Contact your Administrator.');
            else if (response.status === 409) toastr.error('Your account is locked. Contact your Administrator to reset your password.');
            else toastr.error('Login could not be completed. Please try again.');
        } catch (_) { toastr.error('Could not connect to the server. Please try again.'); }
        finally {
            submitting = false;
            button.prop('disabled', false).html('<span class="login-text">Login</span>');
            $('#busy-indicator-container').addClass('d-none');
        }
    };
    $(document).ready(() => {
        $('#log-in-form').on('submit.login', onLogInSubmitted);
        $(document).on('click.login', '.eye-icon', event => {
            const button = $(event.target).closest('.eye-icon');
            const input = document.getElementById(button.data('target'));
            if (input) {
                input.type = input.type === 'password' ? 'text' : 'password';
                button.find('i').toggleClass('fa-eye fa-eye-slash');
            }
        });
    });
})(jQuery);
