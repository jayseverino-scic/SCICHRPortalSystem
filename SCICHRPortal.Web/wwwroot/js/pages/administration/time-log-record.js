(function (root) {
    'use strict';
    const fields = ['EmployeeId', 'DateIn', 'DateOut', 'TimeIn', 'TimeOut', 'ProjectTimeIn', 'ProjectTimeOut', 'DeviceTimeIn', 'DeviceTimeOut'];
    const key = name => name.charAt(0).toLowerCase() + name.slice(1);
    const isProtected = record => !!record && ['biometrics', 'file'].includes((record.systemRemarks || '').toLowerCase());
    const present = value => value !== null && value !== undefined && value !== '';
    function lockedFields(record) {
        return new Set(isProtected(record) ? fields.map(key).filter(name => present(record[name])) : []);
    }
    function payload(record, values) {
        const data = { EmployeeId: Number(values.EmployeeId), IsOB: !!values.IsOB };
        for (const field of fields.slice(1)) data[field] = present(values[field]) ? values[field] : null;
        if (!record) data.DateOut = data.DateOut || data.DateIn;
        for (const side of ['In', 'Out']) {
            const field = 'Time' + side;
            const dateField = 'Date' + side;
            if (!data[field]) continue;
            const previous = record && record[key(field)];
            const previousDate = record && record[key(dateField)];
            data[field] = previous && previousDate && previousDate.slice(0, 10) === data[dateField] &&
                previous.slice(11, 16) === data[field] ? previous : data[dateField] + 'T' + data[field];
        }
        for (const field of fields) {
            if (lockedFields(record).has(key(field))) data[field] = record[key(field)];
        }
        if (record) { data.TimeLogId = record.timeLogId; data.Version = record.version; }
        return data;
    }
    const api = { isProtected, lockedFields, payload };
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
    else root.TimeLogRecord = api;
})(typeof window !== 'undefined' ? window : globalThis);
