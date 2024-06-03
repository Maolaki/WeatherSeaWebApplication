$(document).ready(function () {
    const modal = $('#modal');
    const modalTitle = $('#profileModalLabel');
    const modalBodyText1 = $('#modalBodyText1');
    const modalBodyText2 = $('#modalBodyText2');
    const inputField = $('#inputField');
    const confirmPassword = $('#confirmPassword');
    const saveButton = $('#saveButton');


    UpdateInfo();
    $('.profile-button').on('click', function () {
        let buttonId = $(this).attr('id');

        switch (buttonId) {
            case '1':
                openModal('Изменение имени', 'Введите новое имя:', 'Введите пароль:');
                break;
            case '2':
                openModal('Изменение пароля', 'Введите новый пароль:', 'Повторите пароль:');
                break;
            case '3':
                openModal('Изменение почты', 'Введите новую почту:', 'Введите пароль:');
                break;
            case '4':
                {
                    updateStatus();
                    UpdateInfo();
                    return;
                }

                break;
            default:
                console.error('Неизвестное действие');
                break;
        }
    });

    function GetStatus(type) {
        switch (type) {
            case 0:
                return "Standart";
            case 1:
                return "Premium";
        }
    }

    function UpdateInfo() {
        const token = sessionStorage.getItem("accessToken");
        fetch('/User/GetProfile', {
            method: 'GET',
            headers: {
                "Authorization": "Bearer " + token,
                "Accept": "application/json",
                "Accept-Charset": "utf-8"
            }
        })
            .then(response => response.json())
            .then(data => {
                $('.username').text(data.username);
                $('.email').text(data.email);
                $('.status').text(GetStatus(data.type));
            })
            .catch(error => console.error('Ошибка:', error));
    }

    saveButton.on('click', async function () {
        const token = sessionStorage.getItem("accessToken");
        let action = modalTitle.text();

        let inputValue = inputField.val();
        let passwordValue = confirmPassword.val();

        const formData = new FormData();

        // Пример запроса
        let url = '';
        if (action === 'Изменение имени') {
            url = '/User/UpdateName';
            formData.append('NewName', inputField.val());
            formData.append('Password', confirmPassword.val());
        } else if (action === 'Изменение пароля') {
            url = '/User/UpdatePassword';
            formData.append('NewPassword', inputField.val());
            formData.append('Password', confirmPassword.val());
        } else if (action === 'Изменение почты') {
            url = '/User/UpdateEmail';
            formData.append('NewEmail', inputField.val());
            formData.append('Password', confirmPassword.val());
        } 

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    "Authorization": "Bearer " + token,
                    "Accept": "application/json",
                    "Accept-Charset": "utf-8"
                },
                body: formData
            });

            if (response.ok) {
                UpdateInfo();
                modal.modal('hide');
            } else {
                let errorText = await response.text();
                alert('Ошибка: ' + errorText);
            }
        } catch (error) {
            console.error('Ошибка:', error);
            alert('Ошибка при выполнении запроса.');
        }
    });


    async function updateStatus() {
        const token = sessionStorage.getItem("accessToken");
        const response = await fetch('/User/UpdateStatus', {
            method: 'GET',
            headers: {
                "Authorization": "Bearer " + token,
                "Accept": "application/json",
                "Accept-Charset": "utf-8"
            },
        })

        if (!response.ok) {
            throw new Error('Ошибка при получении полей: ' + response.status);
        }

        const data = response.json();

        window.open(data, '_blank'); // Открываем новую страницу в другой вкладке
        return;
    }
    function openModal(title, bodyText1, bodyText2) {
        modalTitle.text(title);
        modalBodyText1.text(bodyText1);
        modalBodyText2.text(bodyText2);
        inputField.val('');
        confirmPassword.val('');
        modal.modal('show');
    }
});