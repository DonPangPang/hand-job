using HandJob.Domain.ViewModels;

namespace HandJob.Domain.Responses;

public class ResponseBox
{
    public int Code { get; set; }

    public object? Data { get; set; }

    public string? Message { get; set; }
}

public class ResponseToken
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }

    public VUser? User { get; set; }
}