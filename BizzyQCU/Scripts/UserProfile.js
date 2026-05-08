window.onload = function () {
    var nameInput = document.getElementById("nameInput");
    var contactInput = document.getElementById("contactInput");
    var emailInput = document.getElementById("emailInput");
    var editBtn = document.getElementById("editInfoBtn");
    var profileStatus = document.getElementById("profileStatus");
    var editProfileModal = document.getElementById("editProfileModal");
    var modalNameInput = document.getElementById("modalNameInput");
    var modalContactInput = document.getElementById("modalContactInput");
    var modalEmailInput = document.getElementById("modalEmailInput");
    var modalProfileImage = document.getElementById("modalProfileImage");
    var changePhotoBtn = document.getElementById("changePhotoBtn");
    var photoInput = document.getElementById("photoInput");
    var closeModalBtn = document.getElementById("closeModalBtn");
    var saveModalBtn = document.getElementById("saveModalBtn");

    var hasPhotoChanged = false;
    var modalPhotoDataUrl = "";
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
        var profileImage = document.getElementById("profileImage");
        originalState.photo = profileImage ? (profileImage.src || "") : "";
    }

    function restoreReadOnlyFields() {
        nameInput.value = originalState.name;
        contactInput.value = originalState.contact;
        emailInput.value = originalState.email;
        var profileImage = document.getElementById("profileImage");
        if (profileImage) {
            profileImage.src = originalState.photo;
        }
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
                    var profileImage = document.getElementById("profileImage");
                    if (profileImage) {
                        profileImage.src = response.data.photoDataUrl;
                    }
                }

                captureOriginalState();
                setStatus("", "");
            }
        });
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function isValidContact(contact) {
        return /^09\d{9}$/.test(contact);
    }

    function validateInputs(name, contact, email) {

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
        saveModalBtn.disabled = saving;
        closeModalBtn.disabled = saving;
        saveModalBtn.innerText = saving ? "Saving..." : "Save Changes";
    }

    function openModal() {
        modalNameInput.value = nameInput.value || "";
        modalContactInput.value = contactInput.value || "";
        modalEmailInput.value = emailInput.value || "";
        modalPhotoDataUrl = originalState.photo || "";
        hasPhotoChanged = false;
        if (modalProfileImage) {
            modalProfileImage.src = modalPhotoDataUrl;
        }
        editProfileModal.classList.add("show");
        editProfileModal.setAttribute("aria-hidden", "false");
    }

    function closeModal() {
        editProfileModal.classList.remove("show");
        editProfileModal.setAttribute("aria-hidden", "true");
    }

    function saveProfileFromModal() {
        var updatedName = (modalNameInput.value || "").trim();
        var updatedContact = (modalContactInput.value || "").trim();
        var updatedEmail = (modalEmailInput.value || "").trim();

        if (!validateInputs(updatedName, updatedContact, updatedEmail)) {
            return;
        }

        var payload = {
            managerName: updatedName,
            managerContactNumber: updatedContact,
            email: updatedEmail,
            photoDataUrl: hasPhotoChanged ? modalPhotoDataUrl : ""
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

                nameInput.value = updatedName;
                contactInput.value = updatedContact;
                emailInput.value = updatedEmail;
                var profileImage = document.getElementById("profileImage");
                if (profileImage && hasPhotoChanged) {
                    profileImage.src = modalPhotoDataUrl;
                }
                captureOriginalState();
                closeModal();
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

    editBtn.onclick = function () {
        setStatus("", "");
        openModal();
    };

    closeModalBtn.onclick = function () {
        restoreReadOnlyFields();
        closeModal();
    };

    saveModalBtn.onclick = function () {
        saveProfileFromModal();
    };

    changePhotoBtn.onclick = function () {
        photoInput.click();
    };

    photoInput.onchange = function () {
        var file = this.files[0];
        if (!file) {
            return;
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            modalPhotoDataUrl = e.target.result;
            hasPhotoChanged = true;
            if (modalProfileImage) {
                modalProfileImage.src = modalPhotoDataUrl;
            }
        };
        reader.readAsDataURL(file);
        photoInput.value = "";
    };

    editProfileModal.onclick = function (event) {
        if (event.target === editProfileModal) {
            closeModal();
        }
    };

    nameInput.setAttribute("readonly", "true");
    contactInput.setAttribute("readonly", "true");
    emailInput.setAttribute("readonly", "true");
    loadProfile();
};
