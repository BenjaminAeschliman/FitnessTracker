const API_BASE = "";

// ---------- helpers ----------
function setMessage(text) {
    document.getElementById("message").textContent = text || "";
}

function saveToken(token) {
    localStorage.setItem("fitness_token", token);
}

function getToken() {
    return localStorage.getItem("fitness_token");
}

function clearToken() {
    localStorage.removeItem("fitness_token");
    localStorage.removeItem("fitness_email");
}

function saveEmail(email) {
    localStorage.setItem("fitness_email", email);
}

function getEmail() {
    return localStorage.getItem("fitness_email");
}

function setLoggedInUI(isLoggedIn) {
    document.getElementById("loggedOutView").style.display = isLoggedIn ? "none" : "block";
    document.getElementById("loggedInView").style.display = isLoggedIn ? "block" : "none";
    document.getElementById("currentUserEmail").textContent = getEmail() || "unknown";
}

async function sendRequest(url, method, body, needsAuth) {
    const headers = { "Content-Type": "application/json" };

    if (needsAuth) {
        const token = getToken();
        if (!token) throw new Error("Not logged in");
        headers["Authorization"] = "Bearer " + token;
    }

    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);

    const response = await fetch(API_BASE + url, options);

    const text = await response.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!response.ok) {
        if (typeof data === "string" && data.length > 0) throw new Error(data);
        throw new Error((data && data.message) ? data.message : (response.status + " " + response.statusText));
    }

    return data;
}

// ---------- REGISTER ----------
document.getElementById("registerForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = document.getElementById("registerEmail").value;
    const password = document.getElementById("registerPassword").value;

    try {
        await sendRequest("/auth/register", "POST", { email, password }, false);
        setMessage("Registered successfully. Now log in.");
        e.target.reset();
    } catch (err) {
        setMessage("Register failed: " + err.message);
    }
});

// ---------- LOGIN ----------
document.getElementById("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPassword").value;

    try {
        const result = await sendRequest("/auth/login", "POST", { email, password }, false);

        const token = result?.token || result?.Token;
        if (!token) throw new Error("No token returned from server");

        saveToken(token);
        saveEmail(email);

        setLoggedInUI(true);
        setMessage("Logged in.");
        e.target.reset();
    } catch (err) {
        setMessage("Login failed: " + err.message);
    }
});

// ---------- LOGOUT ----------
document.getElementById("logoutBtn").addEventListener("click", () => {
    clearToken();
    setLoggedInUI(false);
    document.getElementById("activitiesOutput").textContent = "";
    document.getElementById("activityResult").textContent = "";
    setMessage("Logged out.");
});

// ---------- LOAD ACTIVITIES ----------
document.getElementById("loadActivitiesBtn").addEventListener("click", async () => {
    const output = document.getElementById("activitiesOutput");
    output.textContent = "Loading...";

    try {
        const data = await sendRequest("/api/fitness/activities", "GET", null, true);
        output.textContent = JSON.stringify(data, null, 2);
        setMessage("");
    } catch (err) {
        output.textContent = "";
        setMessage("Failed to load activities: " + err.message);
    }
});

// ---------- ADD ACTIVITY ----------
document.getElementById("activityForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const type = document.getElementById("activityType").value;
    const durationMinutes = Number(document.getElementById("activityMinutes").value);
    const date = document.getElementById("activityDate").value;

    const resultBox = document.getElementById("activityResult");
    resultBox.textContent = "Saving...";

    try {
        const data = await sendRequest(
            "/api/fitness/activities",
            "POST",
            { type, durationMinutes, date },
            true
        );

        resultBox.textContent = JSON.stringify(data, null, 2);
        setMessage("Activity added.");
        e.target.reset();
    } catch (err) {
        resultBox.textContent = "";
        setMessage("Failed to add activity: " + err.message);
    }
});

// ---------- PAGE LOAD ----------
setLoggedInUI(!!getToken());
setMessage(getToken() ? "Token found. You are logged in." : "Not logged in.");
