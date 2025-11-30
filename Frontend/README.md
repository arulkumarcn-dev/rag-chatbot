# React Frontend - Quick Start Guide

## Prerequisites
- Node.js 18 or later
- npm or yarn

## Installation

1. Navigate to the frontend directory:
```powershell
cd Frontend
```

2. Install dependencies:
```powershell
npm install
```

3. Start the development server:
```powershell
npm start
```

The application will open automatically at `http://localhost:3000`

## Building for Production

```powershell
npm run build
```

The optimized production build will be in the `build/` directory.

## Configuration

### Change API URL

If your backend is running on a different port, update the API calls in:
- `src/components/ChatInterface.js`
- `src/components/DocumentUpload.js`
- `src/components/VideoUpload.js`

Replace `http://localhost:5000` with your backend URL.

### Using Environment Variables

Create a `.env` file in the Frontend directory:

```
REACT_APP_API_URL=http://localhost:5000
```

Then update the code to use:
```javascript
const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';
```

## Troubleshooting

### Module Not Found
```powershell
rm -rf node_modules
rm package-lock.json
npm install
```

### Port 3000 in Use
The app will automatically prompt to use a different port, or set it manually:
```powershell
$env:PORT = "3001"; npm start
```

### CORS Errors
Ensure the backend CORS policy includes your frontend URL in `Program.cs`.
