using Newtonsoft.Json;

public class NPCResponse
{
    [JsonProperty("reply_message")]
    public string ReplyMessage { get; set; }

    [JsonProperty("appearance")]
    public string Appearance { get; set; }

    [JsonProperty("emotion")]
    public string Emotion { get; set; }

    [JsonProperty("StoryImageDescription")]
    public string StoryImageDescription { get; set; }
}
