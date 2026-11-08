(function ($) {
    const initialize = root => {
        if (!$.fn.select2) return;

        $(root).find('select').addBack('select').each(function () {
            const select = $(this);
            // Page-size controls and existing Select2/Kendo widgets own their behavior.
            if (select.closest('.dataTables_length').length || select.data('select2') ||
                select.is('[data-role="dropdownlist"], [data-role="combobox"]')) return;

            const modal = select.closest('.modal');
            select.select2({
                theme: 'bootstrap',
                width: '100%',
                minimumResultsForSearch: 0,
                tags: false,
                closeOnSelect: !select.prop('multiple'),
                dropdownParent: modal.length ? modal : $(document.body)
            });
            select.on('select2:open.searchableDropdowns', function () {
                select.data('select2').$dropdown.find('.select2-search__field').trigger('focus');
            });
        });
    };

    // Run after the page's ready handlers so specialized employee lookups initialize first.
    $(document).ready(function () {
        initialize(document);
    });

    $(document).on('shown.bs.modal.searchableDropdowns', '.modal', function () {
        initialize(this);
    });

    $(document).on('reset.searchableDropdowns', 'form', function () {
        const form = this;
        // Native reset changes values after this event, without notifying Select2.
        window.setTimeout(function () {
            $(form).find('select.select2-hidden-accessible').trigger('change.select2');
        }, 0);
    });
})(jQuery);
