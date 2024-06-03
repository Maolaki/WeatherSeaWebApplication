$(document).ready(async function () {
    try {
        updateAccessTable();
    } catch (error) {
        console.error('Error:', error.message);
    }

    // Делегирование событий
    $(document).on('click', '#accessField', function () {
        const fieldId = $(this).attr('field-id');

        // Записываем название поля в cookie
        setCookie('fieldId', fieldId, 1); // Срок действия cookie 1 день

        window.location.href = '/Access/AccessInfo';
    });
});

async function updateAccessTable() {
    const token = sessionStorage.getItem('accessToken');

    if (!token) {
        throw new Error('Access token not found');
    }

    const response = await fetch("/Access/GetAccessesAndFields", {
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

    const tableContainer = document.querySelector('.table-container table');

    let newHtml = '';

    if (data == "noAccess") {

        newHtml = `
            <tr>
                <td colspan="3" class="no-fields-message">
                    Работа с сущностями доступна только по Premium доступу!
                </td>
            </tr>
        `;

        tableContainer.innerHTML = newHtml;
        return;
    }

    data.forEach(item => {
        newHtml += `
            <tr>
                <td class="field" id="accessField" field-id="${item.fieldId}">
                    <div class="entity-name">${item.fieldName}</div>
                    <div class="class-edit">Редактирование: ${item.editCount}</div>
                    <div class="class-view">Просмотр: ${item.viewCount}</div>
                </td>
            </tr>
        `;
    });

    tableContainer.innerHTML = newHtml;
}

function setCookie(id, value, days) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
    const expires = "expires=" + date.toUTCString();
    document.cookie = id + "=" + value + ";" + expires + ";path=/";
}