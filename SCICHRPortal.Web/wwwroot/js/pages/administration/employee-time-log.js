(function ($) {
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

    let _projects = [];
    let _currentproject = '';
    let _devices = [];
    let projectsLoaded = false;
    let devicesLoaded = false;
    let _currentDevice = '';
    let employeeLookupEpoch = 0;
    let employeeChoiceVersion = 0;
    let selectedEmployee = null;
    let recentEmployeeMatches = new Map();
    const employeeSearchState = {
        employeeNo: { term: '', generation: 0 },
        employeeName: { term: '', generation: 0 }
    };
    let activeEmployeeSearchKind = null;
    let lookupControllers = new Set();
    let hydrationController = null;
    let editingRecord = null;
    let editedLookupFields = new Set();
    let isSubmitting = false;
    let filterRequestVersion = 0;
    let filterDebounceTimer = null;
    let gridRefreshCompletion = null;
    let pendingGridCancel = null;
    let filterResetPending = false;
    let dataTable = null;
    let isDataTableInitialized = false; // Track initialization state

    let attachEvents = () => {
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#employee-time-log-form').on('submit', onFormSubmit);
        $('#employee-time-log-form #employeeName, #employee-time-log-form #employeeNo').on('change', onEmployeeChange);
        $('#projectTimeIn, #projectTimeOut, #deviceTimeIn, #deviceTimeOut').on('change', function () {
            if (editingRecord) editedLookupFields.add(this.id);
        });
        $('#filter').on(CLICK_EVENT, onClickFilter);
        $('#start-import-filter').on('change', onChangeStartImport)
        $('#start-import-filter, #end-import-filter, #project').on('change', scheduleCurrentFilter);
        $('#import').on(CLICK_EVENT, onClickImport);
    };

    let onClickImport = e => {
        e.preventDefault();
        cancelScheduledFilter();
        filterRequestVersion++;
        // Import replaces the page status and may fail before any replacement draw.
        if (pendingGridCancel) pendingGridCancel();
        _currentproject = $('#project').val() || '';
        let startImportDate = $('#start-import-filter').val();
        let endImportDate = $('#end-import-filter').val();
        if (!_currentproject || !startImportDate || !endImportDate) {
            $('#time-log-page-status').text('Select a project and both dates before getting imported logs. These fields are optional for Filter only.');
            return;
        }
        if (startImportDate > endImportDate) {
            $('#time-log-page-status').text('Start date must be on or before end date.');
            return;
        }
        $('#time-log-page-status').text('');
        endImportDate += 'T23:59:59';
        var request = _apiHelper.ajaxRequest('POST', {
            url: `Authenticated/EmployeeTimeLog/Import?startImportDate=${encodeURIComponent(startImportDate)}&endImportDate=${encodeURIComponent(endImportDate)}&projectName=${encodeURIComponent(_currentproject)}`,
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
                alert('Import failed!');
            },
            success: async function () {
                const refreshed = await refreshCurrentFilter();
                if (refreshed !== null) $('#time-log-page-status').text(refreshed ? '' : 'Import completed, but refreshing the time logs failed. Try Filter again.');
                alert('Import successful!');
            }
        });
    }

    let onChangeStartImport = () => {
        let startDateVal = $('#start-import-filter').val();
        let endDateVal = $('#end-import-filter').val();
        if (new Date(startDateVal) > new Date(endDateVal))
            $('#end-import-filter').val(startDateVal);

        if (startDateVal) $('#end-import-filter').attr('min', startDateVal);
        else $('#end-import-filter').removeAttr('min');
    };

    let cancelScheduledFilter = () => {
        window.clearTimeout(filterDebounceTimer);
        filterDebounceTimer = null;
    };

    let scheduleCurrentFilter = () => {
        cancelScheduledFilter();
        filterResetPending = true;
        // Invalidate old responses immediately, including during the debounce.
        filterRequestVersion++;
        $('#time-log-page-status').text('Loading time logs...');
        filterDebounceTimer = window.setTimeout(applyCurrentFilter, 300);
    };

    let refreshCurrentFilter = (resetPage = false) => {
        cancelScheduledFilter();
        const start = $('#start-import-filter').val();
        const end = $('#end-import-filter').val();
        if (!dataTable || (start && end && start > end)) {
            filterRequestVersion++;
            if (pendingGridCancel) pendingGridCancel();
            return Promise.resolve(false);
        }
        const shouldReset = resetPage || filterResetPending;
        filterResetPending = false;
        return new Promise(resolve => {
            if (gridRefreshCompletion) gridRefreshCompletion(null);
            gridRefreshCompletion = resolve;
            dataTable.ajax.reload(null, shouldReset);
        });
    };

    let requestGridPage = async (request, callback) => {
        cancelScheduledFilter();
        const completion = gridRefreshCompletion;
        gridRefreshCompletion = null;
        const requestVersion = ++filterRequestVersion;
        if (filterResetPending) {
            filterResetPending = false;
            if (request.start > 0) {
                gridRefreshCompletion = completion;
                dataTable.page(0).draw('page');
                return;
            }
        }
        _currentproject = $('#project').val() || '';
        const startDate = $('#start-import-filter').val();
        const endDate = $('#end-import-filter').val();
        const empty = { draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] };
        const finish = result => {
            if (pendingGridCancel === cancel) pendingGridCancel = null;
            if (completion) completion(result);
        };
        const cancel = () => { callback(empty); finish(null); };
        pendingGridCancel = cancel;
        if (startDate && endDate && startDate > endDate) {
            $('#time-log-page-status').text('Start date must be on or before end date.');
            callback(empty);
            finish(false);
            return;
        }
        const parameters = [];
        if (startDate) parameters.push(`startDate=${encodeURIComponent(startDate)}`);
        if (endDate) parameters.push(`endDate=${encodeURIComponent(endDate)}`);
        if (_currentproject) parameters.push(`projectName=${encodeURIComponent(_currentproject)}`);
        parameters.push(`draw=${request.draw}`, `start=${request.start}`, `length=${request.length}`);
        if (request.search.value) parameters.push(`search=${encodeURIComponent(request.search.value)}`);
        const order = request.order.filter(sort => request.columns[sort.column].orderable);
        for (const sort of order.length ? order : [{ column: 1, dir: 'asc' }]) {
            parameters.push(`sortColumns=${encodeURIComponent(request.columns[sort.column].data)}`, `sortDirections=${encodeURIComponent(sort.dir)}`);
        }
        $('#time-log-page-status').text('Loading time logs...');
        try {
            const response = await _apiHelper.get({
                url: `Authenticated/EmployeeTimeLog/Page?${parameters.join('&')}`,
            });
            if (requestVersion !== filterRequestVersion) { finish(null); return; }
            if (!response.ok) throw new Error('Time log page unavailable');
            const json = await response.json();
            if (requestVersion !== filterRequestVersion) { finish(null); return; }
            if (!json || json.draw !== request.draw || !Array.isArray(json.data) || json.data.length > request.length ||
                !Number.isInteger(json.recordsTotal) || !Number.isInteger(json.recordsFiltered) ||
                json.recordsFiltered < 0 || json.recordsTotal < json.recordsFiltered) throw new Error('Invalid time log page');
            // A delete (including another user's delete) can empty the last page.
            if (request.start > 0 && request.start >= json.recordsFiltered) {
                gridRefreshCompletion = completion;
                dataTable.page(Math.max(0, Math.ceil(json.recordsFiltered / request.length) - 1)).draw('page');
                return;
            }
            callback(json);
            $('#time-log-page-status').text('');
            finish(true);
        } catch (_) {
            if (requestVersion !== filterRequestVersion) { finish(null); return; }
            callback(empty);
            $('#time-log-page-status').text('Could not refresh the selected filter. Try again.');
            finish(false);
        }
    };

    let applyCurrentFilter = async () => {
        cancelScheduledFilter();
        const startDate = $('#start-import-filter').val();
        const endDate = $('#end-import-filter').val();
        if (startDate && endDate && startDate > endDate) {
            filterRequestVersion++;
            if (pendingGridCancel) pendingGridCancel();
            $('#employee-time-log-grid_processing').hide();
            $('#time-log-page-status').text('Start date must be on or before end date.');
            return;
        }
        const refreshed = await refreshCurrentFilter(true);
        if (refreshed !== null) {
            $('#time-log-page-status').text(refreshed ? '' : 'Could not refresh the selected filter. Try again.');
        }
    };

    let onClickFilter = e => {
        e.preventDefault();
        return applyCurrentFilter();
    };

    let abortEmployeeLookup = () => {
        employeeLookupEpoch++;
        employeeChoiceVersion++;
        for (const state of Object.values(employeeSearchState)) {
            state.term = '';
            state.generation++;
        }
        for (const controller of lookupControllers) controller.abort();
        lookupControllers.clear();
        if (hydrationController) hydrationController.abort();
        hydrationController = null;
        recentEmployeeMatches.clear();
    };

    let recordEmployeeSearch = (kind, value) => {
        const term = (value || '').trim();
        const state = employeeSearchState[kind];
        if (state.term !== term) {
            state.term = term;
            state.generation++;
            recentEmployeeMatches.clear();
        }
        return state.generation;
    };

    let onEmployeeSearchInput = event => {
        const kind = activeEmployeeSearchKind;
        if (!kind) return;
        const term = $(event.target).val().trim();
        recordEmployeeSearch(kind, term);
        const select2 = $(`#${kind}`).data('select2');
        if (select2 && select2.isOpen()) {
            select2.trigger('results:message', {
                message: term.length < 2 ? 'inputTooShort' : 'searching',
                args: { minimum: 2, input: term }
            });
        }
        setLookupStatus(term.length < 2
            ? 'Type at least 2 characters to search employees.'
            : 'Searching employees...');
    };

    let employeeLabel = (item, kind) => kind === 'employeeNo'
        ? (item.employeeNo || String(item.employeeId))
        : `${item.firstName || ''} ${item.lastName || ''}`.trim() || String(item.employeeId);

    let setEmployeeChoice = item => {
        selectedEmployee = item;
        for (const [selector, placeholder] of [['#employeeNo', 'Select Employee Number'], ['#employeeName', 'Select Employee Name']]) {
            const select = $(selector).empty().append($('<option>').val('').text(placeholder));
            if (item) select.append($('<option>').val(String(item.employeeId)).text(employeeLabel(item, selector.slice(1))));
            select.val(item ? String(item.employeeId) : '').trigger('change.select2');
        }
    };

    let isEmployeeItem = item => item && Number.isInteger(item.employeeId) && item.employeeId > 0 &&
        (item.employeeNo == null || typeof item.employeeNo === 'string') &&
        (item.firstName == null || typeof item.firstName === 'string') &&
        (item.lastName == null || typeof item.lastName === 'string');

    let employeeRequest = (path, signal) => fetch(`${_apiHelper.baseApiUrl}/${path}`, {
        method: 'GET', signal,
        headers: { Authorization: `bearer ${_cookieHelper.get('jsonWebToken')}` }
    }).then(async response => {
        if (response.status === 401) {
            if (typeof _apiHelper.logOut === 'function') _apiHelper.logOut();
            throw new Error('Session expired');
        }
        if (!response.ok) throw new Error('Employee lookup failed');
        return response.json();
    });

    let hydrateEmployee = async id => {
        if (hydrationController) hydrationController.abort();
        const controller = new AbortController();
        hydrationController = controller;
        const epoch = employeeLookupEpoch;
        const choiceVersion = employeeChoiceVersion;
        const previous = selectedEmployee;
        setLookupStatus('Loading employee...');
        try {
            const item = await employeeRequest(`Authenticated/EmployeeTimeLog/EmployeeLookup/${encodeURIComponent(id)}`, controller.signal);
            if (controller.signal.aborted || epoch !== employeeLookupEpoch || choiceVersion !== employeeChoiceVersion) return;
            if (!isEmployeeItem(item) || item.employeeId !== Number(id)) throw new Error('Invalid employee lookup');
            setEmployeeChoice(item);
            setLookupStatus('');
        } catch (_) {
            if (controller.signal.aborted || epoch !== employeeLookupEpoch || choiceVersion !== employeeChoiceVersion) return;
            setEmployeeChoice(previous);
            setLookupStatus('Employee could not be loaded or is inactive. Search again or retry the selection.');
        } finally {
            if (hydrationController === controller) hydrationController = null;
        }
    };

    let onEmployeeChange = function () {
        employeeChoiceVersion++;
        if (hydrationController) hydrationController.abort();
        hydrationController = null;
        const id = Number($(this).val());
        if (!Number.isInteger(id) || id <= 0) {
            setEmployeeChoice(null);
            setLookupStatus('');
            return;
        }
        const selectedResult = $(this).select2('data')[0];
        const resultItem = selectedResult && selectedResult.employee;
        const item = (isEmployeeItem(resultItem) && resultItem.employeeId === id && resultItem) ||
            recentEmployeeMatches.get(id) || (selectedEmployee && selectedEmployee.employeeId === id && selectedEmployee);
        if (item) {
            setEmployeeChoice(item);
            setLookupStatus('');
        } else {
            hydrateEmployee(id);
        }
    };

    let onEmployeeSelecting = function (event) {
        const result = event.params && event.params.args && event.params.args.data;
        if (!result || !result.employee) return;
        const kind = this.id;
        const openSearch = $('.select2-container--open .select2-search__field');
        if (openSearch.length) recordEmployeeSearch(kind, openSearch.val());
        if (result.queryEpoch !== employeeLookupEpoch ||
            result.queryGeneration !== employeeSearchState[kind].generation) {
            event.preventDefault();
            setLookupStatus('Search changed. Wait for updated employee results.');
        }
    };

    let onClickAddModal = function () {
        abortEmployeeLookup();
        editingRecord = null;
        editedLookupFields.clear();
        attachmentRetry = null;
        detailRequestVersion++;
        $('#timeLogAttachment').val('');
        applyRecordRules();
        hideShowColumnBaseOnAction(true);
        $('#employee-time-log-form')[0].reset();
        setEmployeeChoice(null);
        $('#dateIn').val(moment().format('YYYY-MM-DD'));
        $('#dateOut').val('');
        setFormStatus('');
        $('#employee-time-log-form').find(':submit').text('Add');
        $('#employee-time-log-modal').modal('show');
    };

    let setFormStatus = message => {
        const status = $('#employee-time-log-status');
        status.text(message);
        if (message) status.closest('.modal-body').scrollTop(0);
    };
    let setLookupStatus = message => $('#employee-lookup-status').text(message);

    let responseMessage = async (response, fallback) => {
        if (![400, 403, 404, 409].includes(response.status)) return fallback;
        try {
            const text = await response.text();
            let body;
            try { body = JSON.parse(text); }
            catch (_) { body = text; }
            let detail = typeof body === 'string' ? body : body && body.message;
            if (typeof detail !== 'string' && body && body.errors) {
                detail = Object.values(body.errors).flat().filter(value => typeof value === 'string').join(' ');
            }
            if (typeof detail === 'string' && detail.trim() &&
                !/[<>]|\b(?:stack\s*trace|\w*Exception)\b|\n\s*at\s/i.test(detail)) return detail.trim();
        } catch (_) { }
        return fallback;
    };

    let attachmentRetry = null;
    let detailRequestVersion = 0;

    async function getRecord(id) {
        const response = await _apiHelper.get({ url: `Authenticated/EmployeeTimeLog/${id}` });
        if (!response.ok) throw new Error(await responseMessage(response, 'The current time record could not be loaded.'));
        return response.json();
    }

    function applyRecordRules() {
        const form = $('#employee-time-log-form');
        form.find('.borrow-initial').prop('disabled', false);
        for (const field of TimeLogRecord.lockedFields(editingRecord)) {
            form.find(field === 'employeeId' ? '#employeeNo, #employeeName' : '#' + field).prop('disabled', true);
        }
        $('#time-log-edit-rule').text(TimeLogRecord.isProtected(editingRecord)
            ? 'Only empty record fields can be filled. OB and an attachment can still be added.' : '');
        $('#timeLogComment').val(editingRecord?.comment || '');
        $('#isOB').prop('checked', !!editingRecord?.isOB).prop('disabled', false);
        $('#timeLogAttachment').prop('disabled', !!editingRecord?.attachment);
        const info = $('#time-log-attachment-info').empty();
        if (editingRecord?.attachment) {
            $('<button type="button" class="btn btn-link">').text(editingRecord.attachment.fileName)
                .on('click', () => downloadAttachment(editingRecord).catch(error => setFormStatus(error.message))).appendTo(info);
        }
    }

    async function downloadAttachment(record) {
        if (!record?.attachment) throw new Error('This time record no longer has an attachment.');
        const response = await _apiHelper.get({
            url: `Authenticated/EmployeeTimeLog/${record.timeLogId}/attachment/${record.attachment.timeLogAttachmentId}`
        });
        if (!response.ok) throw new Error('The attachment could not be downloaded.');
        const url = URL.createObjectURL(await response.blob());
        const anchor = document.createElement('a');
        anchor.href = url; anchor.download = record.attachment.fileName;
        document.body.appendChild(anchor); anchor.click(); anchor.remove();
        window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    }

    async function finishRecordSave() {
        attachmentRetry = null;
        editingRecord = null;
        $('#employee-time-log-form')[0].reset();
        abortEmployeeLookup();
        setEmployeeChoice(null);
        $('#employee-time-log-modal').modal('hide');
        const refreshed = await refreshCurrentFilter();
        $('#time-log-page-status').text(refreshed ? 'Time record saved. Adjust filters if it is outside the current view.'
            : 'Time record saved, but refreshing the grid failed. Try Filter again.');
        toastr.success('Time record saved.');
    }

    async function uploadPendingAttachment() {
        // Reload because an earlier upload may have succeeded despite a lost response.
        const current = await getRecord(attachmentRetry.id);
        if (!current.attachment) {
            const file = $('#timeLogAttachment')[0].files[0];
            if (!file) throw new Error('Select one attachment to upload.');
            const body = new FormData();
            body.append('file', file);
            body.append('version', current.version);
            const response = await fetch(`${_apiHelper.baseApiUrl}/Authenticated/EmployeeTimeLog/${current.timeLogId}/attachment`, {
                method: 'POST', headers: { Authorization: `Bearer ${_cookieHelper.get('jsonWebToken')}` }, body
            });
            if (!response.ok) throw new Error(await responseMessage(response, 'The attachment could not be uploaded.'));
        }
        await finishRecordSave();
    }

    let onFormSubmit = async event => {
        event.preventDefault();
        if (isSubmitting) return;
        const form = $(event.target);
        const selectedFile = $('#timeLogAttachment')[0].files[0];
        if (selectedFile && (selectedFile.size <= 0 || selectedFile.size > 10 * 1024 * 1024 ||
            !/\.(pdf|jpe?g|png|docx|xls|xlsx)$/i.test(selectedFile.name))) {
            setFormStatus('Select a nonempty PDF, JPG/JPEG, PNG, DOCX, XLS or XLSX file up to 10 MB.');
            return;
        }
        if (attachmentRetry) {
            isSubmitting = true; form.find(':submit').prop('disabled', true);
            try { await uploadPendingAttachment(); }
            catch (error) { setFormStatus('Time record saved. Attachment upload failed: ' + error.message + ' Retry the attachment.'); }
            finally { isSubmitting = false; form.find(':submit').prop('disabled', false); }
            return;
        }
        form.validate();
        if (!form.valid()) return;
        const lockedEmployee = TimeLogRecord.lockedFields(editingRecord).has('employeeId');
        const numberId = lockedEmployee ? editingRecord.employeeId : Number($('#employeeNo').val());
        const nameId = lockedEmployee ? editingRecord.employeeId : Number($('#employeeName').val());
        if ((!lockedEmployee && hydrationController) || !Number.isInteger(numberId) || numberId <= 0 || numberId !== nameId) {
            setFormStatus('Select an employee in both fields and wait for the selection to finish loading.');
            return;
        }
        const values = _formHelper.toJsonString(event.target);
        values.EmployeeId = numberId;
        values.IsOB = $('#isOB').prop('checked');
        const data = TimeLogRecord.payload(editingRecord, values);
        if (!data.DateIn || (!data.TimeIn && !data.TimeOut) || (data.TimeOut && !values.DateOut)) {
            setFormStatus('Enter Date In and at least one time. Date Out is required for Time Out.');
            return;
        }
        const isAdd = !editingRecord;
        isSubmitting = true;
        form.find(':submit').prop('disabled', true);
        $('#busy-indicator-container').removeClass('d-none');
        setFormStatus('');
        try {
            const request = {
                url: 'Authenticated/EmployeeTimeLog', data,
                requestOrigin: 'Employee Time Logs', requesterName: $('#current-user').text(), requestSystem: SYSTEM
            };
            const response = await (isAdd ? _apiHelper.post(request) : _apiHelper.put(request));
            if (!response.ok) {
                setFormStatus(await responseMessage(response, 'The time record could not be saved.'));
                return;
            }
            const saved = await response.json();
            if (selectedFile) {
                attachmentRetry = { id: isAdd ? saved : saved.timeLogId };
                form.find('.borrow-initial, #isOB').prop('disabled', true);
                form.find(':submit').text('Retry attachment');
                await uploadPendingAttachment();
            } else {
                await finishRecordSave();
            }
        } catch (error) {
            setFormStatus(attachmentRetry
                ? 'Time record saved. Attachment upload failed: ' + error.message + ' Retry the attachment.'
                : 'Network or connection error while saving. Your entries are still here.');
        } finally {
            isSubmitting = false;
            form.find(':submit').prop('disabled', false);
            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let populateForm = (form, data) => {
        abortEmployeeLookup();
        setEmployeeChoice(null);
        editingRecord = data;
        attachmentRetry = null;
        $('#timeLogAttachment').val('');
        editedLookupFields.clear();
        setFormStatus('');
        $(form).find(':submit').text('Update');
        _formHelper.populateForm(form, data);
        let dateIn = moment(data.dateIn).format('YYYY-MM-DD');
        let dateOut = data.dateOut ? moment(data.dateOut).format('YYYY-MM-DD') : '';
        $(form).find('#dateIn').val(dateIn);
        $(form).find('#dateOut').val(dateOut);
        $(form).find('#isFlexibleShift').prop("checked", data.isFlexibleShift);
        $(form).find('#isNoShift').prop("checked", data.isNoShift);
        $(form).find('#isNoBreak').prop("checked", data.isNoBreak);
        $(form).find('#timeIn').val(data.timeIn ? moment(data.timeIn).format('HH:mm') : '');
        $(form).find('#timeOut').val(data.timeOut ? moment(data.timeOut).format('HH:mm') : '');
        if (data.shiftStart) {
            $(form).find('#shiftStart').val(moment(data.shiftStart).format('HH:mm'));
        }
        if (data.shiftEnd) {
            $(form).find('#shiftEnd').val(moment(data.shiftEnd).format('HH:mm'));
        }
        for (const field of ['ProjectTimeIn', 'ProjectTimeOut', 'DeviceTimeIn', 'DeviceTimeOut']) {
            const recordField = field.charAt(0).toLowerCase() + field.slice(1);
            const select = $(form).find(`#${recordField}`);
            const value = data[recordField] || '';
            if (value && !select.find('option').filter((_, option) => option.value === String(value)).length) {
                select.append($('<option>').val(String(value)).text(String(value)));
            }
            select.val(value).trigger('change.select2');
        }
        applyRecordRules();
        if (data.employeeId) {
            hydrateEmployee(Number(data.employeeId));
        }
    };

    let initializeEmployeeSearch = () => {
        setEmployeeChoice(null);
        setLookupStatus('Type at least 2 characters to search employees.');
        $('#employee-time-log-modal').on('input', '.select2-search__field', onEmployeeSearchInput);
        for (const selector of ['#employeeNo', '#employeeName']) {
            const kind = selector.slice(1);
            $(selector).select2({
                theme: 'bootstrap', width: '100%', dropdownParent: $('#employee-time-log-modal'),
                placeholder: kind === 'employeeNo' ? 'Select Employee Number' : 'Select Employee Name',
                minimumInputLength: 2, allowClear: true,
                ajax: {
                    delay: 300,
                    data: params => {
                        const term = (params.term || '').trim();
                        return { term, page: params.page || 1, epoch: employeeLookupEpoch,
                            generation: recordEmployeeSearch(kind, term) };
                    },
                    transport: (params, success, failure) => {
                        const term = (params.data.term || '').trim();
                        const page = params.data.page || 1;
                        const queuedEpoch = params.data.epoch ?? employeeLookupEpoch;
                        const queuedGeneration = params.data.generation ?? employeeSearchState[kind].generation;
                        const openSearch = $('.select2-container--open .select2-search__field');
                        if (queuedEpoch !== employeeLookupEpoch || queuedGeneration !== employeeSearchState[kind].generation ||
                            (openSearch.length && openSearch.val().trim() !== term))
                            return { abort() {} };
                        if (term.length < 2) {
                            setLookupStatus('Type at least 2 characters to search employees.');
                            success({ data: [], more: false });
                            return { abort() {} };
                        }
                        const controller = new AbortController();
                        lookupControllers.add(controller);
                        const epoch = employeeLookupEpoch;
                        setLookupStatus('Searching employees...');
                        employeeRequest(`Authenticated/EmployeeTimeLog/EmployeeLookup?term=${encodeURIComponent(term)}&page=${encodeURIComponent(page)}`, controller.signal)
                            .then(data => {
                                if (controller.signal.aborted || epoch !== employeeLookupEpoch ||
                                    queuedGeneration !== employeeSearchState[kind].generation) return;
                                const openSearch = $('.select2-container--open .select2-search__field');
                                if (openSearch.length && openSearch.val().trim() !== term) return;
                                if (!data || !Array.isArray(data.data) || typeof data.more !== 'boolean' || data.data.some(item => !isEmployeeItem(item)))
                                    throw new Error('Invalid employee results');
                                recentEmployeeMatches = new Map(data.data.map(item => [item.employeeId, item]));
                                setLookupStatus(data.data.length ? '' : 'No employees found. Try another number or name.');
                                success({ ...data, lookupEpoch: epoch, lookupGeneration: queuedGeneration });
                            })
                            .catch(() => {
                                if (controller.signal.aborted || epoch !== employeeLookupEpoch ||
                                    queuedGeneration !== employeeSearchState[kind].generation) return;
                                const openSearch = $('.select2-container--open .select2-search__field');
                                if (openSearch.length && openSearch.val().trim() !== term) return;
                                setLookupStatus('Employee search failed. Check the connection and try the search again.');
                                failure();
                            })
                            .finally(() => lookupControllers.delete(controller));
                        return { abort: () => controller.abort(), get status() { return controller.signal.aborted ? 0 : 200; } };
                    },
                    processResults: data => ({
                        results: data.data.map(item => ({ id: String(item.employeeId), text: employeeLabel(item, kind), employee: item,
                            queryEpoch: data.lookupEpoch ?? employeeLookupEpoch,
                            queryGeneration: data.lookupGeneration ?? employeeSearchState[kind].generation })),
                        pagination: { more: data.more }
                    })
                },
                language: {
                    inputTooShort: () => 'Type at least 2 characters to search employees.',
                    searching: () => 'Searching employees...',
                    noResults: () => 'No employees found. Try another number or name.',
                    errorLoading: () => 'Employee search failed. Type again to retry.'
                }
            });
            $(selector).on('select2:open', () => { activeEmployeeSearchKind = kind; });
            $(selector).on('select2:close', () => {
                if (activeEmployeeSearchKind === kind) activeEmployeeSearchKind = null;
            });
            $(selector).on('select2:selecting', onEmployeeSelecting);
        }
    };

    let renderSimpleDropdown = (elementId, data, placeholder) => {
        const select = $(elementId);
        const fieldId = elementId.slice(1);
        const currentValue = select.val();
        const recordValue = editingRecord && editingRecord[fieldId];
        const selectedValue = editedLookupFields.has(fieldId) ? currentValue : (currentValue || recordValue || '');
        select.empty().append($('<option>').val('').text(placeholder));
        for (const item of data) select.append($('<option>').val(item.name).text(item.name));
        if (selectedValue && !data.some(item => item.name === selectedValue)) {
            select.append($('<option>').val(selectedValue).text(selectedValue));
        }
        if (!data.length) select.append($('<option>').val('').text('No data available'));
        select.val(selectedValue || '').trigger('change.select2');
    };

    // let getDropdownData = async () => {
    //     try {
    //         const projectResponse = await _apiHelper.get({ url: 'Authenticated/XCompany`' });

    //         if (projectResponse.ok) {
    //             projects = await projectResponse.json();
    //         } else {
    //             projects = [];
    //         }

    //     } catch (error) {
    //         projects = [];
    //     }
    //     console.log(projects);
    // };

    let loadNamedDropdown = async (kind, url, selectors) => {
        const statusSelector = kind === 'Project' ? '#project-lookup-status, #project-filter-status' : '#device-lookup-status';
        $(statusSelector).text(`Loading ${kind.toLowerCase()}s...`);
        try {
            const response = await _apiHelper.get({ url });
            if (!response.ok) throw new Error(`${kind} request failed`);
            const items = await response.json();
            if (!Array.isArray(items) || items.some(item => !item || typeof item.name !== 'string' || !item.name.trim())) {
                throw new Error(`Invalid ${kind} response`);
            }
            if (kind === 'Project') {
                _projects = items;
                projectsLoaded = true;
            } else {
                _devices = items;
                devicesLoaded = true;
            }
            for (const [selector, placeholder] of selectors) renderSimpleDropdown(selector, items, placeholder);
            $(statusSelector).text(items.length ? '' : `No ${kind.toLowerCase()}s available. Ask an administrator to add one.`);
        } catch (_) {
            $(statusSelector).text(`${kind}s could not be loaded. Refresh the page and try again.`);
        }
    };

    let getDropdownData = async () => {
        await Promise.all([
            loadNamedDropdown('Project', 'Authenticated/Project', [
                ['#project', 'All Projects'], ['#projectTimeIn', 'Select Project'], ['#projectTimeOut', 'Select Project']
            ]),
            loadNamedDropdown('Device', 'Authenticated/Device', [
                ['#deviceTimeIn', 'Select Device'], ['#deviceTimeOut', 'Select Device']
            ])
        ]);
    };

    let destroyDataTable = () => {
        if (isDataTableInitialized && dataTable && $.fn.DataTable.isDataTable('#employee-time-log-grid')) {
            dataTable.destroy(true);
            $('#employee-time-log-grid').empty();
            isDataTableInitialized = false;
            dataTable = null;
        }
    };

    let initializeGrid = async () => {
        // Destroy existing DataTable if it exists
        destroyDataTable();

        let columns = await getColumns();
        try {
            dataTable = $('#employee-time-log-grid').DataTable({
                bLengthChange: true,
                lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
                bFilter: true,
                bInfo: true,
                serverSide: true,
                processing: true,
                bSort: true,
                scrollY: "350px",
                scrollX: true,
                order: [[1, 'asc']],
                ajax: requestGridPage,
                columns: columns,
                pageLength: 5,
                dom: '<"d-flex justify-content-between align-items-center mb-2"<"pull-left"lB><"pull-right"f>>tipr',
                language: {
                    emptyTable: "No employee time log data available",
                    zeroRecords: "No matching records found",
                    search: 'Search:',
                    searchPlaceholder: ''
                }
            });

            isDataTableInitialized = true;

            // Attach event handlers after DataTable is initialized
            attachTableEvents();
            // Use one trailing debounce for typing and filter changes; invalidate stale responses immediately.
            $('#employee-time-log-grid_filter input').attr('maxlength', 200).off('.DT')
                .on('input.employeeTimeLog search.employeeTimeLog', function () {
                    dataTable.search(this.value);
                    scheduleCurrentFilter();
                });

        } catch (error) {
            console.error('DataTables initialization error:', error);
            createFallbackTable();
        }
    };

    let attachTableEvents = () => {
        $('#employee-time-log-grid tbody').on('click', '.icon-download-attachment', async function (event) {
            event.preventDefault();
            const button = $(this);
            if (button.prop('disabled')) return;
            const row = dataTable.row(button.closest('tr')).data();
            button.prop('disabled', true).attr('aria-busy', 'true');
            $('#time-log-page-status').text('Downloading attachment...');
            try {
                const record = await getRecord(row.timeLogId);
                await downloadAttachment(record);
                $('#time-log-page-status').text('');
            } catch (error) {
                $('#time-log-page-status').text(error.message);
            } finally {
                button.prop('disabled', false).removeAttr('aria-busy');
            }
        });

        $('#employee-time-log-grid tbody').on('click', '.icon-edit', async function (event) {
            event.preventDefault();
            if (isSubmitting) return;
            const row = dataTable.row($(this).closest('tr')).data();
            const requestVersion = ++detailRequestVersion;
            $('#time-log-page-status').text('Loading current record...');
            try {
                const record = await getRecord(row.timeLogId);
                if (requestVersion !== detailRequestVersion) return;
                populateForm($('#employee-time-log-form'), record);
                $('#employee-time-log-modal').modal('show');
                $('#time-log-page-status').text('');
            } catch (error) { $('#time-log-page-status').text(error.message); }
        });

        $('#employee-time-log-grid tbody').on('click', '.return-btn', function () {
            var rowData = dataTable.row($(this).closest('tr')).data();
            let form = $('#employee-time-log-form');
            let dateIn = moment(rowData.dateIn).format('YYYY-MM-DD');
            let dateOut = moment(rowData.dateOut).format('YYYY-MM-DD');
            form.find('#dateIn').val(dateIn);
            form.find('#dateOut').val(dateOut);

            populateForm(form, rowData);
            hideShowColumnBaseOnAction(false);
            $('#employee-time-log-modal').modal('show');
        });

        $('#employee-time-log-grid tbody').on('click', '.icon-delete', async function (event) {
            event.preventDefault();
            const row = dataTable.row($(this).closest('tr')).data();
            if (!window.confirm('Delete this time record?')) return;
            try {
                const response = await _apiHelper.delete({
                    url: `Authenticated/EmployeeTimeLog/${row.timeLogId}?version=${row.version}`,
                    requestOrigin: 'Employee Time Logs', requesterName: $('#current-user').text(), requestSystem: SYSTEM
                });
                if (!response.ok) throw new Error(await responseMessage(response, 'The time record could not be deleted.'));
                await refreshCurrentFilter();
            } catch (error) { $('#time-log-page-status').text(error.message); }
        });
    };

    let createFallbackTable = () => {
        $('#employee-time-log-grid').html('<table class="table table-striped"><thead><tr><th>Error loading table. Please refresh the page.</th></tr></thead></table>');
    };

    let hideShowColumnBaseOnAction = function (isAdd) {
        if (isAdd) {
            $('.borrow-initial').prop('disabled', false);
            $('#shiftStart, #shiftEnd').prop('disabled', true);
            $('.return-container').addClass('d-none');
        } else {
            $('.borrow-initial').prop('disabled', true);
            $('.return-container').removeClass('d-none');
        }
    };

    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "timeLogId",
                width: "1.5em",
                className: 'noVis dt-center',
                orderable: false,
                searchable: false,
                render: (data, type, row, meta) => {
                    let rowNumber = meta.settings._iDisplayStart + Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Employee No',
                data: "employeeNo",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: 'Employee Name',
                data: "employeeName",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Date In",
                data: "dateIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Date Out",
                data: "dateOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Time In",
                data: "timeIn",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Time Out",
                data: "timeOut",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Shift Start",
                data: "shiftStart",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Shift End",
                data: "shiftEnd",
                className: 'noVis dt-center',
                render: (data, type, row) => {
                    return _dateHelper.formatLocalShortTime(data);
                },
            },
            {
                title: "Flexible Shift",
                data: "isFlexibleShift",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false,
                searchable: false
            },
            {
                title: "No Shift",
                data: "isNoShift",
                className: 'noVis dt-center',
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" class="row-check" data-column="isFlexibleShift" data-row="${meta.row}" ${data ? 'checked' : ''} disabled readonly>`;
                },
                orderable: false,
                searchable: false
            },
            {
                title: "Project In",
                data: "projectTimeIn",
                className: 'noVis dt-center',
            },
            {
                title: "Project Out",
                data: "projectTimeOut",
                className: 'noVis dt-center'
            },
            {
                title: "Device In",
                data: "deviceTimeIn",
                className: 'noVis dt-center'
            },
            {
                title: "Device Out",
                data: "deviceTimeOut",
                className: 'noVis dt-center'
            }, 
            {
                title: "System Log Type",
                data: "systemRemarks",
                className: 'noVis dt-right',
                orderable: true
            },
            {
                title: "OB", data: "isOB", orderable: false, searchable: false, className: 'dt-center',
                render: value => '<input type="checkbox" disabled ' + (value ? 'checked' : '') + ' aria-label="OB">'
            },
            {
                title: "Attachment", data: "hasAttachment", orderable: false, searchable: false,
                render: value => value ? 'Attached' : ''
            },
            {
                title: "Comment", data: "commentPreview", orderable: false, searchable: false,
                render: $.fn.dataTable.render.text()
            },
            {
                title: "Actions",
                data: "timeLogId",
                width: "10em",
                render: function (data, type, full) {
                    let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.timeLogId + '" data-endpoint="Authenticated/EmployeeTimeLog" data-table="employee-time-log-grid"><i class="fas fa-edit"></i></a>';
                    buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.timeLogId + '" data-endpoint="Authenticated/EmployeeTimeLog" data-table="employee-time-log-grid"><i class="fas fa-trash border-icon"></i></a>';
                    if (full.hasAttachment) {
                        buttons += '<button type="button" class="btn btn-link btn-sm m-1 p-0 icon-download-attachment" title="Download attachment" aria-label="Download attachment"><i class="fas fa-download" aria-hidden="true"></i></button>';
                    }
                    return type === 'display' ? buttons : "";
                },
                className: 'noVis dt-center',
                orderable: false,
                searchable: false
            }
        ];
        return columns;
    };

    let initializeModals = e => {
        $('#employee-time-log-modal').modal({ backdrop: 'static', keyboard: false });
        $('#employee-time-log-form #dateIn').attr('value', moment().format('YYYY-MM-DD'));
        $('#employee-time-log-form #dateOut').attr('value', moment().format('YYYY-MM-DD'));
    };

    let initializeGrids = async () => {
        initializeEmployeeSearch();
        await initializeGrid();
        try {
            await getDropdownData();
        }
        catch (error) {
            console.error('Dropdown initialization error:', error);
        }
    };

    $(document).ready(function () {
        if ($('#employee-time-log-grid').length === 0) {
            console.error('Table element not found:', 'employee-time-log-grid');
            return;
        }
        //renderDropDowns();
        initializeGrids();
        attachEvents();
        initializeModals();
    });

})(jQuery);
