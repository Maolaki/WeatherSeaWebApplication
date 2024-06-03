document.addEventListener("DOMContentLoaded", async function () {
    // Получение отчетов при загрузке страницы
    await getReports();

    // Заполнение комбобоксов данными при загрузке страницы
    await getPlants();
    await getAnimals();
    await getFields();

    // Обработчик события для кнопки удаления отчета
    document.addEventListener("click", async function (event) {
        if (event.target.classList.contains("delete-icon")) {
            var reportId = event.target.getAttribute("data-report-id");
            await deleteReport(reportId);
        } else if (event.target.classList.contains("download-icon")) {
            var reportId = event.target.getAttribute("data-report-id");
            await downloadReport(reportId);
        }
    });

    // Обработчик события для изменения типа отчета
    $("#Type").change(async function () {
        var selectedType = $(this).val();
        await toggleSelects(selectedType);
    });

    // Обработчик события для кнопки "Добавить"
    $("#reportForm").submit(async function (event) {
        event.preventDefault(); // Предотвращаем отправку формы по умолчанию
        var reportName = $("#Name").val();
        var reportType = $("#Type").val();
        var fieldId = $("#Field").val();
        var plantId = $("#Plant").val();
        var animalId = $("#Animal").val();

        try {
            const formData = new FormData();
            formData.append('FieldId', fieldId);
            formData.append('Name', reportName);
            formData.append('Type', reportType);

            // Строка запроса для передачи параметров через URL
            let url = `/Report/AddReport?plantId=${plantId}&animalId=${animalId}`;

            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
                },
                body: formData
            });
            if (!response.ok) {
                throw new Error("Ошибка при добавлении отчета.");
            }
            // После успешного добавления обновляем список отчетов
            await getReports();
            // Закрываем модальное окно
            $("#modal").modal("hide");
        } catch (error) {
            console.error("Ошибка при добавлении отчета:", error);
        }
    });

    // Обработчики событий для фильтров и сортировки
    document.getElementById("filter-standart-button").addEventListener("click", function () {
        toggleFilter("Standart");
    });

    document.getElementById("filter-plants-button").addEventListener("click", function () {
        toggleFilter("Plant");
    });

    document.getElementById("filter-animals-button").addEventListener("click", function () {
        toggleFilter("Animal");
    });

    document.getElementById("filter-combined-button").addEventListener("click", function () {
        toggleFilter("Combined");
    });

    document.getElementById("sort-button").addEventListener("click", toggleSort);

    document.getElementById("search-input").addEventListener("input", function () {
        applySearchFilter(this.value);
    });
});

let currentFilter = "";
let currentSortOrder = 0; // 0 - no sort, 1 - asc, 2 - desc

// Функция для фильтрации отчетов
function toggleFilter(filterType) {
    if (currentFilter === filterType) {
        currentFilter = "";
    } else {
        currentFilter = filterType;
    }
    getReports();
}

// Функция для сортировки отчетов
function toggleSort() {
    currentSortOrder = (currentSortOrder + 1) % 3;
    getReports();
}

// Функция для поиска отчетов
function applySearchFilter(searchTerm) {
    getReports(searchTerm);
}

// Функция для отображения или скрытия комбобоксов в зависимости от выбранного типа отчета
async function toggleSelects(selectedType) {
    if (selectedType === "Plant") {
        $("#plantSelect").show();
        $("#animalSelect").hide();
    } else if (selectedType === "Animal") {
        $("#plantSelect").hide();
        $("#animalSelect").show();
    } else if (selectedType === "Combined") {
        $("#plantSelect").show();
        $("#animalSelect").show();
    } else {
        $("#plantSelect").hide();
        $("#animalSelect").hide();
    }
}

// Функция для получения отчетов с сервера с учетом фильтров, сортировки и поиска
async function getReports(searchTerm = "") {
    try {
        const response = await fetch("/Report/GetReports", {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            }
        });
        if (!response.ok) {
            throw new Error("Ошибка при получении отчетов.");
        }
        let data = await response.json();

        // Применение фильтрации
        if (currentFilter) {
            switch (currentFilter) {
                case "Standart":
                    data = data.filter(report => report.type === 0);
                    break;
                case "Plant":
                    data = data.filter(report => report.type === 1);
                    break;
                case "Animal":
                    data = data.filter(report => report.type === 2);
                    break;
                case "Combined":
                    data = data.filter(report => report.type === 3);
                    break;
            }
        }

        // Применение сортировки
        if (currentSortOrder !== 0) {
            data.sort((a, b) => {
                if (currentSortOrder === 1) {
                    return a.name.localeCompare(b.name);
                } else if (currentSortOrder === 2) {
                    return b.name.localeCompare(a.name);
                }
            });
        }

        // Применение поиска
        if (searchTerm) {
            data = data.filter(report => report.name.toLowerCase().includes(searchTerm.toLowerCase()));
        }

        displayReports(data);
    } catch (error) {
        console.error("Ошибка при получении отчетов:", error);
    }
}

// Функция для отображения отчетов в таблице
function displayReports(reports) {
    var table = document.querySelector("#Table table");
    table.innerHTML = "";

    reports.forEach(report => {
        var row = `
            <tr>
                <td class="field">
                    <a id="${report.id}"></a>
                    <div class="entity-name">${report.name}</div>
                    <div class="class">${translateClass(report.type)}</div>
                    <span class="download-button">
                        <img src="/icons/download.png" alt="Скачать" class="download-icon" data-report-id="${report.id}">
                    </span>
                    <span class="delete-button">
                        <img src="/icons/bin.png" alt="Удалить" class="delete-icon" data-report-id="${report.id}">
                    </span>
                </td>
            </tr>
        `;
        table.insertAdjacentHTML("beforeend", row);
    });

    // Добавление кнопки "Добавить сущность"
    var addFieldRow = `
        <tr>
            <td class="addField">
                <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#modal">
                    <img src="/icons/add-button.png" alt="Добавить сущность">
                </button>
            </td>
        </tr>
    `;
    table.insertAdjacentHTML("beforeend", addFieldRow);
}

function translateClass(type) {
    switch (type) {
        case 0:
            return "Стандартный";
        case 1:
            return "Только растение";
        case 2:
            return "Только животное";
        case 3:
            return "Комбинированный";
    }
}

// Функция для удаления отчета
async function deleteReport(reportId) {
    try {
        const response = await fetch(`/Report/DeleteReport?reportId=${reportId}`, {
            method: "DELETE",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            },
        });
        if (!response.ok) {
            throw new Error("Ошибка при удалении отчета.");
        }
        // После успешного удаления обновите список отчетов
        await getReports();
    } catch (error) {
        console.error("Ошибка при удалении отчета:", error);
    }
}

// Функция для скачивания отчета
async function downloadReport(reportId) {
    try {
        const response = await fetch(`/Report/DownloadReport?reportId=${reportId}`, {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            }
        });
        if (!response.ok) {
            throw new Error("Ошибка при скачивании отчета.");
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.style.display = "none";
        a.href = url;
        a.download = `report_${reportId}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
    } catch (error) {
        console.error("Ошибка при скачивании отчета:", error);
    }
}

// Функция для получения списка полей
async function getFields() {
    try {
        const response = await fetch("/Report/GetFields", {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            }
        });
        if (!response.ok) {
            throw new Error("Ошибка при получении списка полей.");
        }
        const data = await response.json();
        // Очищаем список полей
        $("#Field").empty();
        // Заполняем список растениями из полученных данных
        data.forEach(function (field) {
            $("#Field").append(`<option value="${field.fieldId}">${field.name}</option>`);
        });
    } catch (error) {
        console.error("Ошибка при получении списка полей:", error);
    }
}

// Функция для получения списка растений
async function getPlants() {
    try {
        const response = await fetch("/Report/GetPlants", {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            }
        });
        if (!response.ok) {
            throw new Error("Ошибка при получении списка растений.");
        }
        const data = await response.json();
        // Очищаем список растений
        $("#Plant").empty();
        // Заполняем список растениями из полученных данных
        data.forEach(function (plant) {
            $("#Plant").append(`<option value="${plant.id}">${plant.name}</option>`);
        });
    } catch (error) {
        console.error("Ошибка при получении списка растений:", error);
    }
}

// Функция для получения списка животных
async function getAnimals() {
    try {
        const response = await fetch("/Report/GetAnimals", {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + sessionStorage.getItem("accessToken")
            }
        });
        if (!response.ok) {
            throw new Error("Ошибка при получении списка животных.");
        }
        const data = await response.json();
        // Очищаем список животных
        $("#Animal").empty();
        // Заполняем список животными из полученных данных
        data.forEach(function (animal) {
            $("#Animal").append(`<option value="${animal.id}">${animal.name}</option>`);
        });
    } catch (error) {
        console.error("Ошибка при получении списка животных:", error);
    }
}
