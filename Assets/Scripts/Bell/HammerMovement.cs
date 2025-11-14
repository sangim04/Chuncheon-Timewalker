using UnityEngine;
using System.Collections; // IEnumerator 사용을 위해 추가
using UnityEngine.XR.Interaction.Toolkit; 
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRGrabInteractable 사용을 위해 추가

public class HammerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 targetLocalPosition = new Vector3(0, -0.5f, 0); // 놓았을 때 이동할 목표 로컬 위치
    
    [Tooltip("TargetPos로 이동하는 시간입니다 (빠른 이동).")]
    public float initialMoveDuration = 0.5f; // 첫 번째 이동의 기본 시간

    [Tooltip("OriginPos로 복귀하는 기본 시간입니다.")]
    public float baseReturnDuration = 0.8f; // 복귀 시간의 기본값

    // ==== [거리 기반 속도 조절 설정] ====
    [Header("Distance-Based Speed")]
    [Tooltip("그랩 해제 위치가 원점과 멀어질수록 이동 시간이 얼마나 빠르게 줄어들지 결정합니다 (Initial Move).")]
    public float initialDistanceSensitivity = 0.5f; 
    
    [Tooltip("initialMoveDuration이 줄어들 수 있는 최소 시간입니다. (이동 속도의 상한선)")]
    public float minInitialMoveDuration = 0.1f;

    [Tooltip("그랩 해제 위치가 원점과 멀어질수록 복귀 시간(Return Duration)이 얼마나 빠르게 줄어들지 결정합니다.")]
    public float returnDistanceMultiplier = 0.3f; // 복귀 시간에 거리가 미치는 영향 (시간 감소 감도)
    
    [Tooltip("ReturnDuration이 줄어들 수 있는 최소 시간입니다. (복귀 속도의 상한선)")]
    public float minReturnDuration = 0.15f; // 복귀 이동의 최소 시간
    // ======================================

    // ==== [흔들림 효과 설정] ====
    [Header("Oscillation Effect")]
    [Tooltip("복귀 시 오브젝트가 흔들리는 최대 폭입니다.")]
    public float oscillationAmplitude = 0.05f; // 흔들림 진폭
    [Tooltip("복귀 시 오브젝트가 흔들리는 속도입니다 (높을수록 빠르게 흔들림).")]
    public float oscillationFrequency = 15f; // 흔들림 빈도
    // =============================

    private Vector3 originLocalPosition; // 원래 로컬 위치
    private XRGrabInteractable grabInteractable;
    private Coroutine movementCoroutine; // 현재 진행 중인 코루틴을 저장할 변수

    void Awake()
    {
        // XRGrabInteractable 컴포넌트 가져오기 (경로 정리)
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("HammerMovement: XRGrabInteractable 컴포넌트를 찾을 수 없습니다. 이 스크립트는 XRGrabInteractable과 함께 사용되어야 합니다.");
            enabled = false;
            return;
        }

        // 오브젝트의 초기 로컬 위치 저장
        originLocalPosition = transform.localPosition;

        // 그랩 시작/끝 이벤트에 리스너 등록
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 리스너 제거 (메모리 누수 방지)
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // 그랩 되면 진행 중인 이동 코루틴이 있다면 중지
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // 놓으면 이동 코루틴 시작
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        // 1. 그랩 해제 위치와 초기 위치 간의 거리 계산
        float currentDistance = Vector3.Distance(transform.localPosition, originLocalPosition);

        // 2. 거리에 따라 새로운 initialMoveDuration 계산 (멀수록 빠름 = 시간이 줄어듦)
        float calculatedInitialDuration = initialMoveDuration - (currentDistance * initialDistanceSensitivity);
        float finalInitialMoveDuration = Mathf.Max(calculatedInitialDuration, minInitialMoveDuration);

        // 3. 거리에 따라 새로운 returnDuration 계산 (멀수록 복귀 시간이 빨라짐 = 시간이 줄어듦)
        float calculatedReturnDuration = baseReturnDuration - (currentDistance * returnDistanceMultiplier);
        
        // 4. 최소 시간을 보장
        float finalReturnDuration = Mathf.Max(calculatedReturnDuration, minReturnDuration);
        
        // 코루틴에 계산된 시간을 전달
        movementCoroutine = StartCoroutine(MoveAndReturn(finalInitialMoveDuration, finalReturnDuration));
    }

    private IEnumerator MoveAndReturn(float actualInitialMoveDuration, float actualReturnDuration)
    {
        // 1. targetLocalPosition으로 이동 (흔들림 없음)
        yield return StartCoroutine(MoveToLocalPosition(targetLocalPosition, actualInitialMoveDuration, false));

        // 2. 잠시 대기
        yield return new WaitForSeconds(0.2f); 

        // 3. originLocalPosition으로 돌아오기 (흔들림 적용)
        yield return StartCoroutine(MoveToLocalPosition(originLocalPosition, actualReturnDuration, true));

        movementCoroutine = null; // 코루틴 완료
    }

    // 특정 로컬 위치로 부드럽게 이동하는 코루틴 (흔들림 옵션 추가)
    private IEnumerator MoveToLocalPosition(Vector3 targetPos, float duration, bool applyOscillation)
    {
        Vector3 startPos = transform.localPosition;
        float elapsedTime = 0f;

        // **물리 안정화:** Rigidbody의 속도를 초기화하여 중력 OFF 상태에서 튀는 것을 방지
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 키네마틱이 아닐 때만 속도 초기화 (이동 중에도 안정성 확보)
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        while (elapsedTime < duration)
        {
            // Lerp 비율 계산 (0.0 ~ 1.0)
            float t = elapsedTime / duration;
            
            // 1. 기본 Lerp 위치
            Vector3 currentLerpPosition = Vector3.Lerp(startPos, targetPos, t);
            Vector3 finalPosition = currentLerpPosition;

            // 2. 흔들림 효과 적용 (복귀 시에만)
            if (applyOscillation)
            {
                // Sine 함수를 사용하여 시간에 따라 진동하는 값 계산
                float oscillation = Mathf.Sin(elapsedTime * oscillationFrequency) * oscillationAmplitude * (1.0f - (t*1.0f));
                
                // 이동 방향에 수직인 임의의 로컬 축을 따라 흔들림을 적용 (예: 로컬 X축)
                // (1.0f - t)를 곱하여 도착할수록 흔들림이 0에 수렴하도록 만듭니다.
                finalPosition += transform.right * oscillation;
            }

            transform.localPosition = finalPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 위치 보장 (흔들림 제거)
        transform.localPosition = targetPos; 
        
        // **최종 물리 안정화:** 이동이 끝난 후 혹시 남아있을 수 있는 속도를 다시 0으로 설정
        if (rb != null && !rb.isKinematic)
        {
             rb.linearVelocity = Vector3.zero;
             rb.angularVelocity = Vector3.zero;
        }
    }
}
