// Configuration
const API_BASE_URL = 'http://localhost:5000/api/chat';
const AUTH_API_URL = 'http://localhost:5000/api/auth';

// State
let sessionId = '';
let isLoading = false;
let currentUser = null;
let authToken = null;

// ============================================
// AUTHENTICATION
// ============================================

async function handleLogin(event) {
    event.preventDefault();
    
    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;
    
    try {
        const response = await fetch(`${AUTH_API_URL}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        
        const data = await response.json();
        
        if (data.success) {
            currentUser = data.username;
            authToken = data.token;
            
            // Store in session
            sessionStorage.setItem('authToken', authToken);
            sessionStorage.setItem('username', currentUser);
            
            // Show main app
            document.getElementById('login-screen').style.display = 'none';
            document.getElementById('main-app').style.display = 'block';
            
            initializeApp();
        } else {
            showLoginError(data.message);
        }
    } catch (error) {
        console.error('Login error:', error);
        showLoginError('Login failed. Make sure the server is running.');
    }
}

function handleLogout() {
    currentUser = null;
    authToken = null;
    sessionStorage.removeItem('authToken');
    sessionStorage.removeItem('username');
    
    document.getElementById('login-screen').style.display = 'flex';
    document.getElementById('main-app').style.display = 'none';
    
    // Clear login form
    document.getElementById('login-username').value = '';
    document.getElementById('login-password').value = '';
}

function showLoginError(message) {
    const errorDiv = document.getElementById('login-error');
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 3000);
}

// Initialize app
document.addEventListener('DOMContentLoaded', function() {
    // Check if already logged in
    const storedToken = sessionStorage.getItem('authToken');
    const storedUsername = sessionStorage.getItem('username');
    
    if (storedToken && storedUsername) {
        authToken = storedToken;
        currentUser = storedUsername;
        document.getElementById('login-screen').style.display = 'none';
        document.getElementById('main-app').style.display = 'block';
        initializeApp();
    } else {
        document.getElementById('login-screen').style.display = 'flex';
    }
});

function initializeApp() {
    // Generate session ID
    sessionId = generateUUID();
    document.getElementById('session-id').textContent = sessionId.substring(0, 8) + '...';
    
    // Add welcome message
    addMessage('bot', `Hello ${currentUser}! I'm your RAG chatbot assistant. Upload documents or video transcripts to get started, then ask me questions about them.`);
}

// Helper: Generate UUID
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Tab switching
function showTab(tabName) {
    // Remove active class from all tabs
    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.classList.remove('active');
    });
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    
    // Add active class to selected tab
    document.getElementById('btn-' + tabName).classList.add('active');
    document.getElementById('tab-' + tabName).classList.add('active');
    
    // Load documents list when switching to manage tab
    if (tabName === 'manage') {
        refreshDocumentList();
    }
}

// Chat functionality
async function sendMessage(event) {
    event.preventDefault();
    
    const input = document.getElementById('message-input');
    const message = input.value.trim();
    
    if (!message || isLoading) return;
    
    // Check for exit command
    if (message.toLowerCase() === 'exit') {
        addMessage('bot', 'Thank you for using RAG Chatbot! Refresh the page to start a new session.');
        input.value = '';
        return;
    }
    
    // Add user message
    addMessage('user', message);
    input.value = '';
    
    // Show typing indicator
    showTypingIndicator();
    isLoading = true;
    
    try {
        const response = await fetch(`${API_BASE_URL}/message`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                sessionId: sessionId,
                message: message,
                topK: 5
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to get response from server');
        }
        
        const data = await response.json();
        hideTypingIndicator();
        
        // Add bot response with sources
        addMessage('bot', data.response, data.sources);
        
    } catch (error) {
        console.error('Error:', error);
        hideTypingIndicator();
        addMessage('bot', 'Sorry, I encountered an error processing your message. Please make sure the backend is running and try again.');
    } finally {
        isLoading = false;
    }
}

function addMessage(type, content, sources = null) {
    const container = document.getElementById('messages-container');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${type}`;
    
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    
    const textP = document.createElement('p');
    textP.textContent = content;
    contentDiv.appendChild(textP);
    
    messageDiv.appendChild(contentDiv);
    
    // Add voice button for bot messages
    if (type === 'bot') {
        const voiceBtn = document.createElement('button');
        voiceBtn.className = 'response-voice-btn';
        voiceBtn.innerHTML = '🔊';
        voiceBtn.title = 'Read this message';
        voiceBtn.onclick = () => speakResponse(content);
        messageDiv.appendChild(voiceBtn);
    }
    
    // Add sources if available
    if (sources && sources.length > 0) {
        const sourcesDiv = document.createElement('div');
        sourcesDiv.className = 'sources';
        
        const sourcesTitle = document.createElement('p');
        sourcesTitle.className = 'sources-title';
        sourcesTitle.textContent = 'ðŸ“š Sources:';
        sourcesDiv.appendChild(sourcesTitle);
        
        sources.forEach(source => {
            const sourceItem = document.createElement('div');
            sourceItem.className = 'source-item';
            
            const sourceHeader = document.createElement('p');
            sourceHeader.innerHTML = `<strong>${source.documentName}</strong> - Chunk ${source.chunkIndex + 1}`;
            sourceItem.appendChild(sourceHeader);
            
            const sourceText = document.createElement('p');
            sourceText.className = 'source-text';
            sourceText.textContent = source.text;
            sourceItem.appendChild(sourceText);
            
            sourcesDiv.appendChild(sourceItem);
        });
        
        messageDiv.appendChild(sourcesDiv);
    }
    
    // Add timestamp
    const timeSpan = document.createElement('span');
    timeSpan.className = 'message-time';
    timeSpan.textContent = new Date().toLocaleTimeString();
    messageDiv.appendChild(timeSpan);
    
    container.appendChild(messageDiv);
    container.scrollTop = container.scrollHeight;
}

function showTypingIndicator() {
    const container = document.getElementById('messages-container');
    const indicatorDiv = document.createElement('div');
    indicatorDiv.className = 'message bot';
    indicatorDiv.id = 'typing-indicator';
    
    const typingDiv = document.createElement('div');
    typingDiv.className = 'typing-indicator';
    typingDiv.innerHTML = '<span></span><span></span><span></span>';
    
    indicatorDiv.appendChild(typingDiv);
    container.appendChild(indicatorDiv);
    container.scrollTop = container.scrollHeight;
}

function hideTypingIndicator() {
    const indicator = document.getElementById('typing-indicator');
    if (indicator) {
        indicator.remove();
    }
}

// Document upload
async function uploadDocument(event) {
    event.preventDefault();
    
    const topic = document.getElementById('doc-topic').value.trim();
    const fileInput = document.getElementById('doc-file');
    const file = fileInput.files[0];
    const submitBtn = document.getElementById('doc-submit');
    const resultDiv = document.getElementById('doc-result');
    
    if (!topic || !file) {
        alert('Please enter a topic and select a file');
        return;
    }
    
    submitBtn.disabled = true;
    submitBtn.textContent = 'Processing...';
    resultDiv.style.display = 'none';
    
    try {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('topic', topic);
        
        const response = await fetch(`${API_BASE_URL}/upload`, {
            method: 'POST',
            body: formData
        });
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            showResult('doc-result', 'success', 
                `${data.message}\nCreated ${data.totalChunks} chunks for indexing`);
            
            // Reset form
            document.getElementById('doc-topic').value = '';
            document.getElementById('doc-file').value = '';
            document.getElementById('doc-file-name').textContent = '';
            
            // Switch to chat after 2 seconds
            setTimeout(() => showTab('chat'), 2000);
        } else {
            showResult('doc-result', 'error', data.message || 'Error uploading document');
        }
        
    } catch (error) {
        console.error('Error:', error);
        showResult('doc-result', 'error', 'Failed to upload document. Make sure the backend is running.');
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = 'Upload & Process';
    }
}

// Video upload
async function uploadVideo(event) {
    event.preventDefault();
    
    const topic = document.getElementById('video-topic').value.trim();
    const videoUrl = document.getElementById('video-url').value.trim();
    const submitBtn = document.getElementById('video-submit');
    const resultDiv = document.getElementById('video-result');
    
    if (!topic || !videoUrl) {
        alert('Please enter a topic and video URL');
        return;
    }
    
    if (!videoUrl.includes('youtube.com') && !videoUrl.includes('youtu.be')) {
        alert('Please enter a valid YouTube URL');
        return;
    }
    
    submitBtn.disabled = true;
    submitBtn.textContent = 'Processing Transcript...';
    resultDiv.style.display = 'none';
    
    try {
        const response = await fetch(`${API_BASE_URL}/upload-video`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                videoUrl: videoUrl,
                topic: topic
            })
        });
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            showResult('video-result', 'success', 
                `${data.message}\nCreated ${data.totalChunks} chunks from transcript`);
            
            // Reset form
            document.getElementById('video-topic').value = '';
            document.getElementById('video-url').value = '';
            
            // Switch to chat after 2 seconds
            setTimeout(() => showTab('chat'), 2000);
        } else {
            showResult('video-result', 'error', data.message || 'Error processing video');
        }
        
    } catch (error) {
        console.error('Error:', error);
        showResult('video-result', 'error', 'Failed to process video. Make sure the backend is running.');
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = 'Extract & Process';
    }
}

// Helper functions
function updateFileName(type) {
    const fileInput = document.getElementById(`${type}-file`);
    const fileNameDisplay = document.getElementById(`${type}-file-name`);
    
    if (fileInput.files.length > 0) {
        fileNameDisplay.textContent = `Selected: ${fileInput.files[0].name}`;
    } else {
        fileNameDisplay.textContent = '';
    }
}

function showResult(elementId, type, message) {
    const resultDiv = document.getElementById(elementId);
    resultDiv.className = `upload-result ${type}`;
    resultDiv.textContent = message;
    resultDiv.style.display = 'block';
}


// ============================================
// VOICE CONTROL FEATURES
// ============================================

let recognition = null;
let synthesis = window.speechSynthesis;
let isListening = false;
let voiceOutputEnabled = false;
let isSpeaking = false;

// Initialize Speech Recognition
let currentTranscript = ''; // Track accumulated transcript

function initializeSpeechRecognition() {
    if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        recognition = new SpeechRecognition();
        recognition.continuous = false; // Changed to false to capture complete phrases
        recognition.interimResults = true;
        recognition.lang = 'en-US';
        recognition.maxAlternatives = 1;

        recognition.onstart = () => {
            isListening = true;
            // Don't reset currentTranscript - preserve existing text
            document.getElementById('voice-input-btn').classList.add('listening');
            document.getElementById('voice-input-btn').innerHTML = '';
            console.log('Voice recognition started');
        };

        recognition.onresult = (event) => {
            let interimTranscript = '';
            let finalTranscript = '';
            
            // Process all results from the beginning to avoid duplicates
            for (let i = 0; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;
                if (event.results[i].isFinal) {
                    finalTranscript += transcript + ' ';
                } else {
                    interimTranscript += transcript;
                }
            }
            
            const input = document.getElementById('message-input');
            const previousText = currentTranscript ? currentTranscript + ' ' : '';
            
            if (finalTranscript) {
                // Append to existing text instead of replacing
                currentTranscript = previousText + finalTranscript.trim();
                input.value = currentTranscript;
                console.log('Final transcript:', currentTranscript);
            } else if (interimTranscript) {
                // Show interim results with previous text
                input.value = previousText + interimTranscript.trim();
            }
        };

        recognition.onerror = (event) => {
            console.error('Speech recognition error:', event.error);
            stopListening();
            if (event.error === 'no-speech') {
                showToast('No speech detected. Click mic to try again.', 'info');
            } else if (event.error === 'not-allowed') {
                showToast('Microphone access denied. Please enable microphone permissions.', 'error');
            } else if (event.error !== 'aborted') {
                showToast(`Voice error: ${event.error}`, 'error');
            }
        };

        recognition.onend = () => {
            // Auto-restart if still listening (for continuous capture)
            if (isListening) {
                try {
                    setTimeout(() => {
                        if (isListening) {
                            recognition.start();
                            console.log('Voice recognition auto-restarted');
                        }
                    }, 100);
                } catch (e) {
                    console.log('Could not restart recognition:', e);
                    stopListening();
                }
            } else {
                stopListening();
            }
        };

        return true;
    } else {
        console.warn('Speech recognition not supported');
        return false;
    }
}

// Toggle Voice Input
function toggleVoiceInput() {
    if (!recognition) {
        if (!initializeSpeechRecognition()) {
            showToast('Voice input not supported in this browser. Please use Chrome or Edge.', 'error');
            return;
        }
    }

    if (isListening) {
        recognition.stop();
        showToast('Voice input stopped', 'info');
    } else {
        // Preserve existing text in input
        const input = document.getElementById('message-input');
        currentTranscript = input.value.trim(); // Save existing text
        
        recognition.start();
        showToast('Listening... Speak to add more text.', 'info');
    }
}

// Stop Listening
function stopListening() {
    isListening = false;
    document.getElementById('voice-input-btn').classList.remove('listening');
    document.getElementById('voice-input-btn').innerHTML = '';
}

// Toggle Voice Output
function toggleVoiceOutput() {
    voiceOutputEnabled = !voiceOutputEnabled;
    const btn = document.getElementById('voice-output-btn');
    
    if (voiceOutputEnabled) {
        btn.innerHTML = '';
        btn.title = 'Voice Output: ON';
        showToast('Voice output enabled', 'success');
    } else {
        btn.innerHTML = '';
        btn.title = 'Voice Output: OFF';
        synthesis.cancel(); // Stop any ongoing speech
        stopSpeaking();
        showToast('Voice output disabled', 'info');
    }
}

// Speak Text
function speakText(text) {
    if (!voiceOutputEnabled) return;
    
    // Stop any ongoing speech
    synthesis.cancel();
    
    // Remove [MOCK RESPONSE - ...] prefix for cleaner speech
    let cleanText = text.replace(/\[MOCK RESPONSE[^\]]*\]/g, '');
    cleanText = cleanText.replace(/Note: This is a mock response\.[^\n]*/g, '');
    cleanText = cleanText.trim();
    
    if (cleanText.length > 0) {
        const utterance = new SpeechSynthesisUtterance(cleanText);
        utterance.rate = 1.0;
        utterance.pitch = 1.0;
        utterance.volume = 1.0;
        
        utterance.onstart = () => {
            isSpeaking = true;
            document.getElementById('voice-output-btn').classList.add('speaking');
        };
        
        utterance.onend = () => {
            stopSpeaking();
        };
        
        utterance.onerror = (event) => {
            console.error('Speech synthesis error:', event);
            stopSpeaking();
        };
        
        synthesis.speak(utterance);
    }
}

// Detect Language (English or Tamil)
function detectLanguage(text) {
    // Tamil Unicode range: U+0B80 to U+0BFF
    const tamilPattern = /[\u0B80-\u0BFF]/;
    if (tamilPattern.test(text)) {
        return 'ta-IN'; // Tamil
    }
    return 'en-US'; // English (default)
}

// Speak Response with Language Detection
function speakResponse(text) {
    // Stop any ongoing speech
    synthesis.cancel();
    
    // Clean text
    let cleanText = text.replace(/\[MOCK RESPONSE[^\]]*\]/g, '');
    cleanText = cleanText.replace(/Note: This is a mock response\.[^\n]*/g, '');
    cleanText = cleanText.trim();
    
    if (cleanText.length === 0) return;
    
    // Detect language
    const language = detectLanguage(cleanText);
    
    console.log(`Speaking in ${language}:`, cleanText);
    
    const utterance = new SpeechSynthesisUtterance(cleanText);
    utterance.lang = language;
    utterance.rate = 1.0;
    utterance.pitch = 1.0;
    utterance.volume = 1.0;
    
    // Try to select appropriate voice
    const voices = synthesis.getVoices();
    const preferredVoice = voices.find(voice => voice.lang.startsWith(language.split('-')[0]));
    if (preferredVoice) {
        utterance.voice = preferredVoice;
        console.log('Using voice:', preferredVoice.name);
    }
    
    utterance.onstart = () => {
        console.log('Speech started');
    };
    
    utterance.onend = () => {
        console.log('Speech ended');
    };
    
    utterance.onerror = (event) => {
        console.error('Speech error:', event);
        showToast(`Voice error: ${event.error}`, 'error');
    };
    
    synthesis.speak(utterance);
    showToast(`Reading in ${language === 'ta-IN' ? 'Tamil' : 'English'}...`, 'info');
}

// Stop Speaking
function stopSpeaking() {
    isSpeaking = false;
    document.getElementById('voice-output-btn').classList.remove('speaking');
}

// Show Toast Notification
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        background: ${type === 'success' ? '#4CAF50' : type === 'error' ? '#f44336' : '#2196F3'};
        color: white;
        padding: 12px 24px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        z-index: 10000;
        animation: slideIn 0.3s ease;
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Add CSS animations for toast
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(400px); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(400px); opacity: 0; }
    }
`;
document.head.appendChild(style);

// Initialize on page load
window.addEventListener('DOMContentLoaded', () => {
    initializeSpeechRecognition();
});
// Auto-speak responses when voice output is enabled
document.addEventListener('DOMContentLoaded', function() {
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            mutation.addedNodes.forEach(function(node) {
                if (node.classList && node.classList.contains('message') && node.classList.contains('bot-message')) {
                    const text = node.textContent;
                    if (text && voiceOutputEnabled) {
                        setTimeout(() => speakText(text), 100);
                    }
                }
            });
        });
    });
    
    const messagesContainer = document.getElementById('messages-container');
    if (messagesContainer) {
        observer.observe(messagesContainer, { childList: true, subtree: true });
    }
});

// ============================================
// DOCUMENT MANAGEMENT
// ============================================

async function refreshDocumentList() {
    const listContainer = document.getElementById('documents-list');
    listContainer.innerHTML = '<p class="loading-text">Loading documents...</p>';
    
    try {
        const response = await fetch(`${API_BASE_URL}/documents`);
        
        if (!response.ok) {
            throw new Error('Failed to fetch documents');
        }
        
        const documents = await response.json();
        
        if (documents.length === 0) {
            listContainer.innerHTML = '<p class="empty-text">No documents uploaded yet.</p>';
            return;
        }
        
        // Create document list
        let html = '<div class="document-items">';
        documents.forEach((doc, index) => {
            html += `
                <div class="document-item" data-doc-name="${doc}">
                    <div class="doc-info">
                        <span class="doc-icon">📄</span>
                        <span class="doc-name">${doc}</span>
                    </div>
                    <button class="delete-doc-btn" onclick="deleteDocument('${doc.replace(/'/g, "\\'")}')">
                        🗑️ Delete
                    </button>
                </div>
            `;
        });
        html += '</div>';
        
        listContainer.innerHTML = html;
        
    } catch (error) {
        console.error('Error loading documents:', error);
        listContainer.innerHTML = '<p class="error-text">Error loading documents. Make sure the server is running.</p>';
    }
}

async function deleteDocument(documentName) {
    if (!confirm(`Are you sure you want to delete "${documentName}"?\n\nThis will remove all chunks of this document from the knowledge base.`)) {
        return;
    }
    
    try {
        const encodedName = encodeURIComponent(documentName);
        const response = await fetch(`${API_BASE_URL}/documents/${encodedName}`, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        
        if (result.success) {
            showToast(`Document "${documentName}" deleted successfully`, 'success');
            refreshDocumentList();
        } else {
            showToast(result.message || 'Failed to delete document', 'error');
        }
        
    } catch (error) {
        console.error('Error deleting document:', error);
        showToast('Error deleting document. Make sure the server is running.', 'error');
    }
}

async function confirmDeleteAll() {
    const confirmation = prompt('⚠️ WARNING: This will delete ALL uploaded documents!\n\nType "DELETE ALL" to confirm:');
    
    if (confirmation !== 'DELETE ALL') {
        if (confirmation !== null) {
            showToast('Deletion cancelled. Type "DELETE ALL" exactly to confirm.', 'info');
        }
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/documents`, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        
        if (result.success) {
            showToast('All documents deleted successfully', 'success');
            refreshDocumentList();
        } else {
            showToast(result.message || 'Failed to delete documents', 'error');
        }
        
    } catch (error) {
        console.error('Error deleting all documents:', error);
        showToast('Error deleting documents. Make sure the server is running.', 'error');
    }
}
