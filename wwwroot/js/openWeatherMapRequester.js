function fillTableWithWeather() {
    var apiKey = 'd70ee2c7097a6708357e647b25b32615';
    var url = 'https://api.openweathermap.org/data/2.5/forecast?q=London&appid=' + apiKey;

    $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            console.log('Ошибка: ' + data.text);
            // Перебираем массив прогнозов
            for (var i = 0; i < data.list.length; i++) {
                // Получаем прогноз на нужное время
                var forecast = data.list[i];
                var date = new Date(forecast.dt * 1000);
                var hours = date.getHours();

                // Находим соответствующую ячейку по времени
                var cell = $('.table-container .field').filter(function () {
                    return $(this).find('.bottom-text').text().startsWith(hours + ':00');
                });

                if (cell.length > 0) {
                    // Обновляем данные в ячейке
                    cell.find('.top-text-number').text(forecast.wind.speed);
                    cell.find('.center-text-number').text(Math.round(forecast.main.temp - 273.15));
                }
            }
        },
        error: function (error) {
            console.log('Ошибка: ' + error.responseText);
        }
    });
}

// Вызовите функцию при загрузке страницы
$(document).ready(function () {
    fillTableWithWeather();
});