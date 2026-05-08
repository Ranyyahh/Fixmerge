document.addEventListener('DOMContentLoaded', function () {
    loadAllStudentRequests();
    loadAllEnterpriseRequests();
    loadFeedbacks();

    const tabUsers = document.getElementById('tabUsers');
    const tabEnterprises = document.getElementById('tabEnterprises');
    const tabFeedbacks = document.getElementById('tabFeedbacks');
    const panelUsers = document.getElementById('panelUsers');
    const panelEnterprises = document.getElementById('panelEnterprises');
    const panelFeedbacks = document.getElementById('panelFeedbacks');
    const feedbackFilterWrap = document.getElementById('feedbackFilterWrap');

    tabUsers.addEventListener('click', function () {
        tabUsers.classList.add('active');
        tabEnterprises.classList.remove('active');
        tabFeedbacks.classList.remove('active');
        panelUsers.classList.remove('hidden');
        panelEnterprises.classList.add('hidden');
        panelFeedbacks.classList.add('hidden');
        feedbackFilterWrap.classList.add('hidden');
    });

    tabEnterprises.addEventListener('click', function () {
        tabEnterprises.classList.add('active');
        tabUsers.classList.remove('active');
        tabFeedbacks.classList.remove('active');
        panelEnterprises.classList.remove('hidden');
        panelUsers.classList.add('hidden');
        panelFeedbacks.classList.add('hidden');
        feedbackFilterWrap.classList.add('hidden');
    });

    tabFeedbacks.addEventListener('click', function () {
        tabFeedbacks.classList.add('active');
        tabUsers.classList.remove('active');
        tabEnterprises.classList.remove('active');
        panelFeedbacks.classList.remove('hidden');
        panelUsers.classList.add('hidden');
        panelEnterprises.classList.add('hidden');
        feedbackFilterWrap.classList.remove('hidden');
    });

    document.getElementById('searchInput').addEventListener('keyup', function () {
        const searchTerm = this.value.toLowerCase();
        const activeTab = document.querySelector('.tab.active').id;

        if (activeTab === 'tabUsers') {
            filterTable('usersTableBody', searchTerm);
        } else if (activeTab === 'tabEnterprises') {
            filterTable('enterprisesTableBody', searchTerm);
        } else {
            filterTable('feedbacksTableBody', searchTerm);
        }
    });

    document.getElementById('ratingFilter').addEventListener('change', applyFeedbackFilters);
    document.getElementById('categoryFilter').addEventListener('change', applyFeedbackFilters);
});

let allFeedbacks = [];

function loadAllStudentRequests() {
    fetch('/AdminPanel/GetAllStudentRequests')
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '';

            if (!data || data.length === 0) {
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
                const statusText = student.Status || '';
                const statusLower = statusText.toLowerCase();

                if (statusLower === 'pending') {
                    statusClass = 'status-pending';
                } else if (statusLower === 'approved') {
                    statusClass = 'status-approved';
                } else if (statusLower === 'rejected') {
                    statusClass = 'status-rejected';
                }
                row.insertCell(4).innerHTML = `<span class="${statusClass}">${statusText}</span>`;

                if (statusLower === 'pending') {
                    row.insertCell(5).innerHTML = `
                        <div class="action-btns">
                            <button class="approve-btn" onclick="approveRequest(${student.RequestId})">Approve</button>
                            <button class="reject-btn" onclick="rejectRequest(${student.RequestId})">Reject</button>
                        </div>
                    `;
                } else {
                    row.insertCell(5).innerHTML = '<span>-</span>';
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

            if (!data || data.length === 0) {
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
                const statusText = enterprise.Status || '';
                const statusLower = statusText.toLowerCase();

                if (statusLower === 'pending') {
                    statusClass = 'status-pending';
                } else if (statusLower === 'approved') {
                    statusClass = 'status-approved';
                } else if (statusLower === 'rejected') {
                    statusClass = 'status-rejected';
                }
                row.insertCell(4).innerHTML = `<span class="${statusClass}">${statusText}</span>`;

                if (statusLower === 'pending') {
                    row.insertCell(5).innerHTML = `
                        <div class="action-btns">
                            <button class="approve-btn" onclick="approveRequest(${enterprise.RequestId})">Approve</button>
                            <button class="reject-btn" onclick="rejectRequest(${enterprise.RequestId})">Reject</button>
                        </div>
                    `;
                } else {
                    row.insertCell(5).innerHTML = '<span>-</span>';
                }
            });
        })
        .catch(error => {
            console.error('Error loading enterprises:', error);
            document.getElementById('enterprisesTableBody').innerHTML = '<tr><td colspan="6" style="text-align:center;color:red;">Error loading data</td></tr>';
        });
}

function loadFeedbacks() {
    const tbody = document.getElementById('feedbacksTableBody');
    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Loading...</td></tr>';

    fetch('/AdminPanel/GetFeedbacks')
        .then(response => response.json())
        .then(data => {
            allFeedbacks = Array.isArray(data) ? data : [];
            populateCategoryFilter(allFeedbacks);
            applyFeedbackFilters();
        })
        .catch(error => {
            console.error('Error loading feedbacks:', error);
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:red;">Error loading feedbacks</td></tr>';
        });
}

function populateCategoryFilter(feedbacks) {
    const categoryFilter = document.getElementById('categoryFilter');
    const selected = categoryFilter.value;
    const categories = [...new Set(feedbacks.map(f => (f.Category || 'General').trim()).filter(Boolean))].sort();

    categoryFilter.innerHTML = '<option value="">All Categories</option>';
    categories.forEach(category => {
        const option = document.createElement('option');
        option.value = category;
        option.textContent = category;
        categoryFilter.appendChild(option);
    });

    if (categories.includes(selected)) {
        categoryFilter.value = selected;
    }
}

function applyFeedbackFilters() {
    const tbody = document.getElementById('feedbacksTableBody');
    const ratingValue = document.getElementById('ratingFilter').value;
    const categoryValue = document.getElementById('categoryFilter').value;
    const ratingNumber = ratingValue ? parseInt(ratingValue, 10) : null;

    const filtered = allFeedbacks.filter(feedback => {
        const matchesRating = ratingNumber ? Number(feedback.Rating) === ratingNumber : true;
        const currentCategory = (feedback.Category || 'General').trim();
        const matchesCategory = categoryValue ? currentCategory === categoryValue : true;
        return matchesRating && matchesCategory;
    });

    tbody.innerHTML = '';

    if (filtered.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">No feedback found</td></tr>';
        return;
    }

    filtered.forEach(feedback => {
        const row = tbody.insertRow();
        row.insertCell(0).innerHTML = feedback.Email || 'N/A';
        row.insertCell(1).innerHTML = feedback.ContactNumber || 'N/A';
        row.insertCell(2).innerHTML = feedback.Category || 'General';
        row.insertCell(3).innerHTML = feedback.Message || '';
        row.insertCell(4).innerHTML = renderStars(feedback.Rating);
    });
}

function renderStars(rating) {
    const safeRating = Number.isInteger(rating) ? Math.max(1, Math.min(5, rating)) : 0;
    const filled = '&#9733;'.repeat(safeRating);
    const empty = '&#9734;'.repeat(5 - safeRating);
    return `<span class="rating-stars">${filled}${empty}</span>`;
}

function approveRequest(requestId) {
    if (confirm('Approve this user?')) {
        const formData = new URLSearchParams();
        formData.append('requestId', requestId);

        fetch('/AdminPanel/ApproveRequest', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('User approved successfully!');
                    loadAllStudentRequests();
                    loadAllEnterpriseRequests();
                } else {
                    alert('Failed to approve user: ' + (data.message || 'Unknown error'));
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
                    alert('Failed to reject user: ' + (data.message || 'Unknown error'));
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
        for (let j = 0; j < cells.length; j++) {
            const text = cells[j].textContent.toLowerCase();
            if (text.includes(searchTerm)) {
                found = true;
                break;
            }
        }
        rows[i].style.display = found ? '' : 'none';
    }
}
