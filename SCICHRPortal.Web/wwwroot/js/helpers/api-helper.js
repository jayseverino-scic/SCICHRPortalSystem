class ApiHelper {
    constructor() {
        this.baseApiUrl = document.getElementById('base-api-url').value;
        this.cookieHelper = new CookieHelper();
    }

    ajaxRequest = (method, params) => {
        let data = new FormData();

        if (params.dataObject) {
            Object.keys(params.dataObject).forEach(k => {
                data.append(k, params.dataObject[k]);
            });
        } else {
            data.append('file', params.data);

        }
    
        return $.ajax({
            xhr: params.xhr,
            type: method,
            url: `${this.baseApiUrl}/${params.url}`,
            data: data,
            contentType: false,
            processData: false,
            headers: {
                'Authorization': `bearer ${this.cookieHelper.get('jsonWebToken')}`
            },
            beforeSend: params.beforeSend,
            error: params.error,
            success: params.success,
            complete: params.complete
        });
    }

    ajaxAnnouncementRequest = (method, params) => {
        console.log(params.data)
        return $.ajax({
            xhr: params.xhr,
            type: method,
            url: `${this.baseApiUrl}/${params.url}`,
            data: params.data,
            contentType: false,
            processData: false,
            headers: {
                'Authorization': `bearer ${this.cookieHelper.get('jsonWebToken')}`,
                'RequestOrigin': params.requestOrigin,
                'RequesterName': params.requesterName,
                'RequestSystem': params.requestSystem,
            },
            beforeSend: params.beforeSend,
            error: params.error,
            success: params.success,
            complete: params.complete
        });
    }


    jsonRequest = async (method, params) => {
        let response = await fetch(`${this.baseApiUrl}/${params.url}`, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${this.cookieHelper.get('jsonWebToken')}`,
                'RequestOrigin': params.requestOrigin,
                'RequesterName': params.requesterName,
                'RequestSystem': params.requestSystem,
            },
            body: JSON.stringify(params.data)
        });

        if (response.status === 401 && !params.data.IsLogin) {
            this.logOut();
        }

        return response;
    };

    formRequest = async (method, params) => {
        let formData = this.toFormData(params.data);

        let response = await fetch(`${this.baseApiUrl}/${params.url}`, {
            method: method,
            headers: {
                'Authorization': `bearer ${this.cookieHelper.get('jsonWebToken')}`
            },
            body: formData
        });
        if (response.status === 401) {
            this.logOut();
        }

        return response;
    };

    get = async params => await this.jsonRequest('GET', params);

    post = async params => await this.jsonRequest('POST', params);

    put = async params => await this.jsonRequest('PUT', params);

    patch = async params => await this.jsonRequest('PATCH', params);

    delete = async params => await this.jsonRequest('DELETE', params);

    postForm = async params => await this.formRequest('POST', params);

    putForm = async params => await this.formRequest('PUT', params);

    toFormData = data => {
        var formData = new FormData();

        for (var key in data) {
            formData.append(key, data[key]);
        }

        return formData;
    };

    logOut = () => {
        this.cookieHelper.delete('jsonWebToken');
        this.cookieHelper.delete('refreshToken');
        window.location = '/Login/Index';
    };

    refreshTokens = async () => {
        let refreshToken = this.cookieHelper.get('refreshToken');
     
        if (refreshToken) {
            let jsonWebToken = this.cookieHelper.get('jsonWebToken');

            let response = await fetch(`${this.baseApiUrl}/Authenticate/RefreshToken`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    jsonWebToken: jsonWebToken,
                    refreshToken: refreshToken
                })
            });

            if (response.ok) {
                var token = await response.json();
                console.log(token);
                this.cookieHelper.set('jsonWebToken', token.jsonWebToken);
                this.cookieHelper.set('refreshToken', token.refreshToken);
            }
        }
    };
}