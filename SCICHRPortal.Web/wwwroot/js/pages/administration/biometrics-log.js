class UploadDownloadModalHelper {
    constructor(config) {
        this.config = config;
        this.init();
    }

    init() {
        this.setupModalEvents();
    }

    setupModalEvents() {
        $(document).on('click', '.modal-close, .modal-overlay', (e) => {
            this.hideAll();
        });
        $(document).on('click', '.modal-content', (e) => {
            e.stopPropagation();
        });
    }

    show(type, data = {}) {
        this.hideAll();

        switch (type) {
            case 'uploading':
                $('#uploadModal').show();
                if (data.filename) {
                    $('#uploadFileName').text(data.filename);
                }
                break;
            case 'success':
                $('#successModal').show();
                break;
            case 'failed':
                $('#errorModal').show();
                if (data.error) {
                    $('#errorMessage').text(data.error);
                }
                break;
        }
    }

    hideAll() {
        $('.modal').hide();
    }

    resetFileInput() {
        $('#upload-file').val('');
        $(".progress-bar").width('0%');
        $(".progress-bar").html('0%');
    }

    downloadFile() {
        const { xlsxDownloadUri, fileName } = this.config;
        console.log('Downloading file...');
    }
}

(function ($) {
    const _config = {
        pdfButtonEnabled: true,
        fileName: 'biometricsLog',
        xlsxDownloadUri: 'Authenticated/biometricsLog/Download',
        pdfButtonNumber: 0,
        tableId: 'biometrics-log-grid'
    };

    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load';
    const SYSTEM = 'administration';

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    const _uploadDownloadModalHelper = new UploadDownloadModalHelper(_config);

    let tableName = '#biometrics-log-grid';
    let pageSize = 10;
    let searchKeyword = '';
    let dataTable = null;

    let attachEvents = () => {
        $('#end-date-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#start-date-filter').attr('value', moment().format('yyyy-MM-DD'));
        $('#filter').on(CLICK_EVENT, onClickFilter);
        $('#start-date-filter').on('change', onChangeStartFilter)
        $('#import').on('click', onSubmitUploadForm);
    };

    let onSubmitUploadForm = async e => {
        e.preventDefault();
        let fileInput = $('#upload-file');

        if (fileInput[0].files.length == 0) {
            Swal.fire(
                'No File!',
                'Please Select File',
                'error'
            );
            return;
        }

        let fileName = fileInput.val().replace(/C:\\fakepath\\/i, '');
        _uploadDownloadModalHelper.show('uploading', { filename: fileName });

        var request = _apiHelper.ajaxRequest('POST', {
            url: 'Authenticated/BiometricsLog/Import',
            data: $('#upload-file')[0].files[0],
            xhr: function () {
                let xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = ((evt.loaded / evt.total) * 100);
                        $(".progress-bar").width(percentComplete + '%');
                        $(".progress-bar").html(percentComplete + '%');
                    }
                }, false);
                return xhr;
            },
            beforeSend: function () {
                $(".progress-bar").width('0%');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                _uploadDownloadModalHelper.show('failed', { error: XMLHttpRequest.responseText });
            },
            success: function (response) {
                _uploadDownloadModalHelper.resetFileInput();
                initializeGrid(response);
                _uploadDownloadModalHelper.show('success');
            }
        });
    };

    let onChangeStartFilter = () => {
        let startDateVal = $('#start-date-filter').val();
        let endDateVal = $('#end-date-filter').val();
        if (new Date(startDateVal) > new Date(endDateVal))
            $('#end-date-filter').val(startDateVal);

        $('#end-date-filter').attr('min', startDateVal);
    };

    let onClickFilter = async e => {
        e.preventDefault();
        let startDate = $('#start-date-filter').val();
        let endDate = $('#end-date-filter').val();

        // Check if DataTable exists before trying to get page info
        let pageNumber = 1;
        if (dataTable && $.fn.DataTable.isDataTable(tableName)) {
            let gridInfo = dataTable.page.info();
            pageNumber = gridInfo.page + 1;
        }

        let response = await _apiHelper.get({
            url: `Authenticated/BiometricsLog/Filter?pageNumber=${pageNumber}&pageSize=${pageSize}&searchKeyword=${searchKeyword}&startDate=${startDate}&endDate=${endDate}`,
        });

        if (response.ok) {
            let json = await response.json();
            let dataRetrieved = json.data;
            initializeGrid(dataRetrieved);
        }
    };

    let initializeGrid = (gridData) => {
        var data = [];
        try {
            if (gridData && Array.isArray(gridData)) {
                data = gridData;
            } else if (gridData && gridData.data && Array.isArray(gridData.data)) {
                data = gridData.data;
            }
        } catch (e) {
            console.error('Error processing grid data:', e);
            data = [];
        }

        // Clear existing DataTable if it exists
        if (dataTable && $.fn.DataTable.isDataTable(tableName)) {
            dataTable.destroy();
            $(tableName).empty();
        }

        let columns = getColumns();

        try {
            dataTable = $(tableName).DataTable({
                bLengthChange: true,
                lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
                bFilter: true,
                bInfo: true,
                serverSide: false,
                bSort: true, // Changed from false to true since you have orderable columns
                scrollY: "350px",
                scrollX: true,
                order: [[1, 'asc']], // Fixed order syntax
                data: data,
                columns: columns,
                pageLength: 5,
                dom: '<"top"lf>rt<"bottom"ip>',
                language: {
                    emptyTable: "No biometrics log data available",
                    zeroRecords: "No matching records found",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    infoEmpty: "Showing 0 to 0 of 0 entries",
                    infoFiltered: "(filtered from _MAX_ total entries)",
                    search: "Search:",
                    paginate: {
                        previous: "Previous",
                        next: "Next"
                    }
                },
                // Add error handling for column data
                createdRow: function (row, data, dataIndex) {
                    // This can help identify data issues
                }
            });
        } catch (error) {
            console.error('DataTables initialization error:', error);
            // Fallback: create a simple table
            createFallbackTable(data);
        }
    };

    // Fallback function if DataTables fails
    let createFallbackTable = (data) => {
        $(tableName).html('<table class="table table-striped"><thead><tr><th>Error loading table. Please refresh the page.</th></tr></thead></table>');
    };

    let getColumns = () => {
        let columns = [
            {
                title: 'No.',
                data: null, // Use null since we're generating the number
                width: "1.5em",
                className: 'noVis dt-center',
                orderable: false, // Explicitly set to false
                render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            {
                title: 'Personnel Id',
                data: "personnelId",
                className: 'noVis dt-center',
                orderable: true,
                defaultContent: "" // Provide default content
            },
            {
                title: 'Last Name',
                data: "lastName",
                className: 'noVis dt-center',
                orderable: true,
                defaultContent: ""
            },
            {
                title: "First Name",
                data: "firstName", // Fixed: changed from "dateIn" to "employeeName"
                className: 'noVis dt-center',
                orderable: true,
                defaultContent: "",
            },
            {
                title: "Date",
                data: "date",
                className: 'noVis dt-center',
                orderable: true,
                defaultContent: "",
                render: function (data, type, row) {
                    return data ? _dateHelper.formatShortDate(data) : '';
                }
            },
            {
                title: "Time",
                data: "time",
                className: 'noVis dt-center',
                orderable: false,
                defaultContent: "",
                render: function (data, type, row) {
                    return data ? _dateHelper.formatLocalShortTime(data) : '';
                }
            },
            {
                title: "Log Type",
                data: "logType",
                className: 'noVis dt-center',
                orderable: false,
                defaultContent: ""
            },
            {
                title: "Device Name",
                data: "deviceName",
                className: 'noVis dt-center',
                orderable: false,
                defaultContent: ""
            }
        ];
        return columns;
    };

    let initializeGrids = () => {
        // Initialize with empty data first
        initializeGrid([]);

        // Then trigger filter to load actual data
        const button = document.getElementById("filter");
        if (button) {
            button.click();
        }
    };

    $(document).ready(function () {
        // Check if table element exists
        if ($(tableName).length === 0) {
            console.error('Table element not found:', tableName);
            return;
        }

        attachEvents();
        initializeGrids();
    });

})(jQuery);