using System.Net;
using System.Text;

var port = 8080;
var path = Path.Combine(Directory.GetCurrentDirectory(), "Frontend-HTML");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("========================================");
Console.WriteLine("  RAG CHATBOT - FRONTEND SERVER");
Console.WriteLine("========================================");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine($"📁 Serving from: {path}");
Console.WriteLine($"🌐 Port: {port}");
Console.WriteLine($"🔗 URL: http://localhost:{port}");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✓ Server is running...");
Console.ResetColor();
Console.WriteLine("Press Ctrl+C to stop");
Console.WriteLine();

var listener = new HttpListener();
listener.Prefixes.Add($"http://localhost:{port}/");
listener.Start();

while (true)
{
    try
    {
        var context = listener.GetContext();
        var request = context.Request;
        var response = context.Response;

        var filePath = request.Url.AbsolutePath;
        if (filePath == "/") filePath = "/index.html";
        
        var fullPath = Path.Combine(path, filePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            var content = File.ReadAllBytes(fullPath);
            var ext = Path.GetExtension(fullPath).ToLower();
            
            response.ContentType = ext switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
            
            response.ContentLength64 = content.Length;
            response.OutputStream.Write(content, 0, content.Length);
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {filePath} - 200 OK");
            Console.ResetColor();
        }
        else
        {
            response.StatusCode = 404;
            var notFound = Encoding.UTF8.GetBytes("404 - File Not Found");
            response.OutputStream.Write(notFound, 0, notFound.Length);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {filePath} - 404 Not Found");
            Console.ResetColor();
        }
        
        response.Close();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}
