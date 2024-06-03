$(document).ready(function () {
    try {
        updateAccessTable();
    } catch (error) {
        console.error('Ошибка:', error.message);
        alert('Ошибка при получении данных сущности: ' + error.message);
    }

    $('#modal form').on('submit', async function (e) {
        e.preventDefault();

        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            alert('Токен доступа не найден');
            return;
        }

        const form = e.target;
        const formData = new FormData(this);
        const fieldId = getCookie('fieldId');
        formData.append('FieldId', fieldId);

        try {
            const response = await fetch("/Access/AddAccess", {
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

                updateAccessTable();
            } else {
                const errorText = await response.text();
                alert('Ошибка при добавлении доступа: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при добавлении доступа: ' + error.message);
        }
    });

    $(document).on('click', '.update-button', async function () {
        const fieldId = getCookie('fieldId');
        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            alert('Токен доступа не найден');
            return;
        }

        const formData = new FormData();

        // Добавляем значения полей формы в объект FormData
        formData.append('FieldId', fieldId);
        const userLogin = $(this).closest('tr').find('.field-name').text();
        const newType = $(this).closest('tr').find('.access-type').val();

        formData.append('FieldId', fieldId);
        formData.append('UserLogin', userLogin);
        formData.append('Type', newType);

        try {
            const response = await fetch("/Access/UpdateAccess", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Accept": "application/json",
                    "Accept-Charset": "utf-8"
                },
                body: formData
            });

            if (response.ok) {
                updateAccessTable();
            } else {
                const errorText = await response.text();
                alert('Ошибка при сохранении изменений: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при сохранении изменений: ' + error.message);
        }
    });

    $(document).on('click', '.delete-icon', async function () {
        const fieldId = getCookie('fieldId');
        const userLogin = $(this).closest('tr').find('.field-name').text();
        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            alert('Токен доступа не найден');
            return;
        }

        try {
            const response = await fetch(`/Access/DeleteAccess?fieldId=${fieldId}&userLogin=${userLogin}`, {
                method: "DELETE",
                headers: {
                    "Authorization": "Bearer " + token
                }
            });

            if (response.ok) {
                updateAccessTable();
            } else {
                const errorText = await response.text();
                alert('Ошибка при удалении доступа: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при удалении доступа: ' + error.message);
        }
    });

    $(document).on('click', '.gear-icon', function () {
        $(this).closest('tr').find('.update-icon').show();
        $(this).closest('tr').find('.gear-button').hide();
        $(this).closest('tr').find('.delete-button').hide();
        $(this).closest('tr').find('.access-type').prop('disabled', false);
    });


});

async function updateAccessTable() {
    const fieldId = getCookie('fieldId');
    const token = sessionStorage.getItem('accessToken');

    if (!token) {
        throw new Error('Токен доступа не найден');
    }

    const response = await fetch(`/Access/GetAccessesOnField?fieldId=${fieldId}`, {
        method: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Accept": "application/json",
            "Accept-Charset": "utf-8"
        }
    });

    if (!response.ok) {
        throw new Error('Ошибка при получении доступов: ' + response.status);
    }

    const data = await response.json();

    let newHtml = '';
    const tableContainer = document.querySelector('.table-container table');

    data.forEach(access => {
        newHtml += `
            <tr>
                <td class="field">
                    <div class="field-name">${access.userLogin}</div>
                    <select class="access-type" disabled>
                        <option value="0" ${access.type === 0 ? 'selected' : ''}>Просмотр</option>
                        <option value="1" ${access.type === 1 ? 'selected' : ''}>Редактирование</option>
                    </select>
                    <span class="delete-button">
                        <img src="/icons/bin.png" alt="Удалить" class="delete-icon">
                    </span>
                    <span class="gear-button">
                        <img src="/icons/gear.png" alt="Редактировать" class="gear-icon">
                    </span>
                    <span class="update-button">
                        <img src="/icons/save.png" alt="Редактировать" class="update-icon" style="display: none;">
                    </span>
                </td>
            </tr>
        `;
    });

    newHtml += `
                <td class="addField">
                    <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#modal">
                        <img src="/icons/add-button.png" alt="Добавить сущность">
                    </button>
                </td>
            `;

    tableContainer.innerHTML = newHtml;
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}
