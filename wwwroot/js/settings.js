

// ===== LOAD SETTINGS ON PAGE LOAD =====
(async function loadSettings() {
    try {
        const res = await fetch(`/api/settings/me?userType=${userType}&tenant=${tenant}`);
        if (!res.ok) throw new Error("Failed to load settings");
        const data = await res.json();

        document.getElementById("profileName").value = data.name;
        document.getElementById("profileEmail").value = data.email;
        document.getElementById("profileAddress").value = data.address;
        document.getElementById("profilePhone").value = data.phone;
        document.getElementById("ProfileWebsite").value = data.website;
        document.getElementById("prefEmailNotifications").checked = data.emailNotifications;
        document.getElementById("prefDarkMode").checked = data.darkMode;

    } catch (err) {
        console.error(err);
    }
})();

// ===== PROFILE UPDATE =====
document.getElementById("profileForm")?.addEventListener("submit", async e => {
    e.preventDefault();

    const name = document.getElementById("profileName").value;
    const email = document.getElementById("profileEmail").value;
    const address = document.getElementById("profileAddress").value;
    const phone = document.getElementById("profilePhone").value;
    const website = document.getElementById("ProfileWebsite").value;
    try {
        const res = await fetch("/api/settings/profile", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ userType, tenant, name, email, address, phone, website})
        });

        if (!res.ok) throw new Error("Profile update failed");
        alert("Profile updated successfully");

    } catch (err) {
        console.error(err);
        alert(err.message);
    }
});

// ===== PASSWORD CHANGE =====
document.getElementById("passwordForm")?.addEventListener("submit", async e => {
    e.preventDefault();

    const currentPassword = document.getElementById("currentPassword").value;
    const newPassword = document.getElementById("newPassword").value;
    const confirmPassword = document.getElementById("confirmPassword").value;

    if (newPassword !== confirmPassword) {
        alert("Passwords do not match");
        return;
    }

    try {
        const res = await fetch("/api/settings/password", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ userType, tenant, currentPassword, newPassword })
        });

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err);
        }

        alert("Password updated successfully");
        document.getElementById("passwordForm").reset();

    } catch (err) {
        console.error(err);
        alert(err.message);
    }
});

// ===== PREFERENCES UPDATE =====
document.getElementById("preferencesForm")?.addEventListener("submit", async e => {
    e.preventDefault();

    const emailNotifications = document.getElementById("prefEmailNotifications").checked;
    const darkMode = document.getElementById("prefDarkMode").checked;

    try {
        const res = await fetch("/api/settings/preferences", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ userType, tenant, emailNotifications, darkMode })
        });

        if (!res.ok) throw new Error("Preferences update failed");

        // Optionally save locally
        localStorage.setItem("prefEmailNotifications", emailNotifications);
        localStorage.setItem("prefDarkMode", darkMode);

        alert("Preferences saved successfully");

    } catch (err) {
        console.error(err);
        alert(err.message);
    }
});
