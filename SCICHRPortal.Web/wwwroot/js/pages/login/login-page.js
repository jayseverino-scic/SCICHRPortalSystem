(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _cookieHelper = new CookieHelper();

    let _roles = [];

    let attachEvents = () => {
        $('#log-in-form').on('submit', onLogInSubmitted);
        $('#reset-password-form').on('submit', onResetPasswordSubmitted);
        $('.sign-up-span').on('click', onShowSignUp);
        $('.sign-in-span').on('click', onShowSignIn);
        $('#procced-login').on('click', onShowSignIn);
        $('#cancel-reset-pass').on('click', onCancelClicked);
        $('#show-login-pass').on('click', onToggleViewPassword);
        $('#show-new-pass').on('click', onToggleViewNewPassword);
        $('#show-confirm-pass').on('click', onToggleConfirmNewPassword);
        $('#register-form').on('submit', onRegisterFormSubmit);
    };

    let onShowSignUp = event => {
        removeValidation()
        event.stopPropagation();
        $('#login-form-container').addClass('d-none');
        $('#register-form-container').removeClass('d-none');
        $('#register-form')[0].reset();
        $('#log-in-form')[0].reset();
        $('#add-user-role').val(3);
    }

    let onShowSignIn = event => {
        event.stopPropagation();
        removeValidation();
        $('#login-form-container').removeClass('d-none');
        $('#register-form-container').addClass('d-none');
        $('#success-register-container').addClass('d-none');
        $('#register-form')[0].reset();
        $('#log-in-form')[0].reset();
    }

    let onRegisterSuccess = event => {
        $('#login-form-container').addClass('d-none');
        $('#register-form-container').addClass('d-none');
        $('#success-register-container').removeClass('d-none');
    }

    let onToggleViewPassword = event => {
        let accessType = $('#password').attr('type');
        let selectedIconId = event.currentTarget.id;

        if (accessType === 'password') {
            $('#password').attr('type', 'text');
            $('#' + selectedIconId).removeClass('fa-eye');
            $('#' + selectedIconId).addClass('fa-eye-slash');
        }
        else {
            $('#password').attr('type', 'password');
            $('#' + selectedIconId).removeClass('fa-eye-slash');
            $('#' + selectedIconId).addClass('fa-eye');
        }
    };

    let onToggleViewNewPassword = event => {
        let accessType = $('#new-password').attr('type');
        let selectedIconId = event.currentTarget.id;

        if (accessType === 'password') {
            $('#new-password').attr('type', 'text');
            $('#' + selectedIconId).removeClass('fa-eye');
            $('#' + selectedIconId).addClass('fa-eye-slash');
        }
        else {
            $('#new-password').attr('type', 'password');
            $('#' + selectedIconId).removeClass('fa-eye-slash');
            $('#' + selectedIconId).addClass('fa-eye');
        }
    };

    let onToggleConfirmNewPassword = event => {
        let accessType = $('#confirm-password').attr('type');
        let selectedIconId = event.currentTarget.id;

        if (accessType === 'password') {
            $('#confirm-password').attr('type', 'text');
            $('#' + selectedIconId).removeClass('fa-eye');
            $('#' + selectedIconId).addClass('fa-eye-slash');
        }
        else {
            $('#confirm-password').attr('type', 'password');
            $('#' + selectedIconId).removeClass('fa-eye-slash');
            $('#' + selectedIconId).addClass('fa-eye');
        }
    };

    let successMessage = (status, name) => {
        Swal.fire(
            status,
            `${name} has been ${status.replace('!', '').toLowerCase()}.`,
            'success'
        );
    };

    let onRegisterFormSubmit = async event => {
        event.preventDefault();
        $('.user-validation').removeClass('d-none');

        let form = $(event.target);

        if (form.valid()) {
            let response = '';

            if ($('#add-user-role option:selected').val() != '') {
                let data = _formHelper.toJsonString(event.target);
                data.roleId = $('#add-user-role').val();

                response = await _apiHelper.post({
                    url: 'Authenticated/User/Register',
                    data: data
                });
                status = 'Created!';

                if (response.ok) {
                    onRegisterSuccess();
                }
                else if (response.status == 403) {
                    noAccessAlert();
                }
                else if (response.status == 409) {
                    let json = await response.json();
                    Swal.fire(
                        'Error!',
                        json.message,
                        'error'
                    );

                }
            }
            else {
                $('.user-role-validation').removeClass('d-none');
            }
        }
        else {
            if ($('#add-user-role option:selected').val() == '') {
                $('.user-role-validation').removeClass('d-none');
            }
        }
    };

    let removeValidation = function () {
        $('.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid').html('');
        $('.field-validation-valid span').html('');
    }

    let renderUserRoleDropdown = async () => {

        let userRoleDropdown = $('.user-role');
        userRoleDropdown.empty();

        userRoleDropdown.append($('<option />').val("").text('Select Role'));
        userRoleDropdown.append($('<option/>').val(3).text("Parents"));
        $('#add-user-role').val(3);
    }

    let onCancelClicked = event => {
        window.location = '/Home/Index';
    };

    let onResetPasswordSubmitted = async event => {
        event.preventDefault();
        $('.unmatched-password-validation').addClass('d-none');

        if ($('#new-password').val().toLowerCase() !== $('#confirm-password').val().toLowerCase()) {
            $('.unmatched-password-validation').removeClass('d-none');
        }
        else {
            let data = _formHelper.toJsonString(event.target);
            let response;

            response = await _apiHelper.put({
                url: 'Authenticate/Reset',
                data: data
            });

            if (response.ok) {
                _cookieHelper.delete('jsonWebToken');
                _cookieHelper.delete('refreshToken');
                window.location = '/Login/Index';
            } else {

                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };

                if (response.status === 400) {
                    toastr.error('Password must contain at least 8 characters , one letter and one number.');
                }

            }
        }
    };

    let onLogInSubmitted = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('log-in-form');

        console.log('onLogInSubmitted');
        if ($(_form).valid()) {

            $('#busy-indicator-container').removeClass('d-none');
            let data = _formHelper.toJsonString(_form);
            data.IsLogin = true;
            let response;

            response = await _apiHelper.post({
                url: 'Authenticate/User',
                data: data,
                requestOrigin: 'Login Page',
                requesterName: data.UserName
            });

            if (response.ok) {
                var token = await response.json();
                _cookieHelper.set('jsonWebToken', token.accessToken.jsonWebToken);
                _cookieHelper.set('refreshToken', token.accessToken.refreshToken);
                if (token.isPasswordChanged) {
                    $('#log-in-form-container').addClass('d-none');
                    $('#reset-password-form-container').removeClass('d-none');
                }
                else {
                    const hasRegistrationApplication = checkRegistrationApplication(token.accessToken.jsonWebToken);
                    
                    if (hasRegistrationApplication) {
                        //window.location = '/RegistrationApplications/Index';
                        window.location = '/RegistrationApplications/Conform';
                    }
                    else {
                        window.location = '/Home/Index';
                    }
                }
            } else {
                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };

                if (response.status === 401) {
                    toastr.error('Invalid Username/Password');
                }
                else if (response.status === 403) {
                    toastr.error('Please contact the registrar to approve your user account!');
                }
                else if (response.status === 409) {
                    toastr.error('Login attempts exceeded. Please contact administrator for password reset.');
                }

            }
            $('#busy-indicator-container').addClass('d-none');

        }

    };

    const checkRegistrationApplication = (jsonWebToken) => {
        const obj = _cookieHelper.parseToken(jsonWebToken);
        return obj.role.includes('Registration Application');
    }


    let onWindowLoaded = () => {
        attachEvents();
        renderUserRoleDropdown();
    };

    window.addEventListener(LOAD_EVENT, onWindowLoaded);
})(jQuery);