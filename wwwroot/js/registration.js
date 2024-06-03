$(document).ready(function () {
    var tokenKey = "accessToken";

    const form = document.querySelector('form');

    // Проверка ввода
    form.addEventListener('submit', async event => {
        event.preventDefault();

        const usernameField = document.getElementById('username');
        const loginField = document.getElementById('login');
        const passwordField = document.getElementById('password');
        const emailField = document.getElementById('email');

        const userData = {
            Username: usernameField.value,
            Login: loginField.value,
            Password: passwordField.value,
            Email: emailField.value
        };

        // Отправить запрос
        const response = await fetch("/Identification/Registration", {
            method: "POST",
            headers: { "Accept": "application/json", "Content-Type": "application/json" },
            body: JSON.stringify(userData)
        });

        if (response.ok) {
            // Обработка успешного ответа
            const responseData = await response.json();

            sessionStorage.setItem(tokenKey, responseData.access_token)

            const token = sessionStorage.getItem(tokenKey);

            if (!token) {
                throw new Error('Токен доступа не найден');
            }

            // Перенаправление на "/Field/FieldList"
            window.location.href = "/Field/FieldList";
        } else {
            // Обработка ошибки
            console.log("Status: ", response.status);
        }
    });
});
