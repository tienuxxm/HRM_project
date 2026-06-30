using System.Text.Json.Serialization;

namespace Domain.Abstractions;

public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiredIn { get; init; } = 0;

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshTokenExpiredIn { get; init; } = 0;
}