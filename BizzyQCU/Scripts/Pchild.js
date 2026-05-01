// =============================================
// Pchild.js — Product Detail / Order Page
// BizzyQCU Web App
// =============================================

document.addEventListener("DOMContentLoaded", function () {

    // -----------------------------------------------
    // QUANTITY CONTROLS
    // -----------------------------------------------

    const qtyInput = document.getElementById("qtyInput");
    const btnMinus = document.getElementById("btnMinus");
    const btnPlus = document.getElementById("btnPlus");

    /**
     * Decrements quantity by 1.
     * Prevents going below 1.
     */
    btnMinus.addEventListener("click", function () {
        let current = parseInt(qtyInput.value) || 1;
        if (current > 1) {
            qtyInput.value = current - 1;
        }
        // TODO: Update cart badge/count in the navbar if a live cart is implemented
    });

    /**
     * Increments quantity by 1.
     * TODO: Add a maximum stock cap here once product stock data is available from the server
     */
    btnPlus.addEventListener("click", function () {
        let current = parseInt(qtyInput.value) || 1;
        qtyInput.value = current + 1;
        // TODO: Validate against available stock from the model (e.g., Model.StockCount)
    });

    /**
     * Validates manual input in the quantity field.
     * Ensures only positive integers are accepted.
     */
    qtyInput.addEventListener("change", function () {
        let val = parseInt(this.value);
        if (isNaN(val) || val < 1) {
            this.value = 1;
        }
        // TODO: Clamp to max stock if stock data is bound to the page
    });


    // -----------------------------------------------
    // ORDER NOW BUTTON
    // -----------------------------------------------

    const btnOrder = document.getElementById("btnOrder");

    btnOrder.addEventListener("click", function () {
        const quantity = parseInt(qtyInput.value) || 1;

        // TODO: Retrieve the product ID from a hidden input or data attribute
        //       e.g., const productId = document.getElementById("productId").value;

        // TODO: Retrieve the delivery method selected by the user
        //       e.g., const deliveryMethod = document.querySelector('input[name="delivery"]:checked')?.value;

        // TODO: Validate that the user is logged in before placing an order
        //       If not logged in, redirect to /Account/Login or show a modal

        // TODO: Send an AJAX POST request to the order endpoint, e.g.:
        //       fetch('/Order/Place', {
        //           method: 'POST',
        //           headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
        //           body: JSON.stringify({ productId, quantity, deliveryMethod })
        //       })
        //       .then(res => res.json())
        //       .then(data => {
        //           if (data.success) showToast("Order placed successfully!");
        //           else showToast("Failed to place order. Please try again.");
        //       });

        // Temporary feedback — replace with actual AJAX call above
        showToast("Order placed! (" + quantity + " item" + (quantity > 1 ? "s" : "") + ")");
    });


    // -----------------------------------------------
    // VIEW ENTERPRISE BUTTON
    // -----------------------------------------------

    const btnViewEnterprise = document.getElementById("btnViewEnterprise");

    if (btnViewEnterprise) {
        btnViewEnterprise.addEventListener("click", function () {
            // TODO: Get the enterprise ID from a data attribute or hidden input
            //       e.g., const enterpriseId = this.dataset.enterpriseId;

            // TODO: Navigate to the enterprise profile page
            //       e.g., window.location.href = `/Enterprise/Profile/${enterpriseId}`;

            console.log("View Enterprise clicked.");
        });
    }


    // -----------------------------------------------
    // TOAST NOTIFICATION HELPER
    // -----------------------------------------------

    /**
     * Shows a brief toast notification at the bottom-right of the screen.
     * @param {string} message - The message to display.
     */
    function showToast(message) {
        const toast = document.getElementById("checkoutToast");
        if (!toast) return;

        toast.textContent = message;
        toast.classList.add("show");

        // TODO: Consider stacking multiple toasts if rapid actions are supported
        setTimeout(function () {
            toast.classList.remove("show");
        }, 2800);
    }


    // -----------------------------------------------
    // ANTI-FORGERY TOKEN HELPER (for future AJAX use)
    // -----------------------------------------------

    /**
     * Retrieves the ASP.NET Core Anti-Forgery token from the page.
     * Needed for secure POST requests via fetch/AJAX.
     * TODO: Call this inside the fetch() POST request when the order endpoint is wired up
     */
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }

});