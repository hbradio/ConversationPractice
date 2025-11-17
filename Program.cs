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
    static async Task Main()
    {
        AnsiConsole.MarkupLine($"[green]Beginning chat with[/] [blue italic]{MODEL}[/]");

        var messages = new List<Message>
        {
            new("system", "You are a person in a train station, needing to know which train goes downtown. You speak Mexican Spanish and no English."),
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
                return await Ollama.PostChat(req);
            });

            var reply = response.message.content;

            Tts.SpeakText(reply, 200);
            AnsiConsole.MarkupLine($"\n> [grey]{reply}[/]\n");
            Tts.SpeakText(reply, 150);

            messages.Add(new Message("assistant", reply));
        }
    }


}