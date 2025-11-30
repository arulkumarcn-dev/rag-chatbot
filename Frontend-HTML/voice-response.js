
// Enhanced Voice Output with Language Detection
function detectLanguage(text) {
    // Check for Tamil characters (Unicode range 0B80-0BFF)
    const tamilPattern = /[\u0B80-\u0BFF]/;
    if (tamilPattern.test(text)) {
        return 'ta-IN'; // Tamil
    }
    return 'en-US'; // Default to English
}

function speakResponseWithDetection(text, language = null) {
    if (!language) {
        language = detectLanguage(text);
    }
    
    // Clean text
    let cleanText = text.replace(/\[MOCK RESPONSE[^\]]*\]/g, '');
    cleanText = cleanText.replace(/Note: This is a mock response\.[^\n]*/g, '');
    cleanText = cleanText.trim();
    
    if (cleanText.length > 0) {
        const utterance = new SpeechSynthesisUtterance(cleanText);
        utterance.lang = language;
        utterance.rate = 1.0;
        utterance.pitch = 1.0;
        utterance.volume = 1.0;
        
        // Try to find voice for the language
        const voices = window.speechSynthesis.getVoices();
        const langVoice = voices.find(v => v.lang.startsWith(language.split('-')[0]));
        if (langVoice) {
            utterance.voice = langVoice;
        }
        
        window.speechSynthesis.cancel();
        window.speechSynthesis.speak(utterance);
    }
}

// Add voice button to bot messages
function addVoiceButtonToMessage(messageElement, messageText) {
    const voiceBtn = document.createElement('button');
    voiceBtn.className = 'response-voice-btn';
    voiceBtn.innerHTML = '';
    voiceBtn.title = 'Read this response aloud';
    voiceBtn.onclick = function(e) {
        e.stopPropagation();
        const lang = detectLanguage(messageText);
        const langName = lang.startsWith('ta') ? 'Tamil' : 'English';
        voiceBtn.innerHTML = '';
        voiceBtn.title = 'Speaking in ' + langName + '...';
        speakResponseWithDetection(messageText, lang);
        setTimeout(() => {
            voiceBtn.innerHTML = '';
            voiceBtn.title = 'Read this response aloud';
        }, 500);
    };
    messageElement.appendChild(voiceBtn);
}

// Enhanced observer to add voice buttons to bot messages
const messageObserver = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        mutation.addedNodes.forEach(function(node) {
            if (node.nodeType === 1 && node.classList && 
                (node.classList.contains('bot-message') || node.classList.contains('assistant-message'))) {
                
                // Check if voice button already exists
                if (!node.querySelector('.response-voice-btn')) {
                    const messageText = node.textContent || node.innerText;
                    addVoiceButtonToMessage(node, messageText);
                }
            }
        });
    });
});

// Start observing when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        const messagesContainer = document.getElementById('messages-container');
        if (messagesContainer) {
            messageObserver.observe(messagesContainer, { childList: true, subtree: true });
        }
    });
} else {
    const messagesContainer = document.getElementById('messages-container');
    if (messagesContainer) {
        messageObserver.observe(messagesContainer, { childList: true, subtree: true });
    }
}