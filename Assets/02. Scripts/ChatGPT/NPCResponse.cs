using Newtonsoft.Json;

public class NPCResponse
{
    [JsonProperty("user_character")]
    public string UserCharacter { get; set; }

    [JsonProperty("ai_character")]
    public string AiCharacter { get; set; }

    [JsonProperty("battle_description")]
    public string BattleDescription { get; set; }

    [JsonProperty("winner")]
    public string Winner { get; set; }

    [JsonProperty("scene_image_prompt")]
    public string SceneImagePrompt { get; set; }
}
