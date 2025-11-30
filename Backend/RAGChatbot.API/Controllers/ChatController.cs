using Microsoft.AspNetCore.Mvc;
using RAGChatbot.API.Models;
using RAGChatbot.API.Services;

namespace RAGChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var response = await _chatService.ProcessChatAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "An error occurred while processing your message");
        }
    }

    [HttpPost("upload")]
    public async Task<ActionResult<DocumentUploadResponse>> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] string topic)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new DocumentUploadResponse
                {
                    Success = false,
                    Message = "No file uploaded"
                });
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest(new DocumentUploadResponse
                {
                    Success = false,
                    Message = "Topic is required"
                });
            }

            var response = await _chatService.ProcessDocumentAsync(file, topic);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new DocumentUploadResponse
            {
                Success = false,
                Message = $"Error processing document: {ex.Message}"
            });
        }
    }

    [HttpPost("upload-video")]
    public async Task<ActionResult<DocumentUploadResponse>> UploadVideoTranscript(
        [FromBody] DocumentUploadRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                return BadRequest("Video URL is required");
            }

            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest("Topic is required");
            }

            var response = await _chatService.ProcessVideoTranscriptAsync(request.VideoUrl, request.Topic);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video transcript");
            return StatusCode(500, "An error occurred while processing the video");
        }
    }

    [HttpGet("documents")]
    public async Task<ActionResult<List<string>>> GetAllDocuments()
    {
        try
        {
            var documents = await _chatService.GetAllDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents");
            return StatusCode(500, "An error occurred while retrieving documents");
        }
    }

    [HttpDelete("documents/{documentName}")]
    public async Task<ActionResult> DeleteDocument(string documentName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentName))
            {
                return BadRequest("Document name is required");
            }

            var success = await _chatService.DeleteDocumentAsync(documentName);
            
            if (success)
            {
                return Ok(new { success = true, message = $"Document '{documentName}' deleted successfully" });
            }
            else
            {
                return NotFound(new { success = false, message = $"Document '{documentName}' not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            return StatusCode(500, new { success = false, message = "An error occurred while deleting the document" });
        }
    }

    [HttpDelete("documents")]
    public async Task<ActionResult> ClearAllDocuments()
    {
        try
        {
            var success = await _chatService.ClearAllDocumentsAsync();
            
            if (success)
            {
                return Ok(new { success = true, message = "All documents deleted successfully" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Failed to delete documents" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all documents");
            return StatusCode(500, new { success = false, message = "An error occurred while deleting all documents" });
        }
    }
}
