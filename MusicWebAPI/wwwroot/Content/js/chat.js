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

function renderMembers(members) {
    membersList.innerHTML = '';
    members.forEach(m => {
        const div = document.createElement("div");
        div.className = "member-item";
        div.textContent = m.username || m.fullName || m.userId;
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
        const li = document.createElement("li");
        li.id = `message-${message.id}`; // Important: assign an ID for future reference

        const isOwn = message.senderUsername === username;

        const msgContainer = document.createElement("div");
        msgContainer.className = `message ${isOwn ? 'own' : 'other'}`;

        const meta = document.createElement("div");
        meta.style.fontSize = "12px";
        meta.style.color = "#555";
        meta.style.marginBottom = "4px";
        meta.textContent = `${message.senderUsername} • ${new Date(message.sentAt).toLocaleTimeString()}`;
        msgContainer.appendChild(meta);

        // Improved reply preview box (if this message is a reply)
        if (message.replyToContent && !message.replyToDeleted) {
            const replyBox = document.createElement("div");
            replyBox.style.fontSize = "13px";
            replyBox.style.color = "#444";
            replyBox.style.background = "#e8f0fe";
            replyBox.style.borderLeft = "4px solid #4285f4";
            replyBox.style.padding = "8px 12px";
            replyBox.style.marginBottom = "6px";
            replyBox.style.borderRadius = "6px";
            replyBox.style.cursor = "pointer";
            replyBox.style.userSelect = "none";
            replyBox.style.maxWidth = "80%";

            replyBox.title = `Replying to ${message.replyToSenderUsername}: ${message.replyToContent}`;

            const replyUsername = document.createElement("span");
            replyUsername.style.fontWeight = "600";
            replyUsername.style.color = "#1a73e8";
            replyUsername.textContent = message.replyToSenderUsername;

            // Manually trim content if needed
            let trimmedReplyContent = message.replyToContent;
            if (trimmedReplyContent.length > 20) {
                trimmedReplyContent = trimmedReplyContent.substring(0, 20) + "...";
            }

            const replyContent = document.createElement("span");
            replyContent.style.fontStyle = "italic";
            replyContent.style.marginLeft = "6px";
            replyContent.textContent = trimmedReplyContent;

            replyBox.appendChild(replyUsername);
            replyBox.appendChild(document.createTextNode(" "));
            replyBox.appendChild(replyContent);

            replyBox.onclick = () => {
                //console.log(`Clicked reply preview for message ID: ${message.replyToMessageId}`);
                //scrollToMessage(message.replyToMessageId);
            };

            msgContainer.appendChild(replyBox);
        }

        const content = document.createElement("div");
        content.textContent = message.content;
        msgContainer.appendChild(content);

        // 🔁 Add "Reply" button
        const replyBtn = document.createElement("button");
        replyBtn.textContent = "Reply";
        replyBtn.style.fontSize = "11px";
        replyBtn.style.marginTop = "4px";
        replyBtn.style.background = "transparent";
        replyBtn.style.border = "none";
        replyBtn.style.cursor = "pointer";
        replyBtn.style.color = isOwn ? "#555" : "#007bff";
        replyBtn.onclick = () => {
            setReplyContext(message);
        };
        msgContainer.appendChild(replyBtn);

        // ✅ Delete button (only for own messages)
        if (isOwn) {
            const deleteBtn = document.createElement("button");
            deleteBtn.textContent = "Delete";
            deleteBtn.style.fontSize = "11px";
            deleteBtn.style.marginLeft = "10px";
            deleteBtn.style.background = "transparent";
            deleteBtn.style.border = "none";
            deleteBtn.style.cursor = "pointer";
            deleteBtn.style.color = "#EE4B2B";
            deleteBtn.onmouseenter = () => deleteBtn.style.color = "#B22222"; // Darker red
            deleteBtn.onmouseleave = () => deleteBtn.style.color = "#EE4B2B";

            deleteBtn.onclick = async () => {
                await connection.invoke("DeleteMessage", groupName, message.id);
            };

            msgContainer.appendChild(deleteBtn);
        }

        li.appendChild(msgContainer);
        messagesList.appendChild(li);
        scrollToBottom();
    });

    connection.on("MessageDeleted", (messageId) => {
        const msgElement = document.getElementById(`message-${messageId}`);
        if (msgElement) {
            msgElement.style.transition = "opacity 1s";
            msgElement.style.opacity = "0.3";

            setTimeout(() => {
                msgElement.remove();
            }, 1000);
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
                const msgContainer = msgElement.querySelector(".message");
                if (msgContainer) {
                    const divs = msgContainer.querySelectorAll("div");
                    // replyBox should be second div child
                    const replyBox = divs[1];

                    if (replyBox && replyBox.style && replyBox.style.borderLeft === "4px solid rgb(66, 133, 244)") {
                        replyBox.remove();
                    }
                }
            }
        });
    });

    connection.on("ChatHistory", (messages) => {
        messages.reverse().forEach(msg => {
            const li = document.createElement("li");
            const isOwn = msg.senderUsername === username;

            const msgContainer = document.createElement("div");
            msgContainer.className = `message ${isOwn ? 'own' : 'other'}`;

            // Meta info (username and time)
            const meta = document.createElement("div");
            meta.style.fontSize = "12px";
            meta.style.color = "#555";
            meta.style.marginBottom = "4px";
            meta.textContent = `${msg.senderUsername} • ${new Date(msg.sentAt).toLocaleTimeString()}`;
            msgContainer.appendChild(meta);

            // If this message is a reply, show the replied message context
            if (msg.replyToContent && !msg.replyToDeleted) {
                const replyBox = document.createElement("div");
                replyBox.style.fontSize = "13px";
                replyBox.style.color = "#444";
                replyBox.style.background = "#e8f0fe";
                replyBox.style.borderLeft = "4px solid #4285f4";
                replyBox.style.padding = "8px 12px";
                replyBox.style.marginBottom = "6px";
                replyBox.style.borderRadius = "6px";
                replyBox.style.cursor = "pointer";
                replyBox.style.userSelect = "none";
                replyBox.style.maxWidth = "80%";

                // Removed nowrap, overflow, and textOverflow since we handle trimming manually
                replyBox.title = `Replying to ${msg.replyToSenderUsername}: ${msg.replyToContent}`;

                const replyUsername = document.createElement("span");
                replyUsername.style.fontWeight = "600";
                replyUsername.style.color = "#1a73e8";
                replyUsername.textContent = msg.replyToSenderUsername;

                // Manually trim content to max 20 characters
                let trimmedContent = msg.replyToContent;
                if (trimmedContent.length > 20) {
                    trimmedContent = trimmedContent.substring(0, 20) + "...";
                }

                const replyContent = document.createElement("span");
                replyContent.style.fontStyle = "italic";
                replyContent.style.marginLeft = "6px";
                replyContent.textContent = trimmedContent;

                replyBox.appendChild(replyUsername);
                replyBox.appendChild(document.createTextNode(" "));
                replyBox.appendChild(replyContent);

                replyBox.onclick = () => {
                    console.log(`Clicked reply preview for message ID: ${msg.replyToMessageId}`);
                };

                msgContainer.appendChild(replyBox);
            }

            // Message content
            const content = document.createElement("div");
            content.textContent = msg.content;
            msgContainer.appendChild(content);

            // Add Reply button to enable replying to this message from history
            const replyBtn = document.createElement("button");
            replyBtn.textContent = "Reply";
            replyBtn.style.fontSize = "11px";
            replyBtn.style.marginTop = "4px";
            replyBtn.style.background = "transparent";
            replyBtn.style.border = "none";
            replyBtn.style.cursor = "pointer";
            replyBtn.style.color = isOwn ? "#555" : "#007bff";
            replyBtn.onclick = () => {
                setReplyContext(msg);
            };
            msgContainer.appendChild(replyBtn);

            // ✅ Delete button (only for own messages)
            if (isOwn) {
                const deleteBtn = document.createElement("button");
                deleteBtn.textContent = "Delete";
                deleteBtn.style.fontSize = "11px";
                deleteBtn.style.marginLeft = "10px";
                deleteBtn.style.background = "transparent";
                deleteBtn.style.border = "none";
                deleteBtn.style.cursor = "EE4B2B";
                deleteBtn.style.color = "#FF3131";
                deleteBtn.onmouseenter = () => deleteBtn.style.color = "#B22222"; // Darker red
                deleteBtn.onmouseleave = () => deleteBtn.style.color = "#EE4B2B";

                deleteBtn.onclick = async () => {
                    await connection.invoke("DeleteMessage", groupName, message.id);
                };

                msgContainer.appendChild(deleteBtn);
            }

            li.appendChild(msgContainer);
            messagesList.appendChild(li);
        });

        chatHistoryLoaded = true;

        deferredSystemMessages.forEach(msg => {
            messagesList.appendChild(msg);
        });
        scrollToBottom();

        deferredSystemMessages.length = 0;
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

 
