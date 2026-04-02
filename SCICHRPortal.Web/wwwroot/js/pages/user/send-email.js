(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    //const _formHelper = new FormHelper();
    //const _cookieHelper = new CookieHelper();

    let attachEvents = () => {
        console.log($('#resetBtn'));
        $('#forgot-password-form').on('submit', sendEmail);
        $('#Email').on('input', removeRedBorder);
    };

    let busyindicator = $('#busy-indicator-container');

    let sendEmail = async event =>{
        event.preventDefault();
        $('#busy-indicator-container').removeClass('d-none');
        let email = $('#Email').val();
        let response = '';
        validateEmail();
        let request = {
            email: $('#Email').val(),
        }

        if (!$('#Email').hasClass('border border-danger') && email.length !== 0) {
            busyindicator.removeClass('hidden');
            busyindicator.addClass('show');
            response = await _apiHelper.post({
                url: 'Authenticate/User/ForgotPassword',
                data: request,
            });

            if (response.ok) {
                $('#reset-container').addClass('d-none');
                $('#success-container').removeClass('d-none');
            }
            else {
                toastr.error('Conflict');
            }
        }

        $('#busy-indicator-container').addClass('d-none');
    };

    let onCodeReset = function (retrievedData) {
        let email = getElementById('Email').val();

        userService.verifyEmail({
            email: email,
            callback: function (data) {
                onEmailVerified(data, retrievedData);
            }
        });
    };

    let removeRedBorder = function () {
        $(this).removeClass('border border-danger');
    };

   
    let onCancelClicked = event => {
        window.location = '/Dashboard/Index';
    };
    let onEmailVerified = function (data, retrievedData) {
        let emailToken = retrievedData.emailToken;
        let email = $('#Email').val();

        if (data.isEmailExist) {
            userService.saveResetPasswordCode({
                data: JSON.stringify({
                    Email: email,
                    ResetPasswordCode: retrievedData.resetPasswordCode
                })
            });

            emailService.sendEmail({
                data: JSON.stringify({
                    ToEmail: email,
                    Subject: 'Reset Password',
                    Body: rootUrl + '/Users/Reset/1' + '?rpc=' + emailToken
                }), callback: onEmailSent
            });
        } else {
            toastr.error('Please enter a valid email');
        }
    };

    let onEmailSent = function () {

        if (confirm("A link to reset your password has been sent to your email."))
            window.location = '/Users/login';
        else {
            busyindicator.removeClass('show');
            busyindicator.addClass('hidden');
        }
    };

    let validateEmail = function () {
        const emailReg = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
        let emailField = $('#Email');
        let emailVal = emailField.val();


        if (!emailReg.test(emailVal) || emailVal == '') {
            toastr.error('Please enter a valid email');
            emailField.addClass('border border-danger');

        } else {
            emailField.removeClass('border border-danger');
        }
    };


    $(document).ready(function () {
        attachEvents();
    });
})(jQuery);