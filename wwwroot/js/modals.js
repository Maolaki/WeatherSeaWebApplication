$('#modal').modal({
    backdrop: false
});

$(document).ready(function () {

    $('#modal').modal('hide');

    // Добавление обработчика клика на фон модального окна
    $(document).on('click', '.modal-backdrop', function (event) {
        // Проверка, что клик был сделан вне .modal-content
        if (!$(event.target).closest('.modal-content').length) {
            $('#modal').modal('hide');
        }
    });
});