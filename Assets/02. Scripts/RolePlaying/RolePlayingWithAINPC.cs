using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RolePlayingWithAINPC : MonoBehaviour
{

    [Header("# GameObjects")]
    public Transform ChatBox; // 채팅 스크롤뷰 콘텐츠
    public GameObject NewChatObject; // 채팅 오브젝트
    public TMP_InputField PromptField; // 유저가 넣는 인풋필드
    public Button SendButton; // 보내기 버튼
    public RawImage GeneratedImage;

    [Header("# OpenAi")]
    private OpenAIClient _api;
    private List<Message> _messages = new List<Message>();

    [Header("# Components")]
    private AudioSource _audioSource;
    private TypecastTTS _typecastTts;

    [Header("# AI Options")]
    public string Profile = "이름: 아이리스\n소개: 마법학교에서 온 활발한 여학생.";
    public string Character = "역할: 너는 마법학교의 미소녀 NPC '아이리스'다.\n" +
                       "외모: 밝은 은발머리, 파란 눈동자, 마법복을 입고 있다.\n" +
                       "성격: 명랑하고 조금 엉뚱하며, 호기심이 많다.\n" +
                       "말투: '~인 거야!', '~해줄게!'처럼 친근하고 발랄한 말투.";

    public string Advanced = "예시 대화:\n" +
                      "유저: 오늘 뭐하고 놀까?\n" +
                      "아이리스: 음~ 그러게, 같이 마법 실습하러 갈래?\n" +
                      "유저: 좋아! 어떤 마법이 재밌어?\n" +
                      "아이리스: 불꽃 마법! 근데 조심해야 해. 실수하면 머리카락 타버려!";

    public string Greeting = "첫 인사말: 안녕! 혹시 너도 마법에 관심 있는 거야?";
    public string GenreAndTarget = "장르: 로맨스\n타겟: 남성 유저 대상 대화";

    public string Format = "[json 형식으로 답변해줘]\n" +
                    "{\n" +
                    "\"reply_message\": \"대답 내용 (100자 이내)\",\n" +
                    "\"emotion\": \"기쁨 | 놀람 | 당황 | 부끄러움 등\",\n" +
                    "\"appearance\": \"지금 상황에서의 외형 묘사\",\n" +
                    "\"StoryImageDescription\": \"이 장면을 이미지로 만들면 이런 모습\"\n" +
                    "}";

    private void Awake()
    {
        _typecastTts = new TypecastTTS();
        _audioSource = GetComponent<AudioSource>();
        _api = new OpenAIClient(ApiKeys.OPENAI_API_KEY);
        SendButton.onClick.AddListener(OnSendClicked);
    }

    private void Start()
    {
        string systemPrompt =
            $"{Profile}\n{Character}\n{Advanced}\n{Greeting}\n{GenreAndTarget}\n{Format}";

        _messages.Add(new Message(Role.System, systemPrompt));
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
        if (SendButton.interactable == false)
        {
            return;
        }
        SendButton.interactable = false;

        string prompt = PromptField.text.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        PromptField.text = string.Empty;

        // 유저 메시지 추가 및 UI 생성
        var userMessage = new Message(Role.User, prompt);
        _messages.Add(userMessage);
        AddChatBubble($"유저 : {prompt}", isUser: true);

        // ChatGPT 요청
        var chatRequest = new ChatRequest(_messages, Model.GPT4o);

        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NPCResponse>(chatRequest);

        var reply = response.FirstChoice.Message;
        _messages.Add(reply);

        AddChatBubble($"{reply.Role} : {npcResponse.ReplyMessage}", isUser: false);
        PlayTTS(npcResponse.ReplyMessage);
        //GenerateImage(npcResponse.StoryImageDescription);

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
        if (clip != null)
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

        foreach (var result in imageResults)
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