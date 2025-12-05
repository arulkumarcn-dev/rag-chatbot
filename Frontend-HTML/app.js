// Configuration
const API_BASE_URL = 'http://localhost:5000/api/chat';
const AUTH_API_URL = 'http://localhost:5000/api/auth';

// State
let sessionId = '';
let isLoading = false;
let currentUser = null;
let authToken = null;
let preferredInputLanguage = 'en-US'; // Default voice input language
let autoDetectLanguage = true; // Auto-detect language from content

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
    
    // Set language selector to stored preference
    const langSelector = document.getElementById('voice-language');
    if (langSelector) {
        langSelector.value = preferredInputLanguage;
    }
    
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
        recognition.lang = getPreferredInputLanguage(); // Dynamic language selection
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

// Detect Language (Multiple Indian Languages + English)
function detectLanguage(text) {
    // Check for various Indian language scripts
    const tamilPattern = /[\u0B80-\u0BFF]/; // Tamil
    const hindiPattern = /[\u0900-\u097F]/; // Devanagari (Hindi)
    const teluguPattern = /[\u0C00-\u0C7F]/; // Telugu
    const malayalamPattern = /[\u0D00-\u0D7F]/; // Malayalam
    const kannadaPattern = /[\u0C80-\u0CFF]/; // Kannada
    const bengaliPattern = /[\u0980-\u09FF]/; // Bengali
    const gujaratiPattern = /[\u0A80-\u0AFF]/; // Gujarati
    const punjabiPattern = /[\u0A00-\u0A7F]/; // Punjabi (Gurmukhi)
    
    // Check each language in order of likelihood
    if (tamilPattern.test(text)) {
        return 'ta-IN'; // Tamil
    }
    if (hindiPattern.test(text)) {
        return 'hi-IN'; // Hindi
    }
    if (teluguPattern.test(text)) {
        return 'te-IN'; // Telugu
    }
    if (malayalamPattern.test(text)) {
        return 'ml-IN'; // Malayalam
    }
    if (kannadaPattern.test(text)) {
        return 'kn-IN'; // Kannada
    }
    if (bengaliPattern.test(text)) {
        return 'bn-IN'; // Bengali
    }
    if (gujaratiPattern.test(text)) {
        return 'gu-IN'; // Gujarati
    }
    if (punjabiPattern.test(text)) {
        return 'pa-IN'; // Punjabi
    }
    return 'en-US'; // English (default)
}

// Get language name for display
function getLanguageName(langCode) {
    const languageNames = {
        'ta-IN': 'Tamil',
        'hi-IN': 'Hindi',
        'te-IN': 'Telugu',
        'ml-IN': 'Malayalam',
        'kn-IN': 'Kannada',
        'bn-IN': 'Bengali',
        'gu-IN': 'Gujarati',
        'pa-IN': 'Punjabi',
        'en-US': 'English',
        'en-GB': 'English'
    };
    return languageNames[langCode] || 'English';
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
    const languageName = getLanguageName(language);
    
    console.log(`Speaking in ${languageName} (${language}):`, cleanText);
    
    const utterance = new SpeechSynthesisUtterance(cleanText);
    utterance.lang = language;
    utterance.rate = 0.9; // Slightly slower for clarity
    utterance.pitch = 1.0;
    utterance.volume = 1.0;
    
    // Wait for voices to load, then select appropriate voice
    function selectVoice() {
        const voices = synthesis.getVoices();
        console.log('Available voices:', voices.length);
        
        // Try to find exact language match first
        let preferredVoice = voices.find(voice => voice.lang === language);
        
        // If not found, try partial match (e.g., 'ta' in 'ta-IN')
        if (!preferredVoice) {
            const langPrefix = language.split('-')[0];
            preferredVoice = voices.find(voice => voice.lang.startsWith(langPrefix));
        }
        
        // For Tamil and other Indian languages, prefer Google voices if available
        if (!preferredVoice && language !== 'en-US') {
            preferredVoice = voices.find(voice => 
                voice.lang.includes(language.split('-')[0]) && 
                voice.name.includes('Google')
            );
        }
        
        if (preferredVoice) {
            utterance.voice = preferredVoice;
            console.log('Using voice:', preferredVoice.name, preferredVoice.lang);
        } else {
            console.log('No specific voice found, using default');
        }
    }
    
    // Load voices if not already loaded
    if (synthesis.getVoices().length === 0) {
        synthesis.addEventListener('voiceschanged', selectVoice, { once: true });
    } else {
        selectVoice();
    }
    
    utterance.onstart = () => {
        console.log('Speech started');
        isSpeaking = true;
    };
    
    utterance.onend = () => {
        console.log('Speech ended');
        isSpeaking = false;
    };
    
    utterance.onerror = (event) => {
        console.error('Speech error:', event);
        isSpeaking = false;
        if (event.error === 'not-allowed') {
            showToast('Speech blocked. Please check browser permissions.', 'error');
        } else if (event.error === 'language-not-supported') {
            showToast(`${languageName} voice not available. Install language pack.`, 'warning');
        } else {
            showToast(`Voice error: ${event.error}`, 'error');
        }
    };
    
    synthesis.speak(utterance);
    showToast(`🔊 Reading in ${languageName}...`, 'info');
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

// Get preferred input language
function getPreferredInputLanguage() {\n    return preferredInputLanguage;\n}\n\n// Set preferred input language\nfunction setInputLanguage(langCode) {\n    preferredInputLanguage = langCode;\n    localStorage.setItem('preferredInputLanguage', langCode);\n    if (recognition) {\n        recognition.lang = langCode;\n    }\n    showToast(`Voice input set to ${getLanguageName(langCode)}`, 'info');\n}\n\n// Load language preferences from storage\nfunction loadLanguagePreferences() {\n    const savedLang = localStorage.getItem('preferredInputLanguage');\n    if (savedLang) {\n        preferredInputLanguage = savedLang;\n    }\n    const savedAutoDetect = localStorage.getItem('autoDetectLanguage');\n    if (savedAutoDetect !== null) {\n        autoDetectLanguage = savedAutoDetect === 'true';\n    }\n}\n\n// Initialize on page load\nwindow.addEventListener('DOMContentLoaded', () => {\n    loadLanguagePreferences();\n    initializeSpeechRecognition();\n});
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

// ============================================
// QUIZ FUNCTIONALITY
// ============================================

let currentQuiz = null;
let currentQuestionIndex = 0;
let userAnswers = [];
let quizResults = [];

// Load documents into quiz topic selector
async function loadQuizDocuments() {
    try {
        const response = await fetch(`${API_BASE_URL}/documents`);
        if (!response.ok) throw new Error('Failed to fetch documents');
        
        const documents = await response.json();
        const select = document.getElementById('quiz-topic');
        
        if (documents.length === 0) {
            select.innerHTML = '<option value="">No documents available</option>';
            return;
        }
        
        select.innerHTML = '<option value="">All Documents</option>' +
            documents.map(doc => `<option value="${doc}">${doc}</option>`).join('');
    } catch (error) {
        console.error('Error loading documents:', error);
        document.getElementById('quiz-topic').innerHTML = '<option value="">Error loading documents</option>';
    }
}

// Generate quiz
async function generateQuiz() {
    const topic = document.getElementById('quiz-topic').value;
    const count = parseInt(document.getElementById('quiz-count').value);
    const difficulty = document.getElementById('quiz-difficulty').value;
    
    if (!topic && document.getElementById('quiz-topic').options.length <= 1) {
        showToast('Please upload documents first before generating a quiz', 'error');
        return;
    }
    
    const btn = document.getElementById('generate-quiz-btn');
    btn.disabled = true;
    btn.textContent = '⏳ Generating Quiz...';
    
    try {
        const response = await fetch(`${API_BASE_URL}/generate-quiz`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ topic: topic || 'All Documents', questionCount: count })
        });
        
        const data = await response.json();
        
        if (!data.success) {
            throw new Error(data.message || 'Failed to generate quiz');
        }
        
        currentQuiz = data.quiz;
        currentQuestionIndex = 0;
        userAnswers = new Array(currentQuiz.questions.length).fill(null);
        
        // Hide generator, show quiz
        document.getElementById('quiz-generator').style.display = 'none';
        document.getElementById('quiz-container').style.display = 'block';
        document.getElementById('quiz-results-display').style.display = 'none';
        
        // Set quiz title
        document.getElementById('quiz-title').textContent = `Quiz: ${currentQuiz.topic}`;
        
        // Render all questions
        renderQuizQuestions();
        showQuestion(0);
        updateProgress();
        
        showToast('Quiz generated successfully!', 'success');
        
    } catch (error) {
        console.error('Error generating quiz:', error);
        showToast(error.message || 'Failed to generate quiz. Please try again.', 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = '🎯 Generate Quiz';
    }
}

// Render all quiz questions
function renderQuizQuestions() {
    const container = document.getElementById('quiz-questions');
    container.innerHTML = '';
    
    currentQuiz.questions.forEach((q, index) => {
        const questionDiv = document.createElement('div');
        questionDiv.className = 'quiz-question';
        questionDiv.id = `question-${index}`;
        
        const questionText = document.createElement('div');
        questionText.className = 'question-text';
        questionText.textContent = `${index + 1}. ${q.question}`;
        questionDiv.appendChild(questionText);
        
        const optionsDiv = document.createElement('div');
        optionsDiv.className = 'quiz-options';
        
        q.options.forEach((option, optIndex) => {
            const optionDiv = document.createElement('div');
            optionDiv.className = 'quiz-option';
            optionDiv.onclick = () => selectAnswer(index, optIndex);
            
            const radio = document.createElement('input');
            radio.type = 'radio';
            radio.name = `question-${index}`;
            radio.id = `q${index}-opt${optIndex}`;
            radio.value = optIndex;
            
            const label = document.createElement('label');
            label.htmlFor = `q${index}-opt${optIndex}`;
            label.textContent = option;
            
            optionDiv.appendChild(radio);
            optionDiv.appendChild(label);
            optionsDiv.appendChild(optionDiv);
        });
        
        questionDiv.appendChild(optionsDiv);
        
        const feedbackDiv = document.createElement('div');
        feedbackDiv.className = 'answer-feedback';
        feedbackDiv.id = `feedback-${index}`;
        feedbackDiv.style.display = 'none';
        questionDiv.appendChild(feedbackDiv);
        
        container.appendChild(questionDiv);
    });
}

// Select answer
function selectAnswer(questionIndex, optionIndex) {
    userAnswers[questionIndex] = optionIndex;
    
    // Update radio button
    const radio = document.getElementById(`q${questionIndex}-opt${optionIndex}`);
    if (radio) radio.checked = true;
    
    // Show feedback
    const question = currentQuiz.questions[questionIndex];
    const feedbackDiv = document.getElementById(`feedback-${questionIndex}`);
    const isCorrect = optionIndex === question.correctAnswerIndex;
    
    feedbackDiv.className = `answer-feedback ${isCorrect ? 'correct' : 'incorrect'}`;
    feedbackDiv.innerHTML = `
        <strong>${isCorrect ? '✅ Correct!' : '❌ Incorrect'}</strong>
        <p>${question.explanation}</p>
    `;
    feedbackDiv.style.display = 'block';
    
    // Disable all options for this question
    const options = document.querySelectorAll(`#question-${questionIndex} .quiz-option`);
    options.forEach((opt, idx) => {
        opt.classList.add('disabled');
        if (idx === question.correctAnswerIndex) {
            opt.classList.add('correct');
        } else if (idx === optionIndex && !isCorrect) {
            opt.classList.add('incorrect');
        }
    });
}

// Show specific question
function showQuestion(index) {
    document.querySelectorAll('.quiz-question').forEach(q => q.classList.remove('active'));
    const question = document.getElementById(`question-${index}`);
    if (question) question.classList.add('active');
    
    currentQuestionIndex = index;
    updateProgress();
    updateNavigationButtons();
}

// Update progress bar
function updateProgress() {
    const total = currentQuiz.questions.length;
    const current = currentQuestionIndex + 1;
    const percentage = (current / total) * 100;
    
    document.getElementById('quiz-progress-text').textContent = `Question ${current} of ${total}`;
    document.getElementById('quiz-progress-bar').style.width = `${percentage}%`;
}

// Update navigation buttons
function updateNavigationButtons() {
    const prevBtn = document.getElementById('prev-btn');
    const nextBtn = document.getElementById('next-btn');
    const submitBtn = document.getElementById('submit-btn');
    
    prevBtn.disabled = currentQuestionIndex === 0;
    
    const isLastQuestion = currentQuestionIndex === currentQuiz.questions.length - 1;
    if (isLastQuestion) {
        nextBtn.style.display = 'none';
        submitBtn.style.display = 'block';
    } else {
        nextBtn.style.display = 'block';
        submitBtn.style.display = 'none';
    }
}

// Navigation functions
function previousQuestion() {
    if (currentQuestionIndex > 0) {
        showQuestion(currentQuestionIndex - 1);
    }
}

function nextQuestion() {
    if (currentQuestionIndex < currentQuiz.questions.length - 1) {
        showQuestion(currentQuestionIndex + 1);
    }
}

// Submit quiz
function submitQuiz() {
    // Calculate score
    let correct = 0;
    let incorrect = 0;
    
    currentQuiz.questions.forEach((q, index) => {
        if (userAnswers[index] === q.correctAnswerIndex) {
            correct++;
        } else if (userAnswers[index] !== null) {
            incorrect++;
        }
    });
    
    const total = currentQuiz.questions.length;
    const percentage = Math.round((correct / total) * 100);
    
    // Save result
    const result = {
        id: Date.now(),
        topic: currentQuiz.topic,
        date: new Date().toISOString(),
        total: total,
        correct: correct,
        incorrect: incorrect,
        unanswered: total - correct - incorrect,
        percentage: percentage,
        questions: currentQuiz.questions.map((q, index) => ({
            question: q.question,
            userAnswer: userAnswers[index],
            correctAnswer: q.correctAnswerIndex,
            options: q.options,
            explanation: q.explanation
        }))
    };
    
    saveQuizResult(result);
    
    // Show results
    displayQuizResults(correct, incorrect, total, percentage);
}

// Display quiz results
function displayQuizResults(correct, incorrect, total, percentage) {
    document.getElementById('quiz-container').style.display = 'none';
    
    const resultsDiv = document.getElementById('quiz-results-display');
    resultsDiv.innerHTML = `
        <h3>🎉 Quiz Completed!</h3>
        <div class="score-display">${percentage}%</div>
        <div class="score-breakdown">
            <div class="score-item correct">
                <h4>${correct}</h4>
                <p>Correct</p>
            </div>
            <div class="score-item incorrect">
                <h4>${incorrect}</h4>
                <p>Incorrect</p>
            </div>
            <div class="score-item">
                <h4>${total - correct - incorrect}</h4>
                <p>Unanswered</p>
            </div>
        </div>
        <div class="quiz-actions">
            <button onclick="retakeQuiz()" class="action-button">🔄 Retake Quiz</button>
            <button onclick="viewResults()" class="action-button">📊 View All Results</button>
            <button onclick="backToQuizGenerator()" class="action-button">🏠 Back to Quiz Menu</button>
        </div>
    `;
    resultsDiv.style.display = 'block';
    
    showToast(`Quiz completed! Score: ${percentage}%`, 'success');
}

// Quiz actions
function retakeQuiz() {
    currentQuestionIndex = 0;
    userAnswers = new Array(currentQuiz.questions.length).fill(null);
    
    document.getElementById('quiz-results-display').style.display = 'none';
    document.getElementById('quiz-container').style.display = 'block';
    
    // Re-render questions
    renderQuizQuestions();
    showQuestion(0);
    updateProgress();
}

function backToQuizGenerator() {
    currentQuiz = null;
    document.getElementById('quiz-generator').style.display = 'block';
    document.getElementById('quiz-container').style.display = 'none';
    document.getElementById('quiz-results-display').style.display = 'none';
}

function viewResults() {
    showTab('results');
    refreshResults();
}

// ============================================
// RESULTS FUNCTIONALITY
// ============================================

function saveQuizResult(result) {
    const results = getQuizResults();
    results.push(result);
    localStorage.setItem('quizResults', JSON.stringify(results));
}

function getQuizResults() {
    const stored = localStorage.getItem('quizResults');
    return stored ? JSON.parse(stored) : [];
}

function refreshResults() {
    const results = getQuizResults();
    updateStatsSummary(results);
    displayQuizHistory(results);
}

function updateStatsSummary(results) {
    const totalQuizzes = results.length;
    const totalCorrect = results.reduce((sum, r) => sum + r.correct, 0);
    const totalWrong = results.reduce((sum, r) => sum + r.incorrect, 0);
    const avgScore = totalQuizzes > 0 
        ? Math.round(results.reduce((sum, r) => sum + r.percentage, 0) / totalQuizzes) 
        : 0;
    
    document.getElementById('total-quizzes').textContent = totalQuizzes;
    document.getElementById('total-correct').textContent = totalCorrect;
    document.getElementById('total-wrong').textContent = totalWrong;
    document.getElementById('average-score').textContent = `${avgScore}%`;
}

function displayQuizHistory(results) {
    const container = document.getElementById('quiz-history');
    
    if (results.length === 0) {
        container.innerHTML = '<p class="no-results">No quiz results yet. Take a quiz to see your results here!</p>';
        return;
    }
    
    container.innerHTML = results.reverse().map(result => {
        const scoreClass = result.percentage >= 80 ? 'high' : result.percentage >= 60 ? 'medium' : 'low';
        const date = new Date(result.date).toLocaleString();
        
        return `
            <div class="quiz-history-item" onclick="toggleQuizDetail(${result.id})">
                <div class="quiz-history-header">
                    <span class="quiz-history-title">${result.topic}</span>
                    <span class="quiz-history-score ${scoreClass}">${result.percentage}%</span>
                </div>
                <div class="quiz-history-meta">
                    <span>📅 ${date}</span>
                    <span>✅ ${result.correct} Correct</span>
                    <span>❌ ${result.incorrect} Incorrect</span>
                </div>
                <div class="quiz-detail" id="detail-${result.id}">
                    ${result.questions.map((q, idx) => {
                        const isCorrect = q.userAnswer === q.correctAnswer;
                        return `
                            <div class="question-review ${isCorrect ? 'correct' : 'incorrect'}">
                                <strong>Q${idx + 1}: ${q.question}</strong>
                                <p>Your answer: ${q.userAnswer !== null ? q.options[q.userAnswer] : 'Not answered'}</p>
                                <p>Correct answer: ${q.options[q.correctAnswer]}</p>
                                <p><em>${q.explanation}</em></p>
                            </div>
                        `;
                    }).join('')}
                </div>
            </div>
        `;
    }).join('');
}

function toggleQuizDetail(id) {
    const detail = document.getElementById(`detail-${id}`);
    if (detail) {
        detail.classList.toggle('show');
    }
}

function filterResults(filter) {
    // Update active filter button
    document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
    document.getElementById(`filter-${filter}`).classList.add('active');
    
    const allResults = getQuizResults();
    let filtered = allResults;
    
    const now = new Date();
    if (filter === 'today') {
        filtered = allResults.filter(r => {
            const resultDate = new Date(r.date);
            return resultDate.toDateString() === now.toDateString();
        });
    } else if (filter === 'week') {
        const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        filtered = allResults.filter(r => new Date(r.date) >= weekAgo);
    } else if (filter === 'month') {
        const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        filtered = allResults.filter(r => new Date(r.date) >= monthAgo);
    }
    
    updateStatsSummary(filtered);
    displayQuizHistory(filtered);
}

function clearAllResults() {
    if (confirm('Are you sure you want to clear all quiz results? This cannot be undone.')) {
        localStorage.removeItem('quizResults');
        refreshResults();
        showToast('All quiz results cleared', 'success');
    }
}

function exportResults() {
    const results = getQuizResults();
    const dataStr = JSON.stringify(results, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = `quiz-results-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    
    URL.revokeObjectURL(url);
    showToast('Results exported successfully', 'success');
}

// Load quiz documents when switching to quiz tab
const originalShowTab = showTab;
showTab = function(tabName) {
    originalShowTab(tabName);
    
    if (tabName === 'quiz') {
        loadQuizDocuments();
    } else if (tabName === 'results') {
        refreshResults();
    }
};
