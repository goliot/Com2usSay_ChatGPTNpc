using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RolePlaying : MonoBehaviour
{
    [Header("# GameObjects")]
    public Transform ChatBox; // 채팅 스크롤뷰 콘텐츠
    public GameObject NewChatObject; // 채팅 오브젝트
    public TMP_InputField PromptField; // 유저가 넣는 인풋필드
    public Button SendButton; // 보내기 버튼
    public RawImage GeneratedImage;

    public void AddChatBubble(string text, bool isUser)
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
}