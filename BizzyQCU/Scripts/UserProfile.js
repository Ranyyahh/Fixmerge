window.onload = function () {

    // CHANGE PHOTO
    document.getElementById("changePhotoBtn").onclick = function () {
        document.getElementById("photoInput").click();
    };

    document.getElementById("photoInput").onchange = function () {
        var file = this.files[0];
        if (!file) return;

        var reader = new FileReader();

        reader.onload = function (e) {
            document.getElementById("profileImage").src = e.target.result;
        };

        reader.readAsDataURL(file);
    };

    // EDIT INFO
    var editBtn = document.getElementById("editInfoBtn");
    var isEditing = false;

    editBtn.onclick = function () {

        var name = document.getElementById("nameInput");
        var contact = document.getElementById("contactInput");
        var email = document.getElementById("emailInput");

        if (!isEditing) {
            name.removeAttribute("readonly");
            contact.removeAttribute("readonly");
            email.removeAttribute("readonly");

            editBtn.innerText = "Save";
            isEditing = true;

        } else {
            name.setAttribute("readonly", "true");
            contact.setAttribute("readonly", "true");
            email.setAttribute("readonly", "true");

            editBtn.innerText = "Edit Info";
            isEditing = false;
        }
    };
};