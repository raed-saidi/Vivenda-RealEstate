// Chatbot functionality
(function () {
    const chatbotToggle = document.getElementById('chatbot-toggle');
    const chatbotWindow = document.getElementById('chatbot-window');
    const chatbotClose = document.getElementById('chatbot-close');
    const chatbotMessages = document.getElementById('chatbot-messages');
    const chatbotInputField = document.getElementById('chatbot-input-field');
    const chatbotSend = document.getElementById('chatbot-send');
    const chatbotLoading = document.getElementById('chatbot-loading');

    if (!chatbotToggle || !chatbotWindow) {
        return; // Elements not found, exit
    }

    // Toggle chatbot window
    chatbotToggle.addEventListener('click', () => {
        chatbotWindow.classList.toggle('active');
        if (chatbotWindow.classList.contains('active')) {
            chatbotInputField.focus();
        }
    });

    // Close chatbot window
    chatbotClose.addEventListener('click', () => {
        chatbotWindow.classList.remove('active');
    });

    // Send message on button click
    chatbotSend.addEventListener('click', () => {
        sendMessage();
    });

    // Send message on Enter key
    chatbotInputField.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    // Send message function
    async function sendMessage() {
        const message = chatbotInputField.value.trim();
        if (!message) return;

        // Add user message to chat
        addMessage(message, 'user');
        chatbotInputField.value = '';

        // Show loading
        showLoading(true);

        try {
            // Send to API
            const response = await fetch('/api/chatbot/message', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: message,
                    context: null
                })
            });

            if (!response.ok) {
                throw new Error('Failed to get response');
            }

            const data = await response.json();

            // Hide loading
            showLoading(false);

            // Add bot response
            addMessage(data.response, 'bot');

            // Add property cards if any
            if (data.suggestedProperties && data.suggestedProperties.length > 0) {
                addPropertyCards(data.suggestedProperties);
            }

        } catch (error) {
            showLoading(false);
            addMessage('Sorry, I encountered an error. Please try again.', 'bot');
            console.error('Chatbot error:', error);
        }
    }

    // Add message to chat
    function addMessage(text, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `chatbot-message ${sender}-message`;

        const avatarDiv = document.createElement('div');
        avatarDiv.className = 'message-avatar';
        avatarDiv.innerHTML = sender === 'bot' 
            ? '<i class="fas fa-robot"></i>' 
            : '<i class="fas fa-user"></i>';

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        
        // Convert markdown-style formatting to HTML
        const formattedText = text
            .replace(/\n/g, '<br>')
            .replace(/•/g, '&bull;')
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" class="chat-link" target="_blank">$1</a>');
        
        contentDiv.innerHTML = `<p>${formattedText}</p>`;

        messageDiv.appendChild(avatarDiv);
        messageDiv.appendChild(contentDiv);
        chatbotMessages.appendChild(messageDiv);

        // Scroll to bottom
        chatbotMessages.scrollTop = chatbotMessages.scrollHeight;
    }

    // Add property cards
    function addPropertyCards(properties) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chatbot-message bot-message';

        const avatarDiv = document.createElement('div');
        avatarDiv.className = 'message-avatar';
        avatarDiv.innerHTML = '<i class="fas fa-robot"></i>';

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';

        properties.forEach(property => {
            const card = document.createElement('div');
            card.className = 'property-card-mini';
            card.onclick = () => window.open(`/Properties/Details/${property.id}`, '_blank');

            const imageUrl = property.mainImageUrl || '/images/default-property.jpg';
            const bedBath = `${property.bedrooms} bed • ${property.bathrooms} bath`;
            const sqft = property.squareFeet ? ` • ${property.squareFeet.toLocaleString()} sqft` : '';

            card.innerHTML = `
                <img src="${imageUrl}" alt="${property.title}" onerror="this.src='/images/default-property.jpg'" />
                <h6>${truncateText(property.title, 40)}</h6>
                <div class="price">$${property.price.toLocaleString()}</div>
                <div class="details">
                    <i class="fas fa-map-marker-alt"></i> ${property.city}, ${property.state}<br>
                    <i class="fas fa-home"></i> ${bedBath}${sqft}
                </div>
            `;

            contentDiv.appendChild(card);
        });

        messageDiv.appendChild(avatarDiv);
        messageDiv.appendChild(contentDiv);
        chatbotMessages.appendChild(messageDiv);

        // Scroll to bottom
        chatbotMessages.scrollTop = chatbotMessages.scrollHeight;
    }

    // Show/hide loading indicator
    function showLoading(show) {
        if (show) {
            chatbotLoading.classList.add('active');
            chatbotInputField.disabled = true;
            chatbotSend.disabled = true;
        } else {
            chatbotLoading.classList.remove('active');
            chatbotInputField.disabled = false;
            chatbotSend.disabled = false;
            chatbotInputField.focus();
        }
    }

    // Truncate text
    function truncateText(text, maxLength) {
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }

    // Quick action buttons (optional enhancement)
    function addQuickActions() {
        const quickActions = [
            'Show featured properties',
            'Properties under $300,000',
            'What categories are available?',
            'Help'
        ];

        const actionsDiv = document.createElement('div');
        actionsDiv.className = 'chatbot-quick-actions';
        actionsDiv.style.cssText = 'display: flex; flex-wrap: wrap; gap: 8px; padding: 0 16px 16px;';

        quickActions.forEach(action => {
            const button = document.createElement('button');
            button.className = 'btn btn-sm btn-outline-primary';
            button.textContent = action;
            button.onclick = () => {
                chatbotInputField.value = action;
                sendMessage();
            };
            actionsDiv.appendChild(button);
        });

        chatbotWindow.insertBefore(actionsDiv, chatbotMessages.nextSibling);
    }

})();
