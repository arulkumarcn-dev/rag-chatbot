using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using YoutubeExplode;
using YoutubeExplode.Videos.ClosedCaptions;
using OfficeOpenXml;

namespace RAGChatbot.API.Services;

public class DocumentProcessor : IDocumentProcessor
{
    private readonly ILogger<DocumentProcessor> _logger;

    public DocumentProcessor(ILogger<DocumentProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ProcessPdfAsync(Stream fileStream, string fileName)
    {
        try
        {
            _logger.LogInformation("Processing PDF: {FileName}, Stream Position: {Position}, Length: {Length}", 
                fileName, fileStream.Position, fileStream.Length);
            
            // Ensure stream is at the beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            
            // Configure PDF reader to handle various encodings
            var readerProperties = new ReaderProperties();
            using var pdfReader = new PdfReader(fileStream, readerProperties);
            using var pdfDoc = new PdfDocument(pdfReader);
            
            var text = new StringBuilder();
            int totalPages = pdfDoc.GetNumberOfPages();
            _logger.LogInformation("PDF has {Pages} pages", totalPages);
            
            for (int page = 1; page <= totalPages; page++)
            {
                try
                {
                    var strategy = new LocationTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                    text.AppendLine($"[Page {page}]");
                    text.AppendLine(pageText);
                    text.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error extracting text from page {Page}", page);
                    text.AppendLine($"[Page {page} - Error extracting text]");
                }
            }
            
            _logger.LogInformation("Successfully processed PDF with {Pages} pages", totalPages);
            return text.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF: {FileName}", fileName);
            throw new Exception($"Failed to process PDF: {ex.Message}", ex);
        }
    }

    public async Task<string> ProcessCsvAsync(Stream fileStream)
    {
        try
        {
            // Use UTF-8 encoding to properly handle multilingual content
            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });
            
            var records = csv.GetRecords<dynamic>().ToList();
            var text = new StringBuilder();
            
            if (records.Count == 0)
            {
                return "Empty CSV file";
            }
            
            var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
            
            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;
                foreach (var header in headers)
                {
                    text.AppendLine($"{header}: {dict[header]}");
                }
                text.AppendLine();
            }
            
            return text.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV");
            throw new Exception($"Failed to process CSV: {ex.Message}", ex);
        }
    }

    public async Task<string> ProcessExcelAsync(Stream fileStream)
    {
        try
        {
            _logger.LogInformation("Processing Excel file");
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage(fileStream);
            var text = new StringBuilder();
            
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                text.AppendLine($"[Sheet: {worksheet.Name}]");
                text.AppendLine();
                
                if (worksheet.Dimension == null)
                {
                    text.AppendLine("(Empty sheet)");
                    text.AppendLine();
                    continue;
                }
                
                var startRow = worksheet.Dimension.Start.Row;
                var endRow = worksheet.Dimension.End.Row;
                var startCol = worksheet.Dimension.Start.Column;
                var endCol = worksheet.Dimension.End.Column;
                
                // Process header row
                for (int col = startCol; col <= endCol; col++)
                {
                    var cellValue = worksheet.Cells[startRow, col].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        text.Append($"{cellValue}\t");
                    }
                }
                text.AppendLine();
                text.AppendLine();
                
                // Process data rows
                for (int row = startRow + 1; row <= endRow; row++)
                {
                    var rowData = new List<string>();
                    for (int col = startCol; col <= endCol; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            rowData.Add(cellValue);
                        }
                    }
                    
                    if (rowData.Any())
                    {
                        text.AppendLine(string.Join(", ", rowData));
                    }
                }
                
                text.AppendLine();
            }
            
            _logger.LogInformation("Successfully processed Excel file with {Sheets} sheets", package.Workbook.Worksheets.Count);
            return text.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel file");
            throw new Exception($"Failed to process Excel file: {ex.Message}", ex);
        }
    }

    public async Task<string> ProcessImageAsync(Stream fileStream)
    {
        try
        {
            _logger.LogWarning("Image OCR not fully implemented. Consider using Google Cloud Vision API.");
            return "Image text extraction requires Google Cloud Vision API or Tesseract configuration.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            throw;
        }
    }

    public async Task<string> GetVideoTranscriptAsync(string videoUrl)
    {
        try
        {
            var youtube = new YoutubeClient();
            var videoId = YoutubeExplode.Videos.VideoId.Parse(videoUrl);
            
            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);
            
            if (trackManifest.Tracks.Count == 0)
            {
                throw new Exception("No captions available for this video");
            }
            
            var trackInfo = trackManifest.Tracks.FirstOrDefault(t => t.Language.Code == "en") 
                           ?? trackManifest.Tracks.First();
            
            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
            
            var text = new StringBuilder();
            foreach (var caption in track.Captions)
            {
                text.AppendLine($"[{caption.Offset}] {caption.Text}");
            }
            
            return text.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video transcript: {VideoUrl}", videoUrl);
            throw;
        }
    }

    public async Task<string> ProcessTextFileAsync(Stream fileStream)
    {
        try
        {
            // Try to detect encoding, default to UTF-8
            // This ensures proper handling of Tamil, Hindi, and other Unicode content
            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = await reader.ReadToEndAsync();
            _logger.LogInformation("Successfully processed text file, content length: {Length}", content.Length);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text file");
            throw;
        }
    }
}