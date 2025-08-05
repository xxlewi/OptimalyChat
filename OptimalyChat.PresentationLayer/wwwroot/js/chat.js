// Chat application JavaScript
(function () {
    'use strict';

    // SignalR connection
    let connection = null;
    let currentProjectId = null;
    let currentConversationId = null;
    let isStreaming = false;

    // Initialize SignalR connection
    async function initializeSignalR() {
        console.log('Initializing SignalR with config:', window.chatConfig);
        
        connection = new signalR.HubConnectionBuilder()
            .withUrl(window.chatConfig.hubUrl)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Debug)
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
            console.log('Connection state:', connection.state);
            console.log('Connection ID:', connection.connectionId);
            showConnectionStatus('Connected', 'success');

            // Join current project and conversation
            if (currentProjectId) {
                console.log('Joining project:', currentProjectId);
                await connection.invoke('JoinProject', currentProjectId);
            }
            if (currentConversationId) {
                console.log('Joining conversation:', currentConversationId);
                await connection.invoke('JoinConversation', currentConversationId);
            }
        } catch (err) {
            console.error('Failed to connect to SignalR:', err);
            console.error('Error details:', err.message, err.stack);
            console.error('Full error object:', err);
            
            // Check if it's an authentication issue
            if (err.message && err.message.includes('401') || err.message.includes('302')) {
                console.error('Authentication required - user needs to log in');
                alert('You need to log in first. Redirecting to login page...');
                window.location.href = '/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
            }
            
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
        console.log('Sending message:', message);
        console.log('Connection state:', connection ? connection.state : 'No connection');
        console.log('Project ID:', currentProjectId, 'Conversation ID:', currentConversationId);
        
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
            // Get selected model
            const modelSelector = document.getElementById('modelSelector');
            const selectedModelId = modelSelector ? parseInt(modelSelector.value) : null;
            
            // Use SignalR streaming
            let fullResponse = '';
            
            console.log('Starting stream with:', { currentProjectId, currentConversationId, message, selectedModelId });
            
            connection.stream('SendMessage', currentProjectId, currentConversationId, message, selectedModelId)
                .subscribe({
                    next: (chunk) => {
                        console.log('Received chunk:', chunk);
                        fullResponse += chunk;
                        updateMessageContent(aiMessageId, fullResponse);
                        scrollToBottom();
                    },
                    complete: () => {
                        console.log('Stream complete, full response:', fullResponse);
                        isStreaming = false;
                        indicator.classList.add('d-none');
                        sendButton.disabled = false;
                    },
                    error: (err) => {
                        console.error('Stream error:', err);
                        console.error('Error type:', err.constructor.name);
                        console.error('Error message:', err.message);
                        console.error('Error stack:', err.stack);
                        isStreaming = false;
                        indicator.classList.add('d-none');
                        sendButton.disabled = false;
                        updateMessageContent(aiMessageId, 'Error: ' + (err.message || err.toString()));
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
        messageDiv.className = `direct-chat-msg ${role === 'user' ? 'right' : ''}`;
        if (messageId) {
            messageDiv.id = messageId;
        }

        const infosDiv = document.createElement('div');
        infosDiv.className = 'direct-chat-infos clearfix';
        infosDiv.innerHTML = `
            <span class="direct-chat-name ${role === 'user' ? 'float-right' : 'float-left'}">
                ${role === 'user' ? 'You' : 'AI Assistant'}
            </span>
            <span class="direct-chat-timestamp ${role === 'user' ? 'float-left' : 'float-right'}">
                ${timestamp.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
            </span>
        `;

        const img = document.createElement('img');
        img.className = 'direct-chat-img';
        img.src = role === 'user' 
            ? 'https://ui-avatars.com/api/?name=You&background=007bff&color=fff'
            : 'https://ui-avatars.com/api/?name=AI&background=17a2b8&color=fff';
        img.alt = role === 'user' ? 'You' : 'AI';

        const textDiv = document.createElement('div');
        textDiv.className = 'direct-chat-text';
        const contentSpan = document.createElement('span');
        contentSpan.className = 'message-content';
        contentSpan.textContent = content;
        textDiv.appendChild(contentSpan);

        messageDiv.appendChild(infosDiv);
        messageDiv.appendChild(img);
        messageDiv.appendChild(textDiv);

        // Insert before typing indicator
        const typingIndicator = document.getElementById('typingIndicator');
        messagesContainer.insertBefore(messageDiv, typingIndicator);

        scrollToBottom();
    }

    // Update message content
    function updateMessageContent(messageId, content) {
        const messageElement = document.getElementById(messageId);
        if (messageElement) {
            const contentSpan = messageElement.querySelector('.message-content');
            if (contentSpan) {
                contentSpan.textContent = content;
            }
        }
    }

    // Append to last AI message
    function appendToLastAIMessage(chunk) {
        const messages = document.querySelectorAll('.direct-chat-msg:not(.right)');
        if (messages.length > 0) {
            const lastMessage = messages[messages.length - 1];
            const contentSpan = lastMessage.querySelector('.message-content');
            if (contentSpan) {
                contentSpan.textContent += chunk;
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
                method: 'DELETE'
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
        // Initialize configuration
        if (window.chatConfig) {
            currentProjectId = window.chatConfig.projectId;
            currentConversationId = window.chatConfig.conversationId;
            console.log('Chat configuration loaded:', window.chatConfig);
        } else {
            console.error('Chat configuration not found!');
            return;
        }
        
        initializeSignalR();
        initializeEventHandlers();
        scrollToBottom();
    });
})();