using Spectre.Console;

public record ChatRequest(
    string model,
    List<Message> messages,
    int[]? context = null,
    bool stream = false
);

public record Message(string role, string content);

class Program
{
    private static readonly string MODEL = "gpt-oss:20b";
    static async Task Main()
    {
        AnsiConsole.MarkupLine($"[green]Beginning chat with[/] [blue italic]{MODEL}[/]");

        var conversationPartner = new Agent(
            "You are a person in a train station, needing to know which train goes downtown. You speak Mexican Spanish and no English.",
            MODEL);

        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput))
                continue;
            if (userInput == "/quit")
                break;

            var reply = await AnsiConsole.Status()
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("...", async ctx =>
            {
                return await conversationPartner.Chat(userInput);
            });

            Tts.SpeakText(reply, 200);
            AnsiConsole.MarkupLine($"\n> [grey]{reply}[/]\n");
            Tts.SpeakText(reply, 150);

        }
    }


}