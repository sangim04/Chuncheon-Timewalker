using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRGrabInteractable 사용을 위해 필요

public class HanoiController : MonoBehaviour
{
    private XRSocketInteractor socketInteractor;
    private XRGrabInteractable selfGrabInteractable; // 소켓 자체의 잡기 컴포넌트

    void Awake()
    {
        // XRSocketInteractor 컴포넌트를 가져옵니다.
        socketInteractor = GetComponent<XRSocketInteractor>();
        if (socketInteractor == null)
        {
            Debug.LogError("HanoiController: XRSocketInteractor 컴포넌트를 찾을 수 없습니다. 이 스크립트는 소켓과 함께 사용되어야 합니다.");
            enabled = false;
        }

        // 소켓 오브젝트에 붙어있는 XRGrabInteractable 컴포넌트를 가져옵니다.
        selfGrabInteractable = GetComponent<XRGrabInteractable>();
        if (selfGrabInteractable == null)
        {
            Debug.LogWarning("HanoiController: 소켓 오브젝트에서 XRGrabInteractable을 찾을 수 없습니다. 소켓 자체의 잡기 기능을 제어하려면 이 컴포넌트가 필요합니다.");
        }
    }

    void OnEnable()
    {
        if (socketInteractor != null)
        {
            // 소켓에 물건이 '선택(Selected)'되어 들어왔을 때 이벤트 등록
            socketInteractor.selectEntered.AddListener(OnSocketSelectEntered);
            // 소켓에서 물건이 '선택 해제(Deselected)'되어 빠져나갔을 때 이벤트 등록
            socketInteractor.selectExited.AddListener(OnSocketSelectExited);
        }
    }

    void OnDisable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnSocketSelectEntered);
            socketInteractor.selectExited.RemoveListener(OnSocketSelectExited);
        }
    }

    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        // 소켓에 물건이 들어왔을 때 소켓 자체의 잡기 기능 비활성화
        if (selfGrabInteractable != null)
        {
            selfGrabInteractable.enabled = false;
            Debug.Log($"물건 연결됨: {gameObject.name} (소켓)의 잡기 기능 비활성화.");
        }
    }

    private void OnSocketSelectExited(SelectExitEventArgs args)
    {
        // 소켓에서 물건이 빠져나갔을 때 소켓 자체의 잡기 기능 다시 활성화
        if (selfGrabInteractable != null)
        {
            selfGrabInteractable.enabled = true;
            Debug.Log($"물건 해제됨: {gameObject.name} (소켓)의 잡기 기능 활성화.");
        }
    }
}
