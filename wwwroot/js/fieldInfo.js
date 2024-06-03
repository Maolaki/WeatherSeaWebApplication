$(document).ready(function () {
    try {
        fetchFieldInfo();
        updateTables('1');
    }
    catch (error) {
        console.error('Ошибка:', error.message);
    }

    const $updateButton = $('#updateField');
    const $editButton = $('#editField');
    const $deleteButton = $('#deleteField');
    const $inputFields = $('#field-info input');
    const $selectField = $('#field-info select');

    const $showWeatherButton = $('#showWeatherInfo');
    const $showFieldButton = $('#showFieldInfo');
    const $tableContainer = $('.table-container');
    const $sideTableContainer = $('.side-table-container');
    const $fieldInfoContainer = $('#field-info');

    $showWeatherButton.on('click', function () {
        $showWeatherButton.prop('disabled', true);
        $showFieldButton.prop('disabled', false);
        $tableContainer.show();
        $sideTableContainer.show();
        $fieldInfoContainer.hide();
    });

    $showFieldButton.on('click', function () {
        $showWeatherButton.prop('disabled', false);
        $showFieldButton.prop('disabled', true);
        $tableContainer.hide();
        $sideTableContainer.hide();
        $fieldInfoContainer.show();
    });

    $('.side-field-button').on('click', function (e) {
        e.preventDefault();

        // Убираем класс disabled со всех кнопок
        $('.side-field-button').removeClass('disabled');

        // Добавляем класс disabled на нажатую кнопку
        $(this).addClass('disabled');

        // Получаем day нажатой кнопки
        const day = $(this).attr('id');

        updateTables(day);
    });

    $editButton.on('click', function() {
        $inputFields.prop('disabled', false);
        $selectField.prop('disabled', false);
        $updateButton.show();
        $editButton.hide();
        $deleteButton.hide();
    });

    $updateButton.on('click', function () {
        updateFieldInfo();
    });

    $deleteButton.on('click', function () {
        deleteField()
    });

    async function fetchFieldInfo() {
        const token = sessionStorage.getItem('accessToken');
        const fieldId = getCookie("fieldId");

        const response = await fetch(`/Field/GetFieldInfo?fieldId=${fieldId}`, {
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

        document.getElementById('fieldName').value = data.field.name;
        document.getElementById('fieldOwner').innerText = data.field.ownerLogin;
        document.getElementById('fieldType').value = data.field.type;
        document.getElementById('fieldDescription').value = data.field.description;
        document.getElementById('fieldLatitude').value = data.field.latitude;
        document.getElementById('fieldLongitude').value = data.field.longitude;

        if (data.isEdit) {
            $editButton.show();
            $deleteButton.show();
        }
    }
});

async function deleteField() {
    try {
        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            throw new Error('Access token not found');
        }

        const fieldId = getCookie("fieldId");

        const response = await fetch(`/Field/DeleteField?fieldId=${fieldId}`, {
            method: "DELETE",
            headers: {
                "Authorization": "Bearer " + token,
                "Accept": "application/json",
                "Accept-Charset": "utf-8"
            }
        });

        if (response.ok) {
            alert('Поле успешно удалено.');

            window.location.href = '/Field/FieldList';
        } else {
            const errorText = await response.text();
            alert('Ошибка при удалении поля: ' + errorText);
        }
    } catch (error) {
        console.error('Ошибка:', error.message);
        alert('Ошибка при удалении поля: ' + error.message);
    }
}

async function updateFieldInfo() {
    const $updateButton = $('#updateField');
    const $editButton = $('#editField');
    const $deleteButton = $('#deleteField');
    const $inputFields = $('#field-info input');
    const $selectField = $('#field-info select');

    const token = sessionStorage.getItem('accessToken');

    if (!token) {
        throw new Error('Access token not found');
    }

    const formData = new FormData();

    const fieldId = getCookie("fieldId");
    const fieldName = document.getElementById('fieldName').value;
    const ownerLogin = document.getElementById('fieldOwner').innerText;
    const fieldType = document.getElementById('fieldType').value;
    const description = document.getElementById('fieldDescription').value;
    const latitude = document.getElementById('fieldLatitude').value;
    const longitide = document.getElementById('fieldLongitude').value;

    formData.append('FieldId', fieldId);
    formData.append('Name', fieldName);
    formData.append('OwnerLogin', ownerLogin);
    formData.append('Type', fieldType);
    formData.append('Description', description);
    formData.append('Latitude', latitude);
    formData.append('Longitude', longitide);

    const response = await fetch("/Field/UpdateField", {
        method: "POST",
        headers: {
            "Authorization": "Bearer " + token,
            "Accept": "application/json",
            "Accept-Charset": "utf-8"
        },
        body: formData
    });

    if (response.ok) {
        alert('Изменения сохранены.');
    } else {
        const errorText = await response.text();
        alert('Ошибка при сохранении изменений: ' + errorText);
    }

    $inputFields.prop('disabled', true);
    $selectField.prop('disabled', true);
    $updateButton.hide();
    $editButton.show();
    $deleteButton.show();
}

async function updateTables(day) {
    const token = sessionStorage.getItem('accessToken');
    const fieldId = getCookie("fieldId");

    if (!token) {
        throw new Error('Access token not found');
    }

    const response = await fetch(`/Field/GetFieldWeatherData?fieldId=${fieldId}&day=${day}`, {
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

    populateWeatherTable(data.weatherData);
    populateAverages(data.averages);
}

function populateWeatherTable(weatherData) {
    // Пример: заполнение таблицы с данными о погоде
    var table = document.querySelector('.table-container table');
    let newHtml = '<tr>';

    weatherData.forEach(item => {
        newHtml += `
            <td class="field">
                <img src="/icons/arrow-right.png" class="top-image" alt="Верхняя картинка" style="transform: rotate(${item.windDirection}deg);">
                <div class="top-text-number">${item.windSpeed}</div>
                <div class="top-text">м/с</div>
                <img src="${getWeatherIconUrl(item.description, item.time)}" class="center-image" alt="Центральная картинка">
                <div class="center-text-number">${item.temperature}</div>
                <div class="center-text">°С</div>
                <div class="bottom-text">${item.time}</div>
            <td>
        `;
    })

    newHtml += '</tr>';

    table.innerHTML = newHtml;
}

function populateAverages(averages) {
    document.querySelector('.side-field:nth-child(1) .field-text-right').textContent = `${averages.temperature.toFixed(1)} °С`;
    document.querySelector('.side-field:nth-child(2) .field-text-right').textContent = `${averages.windSpeed.toFixed(1)} м/с`;
    document.querySelector('.side-field:nth-child(3) .field-text-right').textContent = `${averages.icv.toFixed(0)}`; // Assuming ICV is an integer value
    document.querySelector('.side-field:nth-child(4) .field-text-right').textContent = `${averages.precipitation.toFixed(1)} мм`;
}

function getWeatherIconUrl(description, time) {
    const nightHours = ["00:00", "03:00", "21:00"];
    const isNight = nightHours.includes(time);

    if (isNight) {
        return "/icons/moon.png";
    }

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

// Получение значения cookie
function getCookie(name) {
    let cookieArr = document.cookie.split(";");

    for (let i = 0; i < cookieArr.length; i++) {
        let cookiePair = cookieArr[i].split("=");

        if (name === cookiePair[0].trim()) {
            return decodeURIComponent(cookiePair[1]);
        }
    }

    return null;
}