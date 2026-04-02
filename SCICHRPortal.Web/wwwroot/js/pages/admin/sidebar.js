(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers

    let attachEvents = () => {
        $('#v-pills-master-data-tab').on(CLICK_EVENT, onMasterDataTabClicked);
        $('#v-pills-report-tab').on(CLICK_EVENT, onReportTabClicked);
        $('#v-pills-user-tab').on(CLICK_EVENT, onUserTabClicked);
        $('#v-pills-transaction-tab').on(CLICK_EVENT, onTransactionTabClicked);
        $('#v-pills-reference-tab').on(CLICK_EVENT, onReferenceTabClicked);
        $('#v-pills-setting-tab').on(CLICK_EVENT, onSettingTabClicked);
        $('.nav-link').on(CLICK_EVENT, onClickSubNav);
    };

    let onMasterDataTabClicked = function () {
        if ($('.master-data-sub-menu').hasClass('d-none'))
            $('.master-data-sub-menu').removeClass('d-none');
        else
            $('.master-data-sub-menu').addClass('d-none');
    };

    let onReportTabClicked = function () {
       
        if ($('.report-sub-menu').hasClass('d-none'))
            $('.report-sub-menu').removeClass('d-none');
        else
            $('.report-sub-menu').addClass('d-none');
    };

    let onUserTabClicked = function () {

        if ($('.user-sub-menu').hasClass('d-none'))
            $('.user-sub-menu').removeClass('d-none');
        else
            $('.user-sub-menu').addClass('d-none');
    };

    let onTransactionTabClicked = function () {

        if ($('.transaction-sub-menu').hasClass('d-none'))
            $('.transaction-sub-menu').removeClass('d-none');
        else
            $('.transaction-sub-menu').addClass('d-none');
    };

    let onReferenceTabClicked = function () {

        if ($('.reference-sub-menu').hasClass('d-none'))
            $('.reference-sub-menu').removeClass('d-none');
        else
            $('.reference-sub-menu').addClass('d-none');
    };

    let onSettingTabClicked = function () {

        if ($('.setting-sub-menu').hasClass('d-none'))
            $('.setting-sub-menu').removeClass('d-none');
        else
            $('.setting-sub-menu').addClass('d-none');
    };

    let onClickSubNav = event => {

        if (!$(event.currentTarget).hasClass('meta-data')) {
            $('#master-data-menu').addClass('d-none');
        }

        if (!$(event.currentTarget).hasClass('report')) {
            $('#report-menu').addClass('d-none');
        }

        if (!$(event.currentTarget).hasClass('user')) {
            $('#user-data-menu').addClass('d-none');
        }

        if (!$(event.currentTarget).hasClass('transaction')) {
            $('#transaction-menu').addClass('d-none');
        }

        if (!$(event.currentTarget).hasClass('reference')) {
            $('#reference-menu').addClass('d-none');
        }
    }
        
    let onWindowLoaded = () => {
        attachEvents();
    };

    window.addEventListener(LOAD_EVENT, onWindowLoaded);
})(jQuery);