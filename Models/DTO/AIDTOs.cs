public class AiMessage
{
    public string? role { get; set; }
    public string? content { get; set; }
}

public class AiRequest
{
    public string? model { get; set; }
    public List<AiMessage>? messages { get; set; }
}
