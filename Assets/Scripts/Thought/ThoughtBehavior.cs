using UnityEngine;
using System;
using TMPro;               // ⬅️ TextMeshPro 사용

public class ThoughtBehavior : MonoBehaviour
{
    [Header("Orbit Settings")]
    public bool isGoodThought = true;
    public float orbitSpeed = 20f;
    public float orbitRadius = 0.65f;
    public float floatAmplitude = 0.03f;
    public float floatSpeed = 1.0f;

    [Header("Vertical Layer Settings")]
    public int layerCount = 3;
    public float layerSpacing = 0.12f;
    public float layerRandomOffset = 0.03f;

    [Header("Effects")]
    public GameObject touchParticlePrefab;


    [Header("Thought Messages")]
    [Tooltip("좋은 생각 문구 (비워두면 기본 2개 사용)")]
    public string[] goodMessages;
    [Tooltip("나쁜 생각 문구 (비워두면 기본 2개 사용)")]
    public string[] badMessages;

    public Action onDestroyed;

    private Transform player;
    private float baseY;
    private float angle;

    [Header("Audio")]
    public AudioClip goodSound;
    public AudioClip badSound;


    void Start()
    {
        player = Camera.main ? Camera.main.transform : null;
        if (player == null)
        {
            Debug.LogError("❌ ThoughtBehavior: Camera.main not found");
            enabled = false;
            return;
        }

        // TMP 탐색을 한 프레임 늦게 실행
        Invoke(nameof(SetThoughtText), 0.05f);

        // 2) 층 랜덤 선택
        int chosenLayer = UnityEngine.Random.Range(0, layerCount);
        float startY = -0.3f + (chosenLayer * layerSpacing);

        // 🔹 높이 랜덤 폭을 살짝 확대 (자연스러움 복원)
        float randomOffset = UnityEngine.Random.Range(-layerRandomOffset * 2f, layerRandomOffset * 2f);

        // 🔹 기본 y위치 계산
        baseY = player.position.y + startY + randomOffset;

        // 3) 초기 위치
        Vector3 offset = new Vector3(Mathf.Cos(0) * orbitRadius, 0, Mathf.Sin(0) * orbitRadius);
        transform.position = player.position + offset;

        // 🔹 너무 위나 아래로 가지 않게 y좌표 제한 (손 닿는 범위 유지)
        float minY = player.position.y - 0.15f;  // 아래쪽 15cm 허용
        float maxY = player.position.y + 0.07f;  // 위쪽 7cm 허용
        baseY = Mathf.Clamp(baseY, minY, maxY);

        // 🔹 개별적인 떠오름 속도 살짝 랜덤 (단조로움 방지)
        floatSpeed *= UnityEngine.Random.Range(0.8f, 1.3f);

        // 🔹 진폭(위아래 흔들림)도 살짝 랜덤
        floatAmplitude *= UnityEngine.Random.Range(0.8f, 1.2f);
    }


    void Update()
    {
        if (player == null) return;

        angle += orbitSpeed * Time.deltaTime;

        // ▶ 한 바퀴 돌면 제거 (요구사항 유지)
        if (angle >= 360f)
        {
            onDestroyed?.Invoke();
            Destroy(gameObject);
            return;
        }

        UpdateOrbitPosition();
    }

    void UpdateOrbitPosition()
    {
        Vector3 center = player.position;
        float rad = Mathf.Deg2Rad * angle;

        float x = center.x + Mathf.Cos(rad) * orbitRadius;
        float z = center.z + Mathf.Sin(rad) * orbitRadius;
        float y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = new Vector3(x, y, z);
        transform.LookAt(center);
        transform.Rotate(0, 180f, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            // 💫 파티클 생성
            if (touchParticlePrefab != null)
            {
                Instantiate(
                    touchParticlePrefab,
                    transform.position,                // 현재 Thought 위치에서
                    Quaternion.identity                // 기본 회전
                );
            }
            // 🔊 사운드 재생
            if (isGoodThought && goodSound != null)
                AudioSource.PlayClipAtPoint(goodSound, transform.position, 0.8f);
            else if (!isGoodThought && badSound != null)
                AudioSource.PlayClipAtPoint(badSound, transform.position, 0.8f);
            
            ThoughtGameManager.Instance.OnThoughtTouched(this);
            onDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 버튼 안의 Text (TMP) 를 good/bad에 따라 랜덤 문구로 설정
    // ─────────────────────────────────────────────────────────────
    void SetThoughtText()
    {
        // 기본 문구(2개씩). 인스펙터에서 채우면 그것을 우선 사용
        if (goodMessages == null || goodMessages.Length == 0)
            goodMessages = new[]
            {
                "마음을 고요히 가다듬는다",
                "배움을 향한 문이 열린다",
                "감사의 마음이 깃든다",
                "오늘도 성실히 배우리라",
                "겸손한 마음으로 나아간다",
                "욕심을 버리고 덕을 쌓는다",
                "정신을 맑히고 호흡을 고른다",
                "내면의 평화를 느낀다",
                "스스로를 돌아본다",
                "경건히 마음을 비운다"
            };

        if (badMessages == null || badMessages.Length == 0)
            badMessages = new[]
            {
                "귀찮다… 그냥 돌아갈까",
                "조금만 더 자고 올 걸",
                "마음이 어지럽다",
                "짜증나고 답답하다",
                "집중이 안 된다",
                "불안한 생각이 머리를 맴돈다",
                "화가 치밀어 오른다",
                "욕심이 나를 끌어당긴다"
            };

        // 프리팹 계층: Canvas/button/Text (TMP) 를 자동 탐색
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            return;
        }

        if (isGoodThought)
        {
            int i = UnityEngine.Random.Range(0, goodMessages.Length);
            tmp.text = goodMessages[i];
        }
        else
        {
            int i = UnityEngine.Random.Range(0, badMessages.Length);
            tmp.text = badMessages[i];
        }
    }
}
