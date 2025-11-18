using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class HanoiController : MonoBehaviour
{
    [Header("Layer Control")]
    [Tooltip("사용자 Hand Controller 또는 Ray Interactor가 사용하는 레이어를 선택하세요.")]
    public InteractionLayerMask grabInteractionLayer; // 인스펙터에서 Hand 레이어를 선택하도록 합니다.

    private XRSocketInteractor socketInteractor;
    private XRGrabInteractable selfGrabInteractable; 
    private InteractionLayerMask originalInteractionMask; // 타입 변경: InteractionLayerMask 사용

    void Awake()
    {
        // 1. XRSocketInteractor 컴포넌트 가져오기
        socketInteractor = GetComponent<XRSocketInteractor>();
        if (socketInteractor == null)
        {
            Debug.LogError("HanoiController: XRSocketInteractor 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
        }

        // 2. 소켓 자체의 XRGrabInteractable 컴포넌트 가져오기
        selfGrabInteractable = GetComponent<XRGrabInteractable>();
        if (selfGrabInteractable == null)
        {
            Debug.LogWarning("HanoiController: 소켓 오브젝트에서 XRGrabInteractable을 찾을 수 없습니다. 소켓을 잡을 수 없게 만들려면 이 컴포넌트가 필요합니다.");
            enabled = false; // 잡기 컴포넌트가 없으면 기능 불필요
            return;
        }
        
        // 3. 원래의 Interaction Layer Mask를 저장합니다.
        // interactionLayerMask -> interactionLayers로 변경
        originalInteractionMask = selfGrabInteractable.interactionLayers; 
    }

    void OnEnable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelectEntered);
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
        
        // 안전을 위해 비활성화될 때 원래 마스크로 복원합니다.
        if (selfGrabInteractable != null)
        {
             // interactionLayerMask -> interactionLayers로 변경
             selfGrabInteractable.interactionLayers = originalInteractionMask;
        }
    }

    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        // 소켓에 물건이 들어왔을 때 소켓 자체의 잡기 기능 비활성화 (Layer Mask 사용)
        if (selfGrabInteractable != null)
        {
            // interactionLayerMask -> interactionLayers로 변경
            // 현재 Interaction Layer Mask에서 grabInteractionLayer(Hand 레이어)를 제거합니다.
            selfGrabInteractable.interactionLayers &= ~grabInteractionLayer;
            
            Debug.Log($"물건 연결됨: {gameObject.name} (소켓)의 Hand 잡기 기능 비활성화.");
        }
    }

    private void OnSocketSelectExited(SelectExitEventArgs args)
    {
        // 소켓에서 물건이 빠져나갔을 때 소켓 자체의 잡기 기능 다시 활성화 (Layer Mask 사용)
        if (selfGrabInteractable != null)
        {
            // interactionLayerMask -> interactionLayers로 변경
            // 원래의 Interaction Layer Mask에 grabInteractionLayer(Hand 레이어)를 다시 추가합니다.
            selfGrabInteractable.interactionLayers |= grabInteractionLayer;
            
            Debug.Log($"물건 해제됨: {gameObject.name} (소켓)의 Hand 잡기 기능 활성화.");
        }
    }
}