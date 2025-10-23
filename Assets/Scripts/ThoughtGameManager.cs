using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThoughtGameManager : MonoBehaviour
{
    [Header("Camera & Spawner")]
    public Transform playerCamera;
    public ThoughtSpawner thoughtSpawner;

    [Header("UI Elements")]
    public GameObject gaugeUI;
    public Slider gaugeBar;
    public GameObject mainPanel;
    public GameObject howPanel;
    public GameObject summaryPanel;

    [Header("Summary Panel Elements")]
    public TextMeshProUGUI scoreText;   // ScoreText true
    public TextMeshProUGUI scoreMsg;    // scoremsg

    [Header("Buttons")]
    public Button startButton;
    public Button howButton;
    public Button backButton;
    public Button quitButton;
    public Button retryButton;          // SummaryPanel 안 Retry 버튼

    private float gaugeValue = 20f;     // 🎯 초기 게이지 (0~100)
    private bool gameEnded = false;

    public static ThoughtGameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 초기 UI 상태
        mainPanel.SetActive(true);
        howPanel.SetActive(false);
        summaryPanel.SetActive(false);
        gaugeUI.SetActive(false);

        // 버튼 이벤트 등록
        startButton.onClick.AddListener(OnStartClicked);
        howButton.onClick.AddListener(OnHowClicked);
        backButton.onClick.AddListener(OnBackClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        retryButton.onClick.AddListener(OnRetryClicked);
    }

    // 🎮 게임 시작
    void OnStartClicked()
    {
        Debug.Log("🎮 게임 시작!");
        mainPanel.SetActive(false);
        howPanel.SetActive(false);
        summaryPanel.SetActive(false);
        gaugeUI.SetActive(true);

        gameEnded = false;
        gaugeValue = 20f; // ✅ 시작 게이지 20
        gaugeBar.value = gaugeValue / 100f;

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

    // 🔁 다시하기
    void OnRetryClicked()
    {
        Debug.Log("🔁 다시 시작");
        summaryPanel.SetActive(false);
        OnStartClicked(); // 게임 재시작
    }

    // ❌ 종료
    void OnQuitClicked()
    {
        Debug.Log("👋 게임 종료");
        Application.Quit();
    }

    // 🧠 잡념 터치 결과 반영
    public void OnThoughtTouched(ThoughtBehavior thought)
    {
        if (gameEnded) return;

        if (thought.isGoodThought)
            gaugeValue += 10f;
        else
            gaugeValue -= 10f;

        gaugeValue = Mathf.Clamp(gaugeValue, 0f, 100f);
        gaugeBar.value = gaugeValue / 100f;

        // ✅ 게이지 상태 확인
        if (gaugeValue >= 100f)
        {
            ShowSummary(true);  // 성공
        }
        else if (gaugeValue <= 0f)
        {
            ShowSummary(false); // 실패
        }
    }

    // 🎯 결과창 표시
// 🎯 결과창 표시
void ShowSummary(bool success)
{
    gameEnded = true;
    gaugeUI.SetActive(false);
    summaryPanel.SetActive(true);

    // 스폰 중지
    if (thoughtSpawner != null)
    {
        thoughtSpawner.StopSpawn();

        // ✅ 남아있는 모든 잡념 제거
        foreach (Transform t in thoughtSpawner.transform)
        {
            Destroy(t.gameObject);
        }

        // ✅ activeThoughts 리스트 클리어 (스포너 내부 변수 접근 버전)
        thoughtSpawner.ClearAllThoughts();
    }

    if (success)
    {
        scoreText.text = "성공 🎉";
        scoreMsg.text = "당신의 마음이 맑고 평온해졌습니다.";
    }
    else
    {
        scoreText.text = "실패 😔";
        scoreMsg.text = "잡념에 휩싸여 집중력을 잃었습니다.";
    }

    Debug.Log($"📊 Game Ended: {(success ? "Success" : "Fail")}, Final Gauge={gaugeValue}");
}



    public bool IsGaugeFull() => gaugeValue >= 100f;
}
