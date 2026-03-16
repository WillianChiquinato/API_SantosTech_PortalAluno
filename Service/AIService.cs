using System.Net.Http.Headers;
using System.Text.Json;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Services;

public class AIService
{
    private readonly HttpClient _httpClient;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AIService(HttpClient httpClient, IConfiguration configuration, IExerciseRepository exerciseRepository)
    {
        _httpClient = httpClient;
        _exerciseRepository = exerciseRepository;
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

        return (result ?? string.Empty).Replace("\"", " ").Trim('"');
    }

    public async Task<GenerateExerciseDTO> GenerateExerciseRepeatAsync(int exerciseId, int userId, int? phaseId)
    {
        var random = new Random();

        int option = random.Next(1, 3); // 1 = multiple choice | 2 = dissertivo

        var baseExercise = await _exerciseRepository.GetByIdAsync(exerciseId);

        if (baseExercise == null)
            throw new Exception("Exercício base não encontrado");

        var prompt = $@"
Você é um gerador de exercícios para uma plataforma gamificada de programação
com público alvo crianças, jovens e adolescentes.

Crie um exercício semelhante ao abaixo.

Título: {baseExercise.Title}
Descrição: {baseExercise.Description}

Tipo de exercício: {(option == 1 ? "multiple choice" : "dissertivo")}

Responda APENAS com JSON válido.

Se for multiple choice (4 opções, apenas 1 correta):

{{
 ""title"": ""string"",
 ""description"": ""string"",
 ""options"": [
    {{
      ""id"": 1,
      ""option_text"": ""string"",
      ""isCorrect"": true
    }}
 ]
}}

Se for dissertivo:

{{
 ""title"": ""string"",
 ""description"": ""string"",
 ""video_url"": ""string"",
 ""type_exercise"": 2
}}
";

        var aiResponse = await GenerateExerciseAsync(prompt);
        aiResponse = ExtractJsonObject(aiResponse);

        var createExercise = new CreateExerciseDTO
        {
            Id = null,
            PhaseId = phaseId ?? baseExercise.PhaseId,
            Title = "",
            Description = "",
            VideoUrl = "",
            PointsRedeem = baseExercise.PointsRedeem,
            TermAt = DateTime.UtcNow.AddDays(7),
            TypeExercise = option == 1 ? Models.ExerciseType.MultipleChoice : Models.ExerciseType.OpenEnded,
            Difficulty = Models.DifficultyLevel.RepeatGenerate,
            IndexOrder = baseExercise.IndexOrder + 1,
            IsDailyTask = false,
            IsFinalExercise = false,
            ExercisePeriod = DateTime.UtcNow
        };

        int createdExerciseId;

        if (option == 1)
        {
            var mc = JsonSerializer.Deserialize<ExerciseMultipleChoiceAIResponse>(aiResponse, _jsonOptions);

            if (mc == null)
                throw new Exception("Falha ao desserializar exercício de múltipla escolha");

            createExercise.Title = mc.Title;
            createExercise.Description = mc.Description;

            var createdExercise = await _exerciseRepository.CreateAsync(createExercise);
            var resultQuestionBased = await _exerciseRepository.CreateQuestionBasedOnExerciseAsync(createdExercise.Id);

            foreach (var optionAI in mc.Options ?? Enumerable.Empty<ExerciseOptionAI>())
            {
                await _exerciseRepository.CreateMultipleChoiceOptionAsync(new CreateMultipleChoiceOptionDTO
                {
                    QuestionId = resultQuestionBased,
                    ExerciseId = createdExercise.Id,
                    OptionText = optionAI.OptionText,
                    IsCorrect = optionAI.IsCorrect
                });
            }

            createdExerciseId = createdExercise.Id;
        }
        else
        {
            var dissert = JsonSerializer.Deserialize<ExerciseDissertativeAIResponse>(aiResponse, _jsonOptions);

            if (dissert == null)
                throw new Exception("Falha ao desserializar exercício dissertativo");

            createExercise.Title = dissert.Title;
            createExercise.Description = dissert.Description;
            createExercise.VideoUrl = dissert.VideoUrl;

            var createdExercise = await _exerciseRepository.CreateAsync(createExercise);
            createdExerciseId = createdExercise.Id;
        }

        createExercise.Id = createdExerciseId;

        return new GenerateExerciseDTO
        {
            PhaseId = phaseId ?? baseExercise.PhaseId,
            UserId = userId,
            CreateExercise = createExercise,
        };
    }

    public async Task<string> GenerateExerciseAsync(string prompt)
    {
        try
        {
            var requestBody = new AiRequest
            {
                model = "llama-3.1-8b-instant",
                messages = new List<AiMessage>
                {
                    new AiMessage
                    {
                        role = "user",
                        content = prompt
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

            var resultString = json
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(resultString))
                throw new Exception("Resposta da IA veio vazia");

            return resultString;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao gerar exercício: {ex.Message}");
        }
    }

    private static string ExtractJsonObject(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new Exception("Resposta da IA veio vazia");

        var text = raw.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        var start = text.IndexOf('{');

        if (start < 0)
            throw new Exception("A resposta da IA não contém um objeto JSON válido");

        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                    inString = false;

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '{')
                depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                    return text[start..(i + 1)];
            }
        }

        throw new Exception("Não foi possível extrair um JSON completo da resposta da IA");
    }
}