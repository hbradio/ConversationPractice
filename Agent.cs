
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

    public async Task<string> Chat(string msg)
    {
        messages.Add(new Message("user", msg));
        var req = new ChatRequest(
            model,
            messages,
            stream: false
        );
        var response = await Ollama.PostChat(req);
        var reply = response.message.content;
        messages.Add(new Message("assistant", reply));
        return reply;
    }
}