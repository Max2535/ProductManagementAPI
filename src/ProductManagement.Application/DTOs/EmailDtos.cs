namespace ProductManagement.Application.DTOs;

public record EmailRequest
{
    public List<string> To { get; init; } = new();
    public List<string>? Cc { get; init; }
    public List<string>? Bcc { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = true;
    public List<EmailAttachment>? Attachments { get; init; }
}

public record EmailAttachment
{
    public string FileName { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = "application/octet-stream";
}