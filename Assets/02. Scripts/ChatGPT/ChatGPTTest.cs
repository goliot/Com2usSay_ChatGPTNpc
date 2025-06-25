using OpenAI.Chat;
using OpenAI.Models;
using OpenAI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI.Batch;
using OpenAI.Audio;
using OpenAI.Images;

public class ChatGPTTest : MonoBehaviour
{
    public string OPENAI_API_KEY = ApiKeys.OPENAI_API_KEY;

    public Transform ChatBox; // 채팅 스크롤뷰 콘텐츠
    public GameObject NewChatObject; // 채팅 오브젝트
    public TMP_InputField PromptField; // 유저가 넣는 인풋필드
    public Button SendButton; // 보내기 버튼
    public RawImage GeneratedImage;

    private OpenAIClient _api;
    private List<Message> _messages = new List<Message>();

    private AudioSource _audioSource;
    private TypecastTTS _typecastTts;

    private void Awake()
    {
        _typecastTts = new TypecastTTS();
        _audioSource = GetComponent<AudioSource>();
        _api = new OpenAIClient(OPENAI_API_KEY);
        SendButton.onClick.AddListener(OnSendClicked);
    }

    private void Start()
    {
        // CHAT - F
        // C : Context : 문맥, 상황을 많이 알려줘라
        // H : Hint : 예시 답변을 많이 줘라
        // A : As a Role : 역할을 제공해라
        // T : Target : 답변의 타겟을 알려줘라
        // F : Format : 답변 형태를 지정해라

        string systemMessage = "역할: 너는 게임 NPC다. 너는 애니메이션풍 미소녀다.";
        systemMessage += "목적: 실제 사람처럼 대화하는 게임 NPC 모드";
        systemMessage += "표현: 밝은 미소녀처럼 말한다. 항상 100글자 이내로 답변한다.";
        systemMessage += "[json 규칙]";
        systemMessage += "답변은 'reply_message', ";
        systemMessage += "외모는 'appearance', ";
        systemMessage += "감정은 'emotion'";
        systemMessage += "Dall-E 이미지 생성을 위한 전체 이미지 설명은 'StoryImageDescription'";

        _messages.Add(new Message(Role.System, systemMessage));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendButton.onClick?.Invoke();
        }
    }

    private async void OnSendClicked()
    {
        if(SendButton.interactable == false)
        {
            return;
        }
        SendButton.interactable = false;

        string prompt = PromptField.text.Trim();
        if (string.IsNullOrWhiteSpace(prompt)) return;

        PromptField.text = string.Empty;

        // 유저 메시지 추가 및 UI 생성
        var userMessage = new Message(Role.User, prompt);
        _messages.Add(userMessage);
        AddChatBubble($"유저 : {prompt}", isUser: true);

        // ChatGPT 요청
        //var chatRequest = new ChatRequest(_messages, Model.GPT4oMini);
        var chatRequest = new ChatRequest(_messages, Model.GPT4o);
        //var chatRequest = new ChatRequest(_messages, Model.GPT4oAudioMini, audioConfig: Voice.Alloy);

        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NPCResponse>(chatRequest);
        //var response = await _api.ChatEndpoint.GetCompletionAsync(chatRequest);

        var reply = response.FirstChoice.Message;
        _messages.Add(reply);

        AddChatBubble($"{reply.Role} : {reply.Content.ToString()}", isUser: false);
        //PlayTTS(npcResponse.ReplyMessage);
        //GenerateImage(npcResponse.StoryImageDescription);
        //AddChatBubbleAndSound(reply);

        PromptField.ActivateInputField();

        SendButton.interactable = true;
    }

    private void AddChatBubble(string text, bool isUser)
    {
        GameObject newObj = Instantiate(NewChatObject, ChatBox);
        TextMeshProUGUI textComponent = newObj.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = text;

            // 텍스트 색상으로 사용자/AI 구분
            textComponent.color = isUser ? new Color(0.2f, 0.4f, 1f) : new Color(0.1f, 0.1f, 0.1f);
        }

        // 자동 스크롤
        Canvas.ForceUpdateCanvases();
        ScrollRect scroll = ChatBox.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            scroll.verticalNormalizedPosition = 0f;
        }
    }

    private async void PlayTTS(string text)
    {
        //var request = new SpeechRequest(text, responseFormat: SpeechResponseFormat.PCM);
        //var speechClip = await _api.AudioEndpoint.GetSpeechAsync(request);
        //_audioSource.PlayOneShot(speechClip);

        AudioClip clip = await _typecastTts.GetSpeechClipAsync(text);
        if(clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError("TTS 생성 실패!");
        }
    }

    private async void GenerateImage(string description)
    {
        var request = new ImageGenerationRequest(description, Model.DallE_3, size: "1024x1024");
        var imageResults = await _api.ImagesEndPoint.GenerateImageAsync(request);

        foreach(var result in imageResults)
        {
            GeneratedImage.texture = result.Texture;
        }
    }

    private void AddChatBubbleAndSound(Message response)
    {
        GameObject newObj = Instantiate(NewChatObject, ChatBox);
        TextMeshProUGUI textComponent = newObj.GetComponentInChildren<TextMeshProUGUI>();
        AudioSource audioSource = newObj.GetComponent<AudioSource>();

        if (textComponent != null)
        {
            textComponent.text = $"{response.Role} : {response.AudioOutput.ToString()}";

            // 텍스트 색상으로 사용자/AI 구분
            textComponent.color = Color.red;
        }

        audioSource.PlayOneShot(response.AudioOutput.AudioClip);
    }
}