class PdfHelper {
    defaultOptions = {
        defaultStyle:
        {
            font: 'Roboto'
        },
        pageSize: 'A4',
        pageOrientation: 'landscape',
        title: '',
        data: [],
        headers: [],
        fileName: 'file_' + Date.now()
    };

    constructor(params) {
    }

    buildTableBody = (data, columns) => {
        var body = [];

        let headers = _.map(_.clone(columns), c => {
            return {
                text: this.capitalize(c),
                style: 'tableHeader'
            }
        });
        body.push(headers);

        data.forEach(function (row) {
            var dataRow = [];

            columns.forEach(function (column, i) {
                dataRow.push({ text: row[column].toString(), style: 'tableData' });
            })

            body.push(dataRow);
        });

        return body;
    }

    createTableStructure = (data) => {
       

        let tables = [];

        _.each(data, (value, key) => {
            tables.push({ text: this.capitalize(key), style: 'subheader' });

            let headers = _.keys(value[0]);
            let widths = _.map(_.clone(headers), h => 'auto');
            tables.push({
                columns: [
                    { width: '*', text: '' },
                    {
                        width: 'auto',
                        table: {
                            width: widths,
                            style: 'table',
                            width: '100',
                            headerRows: 1,
                            body: this.buildTableBody(value, headers),
                        },
                        layout: {
                            fillColor: function (rowIndex, node, columnIndex) {
                                return (rowIndex % 2 === 0) ? '#CCCCCC' : null;
                            },
                            hLineWidth: function (i, node) {
                                return (i === 1 || i === node.table.body.length) ? 2 : 0;
                            },
                            vLineWidth: function (i, node) {
                                return 0;
                            },
                            hLineColor: function (i, node) {
                                return (i === 1 || i === node.table.body.length) ? 'black' : null;
                            },
                        },
                    },
                    { width: '*', text: '' },
                ]
            });
        });


        return tables;
    }

    create = (params) => {

        let config = $.extend(this.defaultOptions, params);

        let tableData = this.createTableStructure(config.data);

        let documentDefinition = {
            content: [
                { text: config.title, style: 'header' },
                ...tableData,
            ],
            defaultStyle: config.defaultStyle,
            pageSize: config.pageSize,
            pageOrientation: config.pageOrientation,
            title: config.title,
            styles: {
                header: {
                    fontSize: 18,
                    bold: true,
                    margin: [0, 0, 0, 30],
                    alignment: "center"
                },
                subheader: {
                    fontSize: 16,
                    bold: true,
                    margin: [0, 30, 0, 20],
                    alignment: "center"
                },
                table: {
                    margin: [0, 30, 0, 50],
                    alignment: 'center'
                },
                tableHeader: {
                    bold: true,
                    fontSize: 9,
                    color: 'white',
                    fillColor: '#2d4154',
                    alignment: "center"
                },
                tableData: {
                    fontSize: 8,
                    alignment: "center"
                }
            },
        };

        pdfMake.createPdf(documentDefinition).download(this.removeExtension(config.fileName) + '_' + Date.now());
    }

    capitalize = (str) => {
        if (str.includes("Count") || str.includes("Trips"))
            return str.charAt(0).toUpperCase() + str.charAt(1).toUpperCase() + str.slice(2)

        return str.charAt(0).toUpperCase() + str.slice(1)
    }

    pascalize = (data) => {
        return _.map(data, d => this.capitalize(d));
    }

    removeExtension = (filename) => {
        var lastDotPosition = filename.lastIndexOf(".");
        if (lastDotPosition === -1) return filename;
        else return filename.substr(0, lastDotPosition);
    }



}