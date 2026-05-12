window.onload = function () {
    var firstNameInput = document.getElementById("firstNameInput");
    var lastNameInput = document.getElementById("lastNameInput");
    var studentNumberInput = document.getElementById("studentNumberInput");
    var sectionInput = document.getElementById("sectionInput");
    var birthdateInput = document.getElementById("birthdateInput");
    var addressInput = document.getElementById("addressInput");
    var contactInput = document.getElementById("contactInput");
    var emailInput = document.getElementById("emailInput");
    var editBtn = document.getElementById("editInfoBtn");
    var profileStatus = document.getElementById("profileStatus");
    var editProfileModal = document.getElementById("editProfileModal");
    var modalFirstNameInput = document.getElementById("modalFirstNameInput");
    var modalLastNameInput = document.getElementById("modalLastNameInput");
    var modalStudentNumberInput = document.getElementById("modalStudentNumberInput");
    var modalSectionInput = document.getElementById("modalSectionInput");
    var modalBirthdateInput = document.getElementById("modalBirthdateInput");
    var modalAddressInput = document.getElementById("modalAddressInput");
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
        firstName: "",
        lastName: "",
        studentNumber: "",
        section: "",
        birthdate: "",
        address: "",
        contact: "",
        email: "",
        photo: ""
    };

    function setStatus(message, type) {
        profileStatus.textContent = message || "";
        profileStatus.className = "status-message" + (type ? " " + type : "");
    }

    function captureOriginalState() {
        originalState.firstName = firstNameInput.value || "";
        originalState.lastName = lastNameInput.value || "";
        originalState.studentNumber = studentNumberInput.value || "";
        originalState.section = sectionInput.value || "";
        originalState.birthdate = birthdateInput.value || "";
        originalState.address = addressInput.value || "";
        originalState.contact = contactInput.value || "";
        originalState.email = emailInput.value || "";
        var profileImage = document.getElementById("profileImage");
        originalState.photo = profileImage ? (profileImage.src || "") : "";
    }

    function restoreReadOnlyFields() {
        firstNameInput.value = originalState.firstName;
        lastNameInput.value = originalState.lastName;
        studentNumberInput.value = originalState.studentNumber;
        sectionInput.value = originalState.section;
        birthdateInput.value = originalState.birthdate;
        addressInput.value = originalState.address;
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

                firstNameInput.value = response.data.firstname || "";
                lastNameInput.value = response.data.lastname || "";
                studentNumberInput.value = response.data.studentNumber || "";
                sectionInput.value = response.data.section || "";
                birthdateInput.value = response.data.birthdate || "";
                addressInput.value = response.data.address || "";
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

    function validateInputs(firstName, lastName, studentNumber, section, contact, email) {

        if (!firstName || !lastName) {
            setStatus("First name and last name are required.", "error");
            return false;
        }

        if (!studentNumber || !section) {
            setStatus("Student number and section are required.", "error");
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
        modalFirstNameInput.value = firstNameInput.value || "";
        modalLastNameInput.value = lastNameInput.value || "";
        modalStudentNumberInput.value = studentNumberInput.value || "";
        modalSectionInput.value = sectionInput.value || "";
        modalBirthdateInput.value = birthdateInput.value || "";
        modalAddressInput.value = addressInput.value || "";
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
        var updatedFirstName = (modalFirstNameInput.value || "").trim();
        var updatedLastName = (modalLastNameInput.value || "").trim();
        var updatedStudentNumber = (modalStudentNumberInput.value || "").trim();
        var updatedSection = (modalSectionInput.value || "").trim();
        var updatedBirthdate = (modalBirthdateInput.value || "").trim();
        var updatedAddress = (modalAddressInput.value || "").trim();
        var updatedContact = (modalContactInput.value || "").trim();
        var updatedEmail = (modalEmailInput.value || "").trim();

        if (!validateInputs(updatedFirstName, updatedLastName, updatedStudentNumber, updatedSection, updatedContact, updatedEmail)) {
            return;
        }

        var payload = {
            firstname: updatedFirstName,
            lastname: updatedLastName,
            studentNumber: updatedStudentNumber,
            section: updatedSection,
            birthdate: updatedBirthdate,
            address: updatedAddress,
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

                firstNameInput.value = updatedFirstName;
                lastNameInput.value = updatedLastName;
                studentNumberInput.value = updatedStudentNumber;
                sectionInput.value = updatedSection;
                birthdateInput.value = updatedBirthdate;
                addressInput.value = updatedAddress;
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

    firstNameInput.setAttribute("readonly", "true");
    lastNameInput.setAttribute("readonly", "true");
    studentNumberInput.setAttribute("readonly", "true");
    sectionInput.setAttribute("readonly", "true");
    birthdateInput.setAttribute("readonly", "true");
    addressInput.setAttribute("readonly", "true");
    contactInput.setAttribute("readonly", "true");
    emailInput.setAttribute("readonly", "true");
    loadProfile();
};
