
document.addEventListener('DOMContentLoaded', function () {
    loadAllStudentRequests();     
    loadAllEnterpriseRequests();  


    document.getElementById('tabUsers').addEventListener('click', function () {
        document.getElementById('tabUsers').classList.add('active');
        document.getElementById('tabEnterprises').classList.remove('active');
        document.getElementById('panelUsers').classList.remove('hidden');
        document.getElementById('panelEnterprises').classList.add('hidden');
    });

    document.getElementById('tabEnterprises').addEventListener('click', function () {
        document.getElementById('tabEnterprises').classList.add('active');
        document.getElementById('tabUsers').classList.remove('active');
        document.getElementById('panelEnterprises').classList.remove('hidden');
        document.getElementById('panelUsers').classList.add('hidden');
    });


    document.getElementById('searchInput').addEventListener('keyup', function () {
        const searchTerm = this.value.toLowerCase();
        const activeTab = document.querySelector('.tab.active').id;

        if (activeTab === 'tabUsers') {
            filterTable('usersTableBody', searchTerm);
        } else {
            filterTable('enterprisesTableBody', searchTerm);
        }
    });
});


function loadAllStudentRequests() {
    fetch('/AdminPanel/GetAllStudentRequests')
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '';

            if (data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">No student requests found</td></tr>';
                return;
            }

            data.forEach(student => {
                const row = tbody.insertRow();
                const fullName = `${student.Firstname || ''} ${student.Lastname || ''}`.trim() || student.Username;
                row.insertCell(0).innerHTML = fullName;
                row.insertCell(1).innerHTML = student.Email;
                row.insertCell(2).innerHTML = student.Section || 'N/A';
                row.insertCell(3).innerHTML = student.StudentNumber || 'N/A';

           
                let statusClass = '';
                let statusText = student.Status;
                if (student.Status === 'Pending') {
                    statusClass = 'status-pending';
                } else if (student.Status === 'Approved') {
                    statusClass = 'status-approved';
                } else if (student.Status === 'Rejected') {
                    statusClass = 'status-rejected';
                }
                row.insertCell(4).innerHTML = `<span class="${statusClass}">${statusText}</span>`;

             
                if (student.Status === 'pending') {
                    row.insertCell(5).innerHTML = `
                        <div class="action-btns">
                            <button class="approve-btn" onclick="approveRequest(${student.RequestId})">Approve</button>
                            <button class="reject-btn" onclick="rejectRequest(${student.RequestId})">Reject</button>
                        </div>
                    `;
                } else {
                    row.insertCell(5).innerHTML = `<span class="status-${student.Status}">—</span>`;
                }
            });
        })
        .catch(error => {
            console.error('Error loading students:', error);
            document.getElementById('usersTableBody').innerHTML = '<tr><td colspan="6" style="text-align:center;color:red;">Error loading data</td></tr>';
        });
}


function loadAllEnterpriseRequests() {
    fetch('/AdminPanel/GetAllEnterpriseRequests')
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('enterprisesTableBody');
            tbody.innerHTML = '';

            if (data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">No enterprise requests found</td></tr>';
                return;
            }

            data.forEach(enterprise => {
                const row = tbody.insertRow();
                row.insertCell(0).innerHTML = enterprise.StoreName || enterprise.Username;
                row.insertCell(1).innerHTML = enterprise.Email;
                row.insertCell(2).innerHTML = enterprise.StoreDescription || 'N/A';
                row.insertCell(3).innerHTML = enterprise.ContactNumber || 'N/A';

              
                let statusClass = '';
                if (enterprise.Status === 'pending') {
                    statusClass = 'status-pending';
                } else if (enterprise.Status === 'approved') {
                    statusClass = 'status-approved';
                } else if (enterprise.Status === 'rejected') {
                    statusClass = 'status-rejected';
                }
                row.insertCell(4).innerHTML = `<span class="${statusClass}">${enterprise.Status}</span>`;

             
                if (enterprise.Status === 'pending') {
                    row.insertCell(5).innerHTML = `
                        <div class="action-btns">
                            <button class="approve-btn" onclick="approveRequest(${enterprise.RequestId})">Approve</button>
                            <button class="reject-btn" onclick="rejectRequest(${enterprise.RequestId})">Reject</button>
                        </div>
                    `;
                } else {
                    row.insertCell(5).innerHTML = `<span class="status-${enterprise.Status}">—</span>`;
                }
            });
        })
        .catch(error => {
            console.error('Error loading enterprises:', error);
            document.getElementById('enterprisesTableBody').innerHTML = '<tr><td colspan="6" style="text-align:center;color:red;">Error loading data</td></tr>';
        });
}


function approveRequest(requestId) {
    if (confirm('Approve this user?')) {
        // Use URLSearchParams instead of JSON
        const formData = new URLSearchParams();
        formData.append('requestId', requestId);

        fetch('/AdminPanel/ApproveRequest', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',  // Changed!
            },
            body: formData  // Send as form data, not JSON
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('User approved successfully!');
                    loadAllStudentRequests();
                    loadAllEnterpriseRequests();
                } else {
                    alert('Failed to approve user. Response: ' + JSON.stringify(data));
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred: ' + error.message);
            });
    }
}

function rejectRequest(requestId) {
    if (confirm('Reject this user?')) {
        const formData = new URLSearchParams();
        formData.append('requestId', requestId);

        fetch('/AdminPanel/RejectRequest', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('User rejected!');
                    loadAllStudentRequests();
                    loadAllEnterpriseRequests();
                } else {
                    alert('Failed to reject user. Response: ' + JSON.stringify(data));
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred: ' + error.message);
            });
    }
}

function filterTable(tableId, searchTerm) {
    const table = document.getElementById(tableId);
    const rows = table.getElementsByTagName('tr');

    for (let i = 0; i < rows.length; i++) {
        const cells = rows[i].getElementsByTagName('td');
        if (cells.length === 0) continue;

        let found = false;
        for (let j = 0; j < cells.length - 1; j++) {
            const text = cells[j].textContent.toLowerCase();
            if (text.includes(searchTerm)) {
                found = true;
                break;
            }
        }
        rows[i].style.display = found ? '' : 'none';
    }
}