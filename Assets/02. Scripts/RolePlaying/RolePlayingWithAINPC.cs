using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.PackageManager;
using System.Collections;
using System.IO;
using System;
using System.Text.RegularExpressions;

enum ChatState
{
    AwaitingName,
    AwaitingDescription,
    ReadyToSimulate
}

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
    public ComfyUIClient Client;
    private NPCResponse _npcResponse;

    private ChatState _chatState = ChatState.AwaitingName;
    private string _userCharacterName = "";
    private string _userCharacterDescription = "";

    [Header("# AI Options")]
    private string _positivePrompt = "";
    /*private string Profile = "이름: 루루\n소개: 바니걸 카페에서 일하는 사랑스러운 미소녀. 당신에게 관심이 많은 듯한 행동을 보인다.";

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
    "}";*/

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
            "다음은 캐릭터 전투 시뮬레이션이다.\n" +
            "1. 사용자는 100자 이내로 자신의 캐릭터를 설명한다.\n" +
            "2. 너도 100자 이내로 너의 캐릭터를 설명한다. 이 설명은 간결하지만 특징을 잘 담아야 한다.\n" +
            "3. 너는 두 캐릭터가 싸우는 장면을 흥미롭고 구체적으로 시뮬레이션해서 묘사한다.두 캐릭터의 전투 장면을 실감나게 묘사해. 마치 소설처럼 감정, 반전, 배경, 기술명을 넣어라.\n" +
            "대사나 행동에서 캐릭터의 성격이 드러나야 한다." +
            "비속어가 포함된 인터넷 커뮤니티식 거칠고 직설적인 말투를 사용한다." +
            "대사는 디씨인사이드 말투로, 거칠고 유쾌한 분위기를 낸다 (선택사항)." +
            "4. 전투 결과는 랜덤 요소도 포함되지만, 양쪽 설명을 기반으로 어느 정도 합리적인 결론을 내려야 한다.\n" +
            "전투 설명은 문장이 너무 길지 않게 작성하며, 중간중간 줄바꿈(\\n)을 넣어 가독성을 높인다."+
            "6. 아래 JSON 형식으로 출력:\n" +
            "{\n" +
            "  \"user_character\": \"사용자 캐릭터 요약\",\n" +
            "  \"ai_character\": \"AI 캐릭터 요약\",\n" +
            "  \"battle_description\": \"전투 시뮬레이션 장면 설명\",\n" +
            "  \"scene_image_prompt\": \"Describe the visual scene in one sentence in English, focusing on character poses, background, and action. Example: 'A masked warrior in black armor leaps through flames, clashing swords with a red-robed mage under a dark sky.'\",\n" +
            "  \"winner\": \"승리자 이름 또는 '무승부'\"\n" +
            "}";

        _messages.Add(new Message(Role.System, systemPrompt));
        AddChatBubble("Assistant : 플레이어 캐릭터의 이름을 입력하세요.", isUser: false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(SubmitAfterMoveCaretRight());
        }
    }

    private IEnumerator SubmitAfterMoveCaretRight()
    {
        yield return null; // 다음 프레임까지 기다림 (조합 확정 유도)

        // 캐럿 위치 한 칸 오른쪽으로 이동 후 다시 복구
        int originalPos = PromptField.caretPosition;
        PromptField.caretPosition = Mathf.Min(originalPos + 1, PromptField.text.Length);
        PromptField.caretPosition = originalPos;

        // 한 번 더 프레임 대기해줘야 조합이 반영되기도 함
        yield return null;

        string prompt = PromptField.text.Trim();
        if (!string.IsNullOrEmpty(prompt))
        {
            SendButton.onClick?.Invoke();
        }
    }

    private async void OnSendClicked()
    {
        if (!SendButton.interactable)
            return;
        SendButton.interactable = false;
        string prompt = PromptField.text.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
            return;

        PromptField.text = string.Empty;
        AddChatBubble($"유저 : {prompt}", isUser: true);

        switch (_chatState)
        {
            case ChatState.AwaitingName:
                _userCharacterName = prompt;
                _chatState = ChatState.AwaitingDescription;
                AddChatBubble("Assistant : 플레이어 캐릭터를 설명하세요.", isUser: false);
                SendButton.interactable = true;
                break;

            case ChatState.AwaitingDescription:
                _userCharacterDescription = prompt;
                _chatState = ChatState.ReadyToSimulate;

                AddChatBubble("Assistant : 시뮬레이션 중입니다...", isUser: false);

                string fullUserCharacter = $"{_userCharacterName} - {_userCharacterDescription}";
                _messages.Add(new Message(Role.User, fullUserCharacter));

                var chatRequest = new ChatRequest(_messages, Model.GPT4o);
                var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NPCResponse>(chatRequest);
                _npcResponse = npcResponse;
                var reply = response.FirstChoice.Message;
                _messages.Add(reply);

                AddChatBubble(
                    $"{reply.Role}:\n" +
                    $"[사용자 캐릭터] {_npcResponse.UserCharacter}\n" +
                    $"[AI 캐릭터] {_npcResponse.AiCharacter}\n\n" +
                    $"[전투 시뮬레이션]\n{_npcResponse.BattleDescription}\n\n" +
                    $"[결과] 승리자: {_npcResponse.Winner}\n",
                    isUser: false
                );
                //GenerateImage(npcResponse.SceneImagePrompt);
                PlayTTS($"승자는 {_npcResponse.Winner}!");
                //PlayTTS(npcResponse.BattleDescription);
                GenerateImageFromNPCResponse(_npcResponse.BattleDescription);

                
                break;
        }

        PromptField.ActivateInputField();
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

    private void GenerateImageFromNPCResponse(string battleDescription)
    {
        try
        {
            // 사용자 캐릭터와 AI 캐릭터 정보를 포함한 프롬프트 생성
            string prompt = $"A scene featuring {_userCharacterName}, who is {_userCharacterDescription}, fighting against " +
                            $"{_npcResponse.AiCharacter}.";

            _positivePrompt = prompt;
            AddChatBubble("\n이미지 생성중...", false);
            Debug.Log($"🎨 이미지 생성 프롬프트: {_positivePrompt}");

            StartCoroutine(Client.GenerateImageAndWait(_positivePrompt, (imagePath) =>
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Debug.Log($"✅ 이미지 생성 완료: {imagePath}");
                    StartCoroutine(LoadAndShowImageAlternative(imagePath, () =>
                    {
                        SendButton.interactable = true;
                        AddChatBubble("이미지 생성 완료\n", false);

                        // 초기화
                        _chatState = ChatState.AwaitingName;
                        _userCharacterName = "";
                        _userCharacterDescription = "";

                        AddChatBubble("--------------------\nAssistant : 새로운 시뮬레이션을 시작합니다.\n플레이어 캐릭터의 이름을 입력하세요.", isUser: false);
                    }));
                }
                else
                {
                    SendButton.interactable = true;
                    Debug.LogError("❌ 이미지 생성 실패");

                    // 초기화
                    _chatState = ChatState.AwaitingName;
                    _userCharacterName = "";
                    _userCharacterDescription = "";

                    AddChatBubble("--------------------\nAssistant : 새로운 시뮬레이션을 시작합니다.\n플레이어 캐릭터의 이름을 입력하세요.", isUser: false);
                }
            }));
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 이미지 프롬프트 생성 중 오류: {e.Message}");
        }
    }


    public IEnumerator LoadAndShowImageAlternative(string imagePath, Action onComplete = null)
    {
        if (File.Exists(imagePath))
        {
            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            GeneratedImage.texture = tex;
        }
        else
        {
            Debug.LogError("File not found: " + imagePath);
        }

        yield return null;

        // 이미지 로딩 완료 콜백 호출
        onComplete?.Invoke();
    }
    private async void PlayTTS(string text)
    {
        string cleanedText = Regex.Replace(text, @"[^\uAC00-\uD7A3\u3131-\u318E\u1100-\u11FF\u0020-\u007E]", "");
        cleanedText = cleanedText.Replace("\r", "").Replace("\n", " ").Replace("\t", " ").Trim();

        if (cleanedText.Length > 300)
            cleanedText = cleanedText.Substring(0, 300);

        Debug.Log("🔊 TTS 요청 텍스트: " + cleanedText);

        AudioClip clip = await _typecastTts.GetSpeechClipAsync(cleanedText);
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