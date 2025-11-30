import React, { useState } from 'react';
import axios from 'axios';
import './VideoUpload.css';

const VideoUpload = ({ onUploadSuccess }) => {
  const [videoUrl, setVideoUrl] = useState('');
  const [topic, setTopic] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [uploadResult, setUploadResult] = useState(null);

  const handleUpload = async (e) => {
    e.preventDefault();

    if (!videoUrl.trim() || !topic.trim()) {
      alert('Please enter a video URL and topic');
      return;
    }

    // Validate YouTube URL
    if (!videoUrl.includes('youtube.com') && !videoUrl.includes('youtu.be')) {
      alert('Please enter a valid YouTube URL');
      return;
    }

    setIsProcessing(true);
    setUploadResult(null);

    try {
      const response = await axios.post('http://localhost:5000/api/chat/upload-video', {
        videoUrl: videoUrl,
        topic: topic,
      });

      setUploadResult({
        success: true,
        message: response.data.message,
        totalChunks: response.data.totalChunks,
      });

      // Reset form
      setVideoUrl('');
      setTopic('');

      // Notify parent
      setTimeout(() => {
        if (onUploadSuccess) onUploadSuccess();
      }, 2000);
    } catch (error) {
      console.error('Error processing video:', error);
      setUploadResult({
        success: false,
        message: error.response?.data?.message || 'Error processing video transcript',
      });
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="video-upload">
      <h2>🎥 Upload Video Transcript</h2>
      <p className="upload-description">
        Extract and index transcripts from YouTube videos
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
            disabled={isProcessing}
          />
        </div>

        <div className="form-group">
          <label htmlFor="videoUrl">YouTube Video URL</label>
          <input
            type="text"
            id="videoUrl"
            value={videoUrl}
            onChange={(e) => setVideoUrl(e.target.value)}
            placeholder="https://www.youtube.com/watch?v=..."
            disabled={isProcessing}
          />
          <p className="video-hint">
            The video must have captions/subtitles available
          </p>
        </div>

        <button
          type="submit"
          disabled={!videoUrl.trim() || !topic.trim() || isProcessing}
          className="upload-button"
        >
          {isProcessing ? 'Processing Transcript...' : 'Extract & Process'}
        </button>
      </form>

      {uploadResult && (
        <div className={`upload-result ${uploadResult.success ? 'success' : 'error'}`}>
          <p>{uploadResult.message}</p>
          {uploadResult.success && (
            <p className="chunks-info">
              Created {uploadResult.totalChunks} chunks from transcript
            </p>
          )}
        </div>
      )}

      <div className="upload-tips">
        <h3>💡 Tips</h3>
        <ul>
          <li>Works with any YouTube video that has captions</li>
          <li>Timestamps are preserved in the transcript</li>
          <li>Great for processing podcast episodes and lectures</li>
          <li>Processing may take a minute depending on video length</li>
        </ul>
      </div>

      <div className="example-section">
        <h3>📺 Example URLs</h3>
        <div className="example-url">
          <code>https://www.youtube.com/watch?v=dQw4w9WgXcQ</code>
        </div>
        <div className="example-url">
          <code>https://youtu.be/dQw4w9WgXcQ</code>
        </div>
      </div>
    </div>
  );
};

export default VideoUpload;
