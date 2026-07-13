(function ($) {

    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load';


    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _cookieHelper = new CookieHelper();
    const _stringHelper = new StringHelper();


    let attachEvents = () => {

        /* $('#tabSignIn').on(CLICK_EVENT, function (e) { e.stopPropagation(); switchTab('signin'); }); */
        /* $('#tabSignUp').on(CLICK_EVENT, function (e) { e.stopPropagation(); switchTab('signup'); }); */
        $('#log-in-form').on('submit', onLogInSubmitted);
        $('#register-form').on('submit', onRegisterFormSubmit);
        $(document).on(CLICK_EVENT, '.eye-icon', onTogglePassword);
        $('#reset-password-form').on('submit', onResetPasswordSubmitted);
        $('#cancel-reset-pass').on('click', onCancelClicked);
        setupValidation();
        //generateAvatarColors();
    };

    window.switchTab = function (tab) {
        const $signInForm = $('#formSignIn');
        const $signUpForm = $('#formSignUp');
        const $tabSignIn = $('#tabSignIn');
        const $tabSignUp = $('#tabSignUp');
        const $loginBorder = $('#login-border');
        const $indicator = $('#tabIndicator');

        if (tab === 'signin') {
            $signInForm.removeClass('hidden');
            $signUpForm.addClass('hidden');
            $tabSignIn.addClass('active');
            $tabSignUp.removeClass('active');
            $loginBorder.removeClass('signup-active');
            $indicator.css('left', '4px');
        } else {
            $signInForm.addClass('hidden');
            $signUpForm.removeClass('hidden');
            $tabSignIn.removeClass('active');
            $tabSignUp.addClass('active');
            $loginBorder.addClass('signup-active');
            $indicator.css('left', 'calc(50% + 2px)');
        }
    };

    let onShowSignIn = (event) => {
        if (event) event.stopPropagation();
        window.switchTab('signin');
        $('#success-register-container').addClass('d-none');
        removeValidation();
        $('#log-in-form')[0].reset();
        $('#register-form')[0].reset();
    };

    let onShowSignUp = (event) => {
        if (event) event.stopPropagation();
        window.switchTab('signup');
        removeValidation();
        $('#log-in-form')[0].reset();
        $('#register-form')[0].reset();
    };

    let onTogglePassword = (event) => {
        event.preventDefault();
        event.stopPropagation();

        const $btn = $(event.target).closest('.eye-icon');
        const targetId = $btn.data('target');

        if (!targetId) return;

        const $input = $('#' + targetId);
        const $icon = $btn.find('i');

        if ($input.attr('type') === 'password') {
            if ($input.val().length === 0) return;
            $input.attr('type', 'text');
            $icon.removeClass('fa-eye-slash').addClass('fa-eye');
        } else {
            $input.attr('type', 'password');
            $icon.removeClass('fa-eye').addClass('fa-eye-slash');
        }
    };

    let onPasswordInput = (event) => {
        const $input = $(event.target);
        const $wrapper = $input.closest('.input-field-wrapper');
        const $icon = $wrapper.find('.eye-icon i');

        if ($input.val().length === 0) {
            $input.attr('type', 'password');
            $icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            $icon.removeClass('fa-eye').addClass('fa-eye-slash');
        }
    };


    let onRegisterSuccess = () => {
        $('#formSignIn').removeClass('hidden');
        $('#formSignUp').addClass('hidden');
        $('#tabSignIn').addClass('active');
        $('#tabSignUp').removeClass('active');
        $('#login-border').removeClass('signup-active');
        $('#tabIndicator').css('left', '4px');
        $('#success-register-container').removeClass('d-none');
    };


    let onRegisterFormSubmit = async (event) => {
        event.preventDefault();
        $('.user-validation').removeClass('d-none');

        const $form = $(event.target);

        if ($form.valid()) {
            let data = _formHelper.toJsonString(event.target);
            data.roleId = 3;

            const response = await _apiHelper.post({
                url: 'Authenticated/User/Register',
                data: data
            });

            if (response.ok) {
                onRegisterSuccess();
            } else if (response.status === 403) {
                noAccessAlert();
            } else if (response.status === 409) {
                const json = await response.json();
                Swal.fire('Error!', json.message, 'error');
            }
        }
    };


    let onLogInSubmitted = async (event) => {
        event.preventDefault();
        const form = document.getElementById('log-in-form');

        if ($(form).valid()) {
            $('#busy-indicator-container').removeClass('d-none');

            let data = _formHelper.toJsonString(form);
            data.IsLogin = true;

            const response = await _apiHelper.post({
                url: 'Authenticate/User',
                data: data,
                requestOrigin: 'Login Page',
                requesterName: data.UserName
            });

            if (response.ok) {
                const token = await response.json();
                console.log(token);
                _cookieHelper.set('jsonWebToken', token.accessToken.jsonWebToken);
                _cookieHelper.set('refreshToken', token.accessToken.refreshToken);

                if (token.isPasswordChanged) {
                    $('#formSignIn').addClass('hidden');
                    $('#reset-password-form-container').removeClass('d-none');
                } else {
                    window.location = '/Home/Index';
                }
            } else {
                toastr.options = { preventDuplicates: true, preventOpenDuplicates: true };

                if (response.status === 401) toastr.error('Invalid Username/Password');
                else if (response.status === 403) toastr.error('Please contact the registrar to approve your user account!');
                else if (response.status === 409) toastr.error('Login attempts exceeded. Please contact administrator for password reset.');
            }

            $('#busy-indicator-container').addClass('d-none');
        }
    };


    let onResetPasswordSubmitted = async (event) => {
        event.preventDefault();
        $('.unmatched-password-validation').addClass('d-none');

        if ($('#new-password').val() !== $('#confirm-password').val()) {
            $('.unmatched-password-validation').removeClass('d-none');
            return;
        }

        let data = _formHelper.toJsonString(event.target);
        const response = await _apiHelper.put({ url: 'Authenticate/Reset', data: data });

        if (response.ok) {
            _cookieHelper.delete('jsonWebToken');
            _cookieHelper.delete('refreshToken');
            window.location = '/Login/Index';
        } else {
            toastr.options = { preventDuplicates: true, preventOpenDuplicates: true };
            if (response.status === 400) {
                toastr.error('Password must contain at least 8 characters, one letter and one number.');
            }
        }
    };


    let onCancelClicked = () => {
        window.location = '/Home/Index';
    };


    let removeValidation = () => {
        $('.field-validation-error')
            .removeClass('field-validation-error')
            .addClass('field-validation-valid')
            .html('');
        $('.field-validation-valid span').html('');
    };

    let setupValidation = () => {

        $('#log-in-form').validate({
            submitHandler: function (form) {
                const $btn = $(form).find('.shape-login');
                $btn.prop('disabled', true).html('<span class="login-text">Logging in...</span>');
                $(form).trigger('submit');
            }
        });

        $('#register-form').validate({
            rules: {
                ConfirmPassword: {
                    equalTo: '#signupPassword'
                }
            },
            messages: {
                ConfirmPassword: {
                    equalTo: 'Passwords do not match'
                }
            },
            submitHandler: function (form) {
                const $btn = $(form).find('.shape-login');
                $btn.prop('disabled', true).html('<span class="login-text">Creating account...</span>');
                $(form).trigger('submit');
            }
        });
    };


    let onWindowLoaded = () => {
        attachEvents();
    };

    window.addEventListener(LOAD_EVENT, onWindowLoaded);

})(jQuery);
