// ===== Orders.js =====
const ordersTableBody = document.getElementById("ordersTableBody");
const ordersPagination = document.getElementById("ordersPagination");
let currentPage = 1;
const pageSize = 10;

loadOrders(currentPage);

async function loadOrders(page = 1) {
    try {
        const res = await fetch(`/api/orders?tenant=${tenant}&userType=${userType}&page=${page}&pageSize=${pageSize}`);
        if (!res.ok) throw new Error("Failed to fetch orders");
        const data = await res.json();

        ordersTableBody.innerHTML = "";
        data.items.forEach((order, i) => {
            ordersTableBody.innerHTML += `
                <tr>
                    <td>${(page - 1) * pageSize + i + 1}</td>
                    <td>${order.trans_id}</td>
                    <td>${order.customerName}</td>
                    <td>${new Date(order.transDate).toLocaleString()}</td>
                    <td><span class="badge ${order.transStatus === 1 ? 'bg-success' : 'bg-warning text-dark'}">
                        ${order.transStatus === 1 ? 'Confirmed' : 'Pending'}</span></td>
                    <td>
                        <button class="btn btn-sm btn-primary" onclick="viewOrderDetails(${order.trans_id})">Details</button>
                    </td>
                </tr>
            `;
        });

        renderPagination(data.totalPages, page);
    } catch (err) { console.error(err); }
}

function renderPagination(totalPages, current) {
    ordersPagination.innerHTML = "";
    for (let i = 1; i <= totalPages; i++) {
        ordersPagination.innerHTML += `
            <li class="page-item ${i === current ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadOrders(${i})">${i}</a>
            </li>`;
    }
}

// Show order details in modal
async function viewOrderDetails(transId) {
    const modal = new bootstrap.Modal(document.getElementById('orderDetailsModal'));
    const body = document.getElementById('orderDetailsBody');
    body.innerHTML = "Loading...";

    try {
        const res = await fetch(`/api/orders/details/${transId}?tenant=${tenant}&userType=${userType}`);
        if (!res.ok) throw new Error("Failed to fetch order details");
        const order = await res.json();

        body.innerHTML = `
            <p><strong>Order ID:</strong> ${order.trans_id}</p>
            <p><strong>Customer:</strong> ${order.customerName}</p>
            <p><strong>Date:</strong> ${new Date(order.transDate).toLocaleString()}</p>
            <p><strong>Status:</strong> ${order.transStatus === 1 ? 'Confirmed' : 'Pending'}</p>
            <p><strong>Category:</strong> ${order.trans_categories}</p>
            <p><strong>Description:</strong> ${order.trans_description}</p>
            <p><strong>URL:</strong> <a href="${order.trans_url}" target="_blank">${order.trans_url}</a></p>
        `;

        modal.show();
    } catch (err) {
        body.innerHTML = "Failed to load order details.";
        console.error(err);
    }
}
