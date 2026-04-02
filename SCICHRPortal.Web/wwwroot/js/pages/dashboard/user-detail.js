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
        $('#user-detail-form').on('submit', onSaveUserDetailForm);
        $('#edit-user-detail').on('click', onClickEditIcon);
    };

    let onSaveUserDetailForm = async (e) => {
        e.preventDefault();
        let _form = document.getElementById('user-detail-form');
        if ($(_form).valid()) {
            let data = _formHelper.toJsonString(_form);
            let response;
            response = await _apiHelper.put({
                url: 'Authenticated/User/' + data.userId,
                data: data,
                requestOrigin: `User Detail Ta`,
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            });

            if (response.ok) {
                toastr.options = {
                    "preventDuplicates": true,
                    "preventOpenDuplicates": true
                };
                $('#edit-user-detail').trigger('click');
                toastr.success('Updated!');
            } else {
                toastr.error('Error!');
            }
        }
    }

    let onClickEditIcon = function (e) {
        let form = $('#user-detail-form');
        let buttonContainer = $('#button-container');
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
            form.find('input').prop("disabled", true);
            buttonContainer.addClass('d-none');
        } else {
            $(this).addClass('active');
            form.find('input').removeAttr('disabled');
            buttonContainer.removeClass('d-none');
        }

    }

    let initializeModals = e => {
        $('.nav-link').removeClass('d-none');
        $('.nav-link.user-detail').addClass('active');
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
            let form = $('#user-detail-form');
            _formHelper.populateForm(form, user);
        }

        $('#busy-indicator-container').addClass('d-none');
    };


    $(document).ready(function () {
        getCurrentLogIn();
        initializeModals();
        attachEvents();
    });

})(jQuery);