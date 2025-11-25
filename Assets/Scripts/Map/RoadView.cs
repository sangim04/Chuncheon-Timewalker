using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[AddComponentMenu("VR/VR Button Goto (SmoothDamp)")]
public class RoadView : MonoBehaviour
{
    [Header("What to move (e.g., XR Origin root)")]
    public Transform rigRoot;

    [Header("Motion")]
    [Tooltip("총 이동 시간(초)")]
    public float duration = 1.2f;

    [Tooltip("가우시안 폭. 작을수록 중간이 더 급해짐. 0.15~0.35 권장")]
    [Range(0.05f, 0.6f)]
    public float sigma = 0.25f;

    [Tooltip("이동 중 충돌/중력 등 비활성화가 필요하면 훅 연결해서 처리")]
    public bool freezeY = false; // true면 목적지의 y로 고정(바닥 높이 유지 등)

    Coroutine _moveCo;

    //===
    public Volume volume;     // Global Volume 할당 (인스펙터에서 Drag)
    public float startWeight = 0f; // 이동 시작 시 weight
    public float endWeight = 1f; // 이동 끝에서 weight
    //===

    /// <summary>
    /// 대상 지점으로 가우시안 속도 프로파일로 이동
    /// </summary>
    public void MoveTo(Vector3 worldTarget)
    {
        if (rigRoot == null)
        {
            Debug.LogWarning("[GaussianMover] rigRoot가 비어있음.");
            return;
        }
        if (freezeY)
            worldTarget.y = rigRoot.position.y;

        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = StartCoroutine(MoveGaussian(worldTarget));
    }

    IEnumerator MoveGaussian(Vector3 target)
    {
        yield return new WaitForEndOfFrame();

        Vector3 start = rigRoot.position;
        float t = 0f;
        float s = Mathf.Max(0.01f, sigma);

        //===
        float initWeight = volume != null ? volume.weight : 0f;
        //===

        // 가우시안 CDF
        System.Func<float, double> CDF = (float u) =>
        {
            double z = (u - 0.5f) / (s * 1.41421356237); // sqrt(2)
            return 0.5 * (1.0 + Erf(z));
        };

        // ★ 끝점 정규화: u=0 → 0, u=1 → 1
        double c0 = CDF(0f);
        double c1 = CDF(1f);
        double denom = System.Math.Max(1e-6, c1 - c0);

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            double raw = CDF(u);
            float blend = (float)((raw - c0) / denom); // 0~1로 재스케일

            Vector3 next = Vector3.LerpUnclamped(start, target, blend);
            rigRoot.position = next;

            //===
            // Volume 효과 적용 (부드러운 Weight Lerp)
            if (volume != null)
            {
                volume.weight = Mathf.Lerp(initWeight, endWeight, u);
            }
            //===
            yield return null;
        }

        // 루프를 시간으로 끊었으니 마지막 한 번 더 보정 (거의 0일 것임)
        rigRoot.position = target;

        //===
        if (volume != null)
            volume.weight = endWeight;
        //===

        _moveCo = null;
    }

    // 수치근사: Abramowitz & Stegun 7.1.26
    static double Erf(double x)
    {
        // sign 보존
        int sign = x < 0 ? -1 : 1;
        x = System.Math.Abs(x);

        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t) * System.Math.Exp(-x * x);
        return sign * y;
    }

}
