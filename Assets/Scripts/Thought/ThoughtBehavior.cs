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
    public int layerCount = 4;
    public float layerSpacing = 0.2f;
    public float layerRandomOffset = 0.05f;

    [Header("Thought Messages")]
    [Tooltip("좋은 생각 문구 (비워두면 기본 2개 사용)")]
    public string[] goodMessages;
    [Tooltip("나쁜 생각 문구 (비워두면 기본 2개 사용)")]
    public string[] badMessages;

    public Action onDestroyed;

    private Transform player;
    private float baseY;
    private float angle;

    void Start()
    {
        player = Camera.main ? Camera.main.transform : null;
        if (player == null)
        {
            Debug.LogError("❌ ThoughtBehavior: Camera.main not found");
            enabled = false;
            return;
        }

        // 1) 버튼/TMP 텍스트 세팅
        SetThoughtText();

        // 2) 층 랜덤 선택
        int chosenLayer = UnityEngine.Random.Range(0, layerCount);
        float startY = -0.3f + (chosenLayer * layerSpacing);
        float randomOffset = UnityEngine.Random.Range(-layerRandomOffset, layerRandomOffset);
        baseY = player.position.y + startY + randomOffset;

        // 3) 초기 위치
        Vector3 offset = new Vector3(Mathf.Cos(0) * orbitRadius, 0, Mathf.Sin(0) * orbitRadius);
        transform.position = player.position + offset;
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
            goodMessages = new[] { "좋은 생각", "고마운 마음" };
        if (badMessages == null || badMessages.Length == 0)
            badMessages = new[] { "나쁜 생각", "불편한 마음" };

        // 프리팹 계층: Canvas/button/Text (TMP) 를 자동 탐색
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            Debug.LogWarning($"⚠️ {name}: TextMeshProUGUI not found under Canvas/button/Text");
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
