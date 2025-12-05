import React, { useState, useEffect, useRef } from 'react';
import './App.css';
import ChatInterface from './components/ChatInterface';
import DocumentUpload from './components/DocumentUpload';
import VideoUpload from './components/VideoUpload';
import { v4 as uuidv4 } from 'uuid';

function App() {
  const [sessionId, setSessionId] = useState('');
  const [messages, setMessages] = useState([]);
  const [activeTab, setActiveTab] = useState('chat');

  useEffect(() => {
    // Generate session ID on mount
    const newSessionId = uuidv4();
    setSessionId(newSessionId);
    
    // Add welcome message
    setMessages([{
      type: 'bot',
      content: 'Hello! I\'m your RAG chatbot assistant. Upload documents or video transcripts to get started, then ask me questions about them.',
      timestamp: new Date()
    }]);
  }, []);

  const handleNewMessage = (message) => {
    setMessages(prev => [...prev, message]);
  };

  return (
    <div className="App">
      <header className="app-header">
        <h1>🤖 RAG Chatbot</h1>
        <p>AI-Powered Document Chat Assistant</p>
      </header>

      <div className="app-container">
        <div className="sidebar">
          <div className="tab-navigation">
            <button 
              className={activeTab === 'chat' ? 'tab-button active' : 'tab-button'}
              onClick={() => setActiveTab('chat')}
            >
              💬 Chat
            </button>
            <button 
              className={activeTab === 'upload' ? 'tab-button active' : 'tab-button'}
              onClick={() => setActiveTab('upload')}
            >
              📄 Upload Document
            </button>
            <button 
              className={activeTab === 'video' ? 'tab-button active' : 'tab-button'}
              onClick={() => setActiveTab('video')}
            >
              🎥 Upload Video
            </button>
          </div>

          <div className="session-info">
            <p><strong>Session ID:</strong></p>
            <p className="session-id">{sessionId.substring(0, 8)}...</p>
          </div>
        </div>

        <div className="main-content">
          {activeTab === 'chat' && (
            <ChatInterface 
              sessionId={sessionId} 
              messages={messages}
              onNewMessage={handleNewMessage}
            />
          )}
          {activeTab === 'upload' && (
            <DocumentUpload onUploadSuccess={() => setActiveTab('chat')} />
          )}
          {activeTab === 'video' && (
            <VideoUpload onUploadSuccess={() => setActiveTab('chat')} />
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
