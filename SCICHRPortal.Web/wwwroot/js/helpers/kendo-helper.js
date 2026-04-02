class KendoHelper {
    validateInputs = () => {
        let isValid = true;
        let datePickers = $('input[data-role="datepicker"]');
        let timePickers = $('input[data-role="timepicker"]');
        let comboBoxes = $('input[data-role="combobox"]');
        let dateTimePickers = $('input[data-role="datetimepicker"]');

        for (let datePicker of datePickers) {
            let id = $(datePicker).prop('id');

            if ($(`#${id}`).val() && $(`#${id}`).data("kendoDatePicker").value() === null) {
                let displayName = $(`label[for="${id}"]`).text();

                toastr["error"](`Invalid ${displayName}.`);
                isValid = false;
            }
        }

        for (let timePicker of timePickers) {
            let id = $(timePicker).prop('id');

            if ($(`#${id}`).val() && $(`#${id}`).data("kendoTimePicker").value() === null) {
                let displayName = $(`label[for="${id}"]`).text();

                toastr["error"](`Invalid ${displayName}.`);
                isValid = false;
            }
        }

        for (let comboBox of comboBoxes) {
            let id = $(comboBox).prop('id');

            if ($(`#${id}`).val() && $(`#${id}`).data('kendoComboBox').dataItem() === undefined) {
                let displayName = $(`label[for="${id}"]`).text();

                toastr["error"](`Invalid ${displayName}.`);
                isValid = false;
            }
        }

        for (let dateTimePicker of dateTimePickers) {
            let id = $(dateTimePicker).prop('id');

            if ($(`#${id}`).val() && $(`#${id}`).data("kendoDateTimePicker").value() === null) {
                let displayName = $(`label[for="${id}"]`).text();

                toastr["error"](`Invalid ${displayName}.`);
                isValid = false;
            }
        }

        return isValid;
    };
}