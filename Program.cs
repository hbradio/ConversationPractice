using Spectre.Console;

public record Message(string role, string content);

class Program
{
    private static readonly string MODEL = "gpt-oss:20b";
    static async Task Main()
    {
        AnsiConsole.MarkupLine($"[green]Beginning chat with[/] [blue italic]{MODEL}[/]");

        var facilitator = new Agent(
            "You are a skilled Spanish teacher who can answer questions in English.",
            MODEL);
        var conversationPartner = new Agent(
            @"Please roleplay that you are a person in a train station, needing to know which train goes downtown.
            Please continue the conversation after I tell you where the train is.
            You are not an assistant.
            You speak Mexican Spanish and no English.",
            MODEL);

        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("grey"))
            .StartAsync("...", async ctx =>
        {
            return await conversationPartner.Chat();
        });
        AnsiConsole.MarkupLine($"\n> [grey]{reply}[/]\n");

        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput))
                continue;
            if (userInput == "/quit")
                break;

            if (userInput.StartsWith("?"))
            {
                HandleHelpRequest(userInput, facilitator, conversationPartner);
            } else
            {
                HandleConversation(userInput, conversationPartner);
            }
        }
    }

    async static void HandleHelpRequest(string msg, Agent helperAgent, Agent conversationPartner)
    {
        var prompt = $@"A student learning Spanish is having a conversation with a partner and has a question.
        Please answer their question, which is: ```{msg}```. 
        Please answer in English.
        The most recent message in the conversation is from the conversation
        partner and was this: \n\n ```{conversationPartner.GetLatestMessage()}```.";
        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await helperAgent.Chat(prompt);
        });

        // Tts.SpeakText(reply, 200);
        AnsiConsole.MarkupLine($"\n> [blue]{reply}[/]\n");
        // Tts.SpeakText(reply, 150);

    }

    async static void HandleConversation(string msg, Agent agent)
    {
        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("grey"))
            .StartAsync("...", async ctx =>
        {
            return await agent.Chat(msg);
        });

        // Tts.SpeakText(reply, 200);
        AnsiConsole.MarkupLine($"\n> [grey]{reply}[/]\n");
        // Tts.SpeakText(reply, 150);

    }
}
