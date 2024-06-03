$(document).ready(function () {
    try {
        updateTables();
    }
    catch (error) {
        console.error('Ошибка:', error.message);
    }

    // Обработка формы добавления поля
    $('#modal form').on('submit', async function (e) {
        e.preventDefault();

        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            alert('Токен доступа не найден');
            return;
        }

        const form = e.target;
        const formData = new FormData(this);

        try {
            const response = await fetch("/Field/AddField", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Accept": "application/json",
                    "Accept-Charset": "utf-8"
                },
                body: formData
            });

            if (response.ok) {
                $('#modal').modal('hide');
                form.reset();

                updateTables();
            } else {
                const errorText = await response.text();
                alert('Ошибка при добавлении поля: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при добавлении поля: ' + error.message);
        }
    });

    $(document).on('click', '.field', function () {
        const fieldId = $(this).find('a').attr('id');

        setCookie('fieldId', fieldId, 1); // Срок действия cookie 1 день

        window.location.href = "/Field/FieldInfo"; // Перенаправляем на новую страницу
    });
});

async function updateTables() {
    const token = sessionStorage.getItem('accessToken');

    if (!token) {
        throw new Error('Access token not found');
    }

    const response = await fetch("/Field/GetFields", {
        method: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Accept": "application/json",
            "Accept-Charset": "utf-8"
        }
    });

    if (!response.ok) {
        throw new Error('Error fetching access data: ' + response.status);
    }

    const data = await response.json();

    renderFields(data.myFields, "#MyTables", true);
    renderFields(data.accessibleFields, "#AlienTables", false);
}

function renderFields(fields, tableContainerId, isMyFields) {
    var tableContainer = $(tableContainerId);
    var table = tableContainer.find('table');

    let newHtml = '';
    let counter = 0;

    fields.forEach(field => {
        if (counter % 3 === 0) {
            newHtml += '<tr>';
        }

        var weatherIcon = getWeatherIconUrl(field.weather.description);
        var windDirection = `rotate(${field.weather.windDirection}deg)`;

        var fieldHtml = `
            <td class="field">
                <a id="${field.fieldId}">
                    <img src="${weatherIcon}" class="main-image" alt="Основное изображение">
                </a>
                <div class="field-name">${field.name}</div>
                <div class="temperature">${field.weather.temperature} °С</div>
                <div class="air-info">
                    <img src="/icons/arrow-right.png" class="small-image" alt="Маленькое изображение" style="transform: ${windDirection};">
                    <div class="air-speed">${field.weather.windSpeed} м/сек</div>
                </div>
            </td>
        `;

        newHtml += fieldHtml;
        counter++;

        if (counter % 3 === 0) {
            newHtml += '</tr>';
        }
    });

    // Добавление пустых ячеек, если последний ряд содержит меньше трех элементов
    if (counter % 3 !== 0) {
        let emptyCells = 3 - (counter % 3);
        for (let i = 0; i < emptyCells; i++) {
            newHtml += '<td class="field invisible"></td>';
        }
        newHtml += '</tr>';
    }

    // Добавление кнопки "Добавить поле" для моих полей
    if (isMyFields) {
        newHtml += `
            <tr>
                <td colspan="3" class="addField">
                    <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#modal">
                        <img src="/icons/add-button.png" alt="Добавить поле">
                    </button>
                </td>
            </tr>
        `;

        counter++;

        if (counter % 3 === 0) {
            newHtml += '</tr>';
        }
    }

    // Показ сообщения, если нет чужих полей
    if (!isMyFields && fields.length === 0) {
        newHtml = `
            <tr>
                <td colspan="3" class="no-fields-message">
                    У вас отсутствуют доступы к чужим полям!
                </td>
            </tr>
        `;
    }

    table.html(newHtml);
}



function getWeatherIconUrl(description) {
    switch (description.toLowerCase()) {
        case "clear sky":
        case "sunny":
            return "/icons/sun.png";
        case "few clouds":
        case "scattered clouds":
        case "broken clouds":
        case "overcast clouds":
            return "/icons/bigCloud.png";
        case "shower rain":
        case "moderate rain":
        case "rain":
        case "light rain":
            return "/icons/rain.png";
        case "thunderstorm":
            return "/icons/storm.png";
        case "snow":
        case "mist":
            return "/icons/happyCloud.png";
        default:
            return "/icons/moon.png"; // Иконка по умолчанию
    }
}

function setCookie(id, value, days) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
    const expires = "expires=" + date.toUTCString();
    document.cookie = id + "=" + value + ";" + expires + ";path=/";
}