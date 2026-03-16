using System.Net.Http.Headers;
using System.Text.Json;

namespace API_PortalSantosTech.Services;

public class AIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? configuration["Groq:ApiKey"]!;
    }

    public async Task<string> GenerateMotivationalMessage()
    {
        var requestBody = new AiRequest
        {
            model = "llama-3.1-8b-instant",
            messages = new List<AiMessage>
            {
                new AiMessage
                {
                    role = "user",
                    content = "Escreva uma frase motivacional de tamanho medio para estudantes de programação. Separe em duas frases."
                }
            }
        };

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions"
        );

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        httpRequest.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(httpRequest);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(responseContent);

        var json = JsonDocument.Parse(responseContent);

        var result = json
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return result.Replace("\"", " ").Trim('"');
    }
}