class UploadDownloadModalHelper {
    defaultOptions = {
        pdfDownloadEnabled: false,
        tableId: '',
        fileName: '',
        xlsxDownloadUri: null,
        pdfButtonNumber: null,
        downloadButtonId: 'download-button',
        pdfButtonEnabled: false
    }

    constructor(params) {
        this.modalElement = $('#upload-download-modal');
        this.modalDetails = $('#modal-details');
        this.apiHelper = new ApiHelper();

        this.config = $.extend(this.defaultOptions, params);

        if (this.config.pdfButtonEnabled)
            $('.dl-button[data-format=pdf]').removeClass('d-none');

        this.attachEvents();
    }

    attachEvents = () => {
        if (this.config.fileName) {
            $('#' + this.config.downloadButtonId).on('click', this.onClickDownload);
            $('#upload-download-modal').on('click', '.dl-button', this.onClickDownloadFile);
            $('#upload-file').on('change', this.onChangeFile);
            $('#upload-download-modal').on('click', '.tryagain-button', this.onClickTryAgain);
            $('#upload-download-modal').on('hidden.bs.modal', this.resetFileInput);
        }
        $('#upload-download-modal').on('click', '.main-button.cancel-download-upload', this.onClickCancel);
    }

    resetFileInput = () => {
        let fileInput = $('#upload-file');
        fileInput.val(null);
        fileInput.next('label').text("");
    }

    onClickTryAgain = e => {
        $('#upload-form').trigger('submit');
    }

    onChangeFile = e => {
        let allowedTypes = ['application/xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'];
        let fileInput = $('#upload-file')[0];

        let file = fileInput.files[0];
        var fileType = file.type;
        if (!allowedTypes.includes(fileType)) {
            Swal.fire(
                'File Format Error!',
                'Please Select Supported File',
                'error'
            );
            $("#fileInput").val('');
            return false;
        }

        var fileName = $('#upload-file')[0].files[0].name;
        $('#upload-file').next('label').text(fileName);
    }

    onClickDownload = () => {
        this.show('download');
    }

    onClickDownloadFile = async e => {
        //XLSX OR PDF
        let format = $(e.currentTarget).attr("data-format");

        switch (format) {
            case 'xlsx':
                await this.downloadExcel();
                break;
            case 'pdf':
                $('#' + this.config.tableId).DataTable().buttons(0, this.config.pdfButtonNumber).trigger();
                this.hide();
                break;
            default:
                Swal.fire(
                    'File Format Error!',
                    'Please Select Supported File',
                    'error'
                );
        }
    }

    downloadExcel = async () => {
        let response = await this.apiHelper.get({
            url: this.config.xlsxDownloadUri
        });

        if (response.ok) {
            let blob = await response.blob();

            //emulate download click
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = this.config.fileName + '_' + Date.now() + '.xlsx';
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);

            this.hide();
        }
    }

    show = (status, params) => {

        let details;

        switch (status) {
            case 'uploading':
                details = $('#uploading-content').clone();
                details.find('#upload-filename').text(params.filename);
                break;
            case 'success':
                details = $('#upload-success-content').clone();
                break;
            case 'failed':
                details = $('#upload-failed-content').clone();
                details.find('#upload-error').text(params.error);
                break;
            case 'download':
                details = $('#download-content').clone(true, true);
                break;
            default:
                details = $('#upload-success-content').clone();
        }

        if (details.length !== 0) {
            if (this.modalElement.data('bs.modal')?._isShown) {
                this.modalDetails.fadeOut(150, function () {
                    $(this).empty().html(details.html()).fadeIn(400);
                });
            } else {
                this.modalDetails.empty().append(details.html()).fadeIn(400);
            }

            this.modalElement.modal({ backdrop: 'static', keyboard: false });
        }
        else {
            noAccessAlert();
        }
    };

    hide = () => {

        this.modalElement.modal('hide');
    }

    onClickCancel = (e) => {
        Swal.fire({
            title: 'Are you sure?',
            text: "You want to close this upload message?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, close it!'
        }).then(async (result) => {
            if (result.value) {
                this.modalElement.modal('hide');
            }
        });
    }
}