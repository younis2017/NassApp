<script>
    const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/notificationHub?tenant=${encodeURIComponent(localStorage.getItem('tenat'))}`)
    .build();

    connection.on("ReceiveNotification", function(notification) {
    const notifCount = document.getElementById("notifCount");
    notifCount.innerText = parseInt(notifCount.innerText || "0") + 1;

    const list = document.getElementById("notificationList");
    const li = document.createElement("li");
    li.textContent = `${notification.title}: ${notification.message}`;
    list.prepend(li);
});

connection.start().catch(err => console.error(err.toString()));
</script>
