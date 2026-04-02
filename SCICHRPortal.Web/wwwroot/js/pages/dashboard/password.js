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
    let _userId = 0;
    let attachEvents = () => {
        $('#change-password-form').on('submit', onResetPasswordSubmitted);
    };

    let onResetPasswordSubmitted = async event => {
        event.preventDefault();
        $('.unmatched-password-validation').addClass('d-none');

        if ($('#new-password').val().toLowerCase() !== $('#confirm-password').val().toLowerCase()) {
            $('.unmatched-password-validation').removeClass('d-none');
        }
        else {
            let data = _formHelper.toJsonString(event.target);
            data.userId = _userId;
            let response;
            let _form = document.getElementById('change-password-form');

            if ($(_form).valid()) {
                response = await _apiHelper.put({
                    url: 'Authenticated/User/Reset',
                    data: data
                });

                if (response.ok) {
                    Swal.fire({
                        title: 'Updated',
                        text: "Do you want to logout, to verify your password?",
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Logout'
                    }).then(async (result) => {
                        _cookieHelper.delete('jsonWebToken');
                        _cookieHelper.delete('refreshToken');
                        window.location = '/Login/Index';

                    });
                } else {

                    toastr.options = {
                        "preventDuplicates": true,
                        "preventOpenDuplicates": true
                    };

                    if (response.status === 409) {
                        toastr.error('Wrong Password');
                    }

                    if (response.status === 400) {
                        toastr.error('Password must contain at least 8 characters , one letter and one number.');
                    }

                }
            }

        }
    };

    let initializeModals = e => {
        $('.nav-link').removeClass('d-none');
        $('.nav-link.change-password').addClass('active');
    }

    let getCurrentLogIn = async () => {

        $('#busy-indicator-container').removeClass('d-none');
        let api = document.getElementById('base-api-url').value;

        let response = await fetch(`${api}/${`Authenticated/User/GetAuthenticatedAsync`}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${_cookieHelper.get('jsonWebToken')}`
            },
            body: JSON.stringify()
        });

        if (response.ok) {
            let user = await response.json();
            _userId = user.userId;
        }

        $('#busy-indicator-container').addClass('d-none');
    };


    $(document).ready(function () {
        getCurrentLogIn();
        initializeModals();
        attachEvents();
    });

})(jQuery);