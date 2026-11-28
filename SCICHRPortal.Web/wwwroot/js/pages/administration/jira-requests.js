(function ($) {
    'use strict';

    const dateFormatter = new Intl.DateTimeFormat('en-US', {
        timeZone: 'Asia/Manila', month: 'short', day: '2-digit', year: 'numeric'
    });
    const timeFormatter = new Intl.DateTimeFormat('en-US', {
        timeZone: 'Asia/Manila', hour: 'numeric', minute: '2-digit'
    });

    const formatDateTime = value => {
        const date = value ? new Date(value) : null;
        if (!date || Number.isNaN(date.getTime())) {
            return ['—', '—'];
        }

        return [dateFormatter.format(date), timeFormatter.format(date)];
    };

    const initialize = () => {
        const panel = document.getElementById('jira-requests-panel');
        const jiraTab = document.getElementById('jira-requests-tab');
        const portalTab = document.getElementById('portal-records-tab');
        const portalGrid = document.querySelector('#portal-records-panel table');
        const tabList = jiraTab.closest('[role="tablist"]');
        const api = new ApiHelper();
        const status = panel.querySelector('[data-jira-status]');
        const table = panel.querySelector('[data-jira-table]');
        const rows = panel.querySelector('[data-jira-rows]');
        const retry = panel.querySelector('[data-jira-retry]');
        const more = panel.querySelector('[data-jira-more]');
        const errorMessage = 'Approved requests could not be loaded. Please try again.';

        let loaded = false;
        let loading = false;
        let reloadOnRetry = false;
        let nextCursor = null;

        const loadPage = async () => {
            if (loading) return;

            const append = Boolean(nextCursor) && !reloadOnRetry;
            const query = append ? '?' + new URLSearchParams({ cursor: nextCursor }) : '';
            loading = true;
            retry.hidden = true;
            more.disabled = true;
            table.setAttribute('aria-busy', 'true');
            status.textContent = 'Loading approved requests...';

            try {
                const response = await api.get({ url: panel.dataset.jiraEndpoint + query });
                if (!response.ok) {
                    if (append && response.status === 400) reloadOnRetry = true;

                    const message = response.headers.get('content-type')?.includes('application/json')
                        ? await api.readErrorMessage(response, { fallback: errorMessage })
                        : response.status === 403 ? 'Administrator access is required.' : errorMessage;
                    throw new Error(message);
                }

                const page = await response.json();
                const newRows = document.createDocumentFragment();
                for (const request of page.data) {
                    const row = document.createElement('tr');
                    const values = [
                        request.key, request.summary, request.employeeNo ?? '—', request.employeeName ?? '—',
                        ...formatDateTime(request.from), ...formatDateTime(request.to), request.status
                    ];
                    for (const value of values) {
                        row.insertCell().textContent = value;
                    }
                    newRows.append(row);
                }

                if (!append) rows.replaceChildren();
                rows.append(newRows);
                loaded = true;
                reloadOnRetry = false;
                nextCursor = page.nextCursor;
                table.hidden = rows.rows.length === 0;
                more.hidden = !nextCursor;
                status.textContent = table.hidden && !nextCursor ? 'No matching approved requests.' : '';
            } catch (error) {
                more.hidden = true;
                retry.textContent = reloadOnRetry ? 'Reload approved requests' : 'Retry';
                retry.hidden = false;
                status.textContent = error instanceof TypeError ? errorMessage : error.message || errorMessage;
            } finally {
                loading = false;
                more.disabled = false;
                table.removeAttribute('aria-busy');
            }
        };

        const onTabShown = event => {
            portalTab.tabIndex = event.target === portalTab ? 0 : -1;
            jiraTab.tabIndex = event.target === jiraTab ? 0 : -1;
            if (event.target === jiraTab && !loaded && retry.hidden) loadPage();
            if (event.target === portalTab && $.fn.dataTable.isDataTable(portalGrid)) {
                $(portalGrid).DataTable().columns.adjust();
            }
        };

        const onTabKeyDown = event => {
            let target;
            switch (event.key) {
                case 'ArrowLeft':
                case 'ArrowRight':
                    target = event.target === portalTab ? jiraTab : portalTab;
                    break;
                case 'Home':
                    target = portalTab;
                    break;
                case 'End':
                    target = jiraTab;
                    break;
                default:
                    return;
            }

            event.preventDefault();
            bootstrap.Tab.getOrCreateInstance(target).show();
            target.focus();
        };

        tabList.addEventListener('shown.bs.tab', onTabShown);
        tabList.addEventListener('keydown', onTabKeyDown);
        retry.addEventListener('click', loadPage);
        more.addEventListener('click', loadPage);
    };

    $(initialize);
})(jQuery);
