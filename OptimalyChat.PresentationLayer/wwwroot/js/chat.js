// Chat application JavaScript
(function () {
    'use strict';

    // SignalR connection
    let connection = null;
    let currentProjectId = window.chatConfig.projectId;
    let currentConversationId = window.chatConfig.conversationId;
    let isStreaming = false;

    // Initialize SignalR connection
    async function initializeSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(window.chatConfig.hubUrl)
            .withAutomaticReconnect()
            .build();

        // Connection event handlers
        connection.onreconnecting(() => {
            console.log('Reconnecting to SignalR...');
            showConnectionStatus('Reconnecting...', 'warning');
        });

        connection.onreconnected(() => {
            console.log('Reconnected to SignalR');
            showConnectionStatus('Connected', 'success');
            // Rejoin rooms
            if (currentProjectId) {
                connection.invoke('JoinProject', currentProjectId);
            }
            if (currentConversationId) {
                connection.invoke('JoinConversation', currentConversationId);
            }
        });

        connection.onclose(() => {
            console.log('Disconnected from SignalR');
            showConnectionStatus('Disconnected', 'danger');
        });

        // Message handlers
        connection.on('JoinedProject', (projectId) => {
            console.log('Joined project:', projectId);
        });

        connection.on('JoinedConversation', (conversationId) => {
            console.log('Joined conversation:', conversationId);
        });

        connection.on('ConversationCreated', (conversation) => {
            console.log('New conversation created:', conversation);
            // Reload page to show new conversation
            window.location.reload();
        });

        connection.on('ConversationTitleUpdated', (conversationId, newTitle) => {
            if (conversationId === currentConversationId) {
                document.getElementById('conversationTitle').textContent = newTitle;
            }
        });

        connection.on('AITyping', (conversationId, isTyping) => {
            if (conversationId === currentConversationId) {
                const indicator = document.getElementById('typingIndicator');
                if (isTyping) {
                    indicator.classList.remove('d-none');
                } else {
                    indicator.classList.add('d-none');
                }
            }
        });

        connection.on('ReceiveMessageChunk', (conversationId, chunk) => {
            if (conversationId === currentConversationId && !isStreaming) {
                // Handle messages from other clients
                appendToLastAIMessage(chunk);
            }
        });

        try {
            await connection.start();
            console.log('Connected to SignalR');
            showConnectionStatus('Connected', 'success');

            // Join current project and conversation
            if (currentProjectId) {
                await connection.invoke('JoinProject', currentProjectId);
            }
            if (currentConversationId) {
                await connection.invoke('JoinConversation', currentConversationId);
            }
        } catch (err) {
            console.error('Failed to connect to SignalR:', err);
            showConnectionStatus('Failed to connect', 'danger');
        }
    }

    // Show connection status
    function showConnectionStatus(message, type) {
        // You can implement a toast notification here
        console.log(`Connection status: ${message} (${type})`);
    }

    // Send message
    async function sendMessage(message) {
        if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
            alert('Not connected to server. Please refresh the page.');
            return;
        }

        if (!currentProjectId || !currentConversationId) {
            alert('Please select a project and conversation first.');
            return;
        }

        isStreaming = true;
        
        // Disable send button
        const sendButton = document.getElementById('sendButton');
        sendButton.disabled = true;

        // Add user message to UI
        addMessageToUI('user', message, new Date());

        // Clear input
        document.getElementById('messageInput').value = '';

        // Show typing indicator
        const indicator = document.getElementById('typingIndicator');
        indicator.classList.remove('d-none');

        // Create AI message container
        const aiMessageId = 'ai-message-' + Date.now();
        addMessageToUI('assistant', '', new Date(), aiMessageId);

        try {
            // Stream the response
            const stream = connection.stream('SendMessage', currentProjectId, currentConversationId, message);
            let fullResponse = '';

            stream.subscribe({
                next: (chunk) => {
                    fullResponse += chunk;
                    updateMessageContent(aiMessageId, fullResponse);
                    scrollToBottom();
                },
                complete: () => {
                    console.log('Stream complete');
                    isStreaming = false;
                    indicator.classList.add('d-none');
                    sendButton.disabled = false;
                },
                error: (err) => {
                    console.error('Stream error:', err);
                    isStreaming = false;
                    indicator.classList.add('d-none');
                    sendButton.disabled = false;
                    updateMessageContent(aiMessageId, 'Error: ' + err.message);
                }
            });
        } catch (err) {
            console.error('Failed to send message:', err);
            isStreaming = false;
            indicator.classList.add('d-none');
            sendButton.disabled = false;
            alert('Failed to send message: ' + err.message);
        }
    }

    // Add message to UI
    function addMessageToUI(role, content, timestamp, messageId) {
        const messagesContainer = document.getElementById('messagesContainer');
        const messageDiv = document.createElement('div');
        messageDiv.className = `message mb-3 ${role === 'user' ? 'text-end' : ''}`;
        if (messageId) {
            messageDiv.id = messageId;
        }

        const messageContent = document.createElement('div');
        messageContent.className = `d-inline-block p-3 rounded ${role === 'user' ? 'bg-primary text-white' : 'bg-light'}`;
        messageContent.style.maxWidth = '70%';

        const header = document.createElement('div');
        header.className = 'mb-1';
        header.innerHTML = `
            <strong>${role === 'user' ? 'You' : 'AI'}</strong>
            <small class="ms-2 ${role === 'user' ? 'text-white-50' : 'text-muted'}">
                ${timestamp.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
            </small>
        `;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        contentDiv.textContent = content;

        messageContent.appendChild(header);
        messageContent.appendChild(contentDiv);
        messageDiv.appendChild(messageContent);

        // Insert before typing indicator
        const typingIndicator = document.getElementById('typingIndicator');
        messagesContainer.insertBefore(messageDiv, typingIndicator);

        scrollToBottom();
    }

    // Update message content
    function updateMessageContent(messageId, content) {
        const messageElement = document.getElementById(messageId);
        if (messageElement) {
            const contentDiv = messageElement.querySelector('.message-content');
            if (contentDiv) {
                contentDiv.textContent = content;
            }
        }
    }

    // Append to last AI message
    function appendToLastAIMessage(chunk) {
        const messages = document.querySelectorAll('.message:not(.text-end)');
        if (messages.length > 0) {
            const lastMessage = messages[messages.length - 1];
            const contentDiv = lastMessage.querySelector('.message-content');
            if (contentDiv) {
                contentDiv.textContent += chunk;
                scrollToBottom();
            }
        }
    }

    // Scroll to bottom of messages
    function scrollToBottom() {
        const messagesContainer = document.getElementById('messagesContainer');
        if (messagesContainer) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    // Create new conversation
    async function createNewConversation() {
        if (!currentProjectId) {
            alert('Please select a project first.');
            return;
        }

        const title = prompt('Enter conversation title:', 'New Conversation');
        if (!title) return;

        try {
            const conversationId = await connection.invoke('CreateConversation', currentProjectId, title);
            // Redirect to new conversation
            window.location.href = `/Chat?projectId=${currentProjectId}&conversationId=${conversationId}`;
        } catch (err) {
            console.error('Failed to create conversation:', err);
            alert('Failed to create conversation: ' + err.message);
        }
    }

    // Update conversation title
    async function updateConversationTitle() {
        if (!currentConversationId) return;

        const currentTitle = document.getElementById('conversationTitle').textContent;
        const newTitle = prompt('Enter new title:', currentTitle);
        if (!newTitle || newTitle === currentTitle) return;

        try {
            await connection.invoke('UpdateConversationTitle', currentConversationId, newTitle);
        } catch (err) {
            console.error('Failed to update title:', err);
            alert('Failed to update title: ' + err.message);
        }
    }

    // Delete conversation
    async function deleteConversation() {
        if (!currentConversationId) return;

        if (!confirm('Are you sure you want to delete this conversation? This cannot be undone.')) {
            return;
        }

        try {
            const response = await fetch(`/Chat/DeleteConversation/${currentConversationId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (response.ok) {
                window.location.href = `/Chat?projectId=${currentProjectId}`;
            } else {
                throw new Error('Failed to delete conversation');
            }
        } catch (err) {
            console.error('Failed to delete conversation:', err);
            alert('Failed to delete conversation: ' + err.message);
        }
    }

    // Initialize event handlers
    function initializeEventHandlers() {
        // Message form
        const messageForm = document.getElementById('messageForm');
        if (messageForm) {
            messageForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                const input = document.getElementById('messageInput');
                const message = input.value.trim();
                if (message) {
                    await sendMessage(message);
                }
            });
        }

        // Keyboard shortcuts
        const messageInput = document.getElementById('messageInput');
        if (messageInput) {
            messageInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    messageForm.dispatchEvent(new Event('submit'));
                }
            });
        }

        // New conversation buttons
        const newConvButtons = document.querySelectorAll('#newConversation, #newConversationCenter');
        newConvButtons.forEach(btn => {
            if (btn) {
                btn.addEventListener('click', createNewConversation);
            }
        });

        // Edit title button
        const editTitleBtn = document.getElementById('editTitle');
        if (editTitleBtn) {
            editTitleBtn.addEventListener('click', updateConversationTitle);
        }

        // Delete conversation button
        const deleteBtn = document.getElementById('deleteConversation');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', deleteConversation);
        }
    }

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', () => {
        initializeSignalR();
        initializeEventHandlers();
        scrollToBottom();
    });
})();