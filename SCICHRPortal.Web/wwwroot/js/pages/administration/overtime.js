(function ($) {
    'use strict';

    const api = new ApiHelper();
    const $status = $('#overtime-page-status');
    const $retry = $('#overtime-retry');
    let grid;
    let pageVersion = 0;

    const attachEvents = () => {
        $retry.on('click', onRetry);
    };

    const onRetry = () => grid.ajax.reload(null, false);

    const loadPage = async (request, callback) => {
        const version = ++pageVersion;
        const query = new URLSearchParams({
            draw: request.draw,
            start: request.start,
            length: request.length,
            search: request.search.value
        });

        $status.text('Loading overtime records…');
        $retry.prop('hidden', true);

        let page;
        try {
            const response = await api.get({ url: `Authenticated/Overtime/Page?${query}` });
            if (version !== pageVersion) return;
            if (response.status === 403) {
                $status.text('You do not have access to overtime records.');
                callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                return;
            }
            if (!response.ok) {
                throw new Error('Overtime request failed.');
            }
            page = await response.json();
        } catch {
            if (version !== pageVersion) return;
            $status.text('Overtime records could not be loaded. Please retry.');
            $retry.prop('hidden', false);
            callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
            return;
        }

        if (version !== pageVersion) return;
        $status.text('');
        callback(page);
    };

    const initializeGrid = () => {
        const fields = [
            'employeeName', 'employeeHrisId', 'overtimeDateFrom', 'approvedStartTime',
            'overtimeDateTo', 'approvedEndTime', 'filedHours', 'approvedHours',
            'requestReference', 'closedAt', 'dateCreated', 'summaryId'
        ];
        const text = $.fn.dataTable.render.text();

        grid = $('#overtime-grid').DataTable({
            serverSide: true,
            processing: true,
            ordering: false,
            scrollX: true,
            searchDelay: 300,
            dom: '<"overtime-table-toolbar"lf>rtip',
            pageLength: 10,
            lengthMenu: [10, 20, 40, 80],
            ajax: loadPage,
            columns: fields.map(data => ({ data, render: text, defaultContent: '—' })),
            language: {
                search: 'Search employee or reference',
                emptyTable: 'No overtime records to display.',
                zeroRecords: 'No matching overtime records.'
            },
            initComplete: function () {
                const $container = $(this.api().table().container());
                $container.find('input[type="search"]').attr('maxlength', 200);
                $container.find('.dataTables_scrollBody').attr({
                    tabindex: 0,
                    role: 'region',
                    'aria-label': 'Overtime records'
                });
            }
        });
    };

    const initialize = () => {
        attachEvents();
        initializeGrid();
    };

    $(initialize);
})(jQuery);
