$(document).ready(function () {
    var tokenKey = "accessToken";

    const form = document.querySelector('form');

    // Проверка ввода
    form.addEventListener('submit', async event => {
        event.preventDefault();

        const loginField = document.getElementById('login');
        const passwordField = document.getElementById('password');

        const userData = {
            Login: loginField.value,
            Password: passwordField.value
        };

        // Отправить запрос на авторизацию
        const authResponse = await fetch("/Identification/Authorization", {
            method: "POST",
            headers: { "Accept": "application/json", "Content-Type": "application/json" },
            body: JSON.stringify(userData)
        });

        if (authResponse.ok) {
            // Обработка успешного ответа
            const responseData = await authResponse.json();

            sessionStorage.setItem(tokenKey, responseData.access_token);

            const token = sessionStorage.getItem(tokenKey);

            if (!token) {
                throw new Error('Токен доступа не найден');
            }

            // Перенаправление на "/Field/FieldList"
            window.location.href = "/Field/FieldList";
        } else {
            // Обработка ошибки
            console.log("Status: ", authResponse.status);
        }
    });
});
