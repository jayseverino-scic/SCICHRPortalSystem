class GridPage {
    constructor(id, pageSize) {
        this.id = id;
        this.pageSize = pageSize;
        this.pageNumber = 1;
        this.attachEvents();
    }

    attachEvents = () => {
        $(`#${this.id}`).on('click', '.previous', this.onPreviousPageClicked);
        $(`#${this.id}`).on('click', '.next', this.onNextPageClicked);
        $(`#${this.id}`).on('click', 'li:not(.active) .page-number', this.onPageNumberClicked);
    };

    getPreviousPageNumber = () => {
        let nextPageNumber = this.pageNumber - 1;

        if (nextPageNumber > 0) {
            return --this.pageNumber;
        }

        return null;
    }

    getNextPageNumber = () => {
        let nextPageNumber = this.pageNumber + 1;

        if (nextPageNumber <= this.pageCount) {
            return ++this.pageNumber;
        }

        return null;
    }

    render = (itemCount, refreshGridFunction) => {
        if (typeof this.refreshGridFunction === 'undefined')
            this.refreshGridFunction = refreshGridFunction;

        this.pageCount = Math.ceil(itemCount / this.pageSize);
        let pager = $(`#${this.id}`).empty();

        if (itemCount === 0) {
            return;
        }

        let template = `<ul>
                            <li>
                                <a class="previous">
                                    <img class="arrow" src="data:image/svg+xml;base64,PHN2ZyBpZD0iYmFzZWxpbmUtYXJyb3dfbGVmdC0yNHB4IiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0Ij4NCiAgPHBhdGggaWQ9IlBhdGhfMTk4NCIgZGF0YS1uYW1lPSJQYXRoIDE5ODQiIGQ9Ik0xNCw3LDksMTJsNSw1WiIgZmlsbD0iIzk3YTRiNSIvPg0KICA8cGF0aCBpZD0iUGF0aF8xOTg1IiBkYXRhLW5hbWU9IlBhdGggMTk4NSIgZD0iTTI0LDBWMjRIMFYwWiIgZmlsbD0ibm9uZSIvPg0KPC9zdmc+DQo=" />
                                </a>
                            </li>`;

        for (var pageNumber = 1; pageNumber <= this.pageCount; pageNumber++) {
            template += `<li${pageNumber === this.pageNumber ? ' class="active"' : ''}>
                            <a class="page-number">${pageNumber}</a>
                         </li>`;
        }

        template += `<li>
                        <a class="next">
                            <img class="arrow" src="data:image/svg+xml;base64,PHN2ZyBpZD0iYmFzZWxpbmUtYXJyb3dfcmlnaHQtMjRweCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCI+DQogIDxwYXRoIGlkPSJQYXRoXzE5ODYiIGRhdGEtbmFtZT0iUGF0aCAxOTg2IiBkPSJNMTAsMTdsNS01TDEwLDdaIiBmaWxsPSIjOTdhNGI1Ii8+DQogIDxwYXRoIGlkPSJQYXRoXzE5ODciIGRhdGEtbmFtZT0iUGF0aCAxOTg3IiBkPSJNMCwyNFYwSDI0VjI0WiIgZmlsbD0ibm9uZSIvPg0KPC9zdmc+DQo=" />
                        </a>
                    </li>
                </ul>`;

        pager.append(template);
    };

    onPreviousPageClicked = () => {
        let pageNumber = this.getPreviousPageNumber();

        if (pageNumber) {
            this.refreshGridFunction();
        }
    };

    onNextPageClicked = () => {
        let pageNumber = this.getNextPageNumber();

        if (pageNumber) {
            this.refreshGridFunction();
        }
    };

    onPageNumberClicked = event => {
        this.pageNumber = parseInt($(event.target).text());

        this.refreshGridFunction();
    };
}