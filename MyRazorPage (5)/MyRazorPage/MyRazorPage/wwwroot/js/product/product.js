const connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();
connection.on("ReloadProduct", function () {
    location.reload();
})
connection.on("ReloadOrder", function () {
    location.reload();
})
connection.on("ReloadCustomer", function () {
    location.reload();
})
connection.start();
