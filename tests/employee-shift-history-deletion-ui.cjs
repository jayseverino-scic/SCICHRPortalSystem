const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../SCICHRPortal.Web/wwwroot/js/pages/administration/employee-shift.js'), 'utf8');

function page({ confirmed = true, status = 200 } = {}) {
    const values = new Map([['#assignment-shift', '-1']]);
    const requests = [], confirmations = [], notices = [];
    let reloads = 0, modalOpens = 0;
    const control = selector => ({
        val(value) { if (value === undefined) return values.get(selector) || ''; values.set(selector, value); return this; },
        prop(name, value) { return value === undefined ? false : this; },
        text(value) { return value === undefined ? 'Synthetic actor' : this; },
        trigger() { return this; }, ready() {}, find() { return this; }, each() { return this; }
    });
    const api = async (method, options) => { requests.push({ method, ...options }); return new Response(JSON.stringify(status === 200 ? { affected: 1 } : 'Synthetic failure'), { status }); };
    const context = vm.createContext({ jQuery: control,
        document: { getElementById: () => ({ classList: { remove() {}, contains() { return false; } } }) },
        bootstrap: { Modal: class { show() { modalOpens++; } hide() {} } },
        ApiHelper: class { post(options) { return api('POST', options); } delete(options) { return api('DELETE', options); } },
        DateHelper: class {}, StringHelper: class {}, Intl, Date, URLSearchParams, setTimeout, clearTimeout,
        Swal: { async fire(options) { confirmations.push(options); return { isConfirmed: confirmed, value: confirmed }; } },
        toastr: Object.fromEntries(['info', 'success', 'warning', 'error'].map(kind => [kind, text => notices.push({ kind, text })])),
        recordReload: () => { reloads++; }
    });
    vm.runInContext(source.replace('})(jQuery);', `globalThis.historyTest = {
        openEffectiveModal, overlayPageRows, applyRowChange,
        deleteRow: (...args) => deleteHistoryRow(...args),
        pendingIds: () => Array.from(_pendingChanges.keys()),
        selection: () => historyDeletionSelection(),
        seedBulk(filter) { _bulkAssignment = { filter, description: 'Synthetic filter', count: 2 }; },
        seedExclusion(id) { _pendingChanges.set(id, { ..._originalRows.get(id), isSelected: false }); _pendingFields.set(id, new Set(['isSelected'])); },
        setup() { dataTable = { ajax: { reload: recordReload }, rows: () => ({ data: () => ({ toArray: () => [] }) }), buttons: () => ({ enable() {} }) }; }
    }; })(jQuery);`), context);
    context.historyTest.setup();
    const rows = context.historyTest.overlayPageRows([1, 2, 3].map(id => ({ employeeId: id, employeeNo: 'QA' + id,
        employeeName: 'Synthetic Employee', projectId: id === 3 ? 2 : 1, shiftId: 1, assignedShiftId: id,
        isAssigned: true, isFlexibleShift: false, isNoShift: false, isNoBreak: false })));
    context.historyTest.applyRowChange(rows[0], 'isSelected', true);
    context.historyTest.applyRowChange(rows[1], 'isNoBreak', true);
    return { ...context.historyTest, requests, confirmations, notices, values, rows,
        reloads: () => reloads, modalOpens: () => modalOpens };
}
const event = () => ({ preventDefault() {} });
(async () => {
    let state = page();
    await state.openEffectiveModal(event());
    assert.equal(state.requests.length, 1, 'Unassigned Save must remove latest history without an effective date.');
    assert.equal(state.requests[0].url, 'Authenticated/EmployeeShift/DeleteLatestFiltered');
    assert.equal(state.requests[0].method, 'POST');
    assert.deepEqual(Array.from(state.requests[0].data.selectedEmployeeIds), [1], 'Flag-only edits must not select employees for removal.');
    assert.equal('effectiveStartDate' in state.requests[0].data, false);
    assert.equal(state.modalOpens(), 0, 'Unassigned must not open the effective-period modal.');
    assert.deepEqual(Array.from(state.pendingIds()), [2], 'Unrelated flag edits must remain pending.');
    assert.equal(state.reloads(), 1);

    state = page({ confirmed: false }); await state.openEffectiveModal(event());
    assert.equal(state.requests.length, 0); assert.deepEqual(Array.from(state.pendingIds()), [1, 2]);
    state = page({ status: 409 }); await state.openEffectiveModal(event());
    assert.deepEqual(Array.from(state.pendingIds()), [1, 2]); assert.equal(state.reloads(), 0);

    state = page(); state.seedBulk({ projectId: 1, shiftId: 0, filterType: 'All', searchKeyword: '', asOf: '2026-10-16T08:00:00' });
    state.seedExclusion(2); state.applyRowChange(state.rows[2], 'isSelected', true);
    await state.openEffectiveModal(event());
    assert.equal(state.requests[0].data.applyToFilter, true);
    assert.equal(state.requests[0].data.projectId, 1);
    assert.deepEqual(Array.from(state.requests[0].data.excludedEmployeeIds), [2]);
    assert.ok(state.requests[0].data.selectedEmployeeIds.includes(3), 'Manual selections outside the bulk filter must be included.');

    state = page(); await state.deleteRow(3, 99, 'Synthetic schedule');
    assert.equal(state.requests[0].method, 'DELETE');
    assert.equal(state.requests[0].url, 'Authenticated/EmployeeShift/History/3/99');
    assert.deepEqual(Array.from(state.pendingIds()), [1, 2]);
    state = page({ confirmed: false }); await state.deleteRow(1, 99, 'Synthetic schedule');
    assert.equal(state.requests.length, 0);

    state = page(); state.seedBulk({ projectId: 1, shiftId: 0, filterType: 'All', searchKeyword: '' });
    state.seedExclusion(2); await state.deleteRow(2, 202, 'Synthetic older period');
    assert.ok(state.selection().excludedEmployeeIds.includes(2), 'Deleting history must not reselect a previously excluded employee.');
    state = page(); state.seedBulk({ projectId: 1, shiftId: 0, filterType: 'All', searchKeyword: '' });
    await state.deleteRow(1, 101, 'Synthetic period');
    assert.ok(state.selection().excludedEmployeeIds.includes(1), 'An employee whose history was removed must be deselected while retaining the rest of the bulk selection.');

    state = page(); state.values.set('#assignment-shift', '1'); await state.openEffectiveModal(event());
    assert.equal(state.modalOpens(), 1, 'Normal assignments must retain the effective-period modal.');
    assert.equal(state.requests.length, 0);
    console.log('PASS: selected-only removal, confirmation, cancellation, conflicts, bulk exclusions, exact history deletion and normal assignment UI scenarios.');
})().catch(error => { console.error(error); process.exitCode = 1; });
