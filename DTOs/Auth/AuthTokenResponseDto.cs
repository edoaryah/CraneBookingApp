namespace AspnetCoreMvcFull.DTOs
{
  public class AuthTokenResponse
  {
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UserInfoDto? User { get; set; }
  }
}
