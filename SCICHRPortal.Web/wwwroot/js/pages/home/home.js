(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _cookieHelper = new CookieHelper();
    let attachEvents = () => {
        $('#log-out').on(CLICK_EVENT, onClickLogOutButton);

    };
    let onClickLogOutButton = () => {
        _cookieHelper.deleteAllContains('jsonWebToken');
        _cookieHelper.delete('refreshToken');
        window.location = '/Login/Index';
    };
    let onWindowLoaded = () => {
        attachEvents();
    };

    window.addEventListener(LOAD_EVENT, onWindowLoaded);
})(jQuery);