using System.Net.Http.Json;
using Spectre.Console;

public record ChatRequest(
    string model,
    List<Message> messages,
    int[]? context = null,
    bool stream = false
);

public record Message(string role, string content);

public record ChatResponse(
    string model,
    Message message,
    int[] context
);

class Program
{

    private static readonly string MODEL = "gpt-oss:20b";
    private static readonly HttpClient http = new()
    {
        BaseAddress = new Uri("http://localhost:11434")
    };

    static async Task Main()
    {
        AnsiConsole.MarkupLine($"[green]Beginning chat with[/] [blue italic]{MODEL}[/]");

        var messages = new List<Message>
        {
            new("system", "You are a helpful assistant."),
        };

        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput))
                continue;
            if (userInput == "/quit")
                break;

            messages.Add(new Message("user", userInput));

            var req = new ChatRequest(
                model: MODEL,
                messages,
                stream: false
            );

            var response = await AnsiConsole.Status()
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("...", async ctx =>
            {
                return await PostChat(req);
            });

            AnsiConsole.MarkupLine($"\n> [grey]{response.message.content}[/]\n");

            messages.Add(new Message("assistant", response.message.content));
        }
    }

    static async Task<ChatResponse> PostChat(ChatRequest req)
    {
        using var result = await http.PostAsJsonAsync("/api/chat", req);
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<ChatResponse>() ?? throw new Exception("Failure");
        return response;
    }

}