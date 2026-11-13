
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

const historySchemesList = document.getElementById("history-schemes-list");

const calculationsSection = document.getElementById("calculations-section");
const calculationResult = document.getElementById("calculation-result");

const targetDaysInput = document.getElementById("target-days");
const recommendationEfficiencyInput = document.getElementById("recommendation-efficiency");
const recommendCapacityButton = document.getElementById("recommend-capacity");
const recommendationResult = document.getElementById("recommendation-result");

let batteries = JSON.parse(localStorage.getItem("planner-batteries") || "[]");
let devices = JSON.parse(localStorage.getItem("planner-devices") || "[]");

batteries = batteries.map(item => ({
    id: item.id || crypto.randomUUID(),
    name: item.name || "",
    capacityKilowattHours: item.capacityKilowattHours ??
        (item.value ? Number(item.value) / 1000 : ""),
    efficiencyPercent: item.efficiencyPercent ?? 100
}));

devices = devices.map(item => ({
    id: item.id || crypto.randomUUID(),
    name: item.name || "",
    powerWatts: item.powerWatts ?? item.value ?? "",
    quantity: item.quantity ?? 1,
    hoursPerDay: item.hoursPerDay ?? ""
}));

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

    if (historySchemesList) {
        historySchemesList.replaceChildren();
    }

    openPage("info");
}

async function checkSession() {
    try {
        const response = await fetch("/api/auth/me");

        if (response.ok) {
            const result = await response.json();

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

function createInputField(labelText, value, type, options, onChange) {
    const label = document.createElement("label");
    label.textContent = labelText;

    const input = document.createElement("input");
    input.type = type;

    if (type === "number") {
        input.min = options.min ?? "0";
        input.step = options.step ?? "any";

        if (options.max !== undefined) {
            input.max = options.max;
        }
    }

    input.placeholder = options.placeholder || "";
    input.value = value ?? "";
    input.required = true;

    input.addEventListener("input", () => {
        onChange(input.value);
        saveCards();
    });

    label.append(input);

    return label;
}

function createCard(type, item) {
    const card = document.createElement("article");
    card.className = "device-card";

    const title = document.createElement("h3");
    title.textContent = type === "battery" ? "Акумулятор" : "Пристрій";

    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.textContent = "Видалити";

    const fields = [];

    if (type === "battery") {
        fields.push(
            createInputField(
                "Назва",
                item.name,
                "text",
                { placeholder: "Наприклад, LiFePO4" },
                value => item.name = value
            ),

            createInputField(
                "Ємність (кВт·год)",
                item.capacityKilowattHours,
                "number",
                { placeholder: "Наприклад, 5", min: "0", step: "any" },
                value => item.capacityKilowattHours = value
            ),

            createInputField(
                "КПД (%)",
                item.efficiencyPercent,
                "number",
                { placeholder: "Наприклад, 95", min: "0", max: "100" },
                value => item.efficiencyPercent = value
            )
        );
    } else {
        fields.push(
            createInputField(
                "Назва",
                item.name,
                "text",
                { placeholder: "Наприклад, холодильник" },
                value => item.name = value
            ),

            createInputField(
                "Потужність (Вт)",
                item.powerWatts,
                "number",
                { placeholder: "Наприклад, 200", min: "0", step: "any" },
                value => item.powerWatts = value
            ),

            createInputField(
                "Кількість",
                item.quantity,
                "number",
                { placeholder: "Наприклад, 1", min: "1", step: "1" },
                value => item.quantity = value
            ),

            createInputField(
                "Час роботи на добу (год)",
                item.hoursPerDay,
                "number",
                {
                    placeholder: "Введіть 1000 для безперервної роботи",
                    min: "0",
                    max: "1000"
                },
                value => item.hoursPerDay = value
            )
        );
    }

    deleteButton.addEventListener("click", () => {
        const items = type === "battery" ? batteries : devices;
        const index = items.findIndex(entry => entry.id === item.id);

        if (index !== -1) {
            items.splice(index, 1);
        }

        saveCards();
        renderCards();
    });

    card.append(title, ...fields, deleteButton);

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
            capacityKilowattHours: "",
            efficiencyPercent: 100
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
            powerWatts: "",
            quantity: 1,
            hoursPerDay: ""
        });

        saveCards();
        renderCards();
    });
}

function validateScheme() {
    if (!schemeNameInput || !schemeNameInput.value.trim()) {
        alert("Введіть назву схеми!");
        schemeNameInput?.focus();
        return false;
    }

    if (batteries.length === 0 || devices.length === 0) {
        alert("Додайте хоча б один акумулятор та один пристрій!");
        return false;
    }

    const allBatteriesFilled = batteries.every(item => {
        const capacity = Number(item.capacityKilowattHours);
        const efficiency = Number(item.efficiencyPercent);

        return item.name.trim() !== "" &&
            item.capacityKilowattHours !== "" &&
            Number.isFinite(capacity) &&
            capacity > 0 &&
            item.efficiencyPercent !== "" &&
            Number.isFinite(efficiency) &&
            efficiency > 0 &&
            efficiency <= 100;
    });

    const allDevicesFilled = devices.every(item => {
        const power = Number(item.powerWatts);
        const quantity = Number(item.quantity);
        const hours = Number(item.hoursPerDay);

        return item.name.trim() !== "" &&
            item.powerWatts !== "" &&
            Number.isFinite(power) &&
            power > 0 &&
            Number.isInteger(quantity) &&
            quantity > 0 &&
            item.hoursPerDay !== "" &&
            Number.isFinite(hours) &&
            hours > 0 &&
            (hours <= 24 || hours === 1000);
    });

    if (!allBatteriesFilled || !allDevicesFilled) {
        alert("Перевірте всі поля акумуляторів та пристроїв!");
        return false;
    }

    return true;
}

function createSavedSchemeCard(scheme) {
    const card = document.createElement("article");
    card.className = "saved-scheme-card";

    const title = document.createElement("h3");
    title.textContent = scheme.name;

    const date = document.createElement("p");
    date.textContent =
        `Дата створення: ${new Date(scheme.createdAt).toLocaleString("uk-UA")}`;

    const accumulatorTitle = document.createElement("h4");
    accumulatorTitle.textContent = "Акумулятори";

    const accumulatorList = document.createElement("ul");

    (scheme.accumulators || []).forEach(accumulator => {
        const item = document.createElement("li");

        item.textContent =
            `${accumulator.name} — ${accumulator.capacityKilowattHours} кВт·год, ` +
            `КПД: ${accumulator.efficiencyPercent ?? "—"}%`;

        accumulatorList.append(item);
    });

    const deviceTitle = document.createElement("h4");
    deviceTitle.textContent = "Пристрої";

    const deviceListElement = document.createElement("ul");

    (scheme.devices || []).forEach(device => {
        const item = document.createElement("li");

        const workingTime = device.hoursPerDay === 1000
            ? "Безперервно"
            : `${device.hoursPerDay} год/добу`;

        item.textContent =
            `${device.name} — ${device.powerWatts} Вт, ` +
            `Кількість: ${device.quantity ?? "—"}, ` +
            `Час роботи: ${workingTime}`;

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
    if (!historySchemesList) {
        return;
    }

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

        if (historySchemesList) {
            historySchemesList.textContent =
                "Не вдалося завантажити збережені схеми.";
        }
    }
}

function displayCalculationResult(result) {
    if (!calculationsSection || !calculationResult) {
        return;
    }

    if (result.continuousMode) {
        const totalMinutes = Math.floor(result.runtimeHours * 60);
        const hours = Math.floor(totalMinutes / 60);
        const minutes = totalMinutes % 60;

        calculationResult.textContent =
            `При безперервній роботі всіх пристроїв акумуляторів вистачить на ${hours} год ${minutes} хв.`;
    } else if (!result.fullDayAvailable) {
        calculationResult.textContent =
            `Акумуляторів не вистачить на повний добовий графік. ` +
            `Вони зможуть забезпечити приблизно ${(result.runtimeDays * 100).toFixed(1)}% одного добового графіка.`;
    } else {
        let message =
            `Схема успішно забезпечить заданий режим протягом першої доби. `;

        if (result.additionalFullDays > 0) {
            message +=
                `Після цього акумуляторів вистачить ще на ${result.additionalFullDays} повних днів. `;
        }

        if (result.nextDayPercent > 0.01) {
            message +=
                `Після цього залишиться енергії ще на ${result.nextDayPercent.toFixed(1)}% наступного добового графіка.`;
        } else {
            message +=
                `Після цього акумулятори будуть розряджені.`;
        }

        calculationResult.textContent = message;
    }

    calculationsSection.hidden = false;
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
                capacityKilowattHours: Number(item.capacityKilowattHours),
                efficiencyPercent: Number(item.efficiencyPercent)
            })),

            devices: devices.map(item => ({
                name: item.name.trim(),
                powerWatts: Number(item.powerWatts),
                quantity: Number(item.quantity),
                hoursPerDay: Number(item.hoursPerDay)
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

            const responseText = await response.text();

            let result = {};

            try {
                result = responseText ? JSON.parse(responseText) : {};
            } catch {
                result = { message: responseText };
            }

            if (!response.ok) {
                throw new Error(
                    result.message || "Не вдалося зберегти схему."
                );
            }

            if (!result.calculationResult) {
                throw new Error(
                    "Схему збережено, але сервер не повернув результат розрахунку."
                );
            }

            displayCalculationResult(result.calculationResult);

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

function isDeviceFilled(item) {
    const power = Number(item.powerWatts);
    const quantity = Number(item.quantity);
    const hours = Number(item.hoursPerDay);

    return item.name.trim() !== "" &&
        item.powerWatts !== "" &&
        Number.isFinite(power) &&
        power > 0 &&
        Number.isInteger(quantity) &&
        quantity > 0 &&
        item.hoursPerDay !== "" &&
        Number.isFinite(hours) &&
        hours > 0 &&
        (hours <= 24 || hours === 1000);
}

function isBatteryFilled(item) {
    const capacity = Number(item.capacityKilowattHours);
    const efficiency = Number(item.efficiencyPercent);

    return item.name.trim() !== "" &&
        item.capacityKilowattHours !== "" &&
        Number.isFinite(capacity) &&
        capacity > 0 &&
        item.efficiencyPercent !== "" &&
        Number.isFinite(efficiency) &&
        efficiency > 0 &&
        efficiency <= 100;
}

function formatKilowattHours(wattHours) {
    return (wattHours / 1000).toFixed(2);
}

function displayRecommendation(result, hasAccumulators) {
    let message =
        `Добове споживання пристроїв: ${formatKilowattHours(result.dailyConsumption)} кВт·год. ` +
        `Щоб пристрої працювали ${result.targetDays} діб, потрібно ${formatKilowattHours(result.requiredEnergy)} кВт·год енергії. ` +
        `Це акумулятори загальною ємністю щонайменше ${result.requiredCapacityKilowattHours.toFixed(2)} кВт·год ` +
        `(при КПД ${result.efficiencyPercent}%).`;

    if (hasAccumulators) {
        if (result.isCurrentCapacityEnough) {
            message +=
                ` Додані акумулятори дають ${formatKilowattHours(result.currentAvailableEnergy)} кВт·год — цього вже достатньо.`;
        } else {
            message +=
                ` Додані акумулятори дають лише ${formatKilowattHours(result.currentAvailableEnergy)} кВт·год. ` +
                `Потрібно додати ще щонайменше ${result.additionalCapacityKilowattHours.toFixed(2)} кВт·год ємності.`;
        }
    }

    recommendationResult.textContent = message;
}

if (recommendCapacityButton) {
    recommendCapacityButton.addEventListener("click", async () => {
        if (devices.length === 0) {
            alert("Додайте хоча б один пристрій!");
            return;
        }

        for (const item of devices) {
            if (!isDeviceFilled(item)) {
                alert("Перевірте всі поля пристроїв!");
                return;
            }
        }

        for (const item of batteries) {
            if (!isBatteryFilled(item)) {
                alert("Перевірте поля акумуляторів або видаліть незаповнені!");
                return;
            }
        }

        const targetDays = Number(targetDaysInput.value);
        const efficiencyPercent = Number(recommendationEfficiencyInput.value);

        if (!Number.isFinite(targetDays) || targetDays <= 0 || targetDays > 365) {
            alert("Бажана автономність має бути більше 0 і не більше 365 діб!");
            targetDaysInput.focus();
            return;
        }

        if (!Number.isFinite(efficiencyPercent) || efficiencyPercent <= 0 || efficiencyPercent > 100) {
            alert("КПД має бути в межах від 0 до 100%!");
            recommendationEfficiencyInput.focus();
            return;
        }

        const request = {
            targetDays: targetDays,
            efficiencyPercent: efficiencyPercent,
            devices: [],
            accumulators: []
        };

        for (const item of devices) {
            request.devices.push({
                name: item.name.trim(),
                powerWatts: Number(item.powerWatts),
                quantity: Number(item.quantity),
                hoursPerDay: Number(item.hoursPerDay)
            });
        }

        for (const item of batteries) {
            request.accumulators.push({
                name: item.name.trim(),
                capacityKilowattHours: Number(item.capacityKilowattHours),
                efficiencyPercent: Number(item.efficiencyPercent)
            });
        }

        recommendCapacityButton.disabled = true;

        try {
            const response = await fetch("/api/calculations/capacity-recommendation", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            });

            const responseText = await response.text();

            let result = {};

            try {
                result = responseText ? JSON.parse(responseText) : {};
            } catch {
                result = { message: responseText };
            }

            if (!response.ok) {
                throw new Error(
                    result.message || "Не вдалося розрахувати потрібну ємність."
                );
            }

            displayRecommendation(result, request.accumulators.length > 0);
        } catch (error) {
            console.error("Помилка розрахунку ємності:", error);
            alert(error.message);
        } finally {
            recommendCapacityButton.disabled = false;
        }
    });
}

renderCards();
checkSession();
