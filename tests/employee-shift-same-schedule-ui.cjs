// Exercise the actual save handler against controlled API responses, without a browser or database.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../SCICHRPortal.Web/wwwroot/js/pages/administration/employee-shift.js'), 'utf8');

function page(status, affected) {
    const values = new Map([['#assignment-shift', '1'], ['#effective-start', '2026-10-16T08:00']]);
    const notices = [];
    let reloads = 0;
    let modalClosed = false;
    const control = selector => ({
        val(value) {
            if (value === undefined) return values.get(selector) || '';
            selector.split(', ').forEach(key => values.set(key, value));
            return this;
        },
        text(value) { return value === undefined ? 'Synthetic actor' : this; },
        prop(name, value) { return value === undefined ? false : this; },
        trigger() { return this; }, ready() {}
    });
    const context = vm.createContext({
        jQuery: control, document: { getElementById: () => ({ classList: { remove() {} } }) },
        bootstrap: { Modal: class { hide() { modalClosed = true; } } },
        ApiHelper: class { async post() { return new Response(JSON.stringify(status === 200 ? { affected } : 'Synthetic conflict'), { status }); } },
        DateHelper: class {}, StringHelper: class {}, URLSearchParams, Intl, Date, setTimeout, clearTimeout,
        toastr: Object.fromEntries(['info', 'success', 'warning', 'error'].map(kind => [kind, text => notices.push({ kind, text })])),
        recordReload: () => { reloads++; }
    });
    // Expose existing lexical state only inside this test VM; no production test hooks.
    vm.runInContext(source.replace('})(jQuery);', `globalThis.saveTest = {
        overlayPageRows, applyRowChange, onEmployeeShiftSubmit,
        pendingCount: () => _pendingChanges.size,
        setup() { dataTable = { ajax: { reload: recordReload }, rows: () => ({ data: () => ({ toArray: () => [] }) }), buttons: () => ({ enable() {} }) }; }
    }; })(jQuery);`), context);
    context.saveTest.setup();
    const row = context.saveTest.overlayPageRows([{ employeeId: 1, assignedShiftId: 1, shiftId: 1, isAssigned: true,
        isFlexibleShift: false, isNoShift: false, isNoBreak: false }])[0];
    context.saveTest.applyRowChange(row, 'isSelected', true);
    return { ...context.saveTest, notices, values, reloads: () => reloads, modalClosed: () => modalClosed };
}

(async () => {
    let state = page(200, 0);
    await state.onEmployeeShiftSubmit({ preventDefault() {} });
    assert.deepEqual(state.notices, [{ kind: 'info', text: 'No schedule changes were needed.' }]);
    assert.equal(state.pendingCount(), 0, 'A skipped save must clear pending selection.');
    assert.equal(state.reloads(), 1, 'A skipped save must reload the saved grid.');
    assert.equal(state.modalClosed(), true);
    assert.equal(state.values.get('#effective-start'), '', 'A skipped save must clear entered dates.');

    state = page(200, 2);
    await state.onEmployeeShiftSubmit({ preventDefault() {} });
    assert.deepEqual(state.notices, [{ kind: 'success', text: 'Employee shift assignments updated.' }]);
    assert.equal(state.pendingCount(), 0);
    assert.equal(state.reloads(), 1);

    state = page(409);
    await state.onEmployeeShiftSubmit({ preventDefault() {} });
    assert.equal(state.notices[0].kind, 'error');
    assert.equal(state.pendingCount(), 1, 'Conflicts must retain pending selection.');
    assert.equal(state.reloads(), 0);
    assert.equal(state.modalClosed(), false);
    assert.equal(state.values.get('#effective-start'), '2026-10-16T08:00', 'Conflicts must retain the effective date.');
    console.log('PASS: zero-change, changed and failed employee-shift save UI scenarios.');
})().catch(error => { console.error(error); process.exitCode = 1; });
