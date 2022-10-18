const checkbox = document.getElementById("flexCheckDefault");
var password = document.getElementById("floatingPassword");
var confirmPassword = document.getElementById("floatingConfirmPassword");
    
checkbox.addEventListener('change', function() {
    if (this.checked) {
        password.setAttribute("type", "text");
        confirmPassword.setAttribute("type", "text");
    } else {
        password.setAttribute("type", "password");
        confirmPassword.setAttribute("type", "password");
    }
});