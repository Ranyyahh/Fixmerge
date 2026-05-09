// Image preview
document.getElementById('productImage').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            const preview = document.getElementById('imagePreview');
            preview.src = event.target.result;
            preview.style.display = 'block';
            document.querySelector('.add-product-image-icon').style.display = 'none';
        };
        reader.readAsDataURL(file);
    }
});

// Save product
document.getElementById('saveProductBtn').addEventListener('click', async function () {
    const name = document.getElementById('productName').value.trim();
    const price = document.getElementById('productPrice').value;
    const category = document.getElementById('productCategory').value;
    const preparationTime = document.getElementById('preparationTime').value.trim();
    const description = document.getElementById('productDescription').value.trim();
    const imageFile = document.getElementById('productImage').files[0];

    if (!name) {
        alert('Please enter product name');
        return;
    }

    if (!price || parseFloat(price) <= 0) {
        alert('Please enter a valid price');
        return;
    }

    if (!category) {
        alert('Please select a category');
        return;
    }

    const formData = new FormData();
    formData.append('name', name);
    formData.append('price', parseFloat(price));
    formData.append('category', category);
    formData.append('preparationTime', preparationTime);
    formData.append('description', description);
    formData.append('productImage', imageFile || '');

    const saveBtn = document.getElementById('saveProductBtn');
    saveBtn.disabled = true;
    saveBtn.textContent = 'Saving...';

    try {
        const response = await fetch('/AddProduct/AddProductAjax', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (data.success) {
            alert('Product submitted for approval! Admin will review it shortly.');
            window.location.href = '/EnterpriseDashboard/EnterpriseDashboard';
        } else {
            alert(data.message);
            saveBtn.disabled = false;
            saveBtn.textContent = 'Save Product';
        }
    } catch (error) {
        alert('Failed to add product');
        saveBtn.disabled = false;
        saveBtn.textContent = 'Save Product';
    }
});