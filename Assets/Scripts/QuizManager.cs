using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;      // 메인 화면
    public GameObject quizPanel;      // 퀴즈 화면
    public GameObject summaryPanel;   // 결과 화면
    public GameObject howtoPanel;     // 놀이 방법(Howto)

    [Header("Quiz UI")]
    public TextMeshProUGUI quizBox;   // 문제 텍스트
    public TextMeshProUGUI mark;      // 정답/오답 표시
    public Button oButton;
    public Button xButton;

    [Header("Summary UI")]
    public TextMeshProUGUI scoreText;
    public Button retryButton;
    public Button explanationButton;  // 🔹 해설 보기 버튼(요약 화면에 추가해 주세요)

    [Header("Main UI")]
    public Button startButton;
    public Button howtoplayButton;

    [Header("Howto UI (놀이 방법)")]
    public Button backButton;         // Howto → 메인으로

    [Header("Explanation UI (해설)")]
    public GameObject explanationPanel;     // 해설 전체 패널
    public Button closeExplanationButton;   // X 버튼
    public Button prevButton;               // 이전 페이지
    public Button nextButton;               // 다음 페이지
    // 왼쪽/오른쪽 페이지의 텍스트 5개씩 (Number, Question, MyAnswer, Correct, Explanation 순서)
    public TextMeshProUGUI[] leftTexts;     // 길이 5
    public TextMeshProUGUI[] rightTexts;    // 길이 5

    // 전체 문제 (질문, 정답, 해설)
    private List<(string question, bool answer, string explanation)> allQuestions =
        new List<(string, bool, string)>()
    {
        ("향교는 조선시대 지방에서 학생들을 가르치던 교육 기관이다.", true, "향교는 지방에 설치된 관립 교육기관으로 유교 경전과 유학 교육을 담당했다."),
        ("명륜당은 향교에서 제사를 지내는 공간이다.", false, "명륜당은 강학 공간이고, 제사는 대성전에서 진행되었다."),
        ("대성전은 공자와 성현에게 제사를 지내던 공간이다.", true, "대성전은 공자 및 성현의 위패를 봉안하고 제사를 올리는 공간이다."),
        ("춘천향교는 현재 보존되어 있으며, 국가 지정 문화재로 보호받고 있다.", true, "춘천향교는 1985년 강원도 유형문화재로 지정되었으며 현재도 보존 중이다."),
        ("향교에서 학생들은 활쏘기와 말타기만을 배웠다.", false, "향교 교육은 유교 경전, 윤리, 문학, 예절 중심이었으며 무예 활동은 주요 과정이 아니었다."),
        ("조선의 향교는 교육뿐만 아니라 제례도 함께 담당했다.", true, "향교는 유교 경전 교육뿐 아니라 석전제 등 제례를 담당하였다."),
        ("“명륜”은 사람 사이의 도리를 밝힌다는 뜻이다.", true, "‘명’은 밝을 명, ‘륜’은 도리 윤으로 사람 간 도리를 밝힌다는 뜻을 지닌다."),
        ("향교의 학생들은 “유생”이라 불렸다.", true, "향교 및 서원에서 유학을 배우던 학생들을 유생이라 불렀다."),
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
        ("조선시대에는 전국 대부분의 군현에 향교가 설치되었다.",true,"세종 이후 전국 모든 군현에 향교를 설치하는 것을 원칙으로 했다."),
        ("향교 생도들은 소학과 사서오경을 중심으로 학습하였다.",true,"향교 교과 과정의 중심은 소학과 사서오경이었다."),
        ("향교의 평가 제도에는 일강, 월과, 도회 등이 포함되었다.",true,"학생들은 매일 강의, 매월 시험, 정기 도회 시험을 거쳤다."),
        ("조선 후기에는 서원이 발달하면서 향교의 교육 기능이 약화되었다.",true,"서원의 성장으로 향교의 역할이 축소되었다."),
        ("조선시대 향교 입학생 정원은 고을 규모에 따라 다르게 정해졌다.", true, "부·목·군·현에 따라 정원이 차등 배정되었다."),
        ("향교는 과거시험의 합격자를 직접 배출하는 기관이었다.", false, "향교는 시험장을 제공하지 않았으며 과거는 중앙 시험장에서 치렀다."),
        ("조선 초기 태종 때 향교 설치가 장려되었다.", true, "태종은 지방 수령에게 향교 설치를 명령했다."),
    };

    // 랜덤 10문제
    private List<(string question, bool answer, string explanation)> quiz;
    // 해설 표시용(내 답 포함)
    private List<(string question, bool correctAnswer, bool playerAnswer, string explanation)> playerAnswers;

    private int currentIndex = 0;
    private int score = 0;

    // 해설 페이지
    private int currentPage = 0;
    private const int ProblemsPerPage = 2; // 좌1 + 우1

    void Start()
    {
        // 초기 표시
        mainPanel.SetActive(true);
        quizPanel.SetActive(false);
        summaryPanel.SetActive(false);
        howtoPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);

        // 버튼 리스너
        startButton.onClick.AddListener(StartQuiz);
        retryButton.onClick.AddListener(RestartGame);
        howtoplayButton.onClick.AddListener(ShowHowto);
        backButton.onClick.AddListener(CloseHowto);

        if (explanationButton) explanationButton.onClick.AddListener(ShowExplanation);
        if (closeExplanationButton) closeExplanationButton.onClick.AddListener(CloseExplanation);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
        if (nextButton) nextButton.onClick.AddListener(NextPage);
    }

    // ---------- 퀴즈 ----------
    void StartQuiz()
    {
        quiz = allQuestions.OrderBy(_ => Random.value).Take(10).ToList();
        playerAnswers = new List<(string, bool, bool, string)>();
        currentIndex = 0;
        score = 0;

        mainPanel.SetActive(false);
        howtoPanel.SetActive(false);
        summaryPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
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
        quizBox.text = $"Q{currentIndex + 1}. {q.question}";
        mark.text = "";
        oButton.interactable = true;
        xButton.interactable = true;
    }

    void OnAnswer(bool choice)
    {
        var q = quiz[currentIndex];
        bool isCorrect = (choice == q.answer);

        // 내 답 저장 (해설용)
        playerAnswers.Add((q.question, q.answer, choice, q.explanation));

        if (isCorrect)
        {
            score++;
            mark.text = "정답입니다!";
            mark.color = Color.green;
        }
        else
        {
            mark.text = "오답입니다!";
            mark.color = Color.red;
        }

        oButton.interactable = false;
        xButton.interactable = false;

        Invoke(nameof(NextQuestion), 1.5f);
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
        scoreText.text = $"{score * 10}점";
    }

    void RestartGame()
    {
        summaryPanel.SetActive(false);
        StartQuiz();
    }

    // ---------- Howto(놀이 방법) ----------
    void ShowHowto()
    {
        mainPanel.SetActive(false);
        howtoPanel.SetActive(true);
    }

    void CloseHowto()
    {
        howtoPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ---------- 해설(Explanation) ----------
    void ShowExplanation()
    {
        summaryPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(true);
        currentPage = 0;
        UpdateExplanationPage();
    }

    void CloseExplanation()
    {
        if (explanationPanel) explanationPanel.SetActive(false);
        summaryPanel.SetActive(true);
    }

    void PrevPage()
    {
        if (currentPage <= 0) return;
        currentPage--;
        UpdateExplanationPage();
    }

    void NextPage()
    {
        if ((currentPage + 1) * ProblemsPerPage >= playerAnswers.Count) return;
        currentPage++;
        UpdateExplanationPage();
    }

    void UpdateExplanationPage()
    {
        int startIdx = currentPage * ProblemsPerPage;

        // 왼쪽 페이지 채우기 (문제 n)
        if (startIdx < playerAnswers.Count)
            FillSide(leftTexts, playerAnswers[startIdx], startIdx);
        else
            ClearSide(leftTexts);

        // 오른쪽 페이지 채우기 (문제 n+1)
        if (startIdx + 1 < playerAnswers.Count)
            FillSide(rightTexts, playerAnswers[startIdx + 1], startIdx + 1);
        else
            ClearSide(rightTexts);

        // 버튼 표시 제어
        if (prevButton) prevButton.gameObject.SetActive(currentPage > 0);
        if (nextButton) nextButton.gameObject.SetActive(startIdx + 2 <= playerAnswers.Count - 1);
    }

    void FillSide(TextMeshProUGUI[] target, (string question, bool correct, bool mine, string exp) data, int idx)
    {
        if (target == null || target.Length < 4) return;

        target[0].text = $"문제 {idx + 1}";
        target[1].text = data.question;

        string myAns = data.mine ? "O" : "X";
        string corrAns = data.correct ? "O" : "X";
        string myColor = (data.mine == data.correct) ? "#008000" : "#FF0000"; // green / red
        string correctColor = "#008000"; // green

        // ✅ “내 답 / 정답”을 한 줄에 표시
        target[2].text = $"<color={myColor}>내 답: {myAns}</color>   <color={correctColor}>정답: {corrAns}</color>";

        // ✅ 배열 마지막 인덱스는 3이므로 여기로 수정
        target[3].text = $"해설: {data.exp}";
    }



    void ClearSide(TextMeshProUGUI[] target)
    {
        if (target == null) return;
        foreach (var t in target) if (t) t.text = "";
    }
}
