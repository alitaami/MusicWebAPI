const deferredSystemMessages = [];
let connection = null;
let groupName = '';
let username = '';
let userId = '';
let isJoined = false;
let chatHistoryLoaded = false;
let replyToMessageId = null;
let replyToContent = null;
let replyToSenderUsername = null;

function scrollToBottom() {
    setTimeout(() => {
        messagesList.scrollTop = messagesList.scrollHeight;
    }, 0); // Delay ensures DOM updates are rendered
}

function setReplyContext(message) {
    replyToMessageId = message.id;
    replyToContent = message.content;
    replyToSenderUsername = message.senderUsername;

    if (replyToContent.length > 10) {
        replyToContent = replyToContent.substring(0, 10) + "...";
    }

    // Set innerHTML to style the username part
    document.getElementById("replyToText").innerHTML =
        `<strong style="color: #1a73e8;">${replyToSenderUsername}</strong>: ${replyToContent}`;
    document.getElementById("replyContext").style.display = "block";

    messageInput.focus();
}

function getUserIdentifierFromToken() {
    // Helper to get a cookie by name
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    const token = getCookie('access_token');
    if (!token) return null;

    // JWT is "header.payload.signature"
    const payloadBase64 = token.split('.')[1];
    if (!payloadBase64) return null;

    // JWT payload is base64Url encoded — convert to base64 first
    const base64 = payloadBase64.replace(/-/g, '+').replace(/_/g, '/');

    try {
        // Decode base64 string
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );

        const payload = JSON.parse(jsonPayload);

        // Return the user identifier claim
        return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || null;
    } catch (e) {
        console.error('Failed to parse token payload', e);
        return null;
    }
}

function setCookie(name, value, days = 1) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function getCookie(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) return decodeURIComponent(match[2]);
    return null;
}

function deleteCookie(name) {
    document.cookie = name + '=; Max-Age=0; path=/;';
    location.reload();
}

function clearReply() {
    replyToMessageId = null;
    replyToContent = null;
    replyToSenderUsername = null;
    
    document.getElementById("replyContext").style.display = "none";
}

const joinBtn = document.getElementById("joinGroup");
const leaveBtn = document.getElementById("leaveGroup");
const sendBtn = document.getElementById("sendButton");
const messagesList = document.getElementById("messagesList");
const membersList = document.getElementById("membersList");
const messageInput = document.getElementById("messageInput");
const cancelReplyBtn = document.getElementById('cancelReplyBtn');
const logoutBtn = document.getElementById('logoutBtn');
function createMessageElement(message, isOwn) {
    const li = document.createElement("li");
    li.id = `message-${message.id}`;
    li.className = `message-container ${isOwn ? 'own' : 'other'}`;

    // Create avatar element
    if (message.senderAvatar) {
        const avatar = document.createElement("img");
        avatar.className = "message-avatar";
        avatar.src = message.senderAvatar;
        avatar.alt = message.senderUsername;
        li.appendChild(avatar);
    }

    const bubble = document.createElement("div");
    bubble.className = "message-bubble";

    // Meta info (username and time)
    const meta = document.createElement("div");
    meta.className = "message-meta";

    if (!isOwn) {
        const usernameSpan = document.createElement("span");
        usernameSpan.className = "message-username";
        usernameSpan.textContent = message.senderUsername;
        meta.appendChild(usernameSpan);
    }

    const timeSpan = document.createElement("span");
    timeSpan.className = "message-time";
    timeSpan.textContent = new Date(message.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    meta.appendChild(timeSpan);

    bubble.appendChild(meta);

    // Reply box if applicable
    if (message.replyToContent && !message.replyToDeleted) {
        const replyBox = document.createElement("div");
        replyBox.className = "reply-box";
        replyBox.title = `Replying to ${message.replyToSenderUsername}: ${message.replyToContent}`;

        // Add avatar if available
        if (message.replyToSenderAvatar) {
            const replyAvatar = document.createElement("img");
            replyAvatar.className = "reply-avatar";
            replyAvatar.src = message.replyToSenderAvatar;
            replyAvatar.alt = message.replyToSenderUsername;
            replyBox.appendChild(replyAvatar);
        }

        const replyText = document.createElement("div");
        const replyUsername = document.createElement("span");
        replyUsername.className = "reply-username";
        replyUsername.textContent = message.replyToSenderUsername;

        let trimmedContent = message.replyToContent;
        if (trimmedContent.length > 30) {
            trimmedContent = trimmedContent.substring(0, 30) + "...";
        }

        const replyContent = document.createElement("span");
        replyContent.className = "reply-content";
        replyContent.textContent = trimmedContent;

        replyText.appendChild(replyUsername);
        replyText.appendChild(document.createTextNode(" "));
        replyText.appendChild(replyContent);
        replyBox.appendChild(replyText);

        replyBox.onclick = () => {
            const repliedMessage = document.getElementById(`message-${message.replyToMessageId}`);
            if (repliedMessage) {
                repliedMessage.scrollIntoView({ behavior: 'smooth' });
                repliedMessage.classList.add('highlight');
                setTimeout(() => repliedMessage.classList.remove('highlight'), 2000);
            }
        };

        bubble.appendChild(replyBox);
    }

    // Message content
    const content = document.createElement("div");
    content.className = "message-text";
    content.textContent = message.content;
    bubble.appendChild(content);

    // Action buttons
    const actions = document.createElement("div");
    actions.className = "message-actions";

    // Reply button
    const replyBtn = document.createElement("button");
    replyBtn.classList.add("reply-button");
    replyBtn.textContent = "Reply";
    replyBtn.onclick = () => {
        setReplyContext(message);
    };
    actions.appendChild(replyBtn);

    // Only allow delete on own messages
    if (isOwn) {
        const deleteBtn = document.createElement("button");
        deleteBtn.className = "delete-button";
        deleteBtn.textContent = "Delete";
        deleteBtn.onclick = async () => {
            await connection.invoke("DeleteMessage", groupName, message.id);
        };
        actions.appendChild(deleteBtn);
    }

    bubble.appendChild(actions);
    li.appendChild(bubble);

    return li;
}
function renderMembers(members) {
    membersList.innerHTML = '';
    members.forEach(m => {
        const div = document.createElement("div");
        div.className = "member-item";

        if (m.avatar) {
            const avatarImg = document.createElement("img");
            avatarImg.src = m.avatar;
            avatarImg.alt = m.username;
            avatarImg.className = "member-avatar";
            div.appendChild(avatarImg);
        }

        const nameSpan = document.createElement("span");
        nameSpan.textContent = m.username || m.fullName || m.userId;
        div.appendChild(nameSpan);

        membersList.appendChild(div);
    });
}


let handlersRegistered = false;

async function initializeConnection() {
    if (connection && connection.state === signalR.HubConnectionState.Connected)
        return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    if (!handlersRegistered) {
        registerEventHandlers();
        handlersRegistered = true;
    }

    try {
        await connection.start();
        console.log("Connected to SignalR Hub");
    } catch (err) {
        console.error("Connection failed: ", err.toString());
    }
}

function registerEventHandlers() {

    connection.off("ReceiveMessage");
    connection.off("UserJoined");
    connection.off("UserLeft");
    connection.off("GroupMembers");
    connection.off("ChatHistory");
    
    connection.on("ReceiveMessage", (message) => {
        const isOwn = message.senderUsername === username;
        const messageElement = createMessageElement(message, isOwn);
        messagesList.appendChild(messageElement);
        scrollToBottom();
    });

    connection.on("ChatHistory", (messages) => {
        messages.reverse().forEach(msg => {
            const isOwn = msg.senderUsername === username;
            const messageElement = createMessageElement(msg, isOwn);
            messagesList.appendChild(messageElement);
        });

        chatHistoryLoaded = true;
        deferredSystemMessages.forEach(msg => {
            messagesList.appendChild(msg);
        });
        scrollToBottom();
        deferredSystemMessages.length = 0;
    });

    connection.on("MessageDeleted", (messageId) => {
        const msgElement = document.getElementById(`message-${messageId}`);
        if (msgElement) {
            msgElement.style.transition = "opacity 1s";
            msgElement.style.opacity = "0.3";

            setTimeout(() => {
                msgElement.remove();
            }, 200);
        }
    });

    connection.on("UserJoined", async (userId) => {
        try {
            const joinedUsername = await connection.invoke("GetUsernameByUserId", userId);

            const li = document.createElement("li");
            li.textContent = `${joinedUsername ?? userId} joined the group`;
            li.className = "system-message";

            if (!chatHistoryLoaded) {
                deferredSystemMessages.push(li);
            } else {
                messagesList.appendChild(li);
                scrollToBottom();
            }

            await connection.invoke("GetGroupMembers", groupName);
        } catch (error) {
            console.error("Error fetching username:", error);
        }
    });

    connection.on("UserLeft", async (userId) => {
        try {
            const leftUsername = await connection.invoke("GetUsernameByUserId", userId);
            const li = document.createElement("li");
            li.textContent = `${leftUsername ?? userId} left the group.`;
            li.className = "system-message";

            if (!chatHistoryLoaded) {
                deferredSystemMessages.push(li);
            } else {
                messagesList.appendChild(li);
                scrollToBottom();
            }

            await connection.invoke("GetGroupMembers", groupName);
        } catch (error) {
            console.error("Error fetching username:", error);
        }
    });

    connection.on("GroupMembers", (members) => {
        renderMembers(members);
    });

    connection.on("RefreshReplies", (replyMessageIds) => {
        replyMessageIds.forEach(id => {
            const msgElement = document.getElementById(`message-${id}`);
            if (msgElement) {
                const replyBox = msgElement.querySelector(".reply-box");
                if (replyBox) {
                    replyBox.remove();
                }
            }
        });
    });
}

joinBtn.addEventListener("click", async () => {
    chatHistoryLoaded = false; // Reset before fetching

    //userId = document.getElementById("userId").value.trim();
    userId = getUserIdentifierFromToken();

    if (!userId) {
        alert("You should login first!");
        return;
    }

    username = await connection.invoke("GetUsernameByUserId", userId);
    groupName = document.getElementById("groupName").value;

    if (isJoined) {
        alert("You have already joined this group.");
        return;
    }

    if (!username || !groupName) {
        alert("You should be logged in and have selected a group to join in!");
        return;
    }

    await initializeConnection();

    try {
        await connection.invoke("JoinGroup", groupName, userId);
        await connection.invoke("GetGroupMembers", groupName);
        await connection.invoke("GetChats", groupName, 0, 50);
        isJoined = true;
        scrollToBottom();
        messageInput.focus();

    } catch (err) {
        console.error("Join failed:", err.toString());
    }
});

leaveBtn.addEventListener("click", async () => {
    if (!isJoined || !connection || connection.state !== signalR.HubConnectionState.Connected) return;

    try {
        await connection.invoke("LeaveGroup", groupName, userId);

        messagesList.innerHTML = "";
        membersList.innerHTML = "";
        isJoined = false;
        groupName = '';
        chatHistoryLoaded = false; // Reset before fetching

    } catch (err) {
        console.error("Leave failed:", err.toString());
    }
});

sendBtn.addEventListener("click", async () => {
    const message = messageInput.value.trim();

    if (!message) return;

    if (!isJoined || !connection || connection.state !== signalR.HubConnectionState.Connected) {
        alert("Please join a group first.");
        return;
    }

    try {
        await connection.invoke("SendMessage", groupName, userId, message, replyToMessageId);
        messageInput.value = '';
        clearReply(); // reset reply context
        scrollToBottom();

    } catch (err) {
        console.error("Failed to send message:", err.toString());
    }
});

logoutBtn.addEventListener("click", async () => {
    try { 
        deleteCookie('access_token');
    } catch (err) {
        console.error("Failed to Logout! ", err.toString());
    }
});
 
cancelReplyBtn.addEventListener("click", async () => {
    try {
        clearReply();
    } catch (err) {
        console.error("Failed to cancel reply!", err.toString());
    }
});

messageInput.addEventListener("keydown", async (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendBtn.click();
    }
});

// Optional: auto-load groups on page load
window.addEventListener("DOMContentLoaded", async () => {
    try {
        await initializeConnection();
        const groups = await connection.invoke("GetGroups");
        const groupSelect = document.getElementById("groupName");
        groupSelect.innerHTML = '<option value="">Select Group</option>';
        groups.forEach(group => {
            const option = document.createElement("option");
            option.value = group.name;
            option.textContent = group.name;
            groupSelect.appendChild(option);
        });

    } catch (error) {
        console.error("Error loading groups:", error);
    }
});

 
