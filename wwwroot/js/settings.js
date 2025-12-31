// ===== Settings.js =====

// Profile update
document.getElementById("profileForm")?.addEventListener("submit", async e => {
    e.preventDefault();
    const name = document.getElementById("profileName").value;
    const email = document.getElementById("profileEmail").value;

    try {
        const res = await fetch(`/api/settings/profile?tenant=${tenant}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name, email })
        });
        if (!res.ok) throw new Error("Profile update failed");
        alert("Profile updated successfully");
    } catch (err) {
        console.error(err);
        alert(err.message);
    }
});

// Password change
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
        const res = await fetch(`/api/settings/password?tenant=${tenant}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ currentPassword, newPassword })
        });
        if (!res.ok) throw new Error("Password update failed");
        alert("Password updated successfully");
        document.getElementById("passwordForm").reset();
    } catch (err) {
        console.error(err);
        alert(err.message);
    }
});

// Preferences
document.getElementById("preferencesForm")?.addEventListener("submit", e => {
    e.preventDefault();
    const emailNotifications = document.getElementById("prefEmailNotifications").checked;
    const darkMode = document.getElementById("prefDarkMode").checked;

    localStorage.setItem("prefEmailNotifications", emailNotifications);
    localStorage.setItem("prefDarkMode", darkMode);
    alert("Preferences saved!");
});
