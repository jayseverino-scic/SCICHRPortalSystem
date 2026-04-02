class NumberHelper {
    formatCommaSeperator = number => {
        if(number)
            return parseFloat(number).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,");;
    }
}