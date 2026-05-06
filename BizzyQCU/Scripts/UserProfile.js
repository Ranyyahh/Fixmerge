window.onload = function () {
    var photoInput = document.getElementById("photoInput");
    var profileImage = document.getElementById("profileImage");
    var nameInput = document.getElementById("nameInput");
    var contactInput = document.getElementById("contactInput");
    var emailInput = document.getElementById("emailInput");
    var editBtn = document.getElementById("editInfoBtn");
    var changePhotoBtn = document.getElementById("changePhotoBtn");
    var cancelEditBtn = document.getElementById("cancelEditBtn");
    var profileStatus = document.getElementById("profileStatus");

    var isEditing = false;
    var hasPhotoChanged = false;
    var originalState = {
        name: "",
        contact: "",
        email: "",
        photo: ""
    };

    function setStatus(message, type) {
        profileStatus.textContent = message || "";
        profileStatus.className = "status-message" + (type ? " " + type : "");
    }

    function captureOriginalState() {
        originalState.name = nameInput.value || "";
        originalState.contact = contactInput.value || "";
        originalState.email = emailInput.value || "";
        originalState.photo = profileImage.src || "";
    }

    function restoreOriginalState() {
        nameInput.value = originalState.name;
        contactInput.value = originalState.contact;
        emailInput.value = originalState.email;
        profileImage.src = originalState.photo;
        hasPhotoChanged = false;
    }

    function loadProfile() {
        $.ajax({
            url: "/Profile/GetUserProfile",
            type: "GET",
            success: function (response) {
                if (!response || !response.success || !response.data) {
                    if (response && response.message) {
                        setStatus(response.message, "error");
                    }
                    return;
                }

                nameInput.value = response.data.managerName || "";
                contactInput.value = response.data.managerContactNumber || "";
                emailInput.value = response.data.email || "";

                if (response.data.photoDataUrl) {
                    profileImage.src = response.data.photoDataUrl;
                }

                captureOriginalState();
                setStatus("", "");
            }
        });
    }

    function setReadonly(readonly) {
        if (readonly) {
            nameInput.setAttribute("readonly", "true");
            contactInput.setAttribute("readonly", "true");
            emailInput.setAttribute("readonly", "true");
            editBtn.innerText = "Edit Profile";
            changePhotoBtn.disabled = true;
            cancelEditBtn.style.display = "none";
            isEditing = false;
            return;
        }

        nameInput.removeAttribute("readonly");
        contactInput.removeAttribute("readonly");
        emailInput.removeAttribute("readonly");
        editBtn.innerText = "Save Changes";
        changePhotoBtn.disabled = false;
        cancelEditBtn.style.display = "inline-block";
        isEditing = true;
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function isValidContact(contact) {
        return /^09\d{9}$/.test(contact);
    }

    function validateInputs() {
        var name = (nameInput.value || "").trim();
        var contact = (contactInput.value || "").trim();
        var email = (emailInput.value || "").trim();

        if (!name) {
            setStatus("Name is required.", "error");
            return false;
        }

        if (!isValidContact(contact)) {
            setStatus("Contact must be 11 digits and start with 09.", "error");
            return false;
        }

        if (!isValidEmail(email)) {
            setStatus("Please enter a valid email address.", "error");
            return false;
        }

        return true;
    }

    function setSavingState(saving) {
        editBtn.disabled = saving;
        cancelEditBtn.disabled = saving;
        changePhotoBtn.disabled = saving || !isEditing;
        editBtn.innerText = saving ? "Saving..." : (isEditing ? "Save Changes" : "Edit Profile");
    }

    function saveProfile() {
        if (!validateInputs()) {
            return;
        }

        var payload = {
            managerName: nameInput.value.trim(),
            managerContactNumber: contactInput.value.trim(),
            email: emailInput.value.trim(),
            photoDataUrl: hasPhotoChanged ? profileImage.src : ""
        };

        setSavingState(true);
        setStatus("", "");

        $.ajax({
            url: "/Profile/UpdateUserProfile",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (!response || !response.success) {
                    setSavingState(false);
                    setStatus((response && response.message) || "Unable to save profile.", "error");
                    return;
                }

                captureOriginalState();
                setReadonly(true);
                hasPhotoChanged = false;
                setSavingState(false);
                setStatus("Profile updated successfully.", "success");
            },
            error: function (xhr) {
                var message = "Unable to save profile right now. Please try again.";
                if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                }
                setSavingState(false);
                setStatus(message, "error");
            }
        });
    }

    changePhotoBtn.onclick = function () {
        if (!isEditing) {
            setStatus("Click Edit Profile first before changing photo.", "error");
            return;
        }
        photoInput.click();
    };

    photoInput.onchange = function () {
        var file = this.files[0];
        if (!file) return;

        var reader = new FileReader();
        reader.onload = function (e) {
            profileImage.src = e.target.result;
            hasPhotoChanged = true;
        };
        reader.readAsDataURL(file);
    };

    editBtn.onclick = function () {
        if (!isEditing) {
            captureOriginalState();
            setStatus("", "");
            setReadonly(false);
            return;
        }

        saveProfile();
    };

    cancelEditBtn.onclick = function () {
        restoreOriginalState();
        setReadonly(true);
        setStatus("Changes discarded.", "");
    };

    setReadonly(true);
    loadProfile();
};
