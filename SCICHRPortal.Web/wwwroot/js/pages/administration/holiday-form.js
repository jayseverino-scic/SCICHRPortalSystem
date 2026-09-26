(function (root, factory) {
    const rules = factory();
    if (typeof module === 'object' && module.exports) module.exports = rules;
    if (root) root.HolidayFormRules = rules;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
    function buildPayload(input) {
        const type = Number(input.holidayType);
        const selected = input.selectedProjects || [];
        const values = Array.isArray(selected) ? selected : [selected];
        const allProjects = values.includes('__all__');
        const selectedIds = allProjects ? [] : values.map(Number).filter(id => Number.isSafeInteger(id) && id > 0);
        const payload = {
            holidayId: Number(input.holidayId) || 0,
            holidayName: input.holidayName,
            holidayDate: input.holidayDate,
            holidayType: type,
            allProjects,
            projectIds: [...new Set(selectedIds)]
        };
        return payload;
    }

    function escapeHtml(value) {
        return String(value == null ? '' : value).replace(/[&<>"']/g, char => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        })[char]);
    }

    function projectSummary(row) {
        if (row.allProjects === true) return 'All Projects';
        const ids = Array.isArray(row.projectIds) ? row.projectIds : [];
        const projects = Array.isArray(row.projects) ? row.projects : [];
        const names = Array.isArray(row.projectNames) ? row.projectNames : [];
        if (!ids.length) return '—';
        return ids.map((id, index) => {
            const project = projects.find(item => Number(item.id) === Number(id));
            const name = project?.name || names[index] || `Project #${Number(id)}`;
            return `${escapeHtml(name)}${project?.deleted ? ' (inactive)' : ''}`;
        }).join(', ');
    }

    return { buildPayload, escapeHtml, projectSummary };
});
