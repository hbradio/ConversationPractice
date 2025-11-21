using Spectre.Console;

public record Message(string role, string content);

class Program
{
    private static readonly string MODEL = "gpt-oss:20b";
    static async Task Main()
    {
        var lessonText = Pdf.ReadPdf(@"Texts/chapter1.pdf");

        var scenarioCreator = new Agent("", MODEL);

        var scenario = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await scenarioCreator.Chat(@"
            Come up with a scenario in which two strangers meet in public.
            One is an adult male, and the other could be anyone.
            In your reply, give instructions for roleplaying the second person.
            Keep it to only a few sentences.
            Don't use much descriptive language; stay focused on the situation.
            Please describe the scenario only; you don't need to tell them to be calm or how to speak.
            Do not include any starting dialogue.");
        });
        AnsiConsole.Write(new Panel(
            new Markup($"[grey]{scenario}[/]")
            ).Header("[blue]Teacher: Hi! I gave your conversation partner this scenario.[/]")
            );

        AnsiConsole.Write(new Panel(
            new Markup("[grey]Begin with a ? to ask me a question. Otherwise, enjoy chatting with your partner! [/]")
            ).Header("[blue]Teacher: Tip[/]"));

        var conversationPartnerPrompt = $@"Please roleplay the following situation: \n\n ```{scenario}``` \n\n
            Give only one response.
            Please keep your replies brief (one or two sentences) and conversational.
            Do not include emojis.
            Do not make lists.
            You are not an assistant.
            You speak Mexican Spanish and no English.";

        if (!string.IsNullOrWhiteSpace(lessonText)) {
           conversationPartnerPrompt += $" Try to use vocabulary and tenses from this lesson: ```{lessonText}```";
        }
        var conversationPartner = new Agent(conversationPartnerPrompt, MODEL);

        var faciliatorPrompt = $@"You are a skilled Spanish teacher who can answer questions in English.";
        if (!string.IsNullOrWhiteSpace(lessonText)) {
            faciliatorPrompt += $" The student is studying this lesson: ```{lessonText}```";
        }
        var facilitator = new Agent(faciliatorPrompt, MODEL);

        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await conversationPartner.Chat();
        });
        Tts.SpeakText(reply, 200);
        AnsiConsole.MarkupLine($"\n> [palegreen3_1]{reply}[/]\n");
        Tts.SpeakText(reply, 150);

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
            }
            else
            {
                HandleConversation(userInput, facilitator, conversationPartner);
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

        AnsiConsole.Write(new Panel(new Markup($"[grey]{reply}[/]")).Header("[blue]Teacher: Great question![/]"));
    }

    async static void HandleConversation(string msg, Agent helperAgent, Agent conversationPartner)
    {

        var prompt = $@"A student learning Spanish is having a conversation with a
        partner and just said this: ```{msg}```. 
        Please give advice to correct the grammar of their reply.
        Please give the advice in English and the suggested correction in Spanish.
        If their reply is pretty good as it is, please response simply `NO ADVICE`.
        For context, the full conversation is this: ```{conversationPartner.GetAllMessagesBlob}```.";
        var helperReply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await helperAgent.Chat(prompt);
        });
        if (!helperReply.Contains("NO ADVICE"))
        {
            AnsiConsole.Write(new Panel(new Markup($"[grey]{helperReply}[/]")).Header("[blue]Teacher: Nice! Here's a tip.[/]"));
            return;
        }

        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("palegreen3_1"))
            .StartAsync("...", async ctx =>
        {
            return await conversationPartner.Chat(msg);
        });

        Tts.SpeakText(reply, 200);
        AnsiConsole.MarkupLine($"\n> [palegreen3_1]{reply}[/]\n");
        Tts.SpeakText(reply, 150);

    }
}
