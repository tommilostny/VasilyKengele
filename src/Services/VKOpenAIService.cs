namespace VasilyKengele.Services;

public class VKOpenAIService
{
    private readonly IOpenAIService? _openAIService;
    private readonly Random _random = new();
    private readonly string[] _adjectives = new[]
    {
        "Insightful", "Enlightening", "Capitalist", "Socialist",
        "Intriguing", "Uplifting", "Provocative", "Crazy",
        "Thought-provoking", "Stimulating", "Remarkable",
        "Funny", "Sad", "Anarchist", "Rebelious", "Stupid",
        "Compelling", "Stimulating", "Profound", "Informal",
        "Affecting", "Moving", "Encouraging", "Pertinent",
        "Irrelevant", "Diverting", "Innovative", "Idiotic",
        "Gratifying", "Edifying", "Technical", "Programming"
    };

    public VKOpenAIService(VKConfiguration configuration, IOpenAIService openAIService)
    {
        if (configuration.OpenAI.Enabled)
        {
            _openAIService = openAIService;
        }
    }

    /// <summary>
    /// Task that calls Open AI API to generate a bird quote.
    /// The API is not going to be called if no message is about to be sent
    /// (i.e. there is no user in a time zone where it is currently 5 AM).
    /// </summary>
    public async Task<string> GenerateQuoteAsync()
    {
        if (_openAIService is not null)
        {
            var nextAdjective = _adjectives[_random.Next(0, _adjectives.Length)];
        
            var completion = await _openAIService.Completions.CreateCompletion(new()
            {
                Prompt = $"{nextAdjective} bird quote for the day {DateTime.UtcNow}:",
                Model = Models.TextDavinciV3,
                Temperature = 1.0f,
                MaxTokens = 300
            });
            if (completion.Successful)
            {
                return completion.Choices.First().Text;
            }
        }
        return string.Empty;
    }
}
