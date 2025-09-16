using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class QuizManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject quizPanel;
    public GameObject summaryPanel;

    [Header("Quiz UI")]
    public Text quizBox;   // 문제 텍스트
    public Text mark;      // 정답/오답 표시
    public Button oButton;
    public Button xButton;

    [Header("Summary UI")]
    public Text scoreText;
    public Button retryButton;

    // 전체 20문제 (질문, 정답, 해설)
    private List<(string question, bool answer, string explanation)> allQuestions =
        new List<(string, bool, string)>()
    {
        ("향교는 조선시대 지방에서 학생들을 가르치던 교육 기관이다.", true, "맞습니다! 향교는 지방의 교육 기관이었습니다."),
        ("명륜당은 제사를 지내는 공간이다.", false, "아닙니다. 제사는 대성전에서, 명륜당은 강당 역할을 했습니다."),
        ("대성전은 공자와 성현들에게 제사를 지내던 공간이다.", true, "정답! 대성전은 제향 공간입니다."),
        ("사서삼경은 사서 4권과 삼경 3권을 합쳐 부르는 말이다.", true, "맞습니다. 사서(논어, 맹자, 대학, 중용)와 삼경으로 구성됩니다."),
        ("춘천향교는 현재 보존되어 있으며, 국가 지정 문화재로 보호받고 있다.", true, "네. 춘천향교는 보존되어 문화재로 관리되고 있습니다."),
        ("향교에서 학생들은 활쏘기와 말타기만을 배웠다.", false, "아닙니다. 유교 경전과 도덕, 학문 중심 교육을 했습니다."),
        ("조선의 향교는 교육뿐만 아니라 제례도 함께 담당했다.", true, "정답! 교육과 제례를 함께 담당했습니다."),
        ("“명륜(明倫)”은 사람 사이의 도리를 밝힌다는 뜻이다.", true, "맞습니다. 명륜은 인륜을 밝힌다는 뜻입니다."),
        ("향교의 학생들은 “유생(儒生)”이라 불렸다.", true, "정답. 향교 학생은 유생이라 불렸습니다."),
        ("향교에서는 과거시험을 직접 치렀다.", false, "과거시험은 별도의 시험장에서 시행되었습니다."),
        ("대성전은 향교에서 가장 중요한 교육 공간이다.", false, "대성전은 제례 공간이고, 교육은 명륜당에서 이루어졌습니다."),
        ("춘천향교 같은 향교는 고려시대부터 설치되었다.", true, "맞습니다. 고려시대에 시작되어 조선에서 제도화되었습니다."),
        ("유교 교육은 단순히 책을 읽는 것이 아니라 토론과 암송도 중요했다.", true, "정답. 유교 교육은 토론과 암송을 중시했습니다."),
        ("조선의 향교는 오늘날로 치면 대학과 같다.", false, "아닙니다. 지방의 중등 교육 기관에 가깝습니다."),
        ("향교에서는 사서삼경 중 ‘논어(論語)’도 가르쳤다.", true, "맞습니다. 논어는 사서에 포함됩니다."),
        ("조선시대에는 전국 모든 고을에 향교가 있었다.", true, "정답. 모든 고을에 향교를 두었습니다."),
        ("향교 학생들은 무료로 공부했으며, 일부는 국가에서 장학금을 지원받았다.", true, "맞습니다. 장학금과 지원이 있었습니다."),
        ("춘천향교 명륜당은 오늘날에도 교육·문화 활동에 사용된다.", true, "정답. 현재도 문화 공간으로 활용됩니다."),
        ("공자는 중국 사람이지만, 조선에서도 매우 존경받았다.", true, "맞습니다. 공자는 유교의 근본으로 존경받았습니다."),
        ("“교육과 제례는 항상 함께 간다”는 원칙은 향교 건축에서도 확인할 수 있다.", true, "정답. 향교는 교육과 제례를 함께하는 공간 구조입니다."),
    };

    private List<(string question, bool answer, string explanation)> quiz; // 랜덤으로 뽑힌 10문제
    private int currentIndex = 0;
    private int score = 0;

    void Start()
    {
        // 퀴즈 20개 중 랜덤으로 10개 선택
        quiz = allQuestions.OrderBy(x => Random.value).Take(10).ToList();

        quizPanel.SetActive(true);
        summaryPanel.SetActive(false);

        ShowQuestion();

        oButton.onClick.AddListener(() => OnAnswer(true));
        xButton.onClick.AddListener(() => OnAnswer(false));
        retryButton.onClick.AddListener(RestartGame);
    }

    void ShowQuestion()
    {
        var q = quiz[currentIndex];
        quizBox.text = $"Q{currentIndex+1}. {q.question} (O/X)";
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
            mark.text = "⭕ 정답입니다!";
            mark.color = Color.green;
        }
        else
        {
            mark.text = "❌ 오답입니다!";
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

        scoreText.text = $"결과: {score} / {quiz.Count} 정답";
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
