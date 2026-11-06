console.log("script.js загружен");
const navigationButtons = document.querySelectorAll("[data-page]");
const pages = document.querySelectorAll(".page");

navigationButtons.forEach(button => {
    button.addEventListener("click", () => {
        const pageId = button.dataset.page;

        pages.forEach(page => {
            page.hidden = true;
        });

        const selectedPage = document.getElementById(pageId);

        if (selectedPage) {
            selectedPage.hidden = false;
        }

        document.querySelectorAll(".nav-button").forEach(navButton => {
            navButton.classList.remove("active");
        });

        if (button.classList.contains("nav-button")) {
            button.classList.add("active");
        }
    });
});

const registerForm = document.getElementById("register-form");

if (registerForm) {
    registerForm.addEventListener("submit", async event => {
        event.preventDefault();

        const email = document.getElementById("register-email").value;
        const password = document.getElementById("register-password").value;
        const message = document.getElementById("register-message");

        try {
            const response = await fetch("/api/auth/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email, password })
            });

            message.textContent = await response.text();
        } catch (error) {
            message.textContent = "Не удалось подключиться к серверу.";
        }
    });
}

const loginForm = document.getElementById("login-form");
const authButtons = document.getElementById("auth-buttons");
const accountInfo = document.getElementById("account-info");
const accountEmail = document.getElementById("account-email");
const loginMessage = document.getElementById("login-message");

function showAccount(email) {
    accountEmail.textContent = email;
    authButtons.hidden = true;
    accountInfo.hidden = false;
}

if (loginForm) {
    loginForm.addEventListener("submit", async event => {
        event.preventDefault();

        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;

        try {
            const response = await fetch("/api/auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email, password })
            });

            if (response.ok) {
                const result = await response.json();
                showAccount(result.email);
                loginMessage.textContent = "Вхід успішний.";
            } else {
                loginMessage.textContent = "Неправильна пошта або пароль.";
            }
        } catch (error) {
            loginMessage.textContent = "Не удалось подключиться к серверу.";
        }
    });
}

const logoutButton = document.getElementById("logout-button");

if (logoutButton) {
    logoutButton.addEventListener("click", async () => {
        try {
            const response = await fetch("/api/auth/logout", {
                method: "POST"
            });

            if (response.ok) {
                accountInfo.hidden = true;
                authButtons.hidden = false;
            }
        } catch (error) {
            console.error("Ошибка при выходе:", error);
        }
    });
}