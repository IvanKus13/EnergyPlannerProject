const navigationButtons = document.querySelectorAll("[data-page]");
const pages = document.querySelectorAll(".page");

const registerForm = document.getElementById("register-form");
const loginForm = document.getElementById("login-form");

const authButtons = document.getElementById("auth-buttons");
const accountInfo = document.getElementById("account-info");
const accountEmail = document.getElementById("account-email");

const loginMessage = document.getElementById("login-message");
const logoutButton = document.getElementById("logout-button");

const authOnlyItems = document.querySelectorAll(".auth-only");

const batteryList = document.getElementById("battery-list");
const deviceList = document.getElementById("device-list");

const addBatteryButton = document.getElementById("add-battery");
const addDeviceButton = document.getElementById("add-device");

const schemeNameInput = document.getElementById("scheme-name");

const calculateContainer = document.getElementById("calculate-container");
const saveAndCalculateButton = document.getElementById("save-and-calculate");

const saveModal = document.getElementById("save-modal");
const closeModalButton = document.getElementById("close-modal");
const cancelSaveButton = document.getElementById("cancel-save");
const confirmSaveButton = document.getElementById("confirm-save");

const modalSchemeName = document.getElementById("modal-scheme-name");
const modalBatteryCount = document.getElementById("modal-battery-count");
const modalDeviceCount = document.getElementById("modal-device-count");

const savedSchemesList = document.getElementById("saved-schemes-list");
const historySchemesList = document.getElementById("history-schemes-list");

let batteries = JSON.parse(localStorage.getItem("planner-batteries") || "[]");
let devices = JSON.parse(localStorage.getItem("planner-devices") || "[]");

function openPage(pageId) {
    pages.forEach(page => {
        page.hidden = true;
    });

    const selectedPage = document.getElementById(pageId);

    if (selectedPage) {
        selectedPage.hidden = false;
    }

    document.querySelectorAll(".nav-button").forEach(button => {
        button.classList.remove("active");
    });

    document.querySelectorAll(`[data-page="${pageId}"]`).forEach(button => {
        button.classList.add("active");
    });

    if (pageId === "history") {
        loadSchemes();
    }
}

navigationButtons.forEach(button => {
    button.addEventListener("click", () => {
        openPage(button.dataset.page);
    });
});

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

    if (savedSchemesList) {
        savedSchemesList.replaceChildren();
    }

    if (historySchemesList) {
        historySchemesList.replaceChildren();
    }

    openPage("info");
}

async function checkSession() {
    try {
        const response = await fetch("/api/auth/me");

        if (response.ok) {
            const responseText = await response.text();

            console.log("HTTP status:", response.status);
            console.log("Ответ сервера:", responseText);

            if (!response.ok) {
                throw new Error(responseText);
            }

            alert("Схему успішно збережено!");

            showAccount(result.email);
            await loadSchemes();
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

        const email = document.getElementById("register-email").value.trim();
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

            const result = await response.text();

            if (message) {
                message.textContent = result;
            }
        } catch (error) {
            if (message) {
                message.textContent = "Не вдалося підключитися до сервера.";
            }
        }
    });
}

if (loginForm) {
    loginForm.addEventListener("submit", async event => {
        event.preventDefault();

        const email = document.getElementById("loginEmail").value.trim();
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

                openPage("calculation");
                await loadSchemes();
            } else if (loginMessage) {
                loginMessage.textContent = "Неправильна пошта або пароль.";
            }
        } catch (error) {
            if (loginMessage) {
                loginMessage.textContent = "Не вдалося підключитися до сервера.";
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

function saveCards() {
    localStorage.setItem("planner-batteries", JSON.stringify(batteries));
    localStorage.setItem("planner-devices", JSON.stringify(devices));
}

function updateCalculateButton() {
    if (calculateContainer) {
        calculateContainer.hidden =
            batteries.length === 0 || devices.length === 0;
    }
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
    valueInput.step = "any";
    valueInput.placeholder = type === "battery"
        ? "Наприклад, 1200"
        : "Наприклад, 150";
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

    card.append(
        title,
        nameLabel,
        valueLabel,
        deleteButton
    );

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

    updateCalculateButton();
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

function validateScheme() {
    if (!schemeNameInput || !schemeNameInput.value.trim()) {
        alert("Введіть назву схеми!");

        if (schemeNameInput) {
            schemeNameInput.focus();
        }

        return false;
    }

    if (batteries.length === 0 || devices.length === 0) {
        alert("Додайте хоча б один акумулятор та один пристрій!");
        return false;
    }

    const allBatteriesFilled = batteries.every(item =>
        item.name.trim() !== "" &&
        item.value !== "" &&
        Number.isFinite(Number(item.value)) &&
        Number(item.value) > 0
    );

    const allDevicesFilled = devices.every(item =>
        item.name.trim() !== "" &&
        item.value !== "" &&
        Number.isFinite(Number(item.value)) &&
        Number(item.value) > 0
    );

    if (!allBatteriesFilled || !allDevicesFilled) {
        alert("Заповніть усі поля акумуляторів та пристроїв!");
        return false;
    }

    return true;
}

function openSaveModal() {
    if (!validateScheme()) {
        return;
    }

    modalSchemeName.textContent = schemeNameInput.value.trim();
    modalBatteryCount.textContent = batteries.length;
    modalDeviceCount.textContent = devices.length;

    saveModal.hidden = false;
}

function closeSaveModal() {
    saveModal.hidden = true;
}

if (saveAndCalculateButton) {
    saveAndCalculateButton.addEventListener("click", openSaveModal);
}

if (closeModalButton) {
    closeModalButton.addEventListener("click", closeSaveModal);
}

if (cancelSaveButton) {
    cancelSaveButton.addEventListener("click", closeSaveModal);
}

if (saveModal) {
    saveModal.addEventListener("click", event => {
        if (event.target === saveModal) {
            closeSaveModal();
        }
    });
}

function createSavedSchemeCard(scheme) {
    const card = document.createElement("article");
    card.className = "saved-scheme-card";

    const title = document.createElement("h3");
    title.textContent = scheme.name;

    const date = document.createElement("p");
    date.textContent = `Дата створення: ${new Date(scheme.createdAt).toLocaleString("uk-UA")}`;

    const accumulatorTitle = document.createElement("h4");
    accumulatorTitle.textContent = "Акумулятори";

    const accumulatorList = document.createElement("ul");

    scheme.accumulators.forEach(accumulator => {
        const item = document.createElement("li");

        const capacityWh = accumulator.capacityKilowattHours * 1000;

        item.textContent =
            `${accumulator.name} — ${capacityWh.toLocaleString("uk-UA")} Вт·год`;

        accumulatorList.append(item);
    });

    const deviceTitle = document.createElement("h4");
    deviceTitle.textContent = "Пристрої";

    const deviceListElement = document.createElement("ul");

    scheme.devices.forEach(device => {
        const item = document.createElement("li");

        item.textContent =
            `${device.name} — ${device.powerWatts.toLocaleString("uk-UA")} Вт`;

        deviceListElement.append(item);
    });

    card.append(
        title,
        date,
        accumulatorTitle,
        accumulatorList,
        deviceTitle,
        deviceListElement
    );

    return card;
}

function renderSavedSchemes(schemes) {
    if (savedSchemesList) {
        savedSchemesList.replaceChildren();

        if (schemes.length === 0) {
            const message = document.createElement("p");
            message.textContent = "У вас поки немає збережених схем.";

            savedSchemesList.append(message);
        } else {
            schemes.forEach(scheme => {
                savedSchemesList.append(createSavedSchemeCard(scheme));
            });
        }
    }

    if (historySchemesList) {
        historySchemesList.replaceChildren();

        if (schemes.length === 0) {
            const message = document.createElement("p");
            message.textContent = "У вас поки немає збережених схем.";

            historySchemesList.append(message);
        } else {
            schemes.forEach(scheme => {
                historySchemesList.append(createSavedSchemeCard(scheme));
            });
        }
    }
}

async function loadSchemes() {
    try {
        const response = await fetch("/api/schemes");

        if (!response.ok) {
            if (response.status === 401) {
                hideAccount();
                return;
            }

            throw new Error("Не вдалося завантажити схеми.");
        }

        const schemes = await response.json();

        renderSavedSchemes(schemes);
    } catch (error) {
        console.error("Помилка завантаження схем:", error);

        if (savedSchemesList) {
            savedSchemesList.textContent =
                "Не вдалося завантажити збережені схеми.";
        }

        if (historySchemesList) {
            historySchemesList.textContent =
                "Не вдалося завантажити збережені схеми.";
        }
    }
}



if (saveAndCalculateButton) {
    saveAndCalculateButton.addEventListener("click", async () => {
        if (!validateScheme()) {
            return;
        }

        const scheme = {
            name: schemeNameInput.value.trim(),

            accumulators: batteries.map(item => ({
                name: item.name.trim(),
                capacityKilowattHours: Number(item.value) / 1000
            })),

            devices: devices.map(item => ({
                name: item.name.trim(),
                powerWatts: Number(item.value)
            }))
        };

        saveAndCalculateButton.disabled = true;

        try {
            const response = await fetch("/api/schemes", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(scheme)
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.message || "Не вдалося зберегти схему."
                );
            }

            batteries = [];
            devices = [];
            schemeNameInput.value = "";

            saveCards();
            renderCards();

            await loadSchemes();

            alert("Схему успішно збережено!");
        } catch (error) {
            console.error("Помилка збереження:", error);
            alert(error.message);
        } finally {
            saveAndCalculateButton.disabled = false;
        }
    });
}

