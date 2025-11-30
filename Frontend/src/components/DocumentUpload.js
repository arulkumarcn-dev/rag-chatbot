import React, { useState } from 'react';
import axios from 'axios';
import './DocumentUpload.css';

const DocumentUpload = ({ onUploadSuccess }) => {
  const [file, setFile] = useState(null);
  const [topic, setTopic] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState(null);

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    setFile(selectedFile);
    setUploadResult(null);
  };

  const handleUpload = async (e) => {
    e.preventDefault();

    if (!file || !topic.trim()) {
      alert('Please select a file and enter a topic');
      return;
    }

    setIsUploading(true);
    setUploadResult(null);

    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('topic', topic);

      const response = await axios.post(
        'http://localhost:5000/api/chat/upload',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      setUploadResult({
        success: true,
        message: response.data.message,
        totalChunks: response.data.totalChunks,
      });

      // Reset form
      setFile(null);
      setTopic('');
      
      // Notify parent
      setTimeout(() => {
        if (onUploadSuccess) onUploadSuccess();
      }, 2000);
    } catch (error) {
      console.error('Error uploading document:', error);
      setUploadResult({
        success: false,
        message: error.response?.data?.message || 'Error uploading document',
      });
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="document-upload">
      <h2>📄 Upload Document</h2>
      <p className="upload-description">
        Upload PDF, CSV, TXT, or image files to create your knowledge base
      </p>

      <form onSubmit={handleUpload} className="upload-form">
        <div className="form-group">
          <label htmlFor="topic">Topic / Category</label>
          <input
            type="text"
            id="topic"
            value={topic}
            onChange={(e) => setTopic(e.target.value)}
            placeholder="e.g., Healthcare, Data Engineering, Gen-AI"
            disabled={isUploading}
          />
        </div>

        <div className="form-group">
          <label htmlFor="file">Select File</label>
          <div className="file-input-wrapper">
            <input
              type="file"
              id="file"
              onChange={handleFileChange}
              accept=".pdf,.csv,.txt,.png,.jpg,.jpeg"
              disabled={isUploading}
            />
            {file && <p className="file-name">Selected: {file.name}</p>}
          </div>
          <p className="file-hint">
            Supported formats: PDF, CSV, TXT, PNG, JPG
          </p>
        </div>

        <button
          type="submit"
          disabled={!file || !topic.trim() || isUploading}
          className="upload-button"
        >
          {isUploading ? 'Processing...' : 'Upload & Process'}
        </button>
      </form>

      {uploadResult && (
        <div className={`upload-result ${uploadResult.success ? 'success' : 'error'}`}>
          <p>{uploadResult.message}</p>
          {uploadResult.success && (
            <p className="chunks-info">
              Created {uploadResult.totalChunks} chunks for indexing
            </p>
          )}
        </div>
      )}

      <div className="upload-tips">
        <h3>💡 Tips</h3>
        <ul>
          <li>Use descriptive topics to organize your documents</li>
          <li>Larger documents will be automatically split into chunks</li>
          <li>PDF files will preserve page numbers for reference</li>
          <li>After upload, switch to Chat tab to ask questions</li>
        </ul>
      </div>
    </div>
  );
};

export default DocumentUpload;
