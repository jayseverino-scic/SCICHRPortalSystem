(function ($) {
    const _apiHelper = new ApiHelper();
    const _dateHelper = new DateHelper();
    const tableName = '#biometrics-log-grid';
    let dataTable = null;
    let requestVersion = 0;
    let filterTimer = null;
    let resetPagePending = false;
    let pendingPageCancel = null;
    let activeFilter = null;
    let filteredCount = 0;
    let isImporting = false;
    let isExporting = false;

    const setPageStatus = message => $('#biometrics-page-status').text(message);
    const updateExportButtons = () => dataTable?.buttons().enable(!!activeFilter && filteredCount > 0 && !isImporting && !isExporting);
    const cancelScheduledFilter = () => {
        clearTimeout(filterTimer);
        filterTimer = null;
    };
    const invalidatePage = () => {
        requestVersion++;
        activeFilter = null;
        if (pendingPageCancel) pendingPageCancel();
        updateExportButtons();
    };

    const validDateRange = () => {
        const start = $('#start-date-filter').val();
        const end = $('#end-date-filter').val();
        if (start && end && start > end) {
            setPageStatus('Start date must be on or before end date.');
            return false;
        }
        return true;
    };

    const refreshCurrentFilter = (resetPage = false) => {
        cancelScheduledFilter();
        invalidatePage();
        if (!dataTable || !validDateRange()) return;
        const reset = resetPage || resetPagePending;
        resetPagePending = false;
        dataTable.ajax.reload(null, reset);
    };

    const scheduleCurrentFilter = () => {
        cancelScheduledFilter();
        invalidatePage();
        resetPagePending = true;
        if (!validDateRange()) return;
        setPageStatus('Loading biometrics logs...');
        filterTimer = setTimeout(() => refreshCurrentFilter(true), 300);
    };

    const onClickFilter = event => {
        event.preventDefault();
        refreshCurrentFilter(true);
    };

    const onChangeStartFilter = () => {
        const start = $('#start-date-filter').val();
        const end = $('#end-date-filter').val();
        if (start && end && start > end) $('#end-date-filter').val(start);
        if (start) $('#end-date-filter').attr('min', start);
        else $('#end-date-filter').removeAttr('min');
    };

    const buildFilter = request => {
        const parameters = new URLSearchParams();
        for (const [name, selector] of [['startDate', '#start-date-filter'], ['endDate', '#end-date-filter'], ['projectName', '#project']]) {
            const value = $(selector).val();
            if (value) parameters.set(name, value);
        }
        if (request.search.value) parameters.set('search', request.search.value);
        const order = request.order.filter(sort => request.columns[sort.column].orderable);
        for (const sort of order.length ? order : [{ column: 4, dir: 'desc' }, { column: 5, dir: 'desc' }]) {
            parameters.append('sortColumns', request.columns[sort.column].data);
            parameters.append('sortDirections', sort.dir);
        }
        return parameters;
    };

    const requestGridPage = async (request, callback) => {
        cancelScheduledFilter();
        if (resetPagePending) {
            resetPagePending = false;
            if (request.start > 0) {
                dataTable.page(0).draw('page');
                return;
            }
        }
        invalidatePage();
        const version = requestVersion;
        const empty = { draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] };
        const cancel = () => {
            if (pendingPageCancel === cancel) pendingPageCancel = null;
            callback(empty);
        };
        pendingPageCancel = cancel;
        if (!validDateRange()) {
            cancel();
            return;
        }
        const filter = buildFilter(request).toString();
        const parameters = new URLSearchParams(filter);
        parameters.set('draw', request.draw);
        parameters.set('start', request.start);
        parameters.set('length', request.length);
        setPageStatus('Loading biometrics logs...');
        try {
            const response = await _apiHelper.get({ url: `Authenticated/BiometricsLog/Page?${parameters}` });
            if (version !== requestVersion) return;
            if (!response.ok) throw new Error('Page unavailable');
            const json = await response.json();
            if (version !== requestVersion) return;
            if (!json || json.draw !== request.draw || !Array.isArray(json.data) || json.data.length > request.length ||
                !json.data.every(row => row && Number.isInteger(row.biometricsLogId)) ||
                !Number.isInteger(json.recordsTotal) || !Number.isInteger(json.recordsFiltered) ||
                json.recordsFiltered < 0 || json.recordsTotal < json.recordsFiltered) throw new Error('Invalid page');
            pendingPageCancel = null;
            if (request.start > 0 && request.start >= json.recordsFiltered) {
                dataTable.page(Math.max(0, Math.ceil(json.recordsFiltered / request.length) - 1)).draw('page');
                return;
            }
            activeFilter = filter;
            filteredCount = json.recordsFiltered;
            callback(json);
            setPageStatus('');
            updateExportButtons();
        } catch (_) {
            if (version !== requestVersion) return;
            cancel();
            setPageStatus('Could not refresh the selected filter. Try again.');
        }
    };

    const getDropdownData = async () => {
        const select = $('#project');
        const selectedValue = select.val() || '';
        select.prop('disabled', true);
        $('#project-filter-status').text('Loading projects...');
        try {
            const response = await _apiHelper.get({ url: 'Authenticated/Project' });
            if (!response.ok) throw new Error('Projects unavailable');
            const items = await response.json();
            if (!Array.isArray(items) || items.some(item => !item || typeof item.name !== 'string' || !item.name.trim())) {
                throw new Error('Invalid projects');
            }
            select.empty().append($('<option>').val('').text('All Projects'));
            for (const item of items) select.append($('<option>').val(item.name).text(item.name));
            if (selectedValue && !items.some(item => item.name === selectedValue)) {
                select.append($('<option>').val(selectedValue).text(selectedValue));
            }
            if (!items.length) select.append($('<option>').val('').text('No data available'));
            select.val(selectedValue).trigger('change.select2');
            $('#project-filter-status').text(items.length ? '' : 'No projects available. Ask an administrator to add one.');
        } catch (_) {
            $('#project-filter-status').text('Projects could not be loaded. Refresh the page and try again.');
        } finally {
            select.prop('disabled', false);
        }
    };

    const setImporting = importing => {
        isImporting = importing;
        $('#import, #dbfilter, #upload-file').prop('disabled', importing);
        updateExportButtons();
    };

    const completeImport = (result, fileImport) => {
        if (!result || typeof result !== 'object' ||
            (!Number.isInteger(result.inserted) && typeof result.message !== 'string')) {
            $('#biometrics-import-status').text('The import response could not be confirmed. Filter the logs before retrying the import.');
            return;
        }
        const inserted = result.inserted ?? 0;
        const failed = result.failed ?? 0;
        const skipped = result.skippedCount ?? 0;
        const message = result.message || `Import completed: ${inserted} inserted, ${failed} failed, ${skipped} skipped.`;
        $('#biometrics-import-status').text(message);
        if (failed > 0) toastr.warning(message);
        else if (inserted > 0) toastr.success(message);
        else toastr.info(message);
        if (fileImport && failed === 0) $('#upload-file').val('');
        // Import endpoints return summaries, not rows. A failed refresh must not retry the import.
        refreshCurrentFilter(true);
    };

    const onSubmitUploadForm = event => {
        event.preventDefault();
        if (isImporting) return;
        const fileInput = $('#upload-file')[0];
        if (!fileInput.files.length) {
            Swal.fire('No File!', 'Please Select File', 'error');
            return;
        }
        const projectName = $('#project').val();
        if (!projectName) {
            Swal.fire('Project required', 'Select a project before importing the file.', 'error');
            return;
        }
        setImporting(true);
        invalidatePage();
        $('#biometrics-import-status').text('Uploading file. The import finishes after server processing.');
        try {
            _apiHelper.ajaxRequest('POST', {
                url: 'Authenticated/BiometricsLog/Import',
                dataObject: { file: fileInput.files[0], projectName },
                xhr: () => {
                    const xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener('progress', progress => {
                        if (progress.lengthComputable) {
                            const percent = Math.round(progress.loaded / progress.total * 100);
                            $('#biometrics-import-status').text(`Upload ${percent}%. Waiting for server import completion.`);
                        }
                    });
                    return xhr;
                },
                error: () => {
                    $('#biometrics-import-status').text('The import could not be confirmed. Filter the logs before retrying.');
                    toastr.error('The file import could not be confirmed.');
                },
                success: result => completeImport(result, true),
                complete: () => setImporting(false)
            });
        } catch (_) {
            setImporting(false);
            $('#biometrics-import-status').text('The import could not be confirmed. Filter the logs before retrying.');
        }
    };

    const onClickDbFilter = async event => {
        event.preventDefault();
        if (isImporting) return;
        const projectName = $('#project').val();
        const start = $('#start-date-filter').val();
        const end = $('#end-date-filter').val();
        if (!projectName || !start || !end) {
            $('#biometrics-import-status').text('Select a project and both dates before importing records.');
            return;
        }
        if (!validDateRange()) return;
        const parameters = new URLSearchParams({ startImport: start, endImport: `${end}T23:59:59.9999999`, projectName });
        setImporting(true);
        invalidatePage();
        $('#biometrics-import-status').text('Importing records...');
        try {
            const response = await _apiHelper.get({ url: `Authenticated/BiometricsLog/ImportDb?${parameters}` });
            if (!response.ok) throw new Error('Import unavailable');
            completeImport(await response.json(), false);
        } catch (_) {
            $('#biometrics-import-status').text('The import could not be confirmed. Filter the logs before retrying.');
            toastr.error('The record import could not be confirmed.');
        } finally {
            setImporting(false);
        }
    };

    const escapeText = value => String(value ?? '').replace(/[&<>"']/g, character =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character]);

    const exportFiltered = async (type, event, table, node, config, buttonContext) => {
        event?.preventDefault?.();
        if (isExporting || isImporting || !activeFilter) return;
        if (!filteredCount) {
            toastr.info('No biometrics logs match these filters.');
            return;
        }
        // Open synchronously with the user click so browser popup blocking does not swallow Print.
        const printWindow = type === 'print' ? window.open('', '_blank') : null;
        if (type === 'print' && !printWindow) {
            toastr.error('Allow popups to open the print view.');
            return;
        }
        const filter = activeFilter;
        const version = requestVersion;
        isExporting = true;
        updateExportButtons();
        try {
            if (printWindow) {
                printWindow.opener = null;
                printWindow.document.write('<p>Loading filtered biometrics logs...</p>');
            }
            const response = await _apiHelper.get({ url: `Authenticated/BiometricsLog/Export?${filter}` });
            if (!response.ok) throw new Error('Export unavailable');
            const rows = await response.json();
            if (!Array.isArray(rows) || rows.some(row => !row || !Number.isInteger(row.biometricsLogId))) throw new Error('Invalid export');
            if (version !== requestVersion) {
                printWindow?.close();
                toastr.info('The table changed while loading. Export again with the current filters.');
                return;
            }
            if (!rows.length) {
                printWindow?.close();
                toastr.info('No biometrics logs match these filters.');
                return;
            }
            const columns = getColumns();
            const header = columns.map(column => column.title);
            const body = rows.map((row, index) => columns.map(column => {
                const value = column.data ? row[column.data] : null;
                return column.render ? column.render(value, 'export', row, { row: index }) : value ?? '';
            }));
            if (printWindow) {
                if (printWindow.closed) return;
                const cells = (values, tag) => '<tr>' + values.map(value => `<${tag}>${escapeText(value)}</${tag}>`).join('') + '</tr>';
                printWindow.document.open();
                printWindow.document.write('<!doctype html><html><head><meta charset="utf-8"><title>Biometrics Logs</title>' +
                    '<style>@page{size:A4 landscape;margin:10mm}body{font-family:Arial,sans-serif;font-size:10px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #aaa;padding:4px;overflow-wrap:anywhere}thead{display:table-header-group}tr{break-inside:avoid}</style>' +
                    '</head><body><h1>Biometrics Logs</h1><table><thead>' + cells(header, 'th') + '</thead><tbody>' +
                    body.map(values => cells(values, 'td')).join('') + '</tbody></table></body></html>');
                printWindow.document.close();
                printWindow.focus();
                printWindow.print();
            } else {
                await new Promise((resolve, reject) => {
                    // Buttons 2.2 signals asynchronous XLSX completion through processing(false).
                    const nativeContext = Object.create(buttonContext);
                    const timeout = setTimeout(() => {
                        reject(new Error('Export generation timed out'));
                        buttonContext.processing?.(false);
                    }, 120000);
                    nativeContext.processing = processing => {
                        buttonContext.processing?.(processing);
                        if (!processing) {
                            clearTimeout(timeout);
                            resolve();
                        }
                        return nativeContext;
                    };
                    try {
                        $.fn.dataTable.ext.buttons[type].action.call(nativeContext, event, table, node, {
                            ...config, footer: false,
                            exportOptions: { ...config.exportOptions, customizeData(data) {
                                data.header = header;
                                data.body = body;
                                data.footer = null;
                            } }
                        });
                    } catch (error) {
                        clearTimeout(timeout);
                        reject(error);
                        buttonContext.processing?.(false);
                    }
                });
            }
        } catch (_) {
            printWindow?.close();
            toastr.error('Could not export biometrics logs. Try again.');
        } finally {
            isExporting = false;
            updateExportButtons();
        }
    };

    const filteredExportButton = (type, label) => ({
        ...(type === 'print' ? {} : { extend: type }),
        text: `${label} (filtered)`, className: 'btn btn-primary ml-3', title: 'Biometrics Logs', filename: 'Biometrics Logs - filtered',
        exportOptions: { modifier: { selected: null } },
        ...(type === 'pdfHtml5' ? { orientation: 'landscape', pageSize: 'A4' } : {}),
        action: function (event, table, node, config) {
            return exportFiltered(type, event, table, node, config, this);
        }
    });

    const getColumns = () => {
        const text = (value, type) => type === 'display' ? escapeText(value) : value ?? '';
        const column = (title, data, orderable = true, render = text) => ({ title, data, orderable, render, defaultContent: '', className: 'noVis dt-center' });
        return [
            column('No.', null, false, (_, type, row, meta) => meta.row + 1 + (type === 'display' ? meta.settings._iDisplayStart : 0)),
            column('Personnel Id', 'personnelId'),
            column('Last Name', 'lastName'),
            column('First Name', 'firstName'),
            column('Date Logged', 'date', true, value => value ? _dateHelper.formatShortDate(value) : ''),
            column('Time Logged', 'time', true, value => value ? _dateHelper.formatLocalShortTime(value) : ''),
            column('Log Type', 'logType', false),
            column('Project', 'projectName', false)
        ];
    };

    const initializeGrid = () => {
        if (dataTable) return;
        dataTable = $(tableName).DataTable({
            lengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            serverSide: true,
            processing: true,
            searching: true,
            ordering: true,
            scrollY: '350px',
            scrollX: true,
            order: [[4, 'desc'], [5, 'desc']],
            ajax: requestGridPage,
            columns: getColumns(),
            pageLength: 5,
            dom: '<"d-flex justify-content-between align-items-center mb-2"<"pull-left"lB><"pull-right"f>>tipr',
            buttons: [filteredExportButton('excelHtml5', 'Excel'), filteredExportButton('pdfHtml5', 'PDF'), filteredExportButton('print', 'Print')],
            language: { emptyTable: 'No biometrics log data available', zeroRecords: 'No matching records found', search: 'Search:' }
        });
        $('#biometrics-log-grid_filter input').attr('maxlength', 200).off('.DT')
            .on('input.biometricsLogs search.biometricsLogs', function () {
                dataTable.search(this.value);
                scheduleCurrentFilter();
            });
        updateExportButtons();
    };

    const attachEvents = () => {
        onChangeStartFilter();
        $('#filter').on('click', onClickFilter);
        $('#dbfilter').on('click', onClickDbFilter);
        $('#import').on('click', onSubmitUploadForm);
        $('#start-date-filter').on('change', onChangeStartFilter);
        $('#start-date-filter, #end-date-filter, #project').on('change', scheduleCurrentFilter);
    };

    // The layout loads jQuery 1.11.3, whose ready callbacks do not recognize async functions.
    $(document).ready(function () {
        if (!$(tableName).length) return;
        attachEvents();
        try { initializeGrid(); }
        catch (_) { setPageStatus('Could not initialize the table. Refresh the page and try again.'); }
        return getDropdownData();
    });
})(jQuery);
