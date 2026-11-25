(function ($) {
    'use strict';

    const api = new ApiHelper();
    const endpoint = 'Authenticated/Leave';
    const element = document.getElementById('leave-import-modal');
    const modal = bootstrap.Modal.getOrCreateInstance(element, { backdrop: 'static', keyboard: false });
    const form = $('#leave-import-form');
    const fileInput = $('#leave-import-file');
    const status = $('#leave-import-status');
    let importing = false;

    const attachEvents = () => {
        $('#leave-import').on('click', onImportButtonClicked);
        $('#leave-template').on('click', downloadTemplate);
        fileInput.on('change', onFileChanged);
        form.on('submit', onImport);
        element.addEventListener('hide.bs.modal', onImportModalHiding);
        element.addEventListener('shown.bs.modal', onImportModalShown);
        element.addEventListener('hidden.bs.modal', onImportModalHidden);
    };

    const setStatus = (message, error = false) => status.text(message).toggleClass('is-error', error);
    const updateControls = () => {
        form.find('input, button').prop('disabled', importing);
        $('#leave-import-submit').prop('disabled', importing || !fileInput[0].files.length)
            .text(importing ? 'Importing...' : 'Import');
        $('#leave-import').prop('disabled', importing);
        if (importing) form.attr('aria-busy', 'true');
        else form.removeAttr('aria-busy');
    };

    const errorMessage = response => api.readErrorMessage(response, {
        fallback: 'The request failed.',
        forbidden: 'Administrator access is required.',
        serverError: 'The server could not complete the import.'
    });

    const clearResults = () => {
        setStatus('');
        $('#leave-import-errors').prop('hidden', true).find('tbody').empty();
    };

    const onImport = async event => {
        event.preventDefault();
        if (importing) return;
        const file = fileInput[0].files[0];
        if (!file || !/\.xlsx$/i.test(file.name) || file.size === 0 || file.size > 10 * 1024 * 1024) {
            setStatus('Select a non-empty .xlsx file no larger than 10 MiB.', true);
            return;
        }
        clearResults();
        importing = true;
        updateControls();
        setStatus('Importing Leave records. Please wait for the final result.');
        let uncertain = true;
        try {
            const response = await api.postForm({
                url: `${endpoint}/Import`, data: { file }, requesterName: $('#current-user').text(),
                requestOrigin: 'Leave Excel Import', requestSystem: 'SCIC HR Portal'
            });
            if (!response.ok) {
                uncertain = response.status >= 500;
                throw new Error(await errorMessage(response));
            }
            const result = await response.json();
            if (!Number.isInteger(result.totalRows) || !Number.isInteger(result.imported) || !Number.isInteger(result.rejected) ||
                result.totalRows < 0 || result.totalRows > 1000 || result.imported < 0 || result.rejected < 0 ||
                result.totalRows !== result.imported + result.rejected || !Array.isArray(result.errors) || result.errors.length !== result.rejected ||
                result.errors.some(error => !Number.isInteger(error.rowNumber) || typeof error.employeeNo !== 'string' || typeof error.message !== 'string'))
                throw new Error('The server did not return a complete import report.');
            uncertain = false;
            const body = $('#leave-import-errors tbody').empty();
            for (const error of result.errors) {
                $('<tr>').append($('<td>').text(error.rowNumber), $('<td>').text(error.employeeNo), $('<td>').text(error.message)).appendTo(body);
            }
            $('#leave-import-errors').prop('hidden', result.rejected === 0);
            setStatus(`${result.imported} imported; ${result.rejected} rejected.` +
                (result.rejected ? ' Correct the rejected rows and upload a file containing only those rows.' : ' Import complete.'));
            fileInput.val('');
            $(document).trigger('leave:imported');
            if (result.imported) toastr.success(`${result.imported} Leave records imported.`);
            else toastr.warning('No records were imported. Review the rejected rows.');
        } catch (error) {
            setStatus((error.message || 'The import could not be completed.') +
                (uncertain ? ' Some rows may already have been saved. Review the refreshed list before retrying.' : ''), true);
            if (uncertain) $(document).trigger('leave:imported');
        } finally {
            importing = false;
            updateControls();
        }
    };

    const downloadTemplate = async () => {
        const button = $('#leave-template').prop('disabled', true);
        try {
            const response = await api.get({ url: `${endpoint}/ImportTemplate` });
            if (!response.ok) throw new Error(await errorMessage(response));
            const url = URL.createObjectURL(await response.blob());
            const link = document.createElement('a');
            link.href = url;
            link.download = 'LeaveImportTemplate.xlsx';
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.setTimeout(() => URL.revokeObjectURL(url), 0);
        } catch (error) {
            $('#leave-page-status').text(error.message || 'The template could not be downloaded.');
        } finally {
            button.prop('disabled', false);
        }
    };

    const onImportButtonClicked = () => {
        if (importing) return;

        fileInput.val('');
        clearResults();
        updateControls();
        modal.show();
    };

    const onFileChanged = () => {
        clearResults();
        updateControls();
    };

    const onImportModalHiding = event => {
        if (importing) {
            event.preventDefault();
        }
    };

    const onImportModalShown = () => {
        $('.modal-backdrop').last().addClass('leave-backdrop');
        fileInput.trigger('focus');
    };

    const onImportModalHidden = () => $('#leave-import').trigger('focus');

    $(attachEvents);
})(jQuery);
