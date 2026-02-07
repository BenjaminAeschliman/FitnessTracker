const API_BASE = "";

// ---------- UI helpers ----------
function setMessage(text, isError = false) {
    const el = document.getElementById("message");
    if (!el) return;
    el.textContent = text || "";
    el.classList.toggle("error", !!isError);
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
    const loggedOut = document.getElementById("loggedOutView");
    const loggedIn = document.getElementById("loggedInView");
    const currentEmail = document.getElementById("currentUserEmail");

    if (loggedOut) loggedOut.style.display = isLoggedIn ? "none" : "block";
    if (loggedIn) loggedIn.style.display = isLoggedIn ? "block" : "none";
    if (currentEmail) currentEmail.textContent = getEmail() || "unknown";
}

// Send YYYY-MM-DD as ISO to avoid locale parsing differences across browsers
function toIsoDateStart(yyyyMmDd) {
    return yyyyMmDd ? new Date(yyyyMmDd + "T00:00:00").toISOString() : "";
}

function toIsoDateEnd(yyyyMmDd) {
    return yyyyMmDd ? new Date(yyyyMmDd + "T23:59:59.999").toISOString() : "";
}

async function sendRequest(url, method, body, needsAuth) {
    const headers = { "Content-Type": "application/json" };

    if (needsAuth) {
        const token = getToken();
        if (!token) throw new Error("Not logged in");
        headers["Authorization"] = "Bearer " + token;
    }

    const options = { method, headers };
    if (body !== null && body !== undefined) options.body = JSON.stringify(body);

    const response = await fetch(API_BASE + url, options);

    const text = await response.text();
    let data = null;

    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!response.ok) {
        throw new Error(
            data?.error ||
            data?.message ||
            (typeof data === "string" ? data : "") ||
            `${response.status} ${response.statusText}`
        );
    }

    return data;
}

// ---------- Activity Types ----------
async function loadActivityTypes() {
    const sel = document.getElementById("activityFilterType");
    if (!sel) return;

    const current = sel.value || "";

    try {
        const types = await sendRequest("/api/fitness/activity-types", "GET", null, true);

        sel.innerHTML = `<option value="">All</option>`;
        for (const t of (types || [])) {
            const opt = document.createElement("option");
            opt.value = t;
            opt.textContent = t;
            sel.appendChild(opt);
        }

        sel.value = current; // restore selection if still present
    } catch (err) {
        console.warn("Failed to load activity types:", err.message);
    }
}

// ---------- Activities ----------
async function loadActivities() {
    const loading = document.getElementById("activitiesLoading");
    const tbody = document.getElementById("activitiesBody");
    const table = document.getElementById("activitiesTable");

    if (!loading || !tbody || !table) return;

    loading.style.display = "block";
    tbody.innerHTML = "";
    table.style.display = "none";

    let empty = document.getElementById("activitiesEmpty");
    if (!empty) {
        empty = document.createElement("div");
        empty.id = "activitiesEmpty";
        empty.className = "muted";
        empty.style.marginTop = "8px";
        table.insertAdjacentElement("afterend", empty);
    }
    empty.style.display = "none";
    empty.textContent = "";

    const start = document.getElementById("startDate")?.value || "";
    const end = document.getElementById("endDate")?.value || "";
    const type = document.getElementById("activityFilterType")?.value || "";

    const params = new URLSearchParams();
    if (type) params.append("type", type);
    if (start) params.append("from", toIsoDateStart(start));
    if (end) params.append("to", toIsoDateEnd(end));

    const url = "/api/fitness/activities" + (params.toString() ? "?" + params.toString() : "");

    try {
        const activities = await sendRequest(url, "GET", null, true);

        if (!activities || activities.length === 0) {
            empty.textContent = "No activities found.";
            empty.style.display = "block";
            setMessage("No activities to show.");
            return;
        }

        for (const a of activities) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${(a.date || "").toString().substring(0, 10)}</td>
                <td>${a.type ?? ""}</td>
                <td>${a.durationMinutes ?? ""}</td>
            `;
            tbody.appendChild(tr);
        }

        table.style.display = "table";
        setMessage(`Loaded ${activities.length} activit${activities.length === 1 ? "y" : "ies"}.`);
    } catch (err) {
        setMessage("Failed to load activities: " + err.message, true);

        if (err.message === "Not logged in") {
            clearToken();
            setLoggedInUI(false);
        }
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

    if (!statsLoading || !statsError || !statsSummary || !breakdown) return;

    statsError.textContent = "";
    statsLoading.style.display = "block";
    statsSummary.style.display = "none";
    breakdown.innerHTML = "";

    const start = document.getElementById("startDate")?.value || "";
    const end = document.getElementById("endDate")?.value || "";

    const params = new URLSearchParams();
    if (start) params.append("startDate", toIsoDateStart(start));
    if (end) params.append("endDate", toIsoDateEnd(end));

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
            breakdown.innerHTML = `<div class="muted">No activity types for this range.</div>`;
        } else {
            for (const t of keys) {
                const div = document.createElement("div");
                div.textContent = `${t}: ${dict[t]} min`;
                breakdown.appendChild(div);
            }
        }

        setMessage("Stats loaded.");
    } catch (err) {
        statsError.textContent = err.message;

        if (err.message === "Not logged in") {
            clearToken();
            setLoggedInUI(false);
            setMessage("Session expired. Please log in again.", true);
        }
    } finally {
        statsLoading.style.display = "none";
    }
}

// ---------- DOM ----------
document.addEventListener("DOMContentLoaded", () => {
    // Register
    document.getElementById("registerForm")?.addEventListener("submit", async (e) => {
        e.preventDefault();

        const email = document.getElementById("registerEmail").value;
        const password = document.getElementById("registerPassword").value;

        try {
            await sendRequest("/auth/register", "POST", { email, password }, false);
            setMessage("Registered successfully. Now log in.");
            e.target.reset();
        } catch (err) {
            setMessage("Register failed: " + err.message, true);
        }
    });

    // Login
    document.getElementById("loginForm")?.addEventListener("submit", async (e) => {
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
            setMessage("Logged in.");
            e.target.reset();

            await loadActivityTypes();
            await loadActivities();
            await loadStats();
        } catch (err) {
            setMessage("Login failed: " + err.message, true);
        }
    });

    // Logout
    document.getElementById("logoutBtn")?.addEventListener("click", () => {
        clearToken();
        setLoggedInUI(false);
        setMessage("Logged out.");
    });

    // Actions
    document.getElementById("loadActivitiesBtn")?.addEventListener("click", loadActivities);
    document.getElementById("loadStatsBtn")?.addEventListener("click", loadStats);

    document.getElementById("clearFiltersBtn")?.addEventListener("click", async () => {
        const start = document.getElementById("startDate");
        const end = document.getElementById("endDate");
        const typeSel = document.getElementById("activityFilterType");

        if (start) start.value = "";
        if (end) end.value = "";
        if (typeSel) typeSel.value = "";

        await loadActivityTypes();
        await loadActivities();
        await loadStats();
    });

    // Add activity
    document.getElementById("activityForm")?.addEventListener("submit", async (e) => {
        e.preventDefault();

        const type = document.getElementById("activityType").value;
        const durationMinutes = Number(document.getElementById("activityMinutes").value);
        const date = document.getElementById("activityDate").value;

        const resultBox = document.getElementById("activityResult");
        if (resultBox) resultBox.textContent = "Saving...";

        try {
            await sendRequest("/api/fitness/activities", "POST", { type, durationMinutes, date }, true);

            if (resultBox) resultBox.textContent = "";
            setMessage("Activity added.");
            e.target.reset();

            await loadActivityTypes();
            await loadActivities();
            await loadStats();
        } catch (err) {
            if (resultBox) resultBox.textContent = "";
            setMessage("Failed to add activity: " + err.message, true);

            if (err.message === "Not logged in") {
                clearToken();
                setLoggedInUI(false);
            }
        }
    });

    // Initial load
    setLoggedInUI(!!getToken());
    if (getToken()) {
        (async () => {
            await loadActivityTypes();
            await loadActivities();
            await loadStats();
        })();
    }
});
