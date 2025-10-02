using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;   // ✅ TextMeshPro 사용을 위한 네임스페이스 추가

public class QuizManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;   // 메인 화면
    public GameObject quizPanel; // 퀴즈 화면
    public GameObject summaryPanel; // 결과 화면
    public GameObject howtoPanel;

    [Header("Quiz UI")]
    public TextMeshProUGUI quizBox;   // 문제 텍스트
    public TextMeshProUGUI mark;      // 정답/오답 표시
    public Button oButton;
    public Button xButton;

    [Header("Summary UI")]
    public TextMeshProUGUI scoreText;
    public Button retryButton;

    [Header("Main UI")]
    public Button startButton;
    public Button howtoplayButton;

    [Header("Explain UI")]
    public Button backButton;
    // 전체 20문제 (질문, 정답, 해설)
    private List<(string question, bool answer, string explanation)> allQuestions =
        new List<(string, bool, string)>()
    {
        ("향교(鄕校)는 조선시대 지방에서 학생들을 가르치던 교육 기관이다.", true, "향교는 지방에 설치된 관립 교육기관으로 유교 경전과 유학 교육을 담당했다."),
        ("명륜당(明倫堂)은 향교에서 제사를 지내는 공간이다.", false, "명륜당은 강당(강학 공간)이고, 제사는 대성전에서 진행되었다."),
        ("대성전(大成殿)은 공자와 성현에게 제사를 지내던 공간이다.", true, "대성전은 공자 및 성현의 위패를 봉안하고 제사를 올리는 공간이다."),
        ("춘천향교는 현재 보존되어 있으며, 국가 지정 문화재로 보호받고 있다.", true, "춘천향교는 1985년 강원도 유형문화재로 지정되었으며 현재도 보존 중이다."),
        ("향교에서 학생들은 활쏘기와 말타기만을 배웠다.", false, "향교 교육은 유교 경전, 윤리, 문학, 예절 중심이었으며 무예 활동은 주요 과정이 아니었다."),
        ("조선의 향교는 교육뿐만 아니라 제례도 함께 담당했다.", true, "향교는 유교 경전 교육뿐 아니라 석전제 등 제례를 담당하였다."),
        ("“명륜(明倫)”은 사람 사이의 도리를 밝힌다는 뜻이다.", true, "‘명’은 밝을 명, ‘륜’은 도리 윤으로 사람 간 도리를 밝힌다는 뜻을 지닌다."),
        ("향교의 학생들은 “유생(儒生)”이라 불렸다.", true, "향교 및 서원에서 유학을 배우던 학생들을 유생이라 불렀다."),
        ("향교에서는 과거시험을 직접 치렀다.", false, "과거시험은 별도의 시험장에서 시행되었으며 향교에서는 치르지 않았다."),
        ("대성전은 향교에서 가장 중요한 교육 공간이다.", false, "대성전은 제례 공간이며 교육은 명륜당이 담당했다."),
        ("춘천향교 같은 향교는 고려시대부터 설치되었다.", true, "향교는 고려시대부터 존재했으며 조선에서 제도가 정비되었다."),
        ("춘천향교는 임진왜란 이후 재건된 기록이 있다.", true, "춘천향교는 임진왜란 때 소실되었고 1594년에 중건되었다."),
        ("춘천향교는 명륜당, 동재/서재, 동무/서무, 내삼문 등을 갖추고 있다.", true, "춘천향교는 명륜당, 동재·서재, 동무·서무, 대성전 등을 포함한다."),
        ("춘천향교의 대성전에는 5성, 송조 2현, 우리나라 18현의 위패가 봉안되어 있다.", true, "공자를 비롯한 성현과 송·조선의 현인들의 위패가 봉안되어 있다."),
        ("춘천향교는 교과서를 보유하고 있지 않다.", false, "춘천향교는 판본 27종 138책 등 다수의 전적을 소장하고 있다."),
        ("춘천향교 향약 관련 문서는 유향소 운영과 관련된 자료이다.", true, "춘천향교에는 향약, 향안 등 유향소 운영과 관련된 기록이 전한다."),
        ("춘천향교는 조선시대에 최초로 건립된 향교이다.", false, "춘천향교는 조선 후기 중건된 향교 중 하나이며 최초 향교는 아니다."),
        ("춘천향교 명륜당은 오늘날 교육 및 문화 활동에 사용된다.", true, "명륜당은 현재 문화·교육 프로그램에 활용되고 있다."),
        //("", false, ""), 20번까지
        //("", true, ""),
    };

    private List<(string question, bool answer, string explanation)> quiz; // 랜덤으로 뽑힌 10문제
    private int currentIndex = 0;
    private int score = 0;

    void Start()
    {
        // 메인 화면 먼저 보여주기
        mainPanel.SetActive(true);
        quizPanel.SetActive(false);
        summaryPanel.SetActive(false);
        howtoPanel.SetActive(false);

        startButton.onClick.AddListener(StartQuiz); // 버튼 연결
        retryButton.onClick.AddListener(RestartGame);
        backButton.onClick.AddListener(GoToMainFromE);
        howtoplayButton.onClick.AddListener(HowtoPlayPanel);
    }

    void StartQuiz()
    {
        // 퀴즈 문제 랜덤 선택
        quiz = allQuestions.OrderBy(x => Random.value).Take(10).ToList();
        currentIndex = 0;
        score = 0;

        mainPanel.SetActive(false);
        quizPanel.SetActive(true);

        ShowQuestion();

        oButton.onClick.RemoveAllListeners();
        xButton.onClick.RemoveAllListeners();
        oButton.onClick.AddListener(() => OnAnswer(true));
        xButton.onClick.AddListener(() => OnAnswer(false));
    }

    void ShowQuestion()
    {
        var q = quiz[currentIndex];
        quizBox.text = $"Q{currentIndex + 1}. {q.question}"; //(O/X)
        mark.text = "";
        oButton.interactable = true;
        xButton.interactable = true;
    }

    void OnAnswer(bool choice)
    {
        var q = quiz[currentIndex];
        bool isCorrect = (choice == q.answer);
        if (isCorrect)
        {
            score++;
            mark.text = "정답입니다!"; //⭕ 
            mark.color = Color.green;
        }
        else
        {
            mark.text = "오답입니다!"; //❌ 
            mark.color = Color.red;
        }

        oButton.interactable = false;
        xButton.interactable = false;

        Invoke("NextQuestion", 1.5f); // 1.5초 뒤 자동으로 다음 문제
    }

    void NextQuestion()
    {
        currentIndex++;
        if (currentIndex < quiz.Count)
        {
            ShowQuestion();
        }
        else
        {
            ShowSummary();
        }
    }

    void ShowSummary()
    {
        quizPanel.SetActive(false);
        summaryPanel.SetActive(true);

        scoreText.text = $"{score*10}점"; //$"10점: {score} / {quiz.Count} 정답";
    }

    // 다시하기 → 현재 씬 리로드
    void RestartGame()
    {
        // 결과 화면 닫고
        summaryPanel.SetActive(false);
        // 퀴즈 다시 시작
        StartQuiz();
    }


    // 메인으로 돌아가기 → Main Scene 로드
    public void GoToMainFromE()
    {
        //SceneManager.LoadScene("quiz");
        howtoPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void HowtoPlayPanel()
    {
        mainPanel.SetActive(false);
        howtoPanel.SetActive(true);
    }
}
