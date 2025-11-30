import React, { useState, useRef, useEffect } from 'react';
import axios from 'axios';
import ReactMarkdown from 'react-markdown';
import './ChatInterface.css';

const ChatInterface = ({ sessionId, messages, onNewMessage }) => {
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendMessage = async (e) => {
    e.preventDefault();
    
    if (!input.trim() || isLoading) return;

    // Check for exit command
    if (input.toLowerCase().trim() === 'exit') {
      onNewMessage({
        type: 'bot',
        content: 'Thank you for using RAG Chatbot! Refresh the page to start a new session.',
        timestamp: new Date()
      });
      setInput('');
      return;
    }

    const userMessage = {
      type: 'user',
      content: input,
      timestamp: new Date()
    };

    onNewMessage(userMessage);
    setInput('');
    setIsLoading(true);

    try {
      const response = await axios.post('http://localhost:5000/api/chat/message', {
        sessionId: sessionId,
        message: input,
        topK: 5
      });

      const botMessage = {
        type: 'bot',
        content: response.data.response,
        sources: response.data.sources,
        timestamp: new Date()
      };

      onNewMessage(botMessage);
    } catch (error) {
      console.error('Error sending message:', error);
      onNewMessage({
        type: 'error',
        content: 'Sorry, I encountered an error processing your message. Please try again.',
        timestamp: new Date()
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="chat-interface">
      <div className="messages-container">
        {messages.map((message, index) => (
          <div key={index} className={`message ${message.type}`}>
            <div className="message-content">
              {message.type === 'bot' ? (
                <ReactMarkdown>{message.content}</ReactMarkdown>
              ) : (
                <p>{message.content}</p>
              )}
            </div>
            
            {message.sources && message.sources.length > 0 && (
              <div className="sources">
                <p className="sources-title">📚 Sources:</p>
                {message.sources.map((source, idx) => (
                  <div key={idx} className="source-item">
                    <p><strong>{source.documentName}</strong> - Chunk {source.chunkIndex + 1}</p>
                    <p className="source-text">{source.text}</p>
                  </div>
                ))}
              </div>
            )}
            
            <span className="message-time">
              {message.timestamp.toLocaleTimeString()}
            </span>
          </div>
        ))}
        
        {isLoading && (
          <div className="message bot">
            <div className="typing-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        )}
        
        <div ref={messagesEndRef} />
      </div>

      <form className="input-container" onSubmit={sendMessage}>
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Ask a question... (type 'exit' to end)"
          disabled={isLoading}
        />
        <button type="submit" disabled={isLoading || !input.trim()}>
          Send
        </button>
      </form>
    </div>
  );
};

export default ChatInterface;
