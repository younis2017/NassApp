// ===== Dashboard.js =====

const tenant = localStorage.getItem("tenant");
const userType = localStorage.getItem("userType");
const roleHeader = document.getElementById("roleHeader");
const welcomeText = document.querySelector(".welcome-text");
const sidebarLinks = document.getElementById("sidebarLinks");
const sidebar = document.getElementById("sidebar");
const toggleBtn = document.getElementById("menuToggle");
const notificationWrapper = document.getElementById("notificationWrapper");
const notificationIcon = document.getElementById("notificationIcon");
const notificationCount = document.getElementById("notificationCount");
const notificationList = document.getElementById("notificationList");

// ===== Sidebar & Header =====
if (tenant && userType) {
    roleHeader.textContent = `${userType}: ${tenant}`;
    welcomeText.textContent = `Welcome, ${tenant} (${userType})`;
}

const linksCustomer = [
    { icon: "bi-speedometer2", text: "Dashboard" },
    { icon: "bi-card-checklist", text: "Orders" },
    { icon: "bi-gear", text: "Settings" },
    { icon: "bi-box-arrow-right", text: "Logout", logout: true }
];

const linksAgency = [
    { icon: "bi-speedometer2", text: "Dashboard" },
    { icon: "bi-card-checklist", text: "Orders" },
    { icon: "bi-gear", text: "Settings" },
    { icon: "bi-box-arrow-right", text: "Logout", logout: true }
];

const links = userType === "Agency" ? linksAgency : linksCustomer;

sidebarLinks.innerHTML = links.map(l => {
    if (l.logout) return `<a href="#" onclick="logout()"><i class="bi ${l.icon} me-2"></i>${l.text}</a>`;
    let sectionId = "";
    switch (l.text.toLowerCase()) {
        case "dashboard": sectionId = "dashboardSection"; break;
        case "orders": sectionId = "ordersSection"; break;
        case "settings": sectionId = "settingsSection"; break;
    }
    return `<a href="#" data-section="${sectionId}"><i class="bi ${l.icon} me-2"></i>${l.text}</a>`;
}).join("");

sidebarLinks.addEventListener("click", e => {
    const link = e.target.closest("[data-section]");
    if (!link) return;
    e.preventDefault();
    document.querySelectorAll(".section").forEach(s => s.classList.remove("active"));
    document.getElementById(link.dataset.section)?.classList.add("active");
    if (window.innerWidth <= 768) sidebar.classList.remove("show");
});

// ===== Logout =====
function logout() {
    localStorage.removeItem("tenant");
    localStorage.removeItem("userType");
    localStorage.removeItem("token");
    window.location.href = "/home/Login";
}
if (!tenant) window.location.href = "/home/Login";

// ===== Mobile toggle =====
toggleBtn?.addEventListener("click", () => sidebar.classList.toggle("show"));
document.addEventListener("click", e => {
    if (window.innerWidth <= 768 && sidebar.classList.contains("show") &&
        !sidebar.contains(e.target) && !toggleBtn.contains(e.target)) sidebar.classList.remove("show");
});

// ===== SignalR Connection =====
const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/notificationHub?tenant=${tenant}`)
    .withAutomaticReconnect()
    .build();

connection.start().then(() => console.log("SignalR connected")).catch(err => console.error(err));

// ===== Live Notifications =====
if (userType === "Agency") initNotifications();
else notificationWrapper.style.display = "none";

function initNotifications() {
    const audio = new Audio("/Sounds/notification.mp3");
    audio.volume = 0.6;

    notificationIcon.addEventListener("click", () => {
        notificationList.style.display = notificationList.style.display === "block" ? "none" : "block";
    });

    async function renderNotifications(playSound = false) {
        try {
            const res = await fetch(`/api/notifications/all`);
            if (!res.ok) return;
            const transactions = await res.json();
            notificationList.innerHTML = "";
            notificationCount.textContent = transactions.length;

            if (!transactions.length) {
                notificationList.innerHTML = `<li class="dropdown-item text-center text-muted">No notifications</li>`;
                return;
            }

            transactions.forEach(t => {
                const li = document.createElement("li");
                li.className = "dropdown-item p-2 border-bottom";
                li.innerHTML = `
                    <strong>${t.trans_categories}</strong>
                    <small class="text-muted d-block">${new Date(t.trans_date).toLocaleString()}</small>
                    <div>${t.trans_description}</div>
                    <div class="mt-2">
                        <button class="btn btn-sm btn-success me-1" data-id="${t.trans_id}" data-action="confirm">✔ Confirm</button>
                        <button class="btn btn-sm btn-danger" data-id="${t.trans_id}" data-action="reject">✖ Reject</button>
                    </div>
                `;
                notificationList.appendChild(li);
            });

            if (playSound) audio.play().catch(() => { });
        } catch (err) { console.error("Notification error:", err); }
    }

    renderNotifications();

    connection.on("ReceiveNotification", () => {
        renderNotifications(true);
        loadAgencyKpis(); // Live KPI update
        loadOrders(currentPage); // Live Orders update
    });

    // Confirm / Reject
    notificationList.addEventListener("click", async e => {
        if (e.target.tagName !== "BUTTON") return;
        const id = e.target.dataset.id;
        const action = e.target.dataset.action;
        const url = action === "confirm"
            ? `/api/notifications/confirm/${id}?tenant=${tenant}`
            : `/api/notifications/reject/${id}?tenant=${tenant}`;

        try {
            const res = await fetch(url, { method: "PUT" });
            if (!res.ok) {
                const errData = await res.json();
                alert(errData.message || "Action failed");
                return;
            }
            e.target.closest("li").remove();
            notificationCount.textContent = notificationList.children.length;
            loadAgencyKpis();
            loadOrders(currentPage);
        } catch (err) { console.error(err); alert("Server error"); }
    });

    setInterval(() => renderNotifications(false), 15000);
}

// ===== KPIs =====
if (userType === "Agency") loadAgencyKpis();
else if (userType === "Customer") loadCustomerKpis();

async function loadAgencyKpis() {
    try {
        const res = await fetch(`/api/AgenciesApi/agency?tenant=${tenant}`);
        if (!res.ok) return;
        const data = await res.json();
        document.getElementById("kpiNewToday").textContent = data.newOrdersToday;
        document.getElementById("kpiPending").textContent = data.pendingOrders;
        document.getElementById("kpiConfirmed").textContent = data.confirmedOrders;
        document.getElementById("kpiTotal").textContent = data.totalOrders;
    } catch (err) { console.error("Failed to load KPIs", err); }
}

async function loadCustomerKpis() {
    try {
        const res = await fetch(`/api/CustomersApi/customer?tenant=${tenant}`);
        if (!res.ok) return;
        const data = await res.json();
        document.getElementById("kpiNewToday").textContent = data.newOrdersToday;
        document.getElementById("kpiPending").textContent = data.pendingOrders;
        document.getElementById("kpiConfirmed").textContent = data.confirmedOrders;
        document.getElementById("kpiTotal").textContent = data.totalOrders;
    } catch (err) { console.error("Failed to load Customer KPIs", err); }
}
