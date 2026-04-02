class StringHelper {
    camelToPascal = str => {
        let result = str.replace(/([A-Z])/g, "$1");

        return result.charAt(0).toUpperCase() + result.slice(1);
    }
    capitalize = str => {
        if (typeof str == 'string' || str instanceof String) {
            str = str.toLowerCase().replace(/\b[a-z]/g, function (letter) {
                return letter.toUpperCase();
            });
        }
        return str;
    }
   
}