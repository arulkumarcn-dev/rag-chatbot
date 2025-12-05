
// Enhanced Voice Output with Language Detection
function detectLanguage(text) {
    // Check for various Indian language scripts
    const tamilPattern = /[\\u0B80-\\u0BFF]/; // Tamil
    const hindiPattern = /[\\u0900-\\u097F]/; // Devanagari (Hindi)
    const teluguPattern = /[\\u0C00-\\u0C7F]/; // Telugu
    const malayalamPattern = /[\\u0D00-\\u0D7F]/; // Malayalam
    const kannadaPattern = /[\\u0C80-\\u0CFF]/; // Kannada
    const bengaliPattern = /[\\u0980-\\u09FF]/; // Bengali
    const gujaratiPattern = /[\\u0A80-\\u0AFF]/; // Gujarati
    const punjabiPattern = /[\\u0A00-\\u0A7F]/; // Punjabi (Gurmukhi)
    
    // Check each language in order of likelihood
    if (tamilPattern.test(text)) return 'ta-IN';
    if (hindiPattern.test(text)) return 'hi-IN';
    if (teluguPattern.test(text)) return 'te-IN';
    if (malayalamPattern.test(text)) return 'ml-IN';
    if (kannadaPattern.test(text)) return 'kn-IN';
    if (bengaliPattern.test(text)) return 'bn-IN';
    if (gujaratiPattern.test(text)) return 'gu-IN';
    if (punjabiPattern.test(text)) return 'pa-IN';
    
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
        utterance.rate = 0.9; // Slightly slower for clarity
        utterance.pitch = 1.0;
        utterance.volume = 1.0;
        
        // Wait for voices to load, then select appropriate voice
        function selectVoice() {
            const voices = window.speechSynthesis.getVoices();
            
            // Try exact match first
            let langVoice = voices.find(v => v.lang === language);
            
            // Try partial match (e.g., 'ta' in 'ta-IN')
            if (!langVoice) {
                const langPrefix = language.split('-')[0];
                langVoice = voices.find(v => v.lang.startsWith(langPrefix));
            }
            
            // Prefer Google voices for Indian languages
            if (!langVoice && language !== 'en-US') {
                langVoice = voices.find(v => 
                    v.lang.includes(language.split('-')[0]) && 
                    v.name.includes('Google')
                );
            }
            
            if (langVoice) {
                utterance.voice = langVoice;
                console.log('Using voice:', langVoice.name, langVoice.lang);
            }
        }
        
        // Load voices if not already loaded
        if (window.speechSynthesis.getVoices().length === 0) {
            window.speechSynthesis.addEventListener('voiceschanged', selectVoice, { once: true });
        } else {
            selectVoice();
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
        const languageNames = {
            'ta-IN': 'Tamil', 'hi-IN': 'Hindi', 'te-IN': 'Telugu',
            'ml-IN': 'Malayalam', 'kn-IN': 'Kannada', 'bn-IN': 'Bengali',
            'gu-IN': 'Gujarati', 'pa-IN': 'Punjabi', 'en-US': 'English'
        };
        const langName = languageNames[lang] || 'English';
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