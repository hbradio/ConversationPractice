using Spectre.Console;

public record Message(string role, string content);

class Program
{
    private static readonly string MODEL = "gpt-oss:20b";
    private static readonly int NORMAL_WORDS_PER_SECOND = 175;
    private static readonly int SLOW_WORDS_PER_SECOND = 150;
    static async Task Main()
    {
        AnsiConsole.Write(new Panel(
            new Markup($@"[grey]Do you have a lesson you'd like to practice? If so, give me a path to the pdf. Otherwise, just hit enter. [/]")
            ).Header("[blue]Teacher: Hello and welcome![/]")
            );
        Console.Write("> ");
        string? lessonPath = Console.ReadLine();

        var lessonText = "";
        if (!string.IsNullOrEmpty(lessonPath))
        {
           lessonText = Pdf.ReadPdf(lessonPath);
        }

        var scenarioCreator = new Agent("", MODEL);
        
        var lessonSummary = "";
        if (!string.IsNullOrEmpty(lessonText)) {
            lessonSummary = await AnsiConsole.Status()
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync("...", async ctx =>
            {
                return await scenarioCreator.Chat(@$"Below is a lesson from a Spanish textbook.
                Give me a very brief list of the vocabulary, grammar, and/or tenses introduced.
                {lessonText}");
            });
        }

        var safeLessonSummary = Markup.Escape(lessonSummary);
        AnsiConsole.Write(new Panel(
            new Markup($@"[grey]{safeLessonSummary} [/]")
            ).Header("[blue]Teacher: Great! Here's what we'll focus on.[/]")
        );

        var scenario = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await scenarioCreator.Chat($@"Come up with a scenario in which two strangers meet in public.
            As your reply, give instructions for roleplaying one of the people, filling in this template: “You are <description of person>, who is <description of where she is and what is happening>.”
            Include *only* this completed template as your response.
            Complete the template only in English.
            Below is a summary of a language lesson. Try to choose a scenario that could use vocabulary and grammar covered in it.
            {lessonSummary}");

        });
        AnsiConsole.Write(new Panel(
            new Markup($"[grey]{scenario}[/]")
            ).Header("[blue]Teacher: I gave your conversation partner this scenario.[/]")
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

        if (!string.IsNullOrWhiteSpace(lessonSummary)) {
           conversationPartnerPrompt += $" Try to use vocabulary and tenses from this lesson: ```{lessonSummary}```";
        }
        var conversationPartner = new Agent(conversationPartnerPrompt, MODEL);

        var faciliatorPrompt = $@"You are a skilled Spanish teacher who can answer questions in English.";
        if (!string.IsNullOrWhiteSpace(lessonSummary)) {
            faciliatorPrompt += $" The student is studying this lesson: ```{lessonSummary}```";
        }
        var facilitator = new Agent(faciliatorPrompt, MODEL);

        var reply = await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("...", async ctx =>
        {
            return await conversationPartner.Chat();
        });
        Tts.SpeakText(reply, NORMAL_WORDS_PER_SECOND);
        AnsiConsole.MarkupLine($"\n> [palegreen3_1]{reply}[/]\n");
        Tts.SpeakText(reply, SLOW_WORDS_PER_SECOND);

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

        Tts.SpeakText(reply, NORMAL_WORDS_PER_SECOND);
        AnsiConsole.MarkupLine($"\n> [palegreen3_1]{reply}[/]\n");
        Tts.SpeakText(reply, SLOW_WORDS_PER_SECOND);

    }
}
