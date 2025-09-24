using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class StageManager : MonoBehaviour
{
    [Header("Stage 1")]
    public List<XRSocketInteractor> stage1Sockets = new List<XRSocketInteractor>(4);

    [Header("Stage 2")]
    public List<XRSocketInteractor> stage2Sockets = new List<XRSocketInteractor>(8);

    [Header("Stage 3")]
    public List<XRSocketInteractor> stage3Sockets = new List<XRSocketInteractor>(8);

    private int currentStage = 0; // 0..2
    private readonly Dictionary<XRSocketInteractor, bool> correctPlaced = new(); // 소켓별 정답 여부

    private void Awake()
    {
        // 리스너 연결 + 초기 상태 세팅
        WireStage(stage1Sockets);
        WireStage(stage2Sockets);
        WireStage(stage3Sockets);

        // 처음에는 1단계만 활성
        SetStageActive(0, true);
        SetStageActive(1, false);
        SetStageActive(2, false);

        // 개수 체크(권장: 4, 8, 8)
        if (stage1Sockets.Count != 4) Debug.LogWarning($"[SocketStageManager] Stage1 소켓 개수 {stage1Sockets.Count}개 (권장 4개)");
        if (stage2Sockets.Count != 8) Debug.LogWarning($"[SocketStageManager] Stage2 소켓 개수 {stage2Sockets.Count}개 (권장 8개)");
        if (stage3Sockets.Count != 8) Debug.LogWarning($"[SocketStageManager] Stage3 소켓 개수 {stage3Sockets.Count}개 (권장 8개)");
    }

    private void OnDestroy()
    {
        UnwireStage(stage1Sockets);
        UnwireStage(stage2Sockets);
        UnwireStage(stage3Sockets);
    }

    private void WireStage(List<XRSocketInteractor> sockets)
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            correctPlaced[s] = false;

            // 로컬 캡처로 어떤 소켓 이벤트인지 명확히
            var socketRef = s;
            socketRef.selectEntered.AddListener(args => OnSelectEntered(socketRef, args));
            socketRef.selectExited.AddListener(args => OnSelectExited(socketRef, args));
        }
    }

    private void UnwireStage(List<XRSocketInteractor> sockets)
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            var socketRef = s;
            socketRef.selectEntered.RemoveListener(args => OnSelectEntered(socketRef, args));
            socketRef.selectExited.RemoveListener(args => OnSelectExited(socketRef, args));
        }
    }

    private List<XRSocketInteractor> GetStageList(int stageIndex)
    {
        return stageIndex switch
        {
            0 => stage1Sockets,
            1 => stage2Sockets,
            2 => stage3Sockets,
            _ => null
        };
    }
    private readonly Dictionary<XRGrabInteractable, InteractionLayerMask> savedMasks = new();

    // ==============================================================
    private InteractionLayerMask lockedMask;

    private void Start()
    {
        lockedMask = InteractionLayerMask.GetMask("Locked"); // 에디터에서 만든 이름과 동일해야 함
    }

    private void SetStageActive(int stageIndex, bool active)
    {
        var list = GetStageList(stageIndex);
        if (list == null) return;

        foreach (var socket in list)
        {
            if (!socket) continue;

            // 소켓은 항상 켜둔다 (이제 비활성화하지 않음)
            socket.enabled = true;
            socket.gameObject.SetActive(true);

            // 현재 소켓에 아이템이 꽂혀 있다면
            if (socket.hasSelection)
            {
                var interactable = socket.firstInteractableSelected as IXRSelectInteractable;
                if (interactable != null)
                {
                    var go = interactable.transform.gameObject;
                    if (go.TryGetComponent<XRGrabInteractable>(out var grab))
                    {
                        if (active)
                        {
                            // 단계 활성화: 필요 시 잠금 해제 (복원)
                            UnlockGrab(grab);
                        }
                        else
                        {
                            // 단계 비활성화(완료됨): 아이템을 잠가서 손으로 못 빼게
                            LockGrab(grab);
                        }
                    }
                }
            }
        }
    }

    private void LockGrab(XRGrabInteractable grab)
    {
        if (!savedMasks.ContainsKey(grab))
            savedMasks[grab] = grab.interactionLayers; // 원래 마스크 저장

        grab.interactionLayers = lockedMask;

        // (선택) 물리 안정화
        var rb = grab.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    private void UnlockGrab(XRGrabInteractable grab)
    {
        if (savedMasks.TryGetValue(grab, out var original))
        {
            grab.interactionLayers = original;
            savedMasks.Remove(grab);
        }
    }

    //======================================================

    private bool IsSocketInCurrentStage(XRSocketInteractor socket)
    {
        var list = GetStageList(currentStage);
        return list != null && list.Contains(socket);
    }

    private void OnSelectEntered(XRSocketInteractor socket, SelectEnterEventArgs args)
    {
        if (!IsSocketInCurrentStage(socket)) return;

        string socketName = socket.gameObject.name;
        string objectName = args.interactableObject.transform.name;

        bool isCorrect = (objectName + "Socket" == socketName);
        correctPlaced[socket] = isCorrect;

        TryAdvanceStage();
    }

    private void OnSelectExited(XRSocketInteractor socket, SelectExitEventArgs args)
    {
        if (!IsSocketInCurrentStage(socket)) return;

        // 빠지면 해당 소켓은 미완료 처리
        correctPlaced[socket] = false;
    }

    private void TryAdvanceStage()
    {
        var curList = GetStageList(currentStage);
        if (curList == null || curList.Count == 0) return;

        // 현재 단계의 모든 소켓이 정답 상태인지 확인
        bool allCorrect = curList.All(s => s && correctPlaced.TryGetValue(s, out var ok) && ok);

        if (!allCorrect) return;

        if (currentStage == 0)
        {
            Debug.Log("4개에 올바르게 들어갔습니다. 다음 단계를 진행합니다.");
            SetStageActive(0, false);
            currentStage = 1;
            SetStageActive(1, true);
        }
        else if (currentStage == 1)
        {
            Debug.Log("8개에 올바르게 들어갔습니다. 다음 단계를 진행합니다.");
            SetStageActive(1, false);
            currentStage = 2;
            SetStageActive(2, true);
        }
        else if (currentStage == 2)
        {
            Debug.Log("잘했습니다!");
            //여기서 후속 로직(연출, 씬 전환 등) 호출
        }
    }
}
