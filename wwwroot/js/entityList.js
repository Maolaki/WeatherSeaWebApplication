let filterAnimals = false;
let filterPlants = false;
let sortState = 0; // 0: no sort, 1: plants first, 2: animals first

$(document).ready(async function () {
    try {
        await updateEntityTable();
    } catch (error) {
        console.error('Ошибка:', error.message);
    }

    $('#search-input').on('input', function () {
        updateEntityTable();
    });

    $('#sort-button').on('click', function () {
        sortState = (sortState + 1) % 3;
        updateEntityTable();
    });

    $('#filter-animals-button').on('click', function () {
        filterAnimals = !filterAnimals;
        filterPlants = false; // Сбросить фильтр по растениям
        updateEntityTable();
    });

    $('#filter-plants-button').on('click', function () {
        filterPlants = !filterPlants;
        filterAnimals = false; // Сбросить фильтр по животным
        updateEntityTable();
    });

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
            const response = await fetch("/Entity/AddEntity", {
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

                updateEntityTable();
            } else {
                const errorText = await response.text();
                alert('Ошибка при добавлении поля: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при добавлении поля: ' + error.message);
        }
    });

    $(document).on('click', '.delete-icon', async function (event) {
        event.stopPropagation();

        const entityId = $(this).data('entity-id');
        const token = sessionStorage.getItem('accessToken');

        if (!token) {
            alert('Токен доступа не найден');
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
                updateEntityTable();
            } else {
                const errorText = await response.text();
                alert('Ошибка при удалении сущности: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error.message);
            alert('Ошибка при удалении сущности: ' + error.message);
        }
    });

    $(document).on('click', '.field', function () {
        const entityId = $(this).find('a').attr('id');
        setCookie('entityId', entityId, 1);
        window.location.href = "/Entity/EntityInfo";
    });
});

async function updateEntityTable(filter = false, sort = false) {
    const token = sessionStorage.getItem('accessToken');

    if (!token) {
        throw new Error('Токен доступа не найден');
    }

    const response = await fetch("/Entity/GetEntities", {
        method: "GET",
        headers: {
            "Authorization": "Bearer " + token,
            "Accept": "application/json",
            "Accept-Charset": "utf-8"
        }
    });

    if (!response.ok) {
        throw new Error('Ошибка при получении полей: ' + response.status);
    }

    const data = await response.json();

    let newHtml = '';
    const tableContainer = document.querySelector('#Table table');

    if (data === "noAccess") {
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

    const searchQuery = $('#search-input').val().toLowerCase();
    let filteredData = data.filter(entity => {
        const entityName = entity.entity.name;
        return entityName && entityName.toLowerCase().includes(searchQuery);
    });

    if (filterAnimals) {
        filteredData = filteredData.filter(entity => translateClass(entity.customClass).toLowerCase() === 'животное');
    } else if (filterPlants) {
        filteredData = filteredData.filter(entity => translateClass(entity.customClass).toLowerCase() === 'растение');
    }

    if (sortState === 1) {
        filteredData.sort((a, b) => {
            const classA = translateClass(a.customClass);
            const classB = translateClass(b.customClass);
            return classA === 'Растение' && classB !== 'Растение' ? -1 : classA !== 'Растение' && classB === 'Растение' ? 1 : 0;
        });
    } else if (sortState === 2) {
        filteredData.sort((a, b) => {
            const classA = translateClass(a.customClass);
            const classB = translateClass(b.customClass);
            return classA === 'Животное' && classB !== 'Животное' ? -1 : classA !== 'Животное' && classB === 'Животное' ? 1 : 0;
        });
    }

    filteredData.forEach(entity => {
        newHtml += `
            <tr>
                <td class="field">
                    <a id="${entity.entity.id}"></a>
                    <div class="entity-name">${entity.entity.name}</div>
                    <div class="class">${translateClass(entity.customClass)}</div>
                    <span class="delete-button">
                        <img src="/icons/bin.png" alt="Удалить" class="delete-icon" data-entity-id="${entity.entity.id}">
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

function translateClass(customClass) {
    switch (customClass) {
        case 'Plant':
            return 'Растение';
        case 'Animal':
            return 'Животное';
        default:
            return customClass || '';
    }
}

function setCookie(id, value, days) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
    const expires = "expires=" + date.toUTCString();
    document.cookie = id + "=" + value + ";" + expires + ";path=/";
}
