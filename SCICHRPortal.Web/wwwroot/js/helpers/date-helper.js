class DateHelper {
    formatLocalDate = date => {
        let output = '';

        if (date !== null) {
            let localDate = moment(date);

            output = localDate.format('dddd, MMMM DD, YYYY')
        }

        return output;
    }
    formatDateFullMonth = date => {
        let output = '';

        if (date !== null) {
            let localDate = moment(date);

            output = localDate.format('MMMM DD, YYYY')
        }

        return output;
    }

    formatShortLocalDate = date => {
        let output = '';

        if (date !== null) {
            let localDate = moment(date);

            output = localDate.format('MM/DD/YYYY')
        }

        return output;
    };

    formatLocalDateTime = date => {
        let output = '';

        if (date !== null) {
            let localDate = moment.utc(date);

            output = moment(localDate).local().format('DD/MM/YYYY hh:mm A')
        }

        return output;
    };

    formatShortDate = date => {
        let output = '';

        if (date !== null)
            output = moment(date).format('MM/DD/YYYY')

        return output;
    };

    formatLocalShortTime = date => {
        let output = '';

        if (date !== null) {
            let localDate = moment(date);

            output = localDate.format('h:mm A')
        }

        return output;
    };

    serializeUtc = str => {
        return moment.utc(str).format();
    };

    // Changes the date's timezone to UTC 
    // while retaining the original date
    convertToUtcDate = date => {
        let dateString = this.formatLocalDate(date);

        return this.serializeUtc(dateString);
    };

    dayOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    getDayOfWeek = date => {
        let day = date.getDay();

        return this.dayOfWeek[day];
    };

    getLocalDayOfWeek = date => {
        let localDate = new Date(this.formatShortLocalDate(date));

        return this.getDayOfWeek(localDate);
    };

    getTime = time => {
        if (time) {
            var date = "2000-01-01";
            return moment(date + ' ' + time).format("hh:mm A")
        }
        return time;
    }

    getLocalTime = utcTime => {
        if (!utcTime) 
            return '';

        let date = new Date(`1/1/2000 ${utcTime} UTC`);

        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    getLocal24HourTime = utcTime => {
        if (!utcTime)
            return '';

        let date = new Date(`1/1/2000 ${utcTime} UTC`);

        return date.toLocaleTimeString([], {
            hourCycle: 'h23',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    validate = (date, format) => {
        let start = moment("1-1-1900").format(format);
        let end = moment("12-31-2300").format(format);

        if (moment(date).format(format) < start || moment(date).format(format) > end)
            return;

        return moment(moment(date).format(format), format, true).isValid();
    }
}