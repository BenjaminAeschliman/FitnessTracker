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
        throw new Error(data?.error || data?.message || (typeof data === "string" ? data : "") || `${response.status} ${response.statusText}`);
    }

    return data;
}

// ---------- Activities ----------
async function loadActivities() {
    const loading = document.getElementById("activitiesLoading");
    const tbody = document.getElementById("activitiesBody");
    const table = document.getElementById("activitiesTable");

    loading.style.display = "block";
    tbody.innerHTML = "";
    table.style.display = "none";

    try {
        const activities = await sendRequest("/api/fitness/activities", "GET", null, true);

        for (const a of activities) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${(a.date || "").toString().substring(0, 10)}</td>
                <td>${a.type ?? ""}</td>
                <td>${a.durationMinutes ?? ""}</td>
            `;
            tbody.appendChild(tr);
        }

        table.style.display = activities.length ? "table" : "none";
        setMessage("");
    } catch (err) {
        setMessage("Failed to load activities: " + err.message);
    } finally {
        loading.style.display = "none";
    }
}

// ---------- Stats ----------
async function loadStats() {
    const statsLoading = document.getElementById("statsLoading");
    const statsError = document.getElementById("statsError");
    const statsSummary = document.getElementById("statsSummary");
    const breakdown = document.getElementById("typeBreakdown");

    statsError.textContent = "";
    statsLoading.style.display = "block";
    statsSummary.style.display = "none";
    breakdown.innerHTML = "";

    const start = document.getElementById("startDate").value;
    const end = document.getElementById("endDate").value;

    const params = new URLSearchParams();
    if (start) params.append("startDate", start);
    if (end) params.append("endDate", end);

    try {
        const stats = await sendRequest(
            "/api/fitness/stats" + (params.toString() ? "?" + params.toString() : ""),
            "GET",
            null,
            true
        );

        document.getElementById("statTotal").textContent = stats.totalMinutes ?? 0;
        document.getElementById("statCount").textContent = stats.activityCount ?? 0;
        document.getElementById("statAvg").textContent = stats.averageDurationMinutes ?? 0;

        statsSummary.style.display = "grid";

        const dict = stats.minutesByType || {};
        const keys = Object.keys(dict);

        if (keys.length === 0) {
            breakdown.textContent = "No activity types for this range.";
        } else {
            for (const type of keys) {
                const div = document.createElement("div");
                div.textContent = `${type}: ${dict[type]} min`;
                breakdown.appendChild(div);
            }
        }
    } catch (err) {
        statsError.textContent = err.message;
    } finally {
        statsLoading.style.display = "none";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    setMessage("✅ app.js loaded and DOMContentLoaded fired");

    // REGISTER
    document.getElementById("registerForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const email = document.getElementById("registerEmail").value;
        const password = document.getElementById("registerPassword").value;

        try {
            await sendRequest("/auth/register", "POST", { email, password }, false);
            setMessage("✅ Registered successfully. Now log in.");
            e.target.reset();
        } catch (err) {
            setMessage("❌ Register failed: " + err.message);
        }
    });

    // LOGIN
    document.getElementById("loginForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const email = document.getElementById("loginEmail").value;
        const password = document.getElementById("loginPassword").value;

        try {
            const result = await sendRequest("/auth/login", "POST", { email, password }, false);
            const token = result?.token || result?.Token;
            if (!token) throw new Error("No token returned from server.");

            saveToken(token);
            saveEmail(email);
            setLoggedInUI(true);
            setMessage("✅ Logged in.");
            e.target.reset();

            // auto-load
            loadActivities();
            loadStats();
        } catch (err) {
            setMessage("❌ Login failed: " + err.message);
        }
    });

    // LOGOUT
    document.getElementById("logoutBtn").addEventListener("click", () => {
        clearToken();
        setLoggedInUI(false);
        setMessage("Logged out.");
    });

    // BUTTONS
    document.getElementById("loadActivitiesBtn").addEventListener("click", loadActivities);
    document.getElementById("loadStatsBtn").addEventListener("click", loadStats);

    // ADD ACTIVITY (prevents refresh)
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
            setMessage("✅ Activity added.");
            e.target.reset();

            loadActivities();
            loadStats();
        } catch (err) {
            resultBox.textContent = "";
            setMessage("❌ Failed to add activity: " + err.message);
        }
    });

    // PAGE LOAD
    setLoggedInUI(!!getToken());
    if (getToken()) {
        loadActivities();
        loadStats();
    }
});
