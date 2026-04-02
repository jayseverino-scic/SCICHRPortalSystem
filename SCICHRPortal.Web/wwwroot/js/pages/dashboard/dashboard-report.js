(function ($) {
    //Events
    const CLICK_EVENT = 'click';
    const LOAD_EVENT = 'load'

    //Helpers
    const _apiHelper = new ApiHelper();
    const _formHelper = new FormHelper();
    const _dateHelper = new DateHelper();
    const _numberHelper = new NumberHelper();
    const _cookieHelper = new CookieHelper();
    const SYSTEM = 'enrolment';
    let attachEvents = () => {

    };

    let populateEnrolledCaurosel = function () {
        let container = $("#enrolled-carousel");

        // Data for the carousel
        const carouselData = {
            items: [
                { grade: "Nursery", count: 4, cssClass: 'text-danger' },
                { grade: "Kindergarten", count: 10, cssClass: 'text-success' },
                { grade: "Grade 1", count: 8, cssClass: 'text-warning' },
                { grade: "Grade 2", count: 12, cssClass: 'text-info' },
            ]
        };

        // Compile the Handlebars template
        const templateSource = document.getElementById('carousel-template').innerHTML;
        const template = Handlebars.compile(templateSource);

        // Generate the HTML and insert into the carousel
        const carouselHTML = template(carouselData);
        container.append(carouselHTML);

    }
    let populateReservedCaurosel = function () {
        let container = $("#reserved-carousel");

        // Data for the carousel
        const carouselData = {
            items: [
                { grade: "Nursery", count: 4, cssClass: 'text-danger' },
                { grade: "Kindergarten", count: 10, cssClass: 'text-success' },
                { grade: "Grade 1", count: 8, cssClass: 'text-warning' },
                { grade: "Grade 2", count: 12, cssClass: 'text-info' },
            ]
        };

        // Compile the Handlebars template
        const templateSource = document.getElementById('carousel-template').innerHTML;
        const template = Handlebars.compile(templateSource);

        // Generate the HTML and insert into the carousel
        const carouselHTML = template(carouselData);
        container.append(carouselHTML);

    }

    let populateSectionCaurosel = function () {
        let container = $("#section-carousel");

        // Data for the carousel
        const carouselData = {
            items: [
                { grade: "Nursery", count: 4, cssClass: 'text-danger' },
                { grade: "Kindergarten", count: 10, cssClass: 'text-success' },
                { grade: "Grade 1", count: 8, cssClass: 'text-warning' },
                { grade: "Grade 2", count: 12, cssClass: 'text-info' },
            ]
        };

        // Compile the Handlebars template
        const templateSource = document.getElementById('carousel-template').innerHTML;
        const template = Handlebars.compile(templateSource);

        // Generate the HTML and insert into the carousel
        const carouselHTML = template(carouselData);
        container.append(carouselHTML);

    }

   

    let initializeModals = e => {
        $('#master-data-menu').removeClass('d-none');
        $('#master-data-menu .dashboard-report').addClass('active');
    }


    $(document).ready(function () {
        initializeModals();
        populateEnrolledCaurosel();
        populateReservedCaurosel();
        populateSectionCaurosel();
        attachEvents();
    });

})(jQuery);