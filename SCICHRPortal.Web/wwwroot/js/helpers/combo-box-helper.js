class ComboBoxHelper {
    render = (id, options) => {
        return $(`#${id}`).kendoComboBox(options);
    };

    getValue = id => {
        let data = this.getData(id);

        if (data)
            return $(`#${id}`).data('kendoComboBox').value();

        return '';
    };

    getData = id => {
        return $(`#${id}`).data('kendoComboBox').dataItem();
    };

    setDataSource = (id, data) => {
        let dataSource = new kendo.data.DataSource({
            data: data
        });

        let comboBox = $(`#${id}`).data('kendoComboBox');
        comboBox.setDataSource(dataSource);
    };

    validateRequired = id => {
        let valid = this.getData(id) !== undefined;

        if (!valid) {
            let displayName = $(`label[for="${id}"]`).text();

            if (displayName)
                toastr["error"](`${displayName} is required.`);
        }

        return valid;
    };
}