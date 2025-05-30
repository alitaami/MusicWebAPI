const registerUrl = 'http://localhost:8080/api/register';
const loginUrl = 'http://localhost:8080/api/login';

document.addEventListener('DOMContentLoaded', () => {
    const loginBtn = document.getElementById('loginBtn');
    if (loginBtn) {
        loginBtn.addEventListener('click', login);
    }

    const registerBtn = document.getElementById('registerBtn');
    if (registerBtn) {
        registerBtn.addEventListener('click', register);
    }

    // Show Register form link
    const showRegisterForm = document.getElementById('showRegisterForm');
    if (showRegisterForm) {
        showRegisterForm.addEventListener('click', (e) => {
            e.preventDefault();
            toggleForm('register');
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

window.onload = function () {
    const accessToken = getCookie('access_token');

    if (accessToken) {
        // Token found — show chat, hide login/register
        document.getElementById('chatContainer').style.display = 'block';
        document.getElementById('authWrapper').style.display = 'none';
    } else {
        // No token — show login form, hide chat and register
        document.getElementById('chatContainer').style.display = 'none';
        document.getElementById('loginForm').style.display = 'block';
        document.getElementById('registerForm').style.display = 'none';
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
    document.getElementById('loginForm').style.display = form === 'login' ? 'block' : 'none';
    document.getElementById('registerForm').style.display = form === 'register' ? 'block' : 'none';
}