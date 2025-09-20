using System.Text;
using System.Text.Json;
using backend.Services.IServices;

namespace backend.Services;
public class IAService : IIAService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public IAService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["OpenAI:ApiKey"];
    }

    public async Task<(int score, string analyse)> EvaluerDepistageAsync(List<(string Question, string Reponse)> questionsEtReponses)
    {
        try
        {
            // 1. Validation des entrées
            if (questionsEtReponses == null || !questionsEtReponses.Any())
            {
                throw new ArgumentException("Aucune donnée à analyser");
            }

            // 2. Construction du prompt
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Analyse médicale - réponds UNIQUEMENT au format JSON: {\"score\": 0-100, \"analyse\": \"texte\"}");
            promptBuilder.AppendLine("Questions/Réponses:");

            foreach (var qr in questionsEtReponses)
            {
                promptBuilder.AppendLine($"- {qr.Question} : {qr.Reponse}");
            }

            // 3. Configuration de la requête
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new
                {
                    role = "system",
                    content = "Expert médical. Donne un score de risque de 0 à 100 (100=max risque) " +
                              "et une analyse médicale détaillée. Format JSON strict: {\"score\": number, \"analyse\": string}"
                },
                new { role = "user", content = promptBuilder.ToString() }
            },
                response_format = new { type = "json_object" },
                temperature = 0.3,
                max_tokens = 800
            };

            // 4. Envoi de la requête
            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                requestContent);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erreur API: {response.StatusCode} - {responseContent}");
            }

            // 5. Traitement de la réponse
            using var responseDoc = JsonDocument.Parse(responseContent);
            var messageContent = responseDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Désérialisation robuste avec vérifications
            using var resultDoc = JsonDocument.Parse(messageContent);
            var root = resultDoc.RootElement;

            if (!root.TryGetProperty("score", out var scoreElement) ||
                !root.TryGetProperty("analyse", out var analyseElement))
            {
                throw new Exception("Réponse IA incomplète - format JSON invalide");
            }

            var scoreSur100 = scoreElement.GetInt32();
            var analyse = analyseElement.GetString() ?? "Analyse non fournie";

            // Conversion du score sur 10 (arrondi à l'entier le plus proche)
            var scoreSur10 = (int)Math.Round(scoreSur100 / 10.0);

            Console.WriteLine($"Score: {scoreSur100}/100 → {scoreSur10}/10");
            Console.WriteLine($"Analyse: {analyse}");

            return (scoreSur10, analyse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur dans EvaluerDepistageAsync: {ex.Message}");
            throw;
        }
    }
}
