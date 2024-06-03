$(document).ready(function () {
    const tokenKey = "accessToken";
    const entityId = getCookie('entityId');
    const token = sessionStorage.getItem(tokenKey);

    if (!token) {
        alert('Токен доступа не найден');
        return;
    }

    // Загрузка данных сущности
    async function loadEntityData() {
        try {
            const response = await fetch(`/Entity/GetEntity?entityId=${entityId}`, {
                method: "GET",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Accept": "application/json"
                }
            });

            if (response.ok) {
                const entity = await response.json();
                $('#Name').val(entity.name);
                $('#Type').val(entity.class);
                $('#Description').val(entity.description);
                $('#RecommendedTemperature').val(entity.recommendedTemperature);
                $('#RecommendedWindSpeed').val(entity.recommendedWindSpeed);
                $('#RecommendedHumidity').val(entity.recommendedHumidity);
            } else {
                const errorText = await response.text();
                alert('Ошибка при получении данных сущности: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при получении данных сущности: ' + error.message);
        }
    }

    loadEntityData();

    // Удаление сущности
    $('#delete-button').on('click', async function () {
        const confirmed = confirm("Вы уверены, что хотите удалить эту сущность?");
        if (!confirmed) {
            return;
        }

        try {
            const response = await fetch(`/Entity/DeleteEntity?entityId=${entityId}`, {
                method: "DELETE",
                headers: {
                    "Authorization": "Bearer " + token
                }
            });

            if (response.ok) {
                alert('Сущность успешно удалена.');
                window.location.href = "/Entity/EntityList";
            } else {
                const errorText = await response.text();
                alert('Ошибка при удалении сущности: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при удалении сущности: ' + error.message);
        }
    });

    // Включение режима редактирования
    $('#edit-button').on('click', function () {
        $('#Name').prop('readonly', false);
        $('#Type').prop('disabled', false);
        $('#Description').prop('readonly', false);
        $('#RecommendedTemperature').prop('readonly', false);
        $('#RecommendedWindSpeed').prop('readonly', false);
        $('#RecommendedHumidity').prop('readonly', false);
        $('#delete-button').hide();
        $('#edit-button').hide();
        $('#save-button').show();
    });

    // Сохранение изменений
    $('#save-button').on('click', async function () {
        const formData = new FormData();

        // Добавляем значения полей формы в объект FormData
        formData.append('Id', entityId);
        formData.append('Name', $('#Name').val());
        formData.append('Class', $('#Type').val());
        formData.append('Description', $('#Description').val());
        formData.append('RecommendedTemperature', $('#RecommendedTemperature').val());
        formData.append('RecommendedWindSpeed', $('#RecommendedWindSpeed').val());
        formData.append('RecommendedHumidity', $('#RecommendedHumidity').val());

        try {
            const response = await fetch("/Entity/UpdateEntity", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Accept": "application/json",
                    "Accept-Charset": "utf-8"
                },
                body: formData
            });

            if (response.ok) {
                $('#Name').prop('readonly', true);
                $('#Type').prop('disabled', true);
                $('#Description').prop('readonly', true);
                $('#RecommendedTemperature').prop('readonly', true);
                $('#RecommendedWindSpeed').prop('readonly', true);
                $('#RecommendedHumidity').prop('readonly', true);
                $('#delete-button').show();
                $('#edit-button').show();
                $('#save-button').hide();

                loadEntityData();
            } else {
                const errorText = await response.text();
                alert('Ошибка при обновлении сущности: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при обновлении сущности: ' + error.message);
        }
    });

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
});
