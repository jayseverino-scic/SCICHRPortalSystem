

class FormHelper {
    _dateHelper = new DateHelper();
    _stringHelper = new StringHelper();
    _apiHelper = new ApiHelper();
    toJsonString = form => {
        let obj = {};
        let elements = form.querySelectorAll("input, select, textarea");

        for (let element of elements) {
            let name = element.name;
            let value = element.value;

            if (name) {
                obj[name] = value;
            }
        }

        return obj;
    }

    populateForm = (form, data) => {
        let $form = $(form);
        for (let key in data) {
            let element = $form.find(`#${this._stringHelper.camelToPascal(key)}`);

            // For Select2 Elements
            if (element.hasClass('select2-hidden-accessible')) {

                var multipleAttribute = element.attr('multiple');
                var isMultiSelect = typeof multipleAttribute !== 'undefined' && multipleAttribute !== false;

                if (isMultiSelect) {
                    element.val(data[key]).change();
                    continue;
                }
            }

            // For Mostly Kendo Elements
            switch (element.data('role')) {
                case 'datepicker':
                    element.data('kendoDatePicker').value(data[key]);
                    break;
                case 'dropdownlist':
                    element.data('kendoDropDownList').value(data[key]);
                    break;
                case 'combobox':
                    element.data('kendoComboBox').value(data[key]);
                    break;
                case 'datetimepicker':
                    element.data('kendoDateTimePicker').value(data[key]);
                    break;
                case 'timepicker':
                    element.data('kendoTimePicker').value(data[key]);
                    break;
                default:
                    if (element.is(':checkbox') || element.is(':radio'))
                        element.prop('checked', data[key]);
                    else if (element.is('select'))
                        element.val(data[key]).trigger('change');
                    else
                        element.val(data[key]);
            }
        }
    }

    populateFormByName = (form, data) => {
        let $form = $(form);
        for (let key in data) {
            let element = $form.find(`[name=${this._stringHelper.camelToPascal(key)}]`);

            // For Select2 Elements
            if (element.hasClass('select2-hidden-accessible')) {

                var multipleAttribute = element.attr('multiple');
                var isMultiSelect = typeof multipleAttribute !== 'undefined' && multipleAttribute !== false;

                if (isMultiSelect) {
                    element.val(data[key]).change();
                    continue;
                }
            }

            // For Mostly Kendo Elements
            switch (element.data('role')) {
                case 'datepicker':
                    element.data('kendoDatePicker').value(data[key]);
                    break;
                case 'dropdownlist':
                    element.data('kendoDropDownList').value(data[key]);
                    break;
                case 'combobox':
                    element.data('kendoComboBox').value(data[key]);
                    break;
                case 'datetimepicker':
                    element.data('kendoDateTimePicker').value(data[key]);
                    break;
                case 'timepicker':
                    element.data('kendoTimePicker').value(data[key]);
                    break;
                default:
                    if (element.is(':checkbox') || element.is(':radio'))
                        element.prop('checked', data[key]);
                    else if (element.is('select'))
                        element.val(data[key]).trigger('change');
                    else
                        element.val(data[key]);
            }
        }
    }

    renderDropdown = params => {
    let parent = '';
    let name = '';
    let dataName = '';

    if (params.hasOwnProperty('name'))
        name = params.name;

    let dropdown = $('#' + name);
    dropdown.empty();

    let valueName = params.valueName != undefined ? params.valueName : 'id';

    if (params.placeHolder) {
        dropdown.append($("<option />").val("").text(params.placeHolder));
    }
    $.each(params.data, function () {

        let text = `${this[params.text]}`;
        let secondText = `${this[params.secondText]}`;
        if (this[valueName] && text && params.secondText) {
            dropdown.append($("<option />").val(this[valueName]).text(text + ' ' + secondText));
        }
        else if (this[valueName] && text) {
            dropdown.append($("<option />").val(this[valueName]).text(text));
        }

    });
    
    if (params.isSelect2) {

        dropdown.select2({
            theme: "bootstrap",
            width: 'element',
            width: 'resolve',
            multiple: params.isMultiple,
            closeOnSelect: !params.autoClose,
            dropdownParent: params.dropdownParent ? $(`${params.dropdownParent}`) : null,
            tags: params.tags ? true : false
        });
    }

    }

    deleteRecord = (e, message, system) => {
        e.preventDefault();
        let deleteIcon = $(e.currentTarget);
        let id = deleteIcon.data('id');

        let active = deleteIcon.data('active');
        let endpoint = deleteIcon.data('endpoint');
        let gridId = deleteIcon.data('table');
        let textMessage = "You won't be able to revert this!";
        if (active) {
            toastr.error('Cannot delete ' + message + '. Please delete first all information using this ' + message + 'before deletion!');
            return;
        }
        if (message)
            textMessage = `Are you sure you want to delete ${message}?`
        Swal.fire({
            title: 'Are you sure?',
            text: textMessage,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then(async (result) => {
            if (result.value) {
                let currentTabTitle = $('.tab-pane.active .title').text();
                let response = await this._apiHelper.delete({
                    url: `${endpoint}/${id}`,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: system
                });
                if (response.ok) {
                    let table = $('#' + gridId).DataTable();
                    let info = table.page.info();
                    let isLast = (info.end - info.start) == 1;
                    if (isLast) {
                        table.draw();
                    } else {
                        table.ajax.reload(null, false);
                    }

                    Swal.fire(
                        'Deleted!',
                        'Successfuly deleted.',
                        'success'
                    );

                }
                else if (response.status === 409) {
                    let json = await response.json();
                    let message = 'Is in use.';

                    if (json) {
                        if (json.message) {
                            message = json.message;
                        }
                    } 

                    Swal.fire(
                        'Error!',
                        message,
                        'error'
                    );
                }
                else if (response.status === 400 || response.status === 404)
                    Swal.fire(
                        'Error!',
                        'Is in use.',
                        'error'
                    );

            }
        });

    }
}