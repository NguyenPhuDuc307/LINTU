// Add smooth animations to the login page
document.addEventListener("DOMContentLoaded", function () {
  // Animation for login container
  const loginContainer = document.querySelector(".login-container");
  if (loginContainer) {
    loginContainer.style.opacity = "0";
    loginContainer.style.transform = "translateY(20px)";
    loginContainer.style.transition = "opacity 0.5s ease, transform 0.5s ease";

    setTimeout(() => {
      loginContainer.style.opacity = "1";
      loginContainer.style.transform = "translateY(0)";
    }, 100);
  }

  // Focus animation for input fields
  const inputs = document.querySelectorAll(".form-control");
  inputs.forEach((input) => {
    input.addEventListener("focus", function () {
      this.parentElement.classList.add("input-focused");
    });

    input.addEventListener("blur", function () {
      if (!this.value) {
        this.parentElement.classList.remove("input-focused");
      }
    });

    // Check if input already has value on page load
    if (input.value) {
      input.parentElement.classList.add("input-focused");
    }
  });

  // Add validation feedback
  const loginForm = document.getElementById("account");
  if (loginForm) {
    loginForm.addEventListener("submit", function (e) {
      const emailInput = document.querySelector('input[name="Input.Email"]');
      const passwordInput = document.querySelector(
        'input[name="Input.Password"]'
      );
      let isValid = true;

      // Simple email validation
      if (emailInput && !isValidEmail(emailInput.value)) {
        showError(emailInput, "Please enter a valid email address");
        isValid = false;
      } else if (emailInput) {
        clearError(emailInput);
      }

      // Password validation (not empty)
      if (passwordInput && passwordInput.value.trim() === "") {
        showError(passwordInput, "Password is required");
        isValid = false;
      } else if (passwordInput) {
        clearError(passwordInput);
      }

      if (!isValid) {
        e.preventDefault();
      }
    });
  }

  // Helper functions
  function showError(input, message) {
    const formGroup = input.parentElement;
    let errorSpan = formGroup.querySelector(".text-danger");

    if (!errorSpan) {
      errorSpan = document.createElement("span");
      errorSpan.className = "text-danger";
      formGroup.appendChild(errorSpan);
    }

    errorSpan.textContent = message;
    formGroup.classList.add("has-error");
  }

  function clearError(input) {
    const formGroup = input.parentElement;
    const errorSpan = formGroup.querySelector(".text-danger");

    if (errorSpan) {
      errorSpan.textContent = "";
    }

    formGroup.classList.remove("has-error");
  }

  function isValidEmail(email) {
    const re =
      /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
  }
});
