class LogImportModal {
    constructor(id) {
        this.element = document.getElementById(id);
        this.form = $(this.element).find('form');
        this.project = this.form.find('[data-import-field="project"]');
        this.hasProjects = false;
        this.isBusy = false;
        // Use the native API: the layout also loads a different jQuery modal version.
        this.modal = new bootstrap.Modal(this.element, { backdrop: 'static' });
        this.element.addEventListener('hide.bs.modal', event => {
            if (this.isBusy) event.preventDefault();
        });
        this.form.find('[data-import-cancel]').on('click', () => this.hide());
    }

    show() {
        if (!this.isBusy) this.modal.show();
    }

    hide() {
        if (!this.isBusy) this.modal.hide();
    }

    setStatus(message) {
        this.form.find('[data-import-status]').text(message);
    }

    setBusy(busy) {
        this.isBusy = busy;
        this.form.find('input, select, button').prop('disabled', busy);
        this.project.prop('disabled', busy || !this.hasProjects);
        this.form.find(':submit').prop('disabled', busy || !this.hasProjects).text(busy ? 'Importing...' : 'Import');
        if (busy) this.form.attr('aria-busy', 'true');
        else this.form.removeAttr('aria-busy');
    }

    setProjects(items) {
        const selected = this.project.val();
        this.project.empty().append($('<option>').val('').text('Select project'));
        for (const item of items) this.project.append($('<option>').val(item.name).text(item.name));
        this.project.val(items.some(item => item.name === selected) ? selected : '').trigger('change.select2');
        this.hasProjects = items.length > 0;
        this.setBusy(this.isBusy);
        this.setStatus(items.length ? '' : 'No projects available. Ask an administrator to add one.');
    }

    getValues() {
        if (!this.hasProjects) {
            this.setStatus('Projects are unavailable. Refresh the page and try again.');
            return null;
        }
        const values = {
            projectName: this.project.val(),
            start: this.form.find('[data-import-field="from"]').val(),
            end: this.form.find('[data-import-field="to"]').val(),
            file: this.form.find('[data-import-field="file"]')[0]?.files[0]
        };
        const isFileImport = this.form.find('[data-import-field="file"]').length > 0;
        if (!values.projectName || (isFileImport ? !values.file : !values.start || !values.end)) {
            this.setStatus(isFileImport ? 'Select a file and project before importing.' : 'Select a project and both dates before importing.');
            return null;
        }
        if (!isFileImport && values.start > values.end) {
            this.setStatus('From date must be on or before To date.');
            return null;
        }
        if (!this.form[0].reportValidity()) return null;
        return values;
    }
}
