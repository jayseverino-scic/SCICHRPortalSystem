(function ($) {
    'use strict';
    const api = new ApiHelper();
    const fields = ['employeeNo', 'employeeName', 'position', 'departmentName', 'projectName', 'attendanceDate',
        'shiftStart', 'shiftEnd', 'timeIn', 'timeOut', 'lateMinutes', 'undertimeMinutes'];
    const headers = ['Employee Id', 'Employee Name', 'Position', 'Department', 'Project', 'Attendance Date',
        'Shift Start', 'Shift End', 'Time In', 'Time Out', 'Late Minutes', 'Undertime Minutes'];
    const textRenderer = $.fn.dataTable.render.text();
    let grid, requestVersion = 0, activeFilter = null, filteredCount = 0, exporting = false, searchTimer;

    const escapeText = value => $('<span>').text(String(value ?? '')).html();
    const status = message => $('#tardiness-status').text(message);
    const updateButtons = () => grid?.buttons().enable(!!activeFilter && filteredCount > 0 && !exporting);
    const invalidate = () => {
        requestVersion++;
        activeFilter = null;
        updateButtons();
    };
    const valueText = (value, index) => {
        if (value === null || value === undefined || value === '') return '—';
        if (index >= 10) return Number(value); // Keep saved minutes numeric in exports.
        if (index >= 5) {
            const date = moment.parseZone(value);
            return date.isValid() ? date.format(index === 5 ? 'YYYY-MM-DD' : 'YYYY-MM-DD HH:mm:ss') : '—';
        }
        return String(value);
    };
    const parameters = filter => {
        const params = new URLSearchParams({ ProjectId: filter.projectId, Search: filter.search });
        if (filter.fromDate) params.set('FromDate', filter.fromDate);
        if (filter.toDate) params.set('ToDate', filter.toDate);
        filter.sortColumns.forEach(value => params.append('SortColumns', value));
        filter.sortDirections.forEach(value => params.append('SortDirections', value));
        return params;
    };
    const readFilter = order => {
        const form = document.getElementById('tardiness-filter');
        const from = document.getElementById('tardiness-from');
        const to = document.getElementById('tardiness-to');
        to.setCustomValidity(from.value && to.value && from.value > to.value ? 'To must be on or after From.' : '');
        if (!form.reportValidity()) return null;
        const sorts = order.filter(item => fields[item.column] && item.column !== 2);
        return { fromDate: from.value, toDate: to.value, projectId: $('#tardiness-project').val() || '0',
            projectName: $('#tardiness-project option:selected').text(), search: $('#tardiness-search').val().trim(),
            sortColumns: sorts.length ? sorts.map(item => fields[item.column]) : ['timeIn'],
            sortDirections: sorts.length ? sorts.map(item => item.dir) : ['desc'] };
    };
    const loadPage = async (request, callback) => {
        const version = ++requestVersion;
        activeFilter = null;
        updateButtons();
        $('#tardiness-warning').prop('hidden', true).text('');
        const filter = readFilter(request.order);
        const empty = () => callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
        if (!filter) { status('Check the filter parameters.'); empty(); return; }
        status('Loading saved attendance…');
        const params = parameters(filter);
        params.set('Draw', request.draw); params.set('Start', request.start); params.set('Length', request.length);
        try {
            const response = await api.get({ url: `Authenticated/Tardiness/Page?${params}` });
            if (version !== requestVersion) { empty(); return; }
            if (!response.ok) throw new Error('Request failed');
            const result = await response.json();
            if (version !== requestVersion) { empty(); return; }
            if (!Array.isArray(result.data) || !Number.isInteger(result.recordsFiltered)) throw new Error('Invalid report');
            activeFilter = filter;
            filteredCount = result.recordsFiltered;
            $('#tardiness-warning').text(result.warning || '').prop('hidden', !result.warning);
            status(filteredCount ? `${filteredCount.toLocaleString()} matching attendance records.` : 'No saved computed attendance matches these filters. Compute attendance first if needed.');
            callback(result);
            updateButtons();
        } catch (_) {
            if (version !== requestVersion) { empty(); return; }
            filteredCount = 0;
            status('Could not load attendance. Select Search to try again.');
            empty();
            updateButtons();
        }
    };

    const restoreExcelText = (workbook, body) => {
        // Buttons guesses numeric, percentage and formula-like values. Preserve identifiers
        // and descriptive fields as literal strings before the workbook is serialized.
        const sheet = workbook.xl.worksheets['sheet1.xml'];
        const rows = Array.from(sheet.getElementsByTagName('row'));
        const cells = row => Array.from(row.getElementsByTagName('c'));
        const start = rows.findIndex(row => cells(row).find(cell => /^A[0-9]+$/.test(cell.getAttribute('r')))?.textContent === headers[0]);
        if (start < 0) throw new Error('Export headers unavailable');
        body.forEach((values, rowIndex) => values.slice(0, 5).forEach((value, columnIndex) => {
            const letter = String.fromCharCode(65 + columnIndex);
            const cell = cells(rows[start + rowIndex + 1]).find(item => item.getAttribute('r').startsWith(letter) && /^[A-E][0-9]+$/.test(item.getAttribute('r')));
            cell.setAttribute('t', 'inlineStr'); cell.removeAttribute('s'); cell.replaceChildren();
            const inline = sheet.createElementNS(cell.namespaceURI, 'is');
            const text = sheet.createElementNS(cell.namespaceURI, 't');
            text.setAttributeNS('http://www.w3.org/XML/1998/namespace', 'xml:space', 'preserve');
            text.textContent = String(value).replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F]/g, '');
            inline.appendChild(text); cell.appendChild(inline);
        }));
    };
    const exportReport = async (type, event, table, node, config, context) => {
        event?.preventDefault?.();
        if (exporting || !activeFilter || !filteredCount) return;
        const printWindow = type === 'print' ? window.open('', '_blank') : null;
        if (type === 'print' && !printWindow) { status('Allow popups to open the print view.'); return; }
        const filter = activeFilter;
        const version = requestVersion;
        exporting = true; updateButtons(); status('Preparing all matching rows…');
        try {
            if (printWindow) { printWindow.opener = null; printWindow.document.write('<p>Loading attendance…</p>'); }
            const response = await api.get({ url: `Authenticated/Tardiness/Export?${parameters(filter)}` });
            if (!response.ok) throw new Error('Export failed');
            const result = await response.json();
            if (version !== requestVersion) { printWindow?.close(); status('Filters changed. Select Search, then export again.'); return; }
            if (!Array.isArray(result.data)) throw new Error('Invalid export');
            if (!result.data.length) { printWindow?.close(); status('No matching attendance to export. Select Search to refresh.'); return; }
            const body = result.data.map(row => fields.map((field, index) => valueText(row[field], index)));
            const originalBody = body.map(row => row.slice());
            const dateRange = filter.fromDate && filter.toDate ? `From ${filter.fromDate} to ${filter.toDate}`
                : filter.fromDate ? `From ${filter.fromDate} onwards` : filter.toDate ? `Through ${filter.toDate}` : 'All dates';
            const description = `${dateRange} | Project: ${filter.projectName} | Employee: ${filter.search || 'All'} | ${body.length} records` +
                (result.warning ? ` | ${result.warning}` : '');
            $('#tardiness-warning').text(result.warning || '').prop('hidden', !result.warning);
            if (printWindow) {
                if (printWindow.closed) return;
                const rowMarkup = (row, tag) => '<tr>' + row.map(value => `<${tag}>${escapeText(value)}</${tag}>`).join('') + '</tr>';
                printWindow.document.open();
                printWindow.document.write('<!doctype html><html><head><meta charset="utf-8"><title>Tardiness Summary</title>' +
                    '<style>@page{size:A3 landscape;margin:10mm}body{font:9px Arial}table{width:100%;border-collapse:collapse;table-layout:fixed}td,th{border:1px solid #aaa;padding:4px;overflow-wrap:anywhere}thead{display:table-header-group}tr{break-inside:avoid}</style>' +
                    '</head><body><h1>Tardiness Summary</h1><p>' + escapeText(description) + '</p><table><thead>' + rowMarkup(headers, 'th') +
                    '</thead><tbody>' + body.map(row => rowMarkup(row, 'td')).join('') + '</tbody></table></body></html>');
                printWindow.document.close(); printWindow.focus(); printWindow.print();
            } else {
                await new Promise((resolve, reject) => {
                    const nativeContext = Object.create(context);
                    const timeout = setTimeout(() => { context.processing?.(false); reject(new Error('Export timed out')); }, 120000);
                    nativeContext.processing = value => {
                        context.processing?.(value);
                        if (!value) { clearTimeout(timeout); resolve(); }
                        return nativeContext;
                    };
                    try {
                        $.fn.dataTable.ext.buttons[type].action.call(nativeContext, event, table, node, {
                            ...config, footer: false, messageTop: description,
                            ...(type === 'excelHtml5' ? { customize: workbook => restoreExcelText(workbook, originalBody) } : {}),
                            exportOptions: { customizeData(data) { data.header = headers; data.body = body; data.footer = null; } }
                        });
                    } catch (error) { clearTimeout(timeout); context.processing?.(false); reject(error); }
                });
            }
            status(`Prepared ${body.length.toLocaleString()} matching attendance records.`);
        } catch (_) { printWindow?.close(); status('Could not export attendance. Try again.'); }
        finally { exporting = false; updateButtons(); }
    };
    const exportButton = (type, label) => ({
        ...(type === 'print' ? {} : { extend: type }), text: label, title: 'Tardiness Summary', filename: 'Tardiness Summary',
        ...(type === 'pdfHtml5' ? { orientation: 'landscape', pageSize: 'A3', customize(doc) {
            doc.defaultStyle.fontSize = 7; doc.styles.tableHeader.fontSize = 8;
            const table = doc.content.find(item => item.table)?.table;
            if (table) table.widths = ['auto', '*', '*', '*', '*', 'auto', 'auto', 'auto', 'auto', 'auto', 'auto', 'auto'];
        } } : {}),
        action: function (event, table, node, config) { return exportReport(type, event, table, node, config, this); }
    });
    const initialize = async () => {
        $('#tardiness-from, #tardiness-to').val('');
        try {
            const response = await api.get({ url: 'Authenticated/Project' });
            if (!response.ok) throw new Error('Projects unavailable');
            const projects = await response.json();
            if (!Array.isArray(projects)) throw new Error('Invalid projects');
            projects.forEach(project => $('#tardiness-project').append($('<option>').val(project.id).text(project.name)));
        } catch (_) { $('#tardiness-project-error').text('Projects could not be loaded. All Projects remains available. Reload the page to retry.').prop('hidden', false); }
        if (!$('#tardiness-project').hasClass('select2-hidden-accessible')) $('#tardiness-project').select2({ width: '100%' });
        grid = $('#tardiness-grid').DataTable({
            processing: true, serverSide: true, searching: false, scrollX: true, autoWidth: false,
            pageLength: 20, lengthMenu: [5, 10, 20, 40, 80], order: [[8, 'desc']], dom: '<"tardiness-table-toolbar"lB>rtip',
            buttons: [exportButton('print', 'Print'), exportButton('excelHtml5', 'Excel'), exportButton('pdfHtml5', 'PDF')],
            ajax: (request, callback) => { loadPage(request, callback); },
            columns: fields.map((field, index) => ({ data: field, title: headers[index], orderable: index !== 2,
                width: index === 1 ? '260px' : index < 5 ? '180px' : index === 5 || index >= 10 ? '150px' : '195px',
                className: index < 5 ? 'tardiness-text' : '',
                render(value, type) {
                    const formatted = valueText(value, index);
                    if (type !== 'display') return formatted;
                    return index >= 10 && typeof formatted === 'number' ? formatted.toLocaleString(undefined, { maximumFractionDigits: 2 }) : textRenderer.display(formatted);
                } })),
            language: { emptyTable: 'No saved computed attendance matches these filters.', processing: 'Loading attendance…' },
            drawCallback: function () { this.api().columns.adjust(); }
        });
        updateButtons();
        $('#tardiness-filter').on('submit', event => { event.preventDefault(); clearTimeout(searchTimer); grid.ajax.reload(); });
        $('#tardiness-from, #tardiness-to, #tardiness-project').on('change input', () => { invalidate(); status('Select Search to apply filters.'); });
        $('#tardiness-search').on('input', () => {
            invalidate(); clearTimeout(searchTimer);
            searchTimer = setTimeout(() => grid.ajax.reload(), 350);
        });
        $(window).on('resize.tardiness', () => grid.columns.adjust());
    };
    $(document).ready(function () { initialize(); });
})(jQuery);
