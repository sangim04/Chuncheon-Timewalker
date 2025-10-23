using UnityEngine;
using System;

public class ThoughtBehavior : MonoBehaviour
{
    [Header("Orbit Settings")]
    public bool isGoodThought = true;
    public float orbitSpeed = 20f;        // 회전 속도
    public float orbitRadius = 0.65f;     // 회전 반경
    public float floatAmplitude = 0.03f;  // 상하 진동 폭
    public float floatSpeed = 1.0f;       // 상하 진동 속도

    [Header("Vertical Layer Settings")]
    public int layerCount = 4;             // 🔹 층 개수
    public float layerSpacing = 0.2f;      // 🔹 층 사이 높이 간격
    public float layerRandomOffset = 0.05f; // 🔹 층 내 랜덤 오차

    public Action onDestroyed; // 파괴 이벤트

    private Transform player;
    private float baseY;   // 기본 높이
    private float angle;   // 회전 각도

    void Start()
    {
        player = Camera.main.transform;

        // 🔹 층 랜덤 선택 (0~layerCount-1)
        int chosenLayer = UnityEngine.Random.Range(0, layerCount);

        // 예시: 4층일 때 -0.3, -0.1, +0.1, +0.3 이런 식으로 분포
        float startY = -0.3f + (chosenLayer * layerSpacing);

        // 🔹 층 내에서 랜덤 오차 추가
        float randomOffset = UnityEngine.Random.Range(-layerRandomOffset, layerRandomOffset);

        // 🔹 플레이어 높이에 상대적으로 위치 설정
        baseY = player.position.y + startY + randomOffset;

        // 초기 위치 (시작은 angle=0)
        Vector3 offset = new Vector3(Mathf.Cos(0) * orbitRadius, 0, Mathf.Sin(0) * orbitRadius);
        transform.position = player.position + offset;
    }

    void Update()
    {
        if (player == null) return;

        // 원형 회전
        angle += orbitSpeed * Time.deltaTime;

        // 🔹 한 바퀴 돌면 제거
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

        // 층 고정 + 살짝 상하 진동
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
}
