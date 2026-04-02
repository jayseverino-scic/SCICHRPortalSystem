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
        $('#setting-form').on('submit', onFormSubmit);

    };

    let onFormSubmit = async event => {
        event.preventDefault();

        let form = $(event.target);
        $(event.target).validate();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            let currentTabTitle = 'Setting';
            response = await _apiHelper.put({
                url: 'Authenticated/TimekeepingAdminSetup',
                data: data,
                requestOrigin: `${currentTabTitle} Tab`,
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            });

            if (response.ok) {
                $('#setting-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
            }
            else if (response.status == 403) {
                noAccessAlert();
            }
            else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }

            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let populateForm = (form, data) => {
        _formHelper.populateForm(form, data);
    }

    let initializeModals = e => {
        $('#setting-data-menu').removeClass('d-none');
        $('#setting-data-menu .reservation-setting').addClass('active');
    }

    let getSetting = async () => {
        $('#busy-indicator-container').removeClass('d-none');
        let api = document.getElementById('base-api-url').value;
        let response = await _apiHelper.get({
            url: `Authenticated/TimekeepingAdminSetup`,
        });

        if (response.ok) {
            let json = await response.json();
            console.log(json);

            $('#busy-indicator-container').addClass('d-none');
            let form = $('#setting-form');
            populateForm(form, json);
        }
    };

    $(document).ready(function () {
        initializeModals();
        getSetting();
        attachEvents();
    });

})(jQuery);