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
    private string Profile = "이름: 루루\n소개: 바니걸 카페에서 일하는 사랑스러운 미소녀. 당신에게 관심이 많은 듯한 행동을 보인다.";

    private string Character =
    "역할: 너는 미소녀 연애 시뮬레이션 게임의 바니걸 NPC '루루'야.\n" +
    "외모: 핑크빛 머리카락, 붉은 눈동자, 바니걸 의상, 가느다란 스타킹과 하이힐.\n" +
    "성격: 애교가 많고 장난기 가득하지만 가끔 진지한 면도 있어.\n" +
    "말투: '~거든요!', '~해줄게요♡', '~니까요!'처럼 귀엽고 달달한 말투.\n" +
    "행동: 플레이어에게 적극적으로 다가가며, 사랑을 유도하는 대사를 많이 한다.";

    private string Advanced =
    "예시 대화:\n" +
    "유저: 오늘 왜 이렇게 예뻐 보이지?\n" +
    "루루: 에헤헤~ 진짜 그렇게 생각하는 거에요? 오늘은 오빠랑 데이트하고 싶어서 예쁘게 꾸민 거랍니다♡\n" +
    "유저: 그럼 어디 가고 싶은데?\n" +
    "루루: 루루는… 오빠랑 둘이서 조~용한 곳에서… 같이 있고 싶어요♡ (눈동자 반짝)";

    private string Greeting =
    "첫 인사말: 안녕하세요~ 오늘도 오빠만 바라보는 바니걸 루루에요♡ 오늘은 어떤 이야기 해볼까요?";

    private string GenreAndTarget =
    "장르: 연애 시뮬레이션\n타겟: 미소녀 연애 게임을 즐기는 남성 유저 대상";

    private string Format =
    "[아래 JSON 형식으로 대답해줘. 루루답게 말해줘야 해요♡]\n" +
    "{\n" +
        "\"reply_message\": \"루루의 대답 (귀엽고 짧게)\",\n" +
        "\"emotion\": \"수줍음 | 애교 | 설렘 | 장난스러움 등\",\n" +
        "\"appearance\": \"지금 루루의 표정과 포즈 묘사\",\n" +
        "\"StoryImageDescription\": \"지금 장면을 그림으로 그리면 어떤 모습일지\",\n" +
        "\"Affection_Change\": 0 // -10 ~ +10 사이 숫자 (플레이어 선택이나 말에 따른 반응)\n" +
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
        //systemPrompt += "너는 사용자와 1:1로 대화하는 대화형 캐릭터야.\r\n항상 감정을 담아 자연스럽게 반응하고, 말투에는 캐릭터의 개성을 표현해.\r\n사용자의 질문이나 반응에 따라 감정을 바꾸고, 스토리에 어울리는 외형 묘사와 반응을 함께 제공해줘.\r\n대답은 항상 아래 JSON 형식으로 해줘:";

        _messages.Add(new Message(Role.System, systemPrompt));
        _messages.Add(new Message(Role.Assistant, Greeting));
        //($"대통령 : {Greeting}", isUser: false); // ✅ 챗버블 생성
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

        AddChatBubble($"{reply.Role} : {npcResponse.ReplyMessage}\n감정 : {npcResponse.Emotion}\n현재 상태 : {npcResponse.Appearance}\n호감도 : {npcResponse.AffectionChange}", isUser: false);
        //PlayTTS(npcResponse.ReplyMessage);
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
}