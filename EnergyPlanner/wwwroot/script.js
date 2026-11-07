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
const loginForm = document.getElementById("login-form");
const authButtons = document.getElementById("auth-buttons");
const accountInfo = document.getElementById("account-info");
const accountEmail = document.getElementById("account-email");
const loginMessage = document.getElementById("login-message");
const logoutButton = document.getElementById("logout-button");

const authOnlyItems = document.querySelectorAll(".auth-only");

function setAuthNavigation(isAuthenticated) {
    authOnlyItems.forEach(item => {
        item.hidden = !isAuthenticated;
    });
}

function showAccount(email) {
    if (accountEmail) {
        accountEmail.textContent = email;
    }

    if (authButtons) {
        authButtons.hidden = true;
    }

    if (accountInfo) {
        accountInfo.hidden = false;
    }

    setAuthNavigation(true);
}

function hideAccount() {
    if (accountInfo) {
        accountInfo.hidden = true;
    }

    if (authButtons) {
        authButtons.hidden = false;
    }

    setAuthNavigation(false);
}

async function checkSession() {
    try {
        const response = await fetch("/api/auth/me");

        if (response.ok) {
            const result = await response.json();
            showAccount(result.email);
        } else {
            hideAccount();
        }
    } catch (error) {
        console.error("Не удалось проверить сессию:", error);
        hideAccount();
    }
}

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

            if (message) {
                message.textContent = await response.text();
            }
        } catch (error) {
            if (message) {
                message.textContent = "Не удалось подключиться к серверу.";
            }
        }
    });
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

                if (loginMessage) {
                    loginMessage.textContent = "Вхід успішний.";
                }
            } else if (loginMessage) {
                loginMessage.textContent = "Неправильна пошта або пароль.";
            }
        } catch (error) {
            if (loginMessage) {
                loginMessage.textContent = "Не удалось подключиться к серверу.";
            }
        }
    });
}

if (logoutButton) {
    logoutButton.addEventListener("click", async () => {
        try {
            const response = await fetch("/api/auth/logout", {
                method: "POST"
            });

            if (response.ok) {
                hideAccount();
            }
        } catch (error) {
            console.error("Ошибка при выходе:", error);
        }
    });
}

const batteryList = document.getElementById("battery-list");
const deviceList = document.getElementById("device-list");
const addBatteryButton = document.getElementById("add-battery");
const addDeviceButton = document.getElementById("add-device");

let batteries = JSON.parse(localStorage.getItem("planner-batteries") || "[]");
let devices = JSON.parse(localStorage.getItem("planner-devices") || "[]");

function saveCards() {
    localStorage.setItem("planner-batteries", JSON.stringify(batteries));
    localStorage.setItem("planner-devices", JSON.stringify(devices));
}

function createCard(type, item) {
    const card = document.createElement("article");
    card.className = "device-card";

    const title = document.createElement("h3");
    title.textContent = type === "battery" ? "Акумулятор" : "Пристрій";

    const nameLabel = document.createElement("label");
    nameLabel.textContent = "Назва";

    const nameInput = document.createElement("input");
    nameInput.type = "text";
    nameInput.placeholder = type === "battery"
        ? "Наприклад, LiFePO4"
        : "Наприклад, холодильник";
    nameInput.value = item.name || "";

    const valueLabel = document.createElement("label");
    valueLabel.textContent = type === "battery"
        ? "Ємність (Вт·год)"
        : "Потужність (Вт)";

    const valueInput = document.createElement("input");
    valueInput.type = "number";
    valueInput.min = "0";
    valueInput.placeholder = type === "battery" ? "Наприклад, 1200" : "Наприклад, 150";
    valueInput.value = item.value || "";

    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.textContent = "Видалити";

    nameInput.addEventListener("input", () => {
        item.name = nameInput.value;
        saveCards();
    });

    valueInput.addEventListener("input", () => {
        item.value = valueInput.value;
        saveCards();
    });

    deleteButton.addEventListener("click", () => {
        const items = type === "battery" ? batteries : devices;
        const index = items.findIndex(entry => entry.id === item.id);

        if (index !== -1) {
            items.splice(index, 1);
        }

        saveCards();
        renderCards();
    });

    nameLabel.append(nameInput);
    valueLabel.append(valueInput);
    card.append(title, nameLabel, valueLabel, deleteButton);

    return card;
}

function renderCards() {
    if (batteryList) {
        batteryList.replaceChildren();

        batteries.forEach(item => {
            batteryList.append(createCard("battery", item));
        });
    }

    if (deviceList) {
        deviceList.replaceChildren();

        devices.forEach(item => {
            deviceList.append(createCard("device", item));
        });
    }
}

if (addBatteryButton) {
    addBatteryButton.addEventListener("click", () => {
        batteries.push({
            id: crypto.randomUUID(),
            name: "",
            value: ""
        });

        saveCards();
        renderCards();
    });
}

if (addDeviceButton) {
    addDeviceButton.addEventListener("click", () => {
        devices.push({
            id: crypto.randomUUID(),
            name: "",
            value: ""
        });

        saveCards();
        renderCards();
    });
}

renderCards();
setAuthNavigation(false);
checkSession();