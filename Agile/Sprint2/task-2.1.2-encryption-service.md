# Task 2.1.2: Encryption Service

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.1 - Windows Registry Service  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Senior Developer  
**Dependencies**: Task 2.1.1 - Registry Operations

## 🎯 Objective
Implement comprehensive encryption service với AES-256 encryption, key management, và secure data protection cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Encryption Operations**:
  - AES-256-GCM encryption/decryption
  - Key generation và management
  - Secure key storage
  - Data integrity verification
  - Performance optimization

- [ ] **Security Features**:
  - Secure key derivation
  - Salt generation
  - IV (Initialization Vector) management
  - Authentication tags
  - Key rotation support

- [ ] **Key Management**:
  - Windows DPAPI integration
  - Key storage trong secure location
  - Key backup/restore
  - Key versioning
  - Access control cho keys

### Technical Requirements
- [ ] **Encryption Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IEncryptionService.cs
  public interface IEncryptionService
  {
      // Basic encryption operations
      Task<string> EncryptAsync(string plainText);
      Task<string> DecryptAsync(string cipherText);
      Task<byte[]> EncryptBytesAsync(byte[] plainBytes);
      Task<byte[]> DecryptBytesAsync(byte[] cipherBytes);
      
      // Advanced encryption operations
      Task<string> EncryptWithKeyAsync(string plainText, string keyId);
      Task<string> DecryptWithKeyAsync(string cipherText, string keyId);
      Task<bool> ValidateEncryptedDataAsync(string cipherText);
      
      // Key management
      Task<string> GenerateKeyAsync(string keyId = null);
      Task<bool> KeyExistsAsync(string keyId);
      Task<IEnumerable<string>> GetAvailableKeysAsync();
      Task DeleteKeyAsync(string keyId);
      Task BackupKeysAsync(string backupPath);
      Task RestoreKeysAsync(string backupPath);
      
      // Security operations
      Task<string> HashAsync(string data);
      Task<bool> VerifyHashAsync(string data, string hash);
      Task<string> GenerateSaltAsync(int length = 32);
      Task<string> DeriveKeyAsync(string password, string salt, int iterations = 10000);
  }
  ```

- [ ] **Encryption Models**:
  ```csharp
  // SystemConfig.Infrastructure/Models/EncryptionKey.cs
  public class EncryptionKey
  {
      public string Id { get; set; }
      public byte[] Key { get; set; }
      public DateTime CreatedAt { get; set; }
      public DateTime? ExpiresAt { get; set; }
      public bool IsActive { get; set; }
      public string Description { get; set; }
      public int Version { get; set; }
  }
  
  // SystemConfig.Infrastructure/Models/EncryptionResult.cs
  public class EncryptionResult
  {
      public string CipherText { get; set; }
      public string KeyId { get; set; }
      public string Iv { get; set; }
      public string Salt { get; set; }
      public string AuthTag { get; set; }
      public DateTime EncryptedAt { get; set; }
      public int Version { get; set; }
  }
  
  // SystemConfig.Infrastructure/Models/DecryptionResult.cs
  public class DecryptionResult
  {
      public string PlainText { get; set; }
      public bool IsValid { get; set; }
      public string ErrorMessage { get; set; }
      public DateTime DecryptedAt { get; set; }
  }
  ```

### Quality Requirements
- [ ] **Security**: Military-grade encryption với AES-256-GCM
- [ ] **Performance**: Efficient encryption/decryption operations
- [ ] **Key Management**: Secure key storage và rotation
- [ ] **Error Handling**: Comprehensive error handling
- [ ] **Compliance**: FIPS 140-2 compliance

## 🏗️ Implementation Plan

### Phase 1: Core Encryption Implementation (3 hours)
```csharp
// SystemConfig.Infrastructure/Services/EncryptionService.cs
public class EncryptionService : IEncryptionService
{
    private readonly ILoggingService _loggingService;
    private readonly IConfigurationService _configurationService;
    private readonly Dictionary<string, EncryptionKey> _keyCache;
    private readonly object _keyCacheLock = new object();
    private readonly string _defaultKeyId = "default";
    
    public EncryptionService(ILoggingService loggingService, IConfigurationService configurationService)
    {
        _loggingService = loggingService;
        _configurationService = configurationService;
        _keyCache = new Dictionary<string, EncryptionKey>();
    }
    
    public async Task<string> EncryptAsync(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
            
            _loggingService.LogDebug("Encrypting text with length: {Length}", plainText.Length);
            
            var key = await GetOrCreateKeyAsync(_defaultKeyId);
            var salt = await GenerateSaltAsync();
            var iv = GenerateIv();
            
            using var aes = Aes.Create();
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key.Key;
            aes.IV = iv;
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            await swEncrypt.WriteAsync(plainText);
            await swEncrypt.FlushAsync();
            await csEncrypt.FlushFinalBlockAsync();
            
            var cipherBytes = msEncrypt.ToArray();
            var authTag = aes.Tag;
            
            var result = new EncryptionResult
            {
                CipherText = Convert.ToBase64String(cipherBytes),
                KeyId = key.Id,
                Iv = Convert.ToBase64String(iv),
                Salt = Convert.ToBase64String(salt),
                AuthTag = Convert.ToBase64String(authTag),
                EncryptedAt = DateTime.UtcNow,
                Version = key.Version
            };
            
            var jsonResult = JsonSerializer.Serialize(result);
            var encryptedResult = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonResult));
            
            _loggingService.LogDebug("Text encrypted successfully");
            
            return encryptedResult;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to encrypt text");
            throw;
        }
    }
    
    public async Task<string> DecryptAsync(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            
            _loggingService.LogDebug("Decrypting text");
            
            var jsonBytes = Convert.FromBase64String(cipherText);
            var jsonText = Encoding.UTF8.GetString(jsonBytes);
            var result = JsonSerializer.Deserialize<EncryptionResult>(jsonText);
            
            var key = await GetKeyAsync(result.KeyId);
            if (key == null)
            {
                throw new InvalidOperationException($"Encryption key not found: {result.KeyId}");
            }
            
            var cipherBytes = Convert.FromBase64String(result.CipherText);
            var iv = Convert.FromBase64String(result.Iv);
            var authTag = Convert.FromBase64String(result.AuthTag);
            
            using var aes = Aes.Create();
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key.Key;
            aes.IV = iv;
            aes.Tag = authTag;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipherBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            var plainText = await srDecrypt.ReadToEndAsync();
            
            _loggingService.LogDebug("Text decrypted successfully");
            
            return plainText;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to decrypt text");
            throw;
        }
    }
    
    public async Task<byte[]> EncryptBytesAsync(byte[] plainBytes)
    {
        try
        {
            if (plainBytes == null || plainBytes.Length == 0)
                return plainBytes;
            
            _loggingService.LogDebug("Encrypting bytes with length: {Length}", plainBytes.Length);
            
            var key = await GetOrCreateKeyAsync(_defaultKeyId);
            var iv = GenerateIv();
            
            using var aes = Aes.Create();
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key.Key;
            aes.IV = iv;
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            
            await csEncrypt.WriteAsync(plainBytes);
            await csEncrypt.FlushFinalBlockAsync();
            
            var cipherBytes = msEncrypt.ToArray();
            var authTag = aes.Tag;
            
            // Combine IV, cipher bytes, and auth tag
            var result = new byte[iv.Length + cipherBytes.Length + authTag.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);
            Buffer.BlockCopy(authTag, 0, result, iv.Length + cipherBytes.Length, authTag.Length);
            
            _loggingService.LogDebug("Bytes encrypted successfully");
            
            return result;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to encrypt bytes");
            throw;
        }
    }
    
    public async Task<byte[]> DecryptBytesAsync(byte[] cipherBytes)
    {
        try
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
                return cipherBytes;
            
            _loggingService.LogDebug("Decrypting bytes");
            
            var key = await GetOrCreateKeyAsync(_defaultKeyId);
            
            // Extract IV, cipher bytes, and auth tag
            var ivLength = 12; // GCM IV length
            var authTagLength = 16; // GCM auth tag length
            var cipherLength = cipherBytes.Length - ivLength - authTagLength;
            
            var iv = new byte[ivLength];
            var cipher = new byte[cipherLength];
            var authTag = new byte[authTagLength];
            
            Buffer.BlockCopy(cipherBytes, 0, iv, 0, ivLength);
            Buffer.BlockCopy(cipherBytes, ivLength, cipher, 0, cipherLength);
            Buffer.BlockCopy(cipherBytes, ivLength + cipherLength, authTag, 0, authTagLength);
            
            using var aes = Aes.Create();
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key.Key;
            aes.IV = iv;
            aes.Tag = authTag;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var msResult = new MemoryStream();
            
            await csDecrypt.CopyToAsync(msResult);
            
            _loggingService.LogDebug("Bytes decrypted successfully");
            
            return msResult.ToArray();
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to decrypt bytes");
            throw;
        }
    }
    
    public async Task<string> EncryptWithKeyAsync(string plainText, string keyId)
    {
        try
        {
            var key = await GetOrCreateKeyAsync(keyId);
            // Similar to EncryptAsync but with specific key
            return await EncryptAsync(plainText); // Simplified for now
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to encrypt with key: {KeyId}", keyId);
            throw;
        }
    }
    
    public async Task<string> DecryptWithKeyAsync(string cipherText, string keyId)
    {
        try
        {
            var key = await GetKeyAsync(keyId);
            if (key == null)
            {
                throw new InvalidOperationException($"Encryption key not found: {keyId}");
            }
            
            // Similar to DecryptAsync but with specific key
            return await DecryptAsync(cipherText); // Simplified for now
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to decrypt with key: {KeyId}", keyId);
            throw;
        }
    }
    
    public async Task<bool> ValidateEncryptedDataAsync(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
                return false;
            
            var jsonBytes = Convert.FromBase64String(cipherText);
            var jsonText = Encoding.UTF8.GetString(jsonBytes);
            var result = JsonSerializer.Deserialize<EncryptionResult>(jsonText);
            
            return result != null && 
                   !string.IsNullOrEmpty(result.CipherText) &&
                   !string.IsNullOrEmpty(result.Iv) &&
                   !string.IsNullOrEmpty(result.AuthTag);
        }
        catch
        {
            return false;
        }
    }
    
    // Helper methods
    private async Task<EncryptionKey> GetOrCreateKeyAsync(string keyId)
    {
        var key = await GetKeyAsync(keyId);
        if (key == null)
        {
            key = await GenerateKeyAsync(keyId);
        }
        return key;
    }
    
    private async Task<EncryptionKey> GetKeyAsync(string keyId)
    {
        lock (_keyCacheLock)
        {
            if (_keyCache.TryGetValue(keyId, out var cachedKey))
            {
                return cachedKey;
            }
        }
        
        // Load from secure storage
        var key = await LoadKeyFromStorageAsync(keyId);
        if (key != null)
        {
            lock (_keyCacheLock)
            {
                _keyCache[keyId] = key;
            }
        }
        
        return key;
    }
    
    private async Task<EncryptionKey> GenerateKeyAsync(string keyId = null)
    {
        try
        {
            var actualKeyId = keyId ?? _defaultKeyId;
            
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            
            var key = new EncryptionKey
            {
                Id = actualKeyId,
                Key = aes.Key,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Version = 1
            };
            
            // Save to secure storage
            await SaveKeyToStorageAsync(key);
            
            lock (_keyCacheLock)
            {
                _keyCache[actualKeyId] = key;
            }
            
            _loggingService.LogInformation("Generated new encryption key: {KeyId}", actualKeyId);
            
            return key;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to generate encryption key: {KeyId}", keyId);
            throw;
        }
    }
    
    private byte[] GenerateIv()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        return aes.IV;
    }
    
    private async Task<string> GenerateSaltAsync(int length = 32)
    {
        var salt = new byte[length];
        using var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }
    
    private async Task SaveKeyToStorageAsync(EncryptionKey key)
    {
        // Use Windows DPAPI to protect the key
        var keyData = JsonSerializer.Serialize(key);
        var keyBytes = Encoding.UTF8.GetBytes(keyData);
        var protectedKeyBytes = ProtectedData.Protect(keyBytes, null, DataProtectionScope.LocalMachine);
        
        var keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "SystemConfig", "Keys", $"{key.Id}.key");
        
        Directory.CreateDirectory(Path.GetDirectoryName(keyPath));
        await File.WriteAllBytesAsync(keyPath, protectedKeyBytes);
    }
    
    private async Task<EncryptionKey> LoadKeyFromStorageAsync(string keyId)
    {
        try
        {
            var keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "SystemConfig", "Keys", $"{keyId}.key");
            
            if (!File.Exists(keyPath))
                return null;
            
            var protectedKeyBytes = await File.ReadAllBytesAsync(keyPath);
            var keyBytes = ProtectedData.Unprotect(protectedKeyBytes, null, DataProtectionScope.LocalMachine);
            var keyData = Encoding.UTF8.GetString(keyBytes);
            
            return JsonSerializer.Deserialize<EncryptionKey>(keyData);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to load key from storage: {KeyId}", keyId);
            return null;
        }
    }
    
    // Additional methods for key management
    public async Task<bool> KeyExistsAsync(string keyId)
    {
        var key = await GetKeyAsync(keyId);
        return key != null;
    }
    
    public async Task<IEnumerable<string>> GetAvailableKeysAsync()
    {
        var keysDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "SystemConfig", "Keys");
        
        if (!Directory.Exists(keysDir))
            return Enumerable.Empty<string>();
        
        var keyFiles = Directory.GetFiles(keysDir, "*.key");
        return keyFiles.Select(f => Path.GetFileNameWithoutExtension(f));
    }
    
    public async Task DeleteKeyAsync(string keyId)
    {
        try
        {
            var keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "SystemConfig", "Keys", $"{keyId}.key");
            
            if (File.Exists(keyPath))
            {
                File.Delete(keyPath);
            }
            
            lock (_keyCacheLock)
            {
                _keyCache.Remove(keyId);
            }
            
            _loggingService.LogInformation("Deleted encryption key: {KeyId}", keyId);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to delete encryption key: {KeyId}", keyId);
            throw;
        }
    }
    
    public async Task<string> HashAsync(string data)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    public async Task<bool> VerifyHashAsync(string data, string hash)
    {
        var computedHash = await HashAsync(data);
        return computedHash == hash;
    }
    
    public async Task<string> DeriveKeyAsync(string password, string salt, int iterations = 10000)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), iterations);
        var key = deriveBytes.GetBytes(32); // 256 bits
        return Convert.ToBase64String(key);
    }
}
```

### Phase 2: Key Management Implementation (2 hours)
```csharp
// SystemConfig.Infrastructure/Services/KeyManagementService.cs
public class KeyManagementService
{
    private readonly ILoggingService _loggingService;
    private readonly string _keysDirectory;
    
    public KeyManagementService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _keysDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "SystemConfig", "Keys");
        Directory.CreateDirectory(_keysDirectory);
    }
    
    public async Task BackupKeysAsync(string backupPath)
    {
        try
        {
            _loggingService.LogInformation("Backing up encryption keys to: {Path}", backupPath);
            
            var backupData = new Dictionary<string, object>();
            var keyFiles = Directory.GetFiles(_keysDirectory, "*.key");
            
            foreach (var keyFile in keyFiles)
            {
                var keyId = Path.GetFileNameWithoutExtension(keyFile);
                var keyData = await File.ReadAllBytesAsync(keyFile);
                backupData[keyId] = Convert.ToBase64String(keyData);
            }
            
            var jsonData = JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(backupPath, jsonData);
            
            _loggingService.LogInformation("Successfully backed up {Count} encryption keys", backupData.Count);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to backup encryption keys");
            throw;
        }
    }
    
    public async Task RestoreKeysAsync(string backupPath)
    {
        try
        {
            _loggingService.LogInformation("Restoring encryption keys from: {Path}", backupPath);
            
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }
            
            var jsonData = await File.ReadAllTextAsync(backupPath);
            var backupData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
            
            foreach (var kvp in backupData)
            {
                var keyId = kvp.Key;
                var keyData = Convert.FromBase64String(kvp.Value);
                var keyPath = Path.Combine(_keysDirectory, $"{keyId}.key");
                
                await File.WriteAllBytesAsync(keyPath, keyData);
            }
            
            _loggingService.LogInformation("Successfully restored {Count} encryption keys", backupData.Count);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to restore encryption keys");
            throw;
        }
    }
}
```

### Phase 3: Dependency Injection Setup (1 hour)
```csharp
// SystemConfig.Infrastructure/DependencyInjection/EncryptionServiceCollectionExtensions.cs
public static class EncryptionServiceCollectionExtensions
{
    public static IServiceCollection AddEncryptionServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<KeyManagementService>();
        
        return services;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Services/EncryptionServiceTests.cs
public class EncryptionServiceTests
{
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly Mock<IConfigurationService> _mockConfigurationService;
    private readonly EncryptionService _encryptionService;
    
    public EncryptionServiceTests()
    {
        _mockLoggingService = new Mock<ILoggingService>();
        _mockConfigurationService = new Mock<IConfigurationService>();
        _encryptionService = new EncryptionService(_mockLoggingService.Object, _mockConfigurationService.Object);
    }
    
    [Fact]
    public async Task EncryptAsync_WithValidText_ShouldEncryptSuccessfully()
    {
        // Arrange
        var plainText = "test_encryption_data";
        
        // Act
        var encryptedText = await _encryptionService.EncryptAsync(plainText);
        
        // Assert
        Assert.NotNull(encryptedText);
        Assert.NotEqual(plainText, encryptedText);
        Assert.True(await _encryptionService.ValidateEncryptedDataAsync(encryptedText));
    }
    
    [Fact]
    public async Task DecryptAsync_WithValidEncryptedText_ShouldDecryptSuccessfully()
    {
        // Arrange
        var plainText = "test_decryption_data";
        var encryptedText = await _encryptionService.EncryptAsync(plainText);
        
        // Act
        var decryptedText = await _encryptionService.DecryptAsync(encryptedText);
        
        // Assert
        Assert.Equal(plainText, decryptedText);
    }
    
    [Fact]
    public async Task EncryptBytesAsync_WithValidBytes_ShouldEncryptSuccessfully()
    {
        // Arrange
        var plainBytes = Encoding.UTF8.GetBytes("test_bytes_encryption");
        
        // Act
        var encryptedBytes = await _encryptionService.EncryptBytesAsync(plainBytes);
        
        // Assert
        Assert.NotNull(encryptedBytes);
        Assert.NotEqual(plainBytes, encryptedBytes);
    }
    
    [Fact]
    public async Task DecryptBytesAsync_WithValidEncryptedBytes_ShouldDecryptSuccessfully()
    {
        // Arrange
        var plainBytes = Encoding.UTF8.GetBytes("test_bytes_decryption");
        var encryptedBytes = await _encryptionService.EncryptBytesAsync(plainBytes);
        
        // Act
        var decryptedBytes = await _encryptionService.DecryptBytesAsync(encryptedBytes);
        
        // Assert
        Assert.Equal(plainBytes, decryptedBytes);
    }
    
    [Fact]
    public async Task HashAsync_WithValidData_ShouldGenerateHash()
    {
        // Arrange
        var data = "test_hash_data";
        
        // Act
        var hash = await _encryptionService.HashAsync(data);
        
        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(data, hash);
    }
    
    [Fact]
    public async Task VerifyHashAsync_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        var data = "test_verify_hash_data";
        var hash = await _encryptionService.HashAsync(data);
        
        // Act
        var isValid = await _encryptionService.VerifyHashAsync(data, hash);
        
        // Assert
        Assert.True(isValid);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Services/EncryptionServiceIntegrationTests.cs
public class EncryptionServiceIntegrationTests : IDisposable
{
    private readonly IEncryptionService _encryptionService;
    private readonly string _testKeyId = "test_key_" + Guid.NewGuid().ToString("N");
    
    public EncryptionServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddEncryptionServices();
        var serviceProvider = services.BuildServiceProvider();
        _encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();
    }
    
    [Fact]
    public async Task EncryptionService_ShouldPerformFullEncryptionCycle()
    {
        // Arrange
        var testData = "sensitive_configuration_data_" + Guid.NewGuid();
        
        // Act - Encrypt
        var encryptedData = await _encryptionService.EncryptAsync(testData);
        
        // Assert - Encrypt
        Assert.NotNull(encryptedData);
        Assert.NotEqual(testData, encryptedData);
        Assert.True(await _encryptionService.ValidateEncryptedDataAsync(encryptedData));
        
        // Act - Decrypt
        var decryptedData = await _encryptionService.DecryptAsync(encryptedData);
        
        // Assert - Decrypt
        Assert.Equal(testData, decryptedData);
    }
    
    [Fact]
    public async Task EncryptionService_ShouldHandleLargeData()
    {
        // Arrange
        var largeData = new string('A', 10000); // 10KB of data
        
        // Act
        var encryptedData = await _encryptionService.EncryptAsync(largeData);
        var decryptedData = await _encryptionService.DecryptAsync(encryptedData);
        
        // Assert
        Assert.Equal(largeData, decryptedData);
    }
    
    [Fact]
    public async Task EncryptionService_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var specialData = "test@#$%^&*()_+-=[]{}|;':\",./<>?`~";
        
        // Act
        var encryptedData = await _encryptionService.EncryptAsync(specialData);
        var decryptedData = await _encryptionService.DecryptAsync(encryptedData);
        
        // Assert
        Assert.Equal(specialData, decryptedData);
    }
    
    public void Dispose()
    {
        // Cleanup test keys
        try
        {
            _encryptionService.DeleteKeyAsync(_testKeyId).Wait();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
```

## 📊 Definition of Done
- [ ] **Encryption Service Interface**: IEncryptionService interface được implement đầy đủ
- [ ] **AES-256-GCM Encryption**: Core encryption/decryption hoạt động đúng
- [ ] **Key Management**: Key generation, storage, và rotation hoàn thành
- [ ] **Security Features**: Salt, IV, auth tags được implement
- [ ] **Performance**: Efficient encryption operations
- [ ] **Unit Tests**: >95% coverage cho encryption service
- [ ] **Integration Tests**: Encryption operations tests pass
- [ ] **Code Review**: Encryption service được approve

## 🚨 Risks & Mitigation

### Security Risks
- **Risk**: Key exposure trong memory
- **Mitigation**: Use secure memory management và key rotation

- **Risk**: Weak encryption algorithms
- **Mitigation**: Use AES-256-GCM với proper key management

- **Risk**: Key storage vulnerabilities
- **Mitigation**: Use Windows DPAPI cho key protection

### Technical Risks
- **Risk**: Performance issues với large data
- **Mitigation**: Implement streaming encryption cho large files

- **Risk**: Memory leaks với encryption operations
- **Mitigation**: Proper disposal pattern implementation

## 📚 Resources & References
- AES-256-GCM Encryption Best Practices
- Windows DPAPI Documentation
- .NET Cryptography Guidelines
- FIPS 140-2 Compliance
- Key Management Best Practices

## 🔄 Dependencies
- Task 2.1.1: Registry Operations
- Task 1.1.2: Setup Dependency Injection
- Task 1.1.3: Configure Logging Framework

## 📈 Success Metrics
- Encryption operations work correctly
- Security standards met
- Performance benchmarks achieved
- High test coverage achieved
- Key management functions properly
- Compliance requirements met

## 📝 Notes
- Use AES-256-GCM for best security
- Implement proper key rotation
- Use Windows DPAPI for key protection
- Consider performance implications
- Document encryption standards
- Regular security audits 