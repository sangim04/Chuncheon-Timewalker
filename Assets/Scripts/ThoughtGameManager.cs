using UnityEngine;
using UnityEngine.UI;

public class ThoughtGameManager : MonoBehaviour
{
    public static ThoughtGameManager Instance;

    [Header("Camera & Spawner")]
    public Transform playerCamera;
    public ThoughtSpawner thoughtSpawner;

    [Header("UI Elements")]
    public GameObject gaugeUI;
    public Slider gaugeBar;
    public GameObject mainPanel;
    public GameObject howPanel;
    public GameObject summaryPanel;

    [Header("Buttons")]
    public Button startButton;
    public Button howButton;
    public Button backButton;
    public Button quitButton;

    private float gaugeValue = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainPanel.SetActive(true);
        howPanel.SetActive(false);
        summaryPanel.SetActive(false);
        gaugeUI.SetActive(false);

        // 버튼 이벤트 등록
        startButton.onClick.AddListener(OnStartClicked);
        howButton.onClick.AddListener(OnHowClicked);
        backButton.onClick.AddListener(OnBackClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    // 🎮 게임 시작 버튼
    void OnStartClicked()
    {
        Debug.Log("🎮 게임 시작!");
        mainPanel.SetActive(false);
        howPanel.SetActive(false);
        summaryPanel.SetActive(false);

        // 게이지 초기화 및 표시
        gaugeValue = 0f;
        gaugeBar.value = 0f;
        gaugeUI.SetActive(true);

        // 잡념 생성 시작
        if (thoughtSpawner != null)
        {
            Vector3 forward = playerCamera != null ? playerCamera.forward : Camera.main.transform.forward;
            thoughtSpawner.BeginSpawn();
        }
    }

    // 📘 방법 보기
    void OnHowClicked()
    {
        mainPanel.SetActive(false);
        howPanel.SetActive(true);
    }

    // ⬅️ 돌아가기
    void OnBackClicked()
    {
        howPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ❌ 종료
    void OnQuitClicked()
    {
        Debug.Log("👋 게임 종료");
        Application.Quit();
    }

    // 🧠 ThoughtBehavior에서 호출될 함수
    public void OnThoughtTouched(ThoughtBehavior thought)
    {
        if (thought.isGoodThought)
            gaugeValue += 10f;
        else
            gaugeValue -= 10f;

        gaugeValue = Mathf.Clamp(gaugeValue, 0f, 100f);

        if (gaugeBar != null)
            gaugeBar.value = gaugeValue / 100f;

        // 게이지가 다 차면 요약 패널 표시
        if (gaugeValue >= 100f)
        {
            Debug.Log("🧘 마음이 맑아졌습니다!");
            ShowSummary();
        }
    }

    void ShowSummary()
    {
        gaugeUI.SetActive(false);
        summaryPanel.SetActive(true);
    }

    public bool IsGaugeFull()
    {
        return gaugeValue >= 100f;
    }
}
