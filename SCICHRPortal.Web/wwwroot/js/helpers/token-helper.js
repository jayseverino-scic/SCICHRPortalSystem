class TokenHelper {
    constructor() {
        this.cookieHelper = new CookieHelper();
    }

    parseCurrentJwt = () => {
        let token = this.cookieHelper.get('jsonWebToken')
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        console.log(JSON.parse(jsonPayload))
        return JSON.parse(jsonPayload);
    }

    checkCVUDAccess = (pageName) => {
        let token = this.parseCurrentJwt();
        let permissions = token.permissions.split(',');

        return permissions.includes(`${pageName}-AllAccess`);
    }

    checkCVUAccess = (pageName) => {
        let token = this.parseCurrentJwt();
        let permissions = token.permissions.split(',');

        return permissions.includes(`${pageName}-NoDeleteAccess`);
    }
}