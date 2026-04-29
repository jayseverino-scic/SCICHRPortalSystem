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
    const _daysOfWeekLookup = {
        '1': 'Monday',
        '2': 'Tuesday',
        '3': 'Wednesday',
        '4': 'Thursday',
        '5': 'Friday',
        '6': 'Saturday',
        '7': 'Sunday'
    };
    const SYSTEM = 'enrolment';
    let getDayNames = function (days) {
        return _.map(normalizeDays(days), function (day) {
            return _daysOfWeekLookup[day] || day;
        });
    }
    let normalizeDays = function (days) {
        if (!days) {
            return [];
        }

        let normalizedDays = _.chain($.isArray(days) ? days : [days])
            .map(function (day) {
                return day != null ? day.toString().split(/[;,]/) : [];
            })
            .flatten()
            .value();

        return _.chain(normalizedDays)
            .map(function (day) {
                return day != null ? day.toString().trim() : '';
            })
            .filter(function (day) {
                return day !== '';
            })
            .uniq()
            .value();
    }
    let attachEvents = () => {
        $('#setting-form').on('submit', onFormSubmit);
        $('#setting-form #RestDays').val(null).trigger('change');

    };

    let daysofWeekSelect2 = function (isMultiple) {
        $('#setting-form #RestDays').select2({
            multiple: isMultiple,
            theme: "bootstrap",
            width: 'element',
            width: 'resolve',
            //closeOnSelect: !params.autoClose,
        });
    }
    let onFormSubmit = async event => {
        event.preventDefault();
        let selectedAssignmentDays = $('#RestDays').val() || [];
        let form = $(event.target);
        $(event.target).validate();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';
            let daysOfWeek = selectedAssignmentDays;
            let data = _formHelper.toJsonString(event.target);
            data.RestDays = daysOfWeek.join(';');
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
            let data = json;
            //console.log(data);
            $('#busy-indicator-container').addClass('d-none');
            let form = $('#setting-form');
            data.restDays = normalizeDays(data.restDays);
            daysofWeekSelect2(true);
            populateForm(form, json);
        }
    };

    $(document).ready(function () {
        initializeModals();
        getSetting();
        attachEvents();
    });

})(jQuery);