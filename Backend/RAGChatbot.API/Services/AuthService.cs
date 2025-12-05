using RAGChatbot.API.Models;
using System.Text.Json;

namespace RAGChatbot.API.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string password, string email);
    Task<bool> ValidateTokenAsync(string token);
}

public class AuthService : IAuthService
{
    private readonly string _usersFilePath;
    private readonly ILogger<AuthService> _logger;
    private Dictionary<string, UserData> _users;

    private class UserData
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _logger = logger;
        _usersFilePath = Path.Combine(configuration["VectorStore:StoragePath"] ?? "./vectorstore", "users.json");
        _users = new Dictionary<string, UserData>();
        LoadUsers();
    }

    private void LoadUsers()
    {
        try
        {
            if (File.Exists(_usersFilePath))
            {
                var json = File.ReadAllText(_usersFilePath);
                _users = JsonSerializer.Deserialize<Dictionary<string, UserData>>(json) ?? new();
                _logger.LogInformation($"Loaded {_users.Count} users");
            }
            else
            {
                // Create default admin user
                var defaultPassword = HashPassword("admin123");
                _users["admin"] = new UserData
                {
                    Username = "admin",
                    PasswordHash = defaultPassword,
                    Email = "admin@ragchatbot.com",
                    CreatedAt = DateTime.UtcNow
                };
                SaveUsers();
                _logger.LogInformation("Created default admin user (username: admin, password: admin123)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
        }
    }

    private void SaveUsers()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_usersFilePath) ?? "./vectorstore");
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving users");
        }
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                };
            }

            if (!_users.ContainsKey(username))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var user = _users[username];
            var passwordHash = HashPassword(password);

            if (user.PasswordHash != passwordHash)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Generate simple token (in production, use JWT)
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{DateTime.UtcNow.Ticks}"));

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Username = username
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task<bool> RegisterAsync(string username, string password, string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (_users.ContainsKey(username))
            {
                _logger.LogWarning($"Registration failed: Username '{username}' already exists");
                return false;
            }

            _users[username] = new UserData
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            SaveUsers();
            _logger.LogInformation($"User '{username}' registered successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // Simple validation - in production, use JWT validation
        return !string.IsNullOrWhiteSpace(token);
    }

    private string HashPassword(string password)
    {
        // Simple hash - in production, use BCrypt or PBKDF2
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "RAGChatbotSalt");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
