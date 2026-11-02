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

            const result = await response.text();
            message.textContent = result;
        } catch (error) {
            message.textContent = "Не удалось подключиться к серверу.";
        }
    });
}