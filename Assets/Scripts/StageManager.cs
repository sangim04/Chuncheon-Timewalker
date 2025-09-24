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
    private readonly Dictionary<XRSocketInteractor, bool> correctPlaced = new(); // ���Ϻ� ���� ����

    private void Awake()
    {
        // ������ ���� + �ʱ� ���� ����
        WireStage(stage1Sockets);
        WireStage(stage2Sockets);
        WireStage(stage3Sockets);

        // ó������ 1�ܰ踸 Ȱ��
        SetStageActive(0, true);
        SetStageActive(1, false);
        SetStageActive(2, false);

        // ���� üũ(����: 4, 8, 8)
        if (stage1Sockets.Count != 4) Debug.LogWarning($"[SocketStageManager] Stage1 ���� ���� {stage1Sockets.Count}�� (���� 4��)");
        if (stage2Sockets.Count != 8) Debug.LogWarning($"[SocketStageManager] Stage2 ���� ���� {stage2Sockets.Count}�� (���� 8��)");
        if (stage3Sockets.Count != 8) Debug.LogWarning($"[SocketStageManager] Stage3 ���� ���� {stage3Sockets.Count}�� (���� 8��)");
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

            // ���� ĸó�� � ���� �̺�Ʈ���� ��Ȯ��
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
        lockedMask = InteractionLayerMask.GetMask("Locked"); // �����Ϳ��� ���� �̸��� �����ؾ� ��
    }

    private void SetStageActive(int stageIndex, bool active)
    {
        var list = GetStageList(stageIndex);
        if (list == null) return;

        foreach (var socket in list)
        {
            if (!socket) continue;

            // ������ �׻� �ѵд� (���� ��Ȱ��ȭ���� ����)
            socket.enabled = true;
            socket.gameObject.SetActive(true);

            // ���� ���Ͽ� �������� ���� �ִٸ�
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
                            // �ܰ� Ȱ��ȭ: �ʿ� �� ��� ���� (����)
                            UnlockGrab(grab);
                        }
                        else
                        {
                            // �ܰ� ��Ȱ��ȭ(�Ϸ��): �������� �ᰡ�� ������ �� ����
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
            savedMasks[grab] = grab.interactionLayers; // ���� ����ũ ����

        grab.interactionLayers = lockedMask;

        // (����) ���� ����ȭ
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

        // ������ �ش� ������ �̿Ϸ� ó��
        correctPlaced[socket] = false;
    }

    private void TryAdvanceStage()
    {
        var curList = GetStageList(currentStage);
        if (curList == null || curList.Count == 0) return;

        // ���� �ܰ��� ��� ������ ���� �������� Ȯ��
        bool allCorrect = curList.All(s => s && correctPlaced.TryGetValue(s, out var ok) && ok);

        if (!allCorrect) return;

        if (currentStage == 0)
        {
            Debug.Log("4���� �ùٸ��� �����ϴ�. ���� �ܰ踦 �����մϴ�.");
            SetStageActive(0, false);
            currentStage = 1;
            SetStageActive(1, true);
        }
        else if (currentStage == 1)
        {
            Debug.Log("8���� �ùٸ��� �����ϴ�. ���� �ܰ踦 �����մϴ�.");
            SetStageActive(1, false);
            currentStage = 2;
            SetStageActive(2, true);
        }
        else if (currentStage == 2)
        {
            Debug.Log("���߽��ϴ�!");
            //���⼭ �ļ� ����(����, �� ��ȯ ��) ȣ��
        }
    }
}
