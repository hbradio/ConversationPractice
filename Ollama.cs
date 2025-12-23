using System.Net.Http.Json;

class Ollama
{
    public record ChatResponse(
        string model,
        Message message
    );
    public record ChatRequest(
        string model,
        List<Message> messages,
        bool stream = false
    );

    private readonly HttpClient _http;
    private readonly string _bearerToken;

    public Ollama(string baseUrl, string bearerToken)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _bearerToken = bearerToken;
    }

    public async Task<ChatResponse> PostChat(ChatRequest req)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
        {
            Content = JsonContent.Create(req)
        };
        request.Headers.Add("Authorization", $"Bearer {_bearerToken}");
        
        using var result = await _http.SendAsync(request);
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<ChatResponse>() ?? throw new Exception("Failure");
        return response;
    }
}    