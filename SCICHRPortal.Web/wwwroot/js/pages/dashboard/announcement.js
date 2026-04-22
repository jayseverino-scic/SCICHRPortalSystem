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
    const ROLE_DROPDOWN_SELECTOR = '#announcement-form #RoleIds';
    const SECTION_DROPDOWN_SELECTOR = '#announcement-form #SectionIds';
    const STUDENT_RECIPIENT_VALUE = 'student';
    const SELECT_ALL_SECTION_VALUE = 'all';
    let _currentUser = undefined;
    let _role = [];
    let _section = [];
    let _isSyncingSectionSelection = false;
    let _previousSectionValues = [];
    let attachEvents = () => {

        $('#announcement-form').on('submit', onFormSubmit);
        $('#add-button').on(CLICK_EVENT, onClickAddModal);
        $('#upload-file').on('change', onChangeFile);
        $('#announcement-form').on('change', '#RoleIds', onRoleChange);
        $('#announcement-form').on('change', '#SectionIds', onSectionChange);
    };

    let isRecipientSelected = () => {
        return getSelectedRecipientValues().length > 0;
    };

    let getSelectedRecipientValues = () => {
        return $(ROLE_DROPDOWN_SELECTOR).val() || [];
    };

    let getSelectedRoleIds = () => {
        return getSelectedRecipientValues()
            .map(value => Number(value))
            .filter(value => !Number.isNaN(value) && value > 0);
    };

    let shouldNotifyStudents = () => {
        return getSelectedRecipientValues().includes(STUDENT_RECIPIENT_VALUE);
    };

    let getSelectedSectionValues = () => {
        return $(SECTION_DROPDOWN_SELECTOR).val() || [];
    };

    let isAllSectionsSelected = () => {
        return getSelectedSectionValues().includes(SELECT_ALL_SECTION_VALUE);
    };

    let getSelectedSectionIds = () => {
        if (isAllSectionsSelected()) {
            return _section
                .map(section => Number(section.sectionId))
                .filter(sectionId => !Number.isNaN(sectionId) && sectionId > 0);
        }

        return getSelectedSectionValues()
            .map(value => Number(value))
            .filter(value => !Number.isNaN(value) && value > 0);
    };

    let resetRoleSelection = isDisabled => {
        let dropdown = $(ROLE_DROPDOWN_SELECTOR);
        if (!dropdown.length) {
            return;
        }

        dropdown.prop('disabled', !!isDisabled);
        dropdown.val(null).trigger('change');
        dropdown.trigger('change.select2');
    };

    let resetSectionSelection = isDisabled => {
        let dropdown = $(SECTION_DROPDOWN_SELECTOR);
        if (!dropdown.length) {
            return;
        }

        _previousSectionValues = [];
        dropdown.prop('disabled', !!isDisabled);
        dropdown.val(null).trigger('change');
        dropdown.trigger('change.select2');
    };

    let onChangeFile = e => {
        let allowedTypes = ['application/xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'];
        let fileInput = $('#upload-file')[0];

        var fileName = $('#upload-file')[0].files[0].name;
        $('#upload-file').next('label').text(fileName);
    }

    let toggleRoleSelection = isVisible => {
        let roleSection = $(ROLE_DROPDOWN_SELECTOR).closest('.form-group');
        if (!roleSection.length) {
            return;
        }

        if (isVisible) {
            roleSection.removeClass('d-none');
        } else {
            roleSection.addClass('d-none');
        }
    }

    let toggleSectionSelection = isVisible => {
        let sectionSelection = $(SECTION_DROPDOWN_SELECTOR).closest('.form-group');
        let dropdown = $(SECTION_DROPDOWN_SELECTOR);
        if (!sectionSelection.length || !dropdown.length) {
            return;
        }

        dropdown.prop('disabled', !isVisible);

        if (isVisible) {
            sectionSelection.removeClass('d-none');
        } else {
            sectionSelection.addClass('d-none');
        }
    }

    let setModalSize = isEdit => {
        let modalDialog = $('#announcement-modal .modal-dialog');
        let announcementFormSection = $('#announcement-form-section');
        if (!modalDialog.length) {
            return;
        }

        modalDialog.removeClass('modal-md modal-lg modal-xl');
        modalDialog.addClass(isEdit ? 'modal-md' : 'modal-lg');

        announcementFormSection.removeClass('col-6').addClass('col-12');
    }

    let onClickAddModal = function () {
        $('#announcement-form')[0].reset();
        setModalSize(false);
        toggleRoleSelection(true);
        resetRoleSelection(false);
        toggleSectionSelection(false);
        $('#role-validation').addClass('d-none');
        let fileInput = $('#upload-file');
        fileInput.val(null);
        fileInput.next('label').text("");
        $('#announcement-form').find(':submit').text('Add');
        $('#announcement-modal').modal('show');
    }
    let onRoleChange = function () {
        toggleRoleValidation();
        syncSectionSelectionVisibility();
    }

    let onSectionChange = function () {
        if (_isSyncingSectionSelection) {
            return;
        }

        normalizeSectionSelection();
    }

    let toggleRoleValidation = () => {
        if (isRecipientSelected()) {
            $('#role-validation').addClass('d-none');
        } else {
            $('#role-validation').removeClass('d-none');
        }
    }

    let syncSectionSelectionVisibility = () => {
        if (!shouldNotifyStudents()) {
            toggleSectionSelection(false);
            resetSectionSelection(true);
            return;
        }

        toggleSectionSelection(true);
        $(SECTION_DROPDOWN_SELECTOR).prop('disabled', false);
    }

    let normalizeSectionSelection = () => {
        let dropdown = $(SECTION_DROPDOWN_SELECTOR);
        if (!dropdown.length) {
            return;
        }

        let selectedValues = getSelectedSectionValues();
        if (!selectedValues.includes(SELECT_ALL_SECTION_VALUE)) {
            _previousSectionValues = selectedValues;
            return;
        }

        if (selectedValues.length === 1) {
            _previousSectionValues = selectedValues;
            return;
        }

        let previousHadSelectAll = _previousSectionValues.includes(SELECT_ALL_SECTION_VALUE);
        let normalizedValues = previousHadSelectAll
            ? selectedValues.filter(value => value !== SELECT_ALL_SECTION_VALUE)
            : [SELECT_ALL_SECTION_VALUE];

        _isSyncingSectionSelection = true;
        dropdown.val(normalizedValues).trigger('change');
        _isSyncingSectionSelection = false;
        _previousSectionValues = normalizedValues;
    }

    let onFormSubmit = async event => {
        event.preventDefault();
        let file = $('#upload-file')[0].files[0];
        let form = $(event.target);
        $(event.target).validate();
        let button = $(event.target).find(':submit').text().toLowerCase();
        if ($(event.target).valid()) {
            $('#busy-indicator-container').removeClass('d-none');
            let response = '';

            let data = _formHelper.toJsonString(event.target);
            let formData = new FormData();
            formData.append('announcement', JSON.stringify(data))
            formData.append('file', file)
            let currentTabTitle = 'Announcement';
            if (button == 'add') {
                if (!isRecipientSelected()) {
                    toggleRoleValidation();
                    $('#busy-indicator-container').addClass('d-none');
                    return;
                }

                getSelectedRoleIds().forEach(function (roleId) {
                    formData.append('roleIds', roleId);
                });
                if (shouldNotifyStudents()) {
                    formData.append('notifyStudents', 'true');
                    getSelectedSectionIds().forEach(function (sectionId) {
                        formData.append('sectionIds', sectionId);
                    });
                }
                response = _apiHelper.ajaxAnnouncementRequest('POST', {
                    url: 'Authenticated/Announcement',
                    data: formData,
                    requestOrigin: `${currentTabTitle} Tab`,
                    requesterName: $('#current-user').text(),
                    requestSystem: SYSTEM,
                    xhr: function () {
                        let xhr = new window.XMLHttpRequest();
                        xhr.upload.addEventListener("progress", function (evt) {

                        }, false);

                        return xhr;
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        console.log(XMLHttpRequest)
                        if (XMLHttpRequest.status === 409) {
                            let json = XMLHttpRequest.responseJSON;
                            Swal.fire(
                                'Error!',
                                json.message,
                                'error'
                            );
                        } else if (XMLHttpRequest.status == 403) {
                            noAccessAlert();
                        }
                        $('#busy-indicator-container').addClass('d-none');
                    },
                    success: function (response) {
                        $('#busy-indicator-container').addClass('d-none');
                        toastr.success('Success');
                        $('#announcement-grid').DataTable().ajax.reload(null, false);
                        $(event.target)[0].reset();
                        resetRoleSelection(false);
                        $('#role-validation').addClass('d-none');
                        $(event.target)[0].elements[1].focus();
                        $(event.target).find(':submit').prop("disabled", false).text('Add');
                        let fileInput = $('#upload-file');
                        fileInput.val(null);
                        fileInput.next('label').text("");
                        $('#announcement-modal').modal('hide');
                    }
                });
                status = 'Created!';
                return;
            }

            response = await _apiHelper.put({
                url: 'Authenticated/Announcement',
                data: data,
                requestOrigin: `${currentTabTitle} Tab`,
                requesterName: $('#current-user').text(),
                requestSystem: SYSTEM
            });
            status = 'Created!';

            if (response.ok) {
                $('#announcement-grid').DataTable().ajax.reload(null, false);
                toastr.success('Success');
                $(event.target)[0].reset();
                resetRoleSelection(false);
                $('#role-validation').addClass('d-none');
                $(event.target)[0].elements[1].focus();
                $(event.target).find(':submit').prop("disabled", false).text('Add');
                let fileInput = $('#upload-file');
                fileInput.val(null);
                fileInput.next('label').text("");
                $('#announcement-modal').modal('hide');
            }
            else if (response.status == 403) {
                noAccessAlert();
            }
            else if (response.status === 409) {
                let json = await response.json();
                Swal.fire(
                    'Error!',
                    json.message,
                    'error'
                );
            }

            $('#busy-indicator-container').addClass('d-none');
        }
    };

    let initializeGrid = async () => {
        let columns = await getColumns();
        let table = $('#announcement-grid').DataTable({
            bLengthChange: true,
            lengthMenu: [[5, 10, 20, 40, 80], [5, 10, 20, 40, 80]],
            bFilter: true,
            bInfo: true,
            serverSide: true,
            targets: 'no-sort',
            bSort: false,
            scrollY: "350px",
            scrollX: true,
            order: [1, 'asc'],
            "createdRow": function (row, data, dataIndex) {
                if (data.isTodayAnnouncement) {
                    $(row).addClass('red-background');
                }
            },
            ajax: async function (params, success, settings) {
                let gridInfo = $('#announcement-grid').DataTable().page.info();
                let searchKeyword = params.search.value;
                let pageSize = params.length;
                let response = await _apiHelper.get({
                    url: `Authenticated/Announcement/Filter?pageNumber=${gridInfo.page + 1}&pageSize=${pageSize}&searchKeyword=${searchKeyword}`,
                });

                if (response.ok) {
                    let json = await response.json();
                    let total = json.total;
                    success({
                        recordsFiltered: total,
                        recordsTotal: total,
                        data: json.data
                    });

                    $('.icon-delete').on('click', function (e) {
                        var data = table.row($(this).closest('tr')).data();
                        _formHelper.deleteRecord(e, data.title, SYSTEM);
                    });

                    $('#announcement-grid tbody').on('click', '.icon-edit', function () {
                        var data = table.row($(this).closest('tr')).data();
                        let form = $('#announcement-form');
                        populateForm(form, data);
                        let date = moment(data.createdAt);
                        form.find('#createdAt').val(date.format('yyyy-MM-DD'));
                        $('#announcement-grid .add-button').text('Update');
                        $('#announcement-modal').modal('show');
                    });

                } else {
                    success(null);
                }
            },
            columns: columns,
            pageLength: 5,
            dom: '<"pull-left">lBf<"pull-right">tipr',
        });
    }

    let populateForm = (form, data) => {
        $(form).find(':submit').text('Update');
        setModalSize(true);
        toggleRoleSelection(false);
        toggleSectionSelection(false);
        _formHelper.populateForm(form, data);
        resetRoleSelection(true);
        resetSectionSelection(true);
        $('#role-validation').addClass('d-none');
    }


    let getColumns = async () => {
        let columns = [
            {
                title: 'No.',
                data: "announcementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    let rowNumber = Number(meta.row) + 1;
                    return rowNumber;
                },
            },
            {
                title: 'Id',
                data: "announcementId",
                width: "1.5em",
                className: 'noVis dt-center',
                render: (data, type, row, meta) => {
                    return data;
                },
            },
            {
                title: "Date",
                data: "createdAt",
                render: (data, type, row) => {
                    return _dateHelper.formatShortLocalDate(data);
                },
            },
            {
                title: "Title",
                data: "title",
                render: (data, type, row) => {
                    return data
                },
            },


        ];
        let lastColumn = {
            data: "announcementId",
            width: "3em",
            render: function (data, type, full) {
                let buttons = '<a href="#" class="m-1 icon-edit" data-id="' + full.announcementId + '" data-endpoint="Authenticated/Announcement" data-table="announcement-grid"><i class="fas fa-edit"></i></a>';

                if (!(_currentUser.roleName == 'Parent'))
                    buttons += '<a href="#" class="m-1 icon-delete" data-id="' + full.announcementId + '" data-endpoint="Authenticated/Announcement" data-table="announcement-grid"><i class="fas fa-trash border-icon"></i></a>';

                return type === 'display' ?
                    buttons
                    :
                    "";
            },
            className: 'noVis dt-center'
        };
        columns.push(lastColumn);
        return columns;
    };

    let renderRoleDropdown = () => {
        let dropdown = $(ROLE_DROPDOWN_SELECTOR);
        if (!dropdown.length) {
            return;
        }

        if (dropdown.data('select2')) {
            dropdown.select2('destroy');
        }

        dropdown.empty();

        dropdown.append($('<option />').val(STUDENT_RECIPIENT_VALUE).text('Student'));

        _role.forEach(function (role) {
            dropdown.append($('<option />').val(role.roleId).text(role.name));
        });

        dropdown.select2({
            theme: 'bootstrap',
            width: '100%',
            multiple: true,
            closeOnSelect: false,
            dropdownParent: $('#announcement-modal'),
            placeholder: 'Select recipient(s)'
        });
    };

    let renderSectionDropdown = () => {
        let dropdown = $(SECTION_DROPDOWN_SELECTOR);
        if (!dropdown.length) {
            return;
        }

        if (dropdown.data('select2')) {
            dropdown.select2('destroy');
        }

        dropdown.empty();

        dropdown.append($('<option />').val(SELECT_ALL_SECTION_VALUE).text('Select All Section'));

        _section.forEach(function (section) {
            dropdown.append($('<option />').val(section.sectionId).text(`${section.gradeLevel.description} - ${section.description}`));
        });

        dropdown.select2({
            theme: 'bootstrap',
            width: '100%',
            multiple: true,
            closeOnSelect: false,
            dropdownParent: $('#announcement-modal'),
            placeholder: 'Select section(s)'
        });
    };

    let renderDropDowns = async () => {
        await getDropdownData();
        renderRoleDropdown();
        renderSectionDropdown();
        toggleSectionSelection(false);
        resetSectionSelection(true);
    };

    let getDropdownData = async () => {
        let responses = await Promise.all([
            _apiHelper.get({
                url: `Authenticated/Role`
            }),
            //_apiHelper.get({
            //    url: `Authenticated/Section`
            //})
        ]);

        let roleResponse = responses[0];
        if (roleResponse.ok) {
            _role = await roleResponse.json();
        }

        //let sectionResponse = responses[1];
        //if (sectionResponse.ok) {
        //    _section = await sectionResponse.json();
        //    _section = _section.sort((left, right) => {
        //        return (left.description || '').localeCompare(right.description || '');
        //    });
        //}
    }

    let initializeModals = e => {
        $('#announcement-modal').modal({ backdrop: 'static', keyboard: false });
        $('.nav-link').removeClass('d-none');
        $('.nav-link.announcement').addClass('active');

        $('#announcement-form #createdAt').attr('value', moment().format('yyyy-MM-DD'));
    }

    let getCurrentLogIn = async () => {
        let api = document.getElementById('base-api-url').value;

        let response = await fetch(`${api}/${`Authenticated/User/GetAuthenticatedAsync`}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${_cookieHelper.get('jsonWebToken')}`
            },
            body: JSON.stringify()
        });

        if (response.ok) {
            _currentUser = await response.json();
            $('#current-user').html(_currentUser.username);
            $('#user-role').html(_currentUser.roleName);
            $('#userId').val(_currentUser.userId);
            initializeGrids();
        }
    };


    let initializeGrids = e => {
        initializeGrid();
    }


    $(document).ready(function () {
        initializeModals();
        renderDropDowns();
        getCurrentLogIn();
        attachEvents();
    });

})(jQuery);
