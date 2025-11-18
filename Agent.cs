
class Agent
{
    private List<Message> messages;
    private string model;

    public Agent(string roleDescription, string model)
    {
        messages = new List<Message>
        {
            new("system", roleDescription),
        };
        this.model = model;
    }

    public async Task<string> Chat()
    {
        return await Chat("");
    }
    
    public async Task<string> Chat(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            messages.Add(new Message("user", msg));
        }
        var req = new Ollama.ChatRequest(
            model,
            messages,
            stream: false
        );
        var response = await Ollama.PostChat(req);
        var reply = response.message.content;
        messages.Add(new Message("assistant", reply));
        return reply;
    }

    public string GetAllMessagesBlob()
    {
        return string.Join(", ", messages.Select(m => $"{m.role}: {m.content}\n"));
    }
    
    public string GetLatestMessage()
    {
        return messages.Last().content;
    }
}