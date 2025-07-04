﻿const recoverPassUrl = 'http://localhost:8080/api/resetPassword';
const forgetPassUrl = 'http://localhost:8080/api/forgetPassword';
const registerUrl = 'http://localhost:8080/api/register';
const loginUrl = 'http://localhost:8080/api/login';
const googleLoginUrl = 'http://localhost:8080/api/auth/google-login';
const googleClientIdUrl = 'http://localhost:8080/api/auth/google/client_id';
const googleClientScriptUrl = 'https://accounts.google.com/gsi/client';

const passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

document.addEventListener('DOMContentLoaded', () => {
    const loginBtn = document.getElementById('loginBtn');
    if (loginBtn) {
        loginBtn.addEventListener('click', login);
    }

    const registerBtn = document.getElementById('registerBtn');
    if (registerBtn) {
        registerBtn.addEventListener('click', register);
    }

    const forgetPassBtn = document.getElementById('forgetPassBtn');
    if (forgetPassBtn) {
        forgetPassBtn.addEventListener('click', forgetPass);
    }

    const recoverPassBtn = document.getElementById('recoverPassBtn');
    if (recoverPassBtn) {
        recoverPassBtn.addEventListener('click', recoverPass);
    }

    // Show Register form link
    const showRegisterForm = document.getElementById('showRegisterForm');
    if (showRegisterForm) {
        showRegisterForm.addEventListener('click', (e) => {
            e.preventDefault();
            toggleForm('register');
        });
    }

    // Show Forget Pass form link
    const showForgetPassForm = document.getElementById('showForgetPassForm');
    if (showForgetPassForm) {
        showForgetPassForm.addEventListener('click', (e) => {
            e.preventDefault();
            toggleForm('forgetPass');
        });
    }

    // Show Login form link
    const showLoginForm = document.getElementById('showLoginForm');
    if (showLoginForm) {
        showLoginForm.addEventListener('click', (e) => {
            e.preventDefault();
            toggleForm('login');
        });
    }

    // Logout button already has id "logoutBtn"
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            deleteCookie('access_token');
        });
    }
});

async function login() {
    const email = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;

    if (!email || !password) {
        alert('Please enter username and password.');
        return;
    }

    try {
        const response = await fetch(loginUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email, password }),
        });

        const data = await response.json();

        if (response.status === 422 || response.status === 401) {
            alert(data.errorMessage || 'Validation error occurred');
            return;
        }

        if (!response.ok) {
            throw new Error(data.errorMessage || 'Login failed');
        }

        console.log('Login successful:', data);

        //Save token to cookie
        setCookie('access_token', data.data.token);

        //show chat section after login
        document.querySelector('.auth-container').style.display = 'none';
        document.getElementById('chatContainer').style.display = 'block';

    } catch (error) {
        console.error('Login error:', error.message);
        alert(error.message);
    }
}

async function register() {
    const userName = document.getElementById('registerUsername').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const isArtist = document.getElementById('isArtistCheckbox').checked;

    if (!userName || !email || !password) {
        alert('Please fill in all fields.');
        return;
    }

    try {
        const response = await fetch(registerUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userName,       // username
                email,
                password,
                fullName: userName,  // assuming fullName = userName here, adjust if you want separate input
                isArtist
            }),
        });

        const data = await response.json();

        if (response.status === 422 || response.status === 401) {
            alert(data.errorMessage || 'Validation error occurred');
            return;
        }

        if (!response.ok) {
            throw new Error(data.errorMessage || 'Registration failed');
        }

        // Save token to cookie
        setCookie('access_token', data.data.token);

        //show chat section after registration
        document.querySelector('.auth-container').style.display = 'none';
        document.getElementById('chatContainer').style.display = 'block';
    } catch (error) {
        console.error('Register error:', error.message);
        alert(error.message);
    }
}

async function forgetPass() {
    const email = document.getElementById('forgetPassEmail').value.trim();

    if (!email) {
        alert('Please fill in Email field.');
        return;
    }

    try {
        const response = await fetch(forgetPassUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email,
            }),
        });

        const data = await response.json();

        if (response.status === 422 || response.status === 401) {
            alert(data.errorMessage || 'Validation error occurred');
            return;
        }

        if (!response.ok) {
            //throw new Error(data.errorMessage || 'Error occurred!');
            alert(data.errorMessage);
            return;
        }
         
        //show recover pass form
        toggleForm("recoverPass");

        document.getElementById('recoverPassEmail').value = email;

    } catch (error) {
        console.error('ForgetPass error:', error.message);
        alert(error.message);
    }
}

async function recoverPass() {
    const email = document.getElementById('recoverPassEmail').value.trim();
    const newPassword = document.getElementById('recoverPassNewPassword').value.trim();
    const otpCode = document.getElementById('recoverPassOtp').value.trim();

    if (!email || !otpCode || !newPassword) {
        alert('Please fill in all fields.');
        return;
    }

    if (!passwordRegex.test(newPassword)) {
        alert(
            "Password must be at least 8 characters long and include:\n" +
            "• 1 uppercase letter\n" +
            "• 1 lowercase letter\n" +
            "• 1 number\n" +
            "• 1 special character (@$!%*?&)"
        );
        return;
    }

    if (otpCode.length !== 6) {
        alert("Enter your OTP code correctly!");
        return;
    }

    try {
        const response = await fetch(recoverPassUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email,
                newPassword,
                otpCode
            }),
        });

        const data = await response.json();

        if (response.status === 422 || response.status === 401) {
            alert(data.errorMessage || 'Validation error occurred');
            return;
        }

        if (!response.ok) {
            //throw new Error(data.errorMessage || 'reset pass failed');
            alert.error(data.errorMessage);
        }

        //show login section
        toggleForm("login");

    } catch (error) {
        console.error('recover pass error:', error.message);
        alert(error.message);
    }
}

window.onload = function () {
    const accessToken = getCookie('access_token');
    setGoogleClientId();

    if (accessToken) {
        // Token found — show chat, hide login/register
        document.getElementById('chatContainer').style.display = 'block';
        document.getElementById('authWrapper').style.display = 'none';
    } else {
        // No token — show login form, hide chat and register
        document.getElementById('chatContainer').style.display = 'none';
        document.getElementById('loginForm').style.display = 'block';
        document.getElementById('registerForm').style.display = 'none';
        document.getElementById('recoverPassForm').style.display = 'none';
        document.getElementById('forgetPassForm').style.display = 'none';
    }
};

function setCookie(name, value, days = 1) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function getCookie(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) return decodeURIComponent(match[2]);
    return null;
}

function toggleForm(form) {


    if (form === 'login') {
        document.getElementById('loginForm').style.display = 'block';
        document.getElementById('forgetPassForm').style.display = 'none';
        document.getElementById('recoverPassForm').style.display = 'none';
    }

    else if (form === 'register') {
        document.getElementById('registerForm').style.display = 'block';
        document.getElementById('loginForm').style.display = 'none';
        document.getElementById('forgetPassForm').style.display = 'none';
        document.getElementById('recoverPassForm').style.display = 'none';
    }

    else if (form === 'forgetPass') {
        document.getElementById('loginForm').style.display = 'none';
        document.getElementById('registerForm').style.display = 'none';
        document.getElementById('forgetPassForm').style.display = 'block';
    }

    else if (form === 'recoverPass') {
        document.getElementById('loginForm').style.display = 'none';
        document.getElementById('registerForm').style.display = 'none';
        document.getElementById('forgetPassForm').style.display = 'none';
        document.getElementById('recoverPassForm').style.display = 'block';
    }
}

async function setGoogleClientId() {
    try {
        const res = await fetch(googleClientIdUrl);
        if (!res.ok) throw new Error('Failed to fetch client id');

        const data = await res.json();

        const clientId = data.data;

        const gIdOnload = document.getElementById('g_id_onload');
        gIdOnload.setAttribute('data-client_id', clientId);

        // Now load the Google script dynamically AFTER setting client_id
        const script = document.createElement('script');
        script.src = googleClientScriptUrl;
        script.async = true;
        script.defer = true;
        document.head.appendChild(script);
    } catch (error) {
        console.error('Error fetching Google Client ID:', error);
    }
}

async function onGoogleSignIn(response) {
    const idToken = response.credential;

    try {
        const res = await fetch(googleLoginUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idToken })
        });

        const data = await res.json();

        if (!res.ok) {
            alert(data.errorMessage || 'Google login failed');
            return;
        }

        // Extract token directly from data.data (which is a string token)
        setCookie('access_token', data.data);

        document.getElementById('authWrapper').style.display = 'none';
        document.getElementById('chatContainer').style.display = 'block';
    } catch (err) {
        console.error("Google login error:", err);
        alert('An error occurred during Google login.');
    }
}
