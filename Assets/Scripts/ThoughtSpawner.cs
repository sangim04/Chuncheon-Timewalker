using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThoughtSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject goodThoughtPrefab;
    public GameObject badThoughtPrefab;

    [Header("References")]
    public Transform playerCamera;  // 🎯 XR 카메라 직접 연결 (Inspector에서 Main Camera 드래그)

    [Header("Spawn Settings")]
    public int maxThoughts = 5;            // 최대 동시 잡념 수
    public float spawnCheckInterval = 1f;  // 스폰 주기 (초)
    public float minRadius = 0.5f;         // 최소 반경
    public float maxRadius = 0.9f;         // 최대 반경
    public float heightOffset = 0.2f;      // 기본 y 보정 (플레이어 눈 높이 기준)
    public float spawnClearance = 0.25f;   // 중복 방지 거리
    public bool useForwardArc = true;      // 전방 180도 제한
    [Range(10f, 180f)] public float halfArcDeg = 80f; // 시야각 (기본 160도)

    [Header("Diagnostics")]
    public bool logDebug = true;
    public LayerMask thoughtLayer = 0;

    private Transform player;
    private bool isSpawning = false;
    private readonly List<GameObject> activeThoughts = new();

    void Start()
    {
        // XR 카메라 직접 지정이 있으면 그것을 사용
        if (playerCamera != null)
            player = playerCamera;
        else
            player = Camera.main?.transform;

        if (player == null)
            Debug.LogError("❌ ThoughtSpawner: playerCamera가 지정되지 않았습니다!");
    }

    // 🟢 스폰 시작
    public void BeginSpawn()
    {
        if (player == null)
        {
            if (playerCamera != null)
                player = playerCamera;
            else
                player = Camera.main?.transform;
        }

        if (player == null)
        {
            Debug.LogError("❌ ThoughtSpawner.BeginSpawn(): Camera가 없습니다!");
            return;
        }

        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    // 🔴 스폰 중지
    public void StopSpawn()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    // 🎯 주기적으로 잡념 생성
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (activeThoughts.Count < maxThoughts)
            {
                GameObject spawned = TrySpawnOnce();
                if (spawned != null)
                    activeThoughts.Add(spawned);
            }
            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }

    // 🧠 실제 생성 로직 (전방 시야 안)
    GameObject TrySpawnOnce()
    {
        if (player == null) return null;

        float angleDeg = useForwardArc
            ? Random.Range(-halfArcDeg, halfArcDeg)
            : Random.Range(-180f, 180f);

        float rad = Mathf.Deg2Rad * angleDeg;

        // 🎯 XR 카메라의 forward 그대로 사용
       // Vector3 forward = new Vector3(player.forward.x, 0, player.forward.z).normalized; //기존
        Vector3 forward = new Vector3(-player.forward.x, 0, -player.forward.z).normalized;
        Vector3 right = new Vector3(player.right.x, 0, player.right.z).normalized;

        Vector3 spawnDir = (forward * Mathf.Cos(rad)) + (right * Mathf.Sin(rad));

        

        
        spawnDir.Normalize();
        spawnDir.Normalize();

        float radius = Random.Range(minRadius, maxRadius);
        Vector3 spawnPos = player.position + spawnDir * radius;
        spawnPos.y += heightOffset;

        // 🚫 중복 방지
        foreach (var t in activeThoughts)
        {
            if (t == null) continue;
            if (Vector3.Distance(t.transform.position, spawnPos) < spawnClearance)
            {
                if (logDebug)
                    Debug.Log($"🚫 Spawn skipped — overlap with existing thought at {spawnPos}");
                return null;
            }
        }

        // 🧩 프리팹 선택
        bool isGood = Random.value > 0.35f;
        GameObject prefab = isGood ? goodThoughtPrefab : badThoughtPrefab;

        // ✅ 생성
        GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 🧠 ThoughtBehavior 설정
        var tb = spawned.GetComponent<ThoughtBehavior>();
        tb.isGoodThought = isGood;

        // 💬 onDestroyed 연결 (360° 회전 후 제거 시 자동 정리)
        tb.onDestroyed += () =>
        {
            if (activeThoughts.Contains(spawned))
                activeThoughts.Remove(spawned);
        };

        if (logDebug)
        {
            float distFromPlayer = Vector3.Distance(player.position, spawnPos);
            Debug.Log($"🧠 Spawned {(isGood ? "Good" : "Bad")}Thought@{angleDeg:F0}° at {spawnPos} (dist {distFromPlayer:F2}m)");
        }

        return spawned;
    }

    // 🧹 모든 잡념 강제 제거 (게임 종료 시)
public void ClearAllThoughts()
{
    foreach (var t in new List<GameObject>(activeThoughts))
    {
        if (t != null)
            Destroy(t);
    }
    activeThoughts.Clear();
}

}
