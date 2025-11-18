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


    private static readonly HttpClient http = new()
    {
        BaseAddress = new Uri("http://localhost:11434")
    };

    public static async Task<ChatResponse> PostChat(ChatRequest req)
    {
        using var result = await http.PostAsJsonAsync("/api/chat", req);
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<ChatResponse>() ?? throw new Exception("Failure");
        return response;
    }
}    