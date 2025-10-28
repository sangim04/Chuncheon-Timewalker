using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // IXRSelectInteractable 사용

// 이 스크립트는 각 소켓 오브젝트에 부착되어야 함
public class SocketManager : MonoBehaviour
{
    [Tooltip("이 소켓에 들어와야 정답으로 처리될 물건의 이름입니다 (정확히 일치해야 함).")]
    public string correctItemName;
    public GameObject connectEffect;
    
    private XRSocketInteractor socketInteractor;

    private void Awake()
    {
        // XRSocketInteractor 컴포넌트를 미리 가져옴
        socketInteractor = GetComponent<XRSocketInteractor>();
        
        if (socketInteractor == null)
        {
            Debug.LogError("SocketManager: XRSocketInteractor 컴포넌트를 찾을 수 없습니다. 이 스크립트는 XRSocketInteractor와 함께 사용되어야 합니다.");
        }
    }

    // JulyeonManager에서 현재 소켓이 정답을 포함하고 있는지 확인하기 위해 호출
    public bool IsCorrect()
    {
        if (socketInteractor == null) return false;
        
        // 소켓에 물건이 연결되어 있는지 확인합니다. 
        if (socketInteractor.interactablesSelected.Count > 0)
        {
            Destroy(Instantiate(connectEffect, transform.position, Quaternion.identity), 3);
            IXRSelectInteractable currentItem = socketInteractor.interactablesSelected[0];
            
            // Null 체크는 안전을 위해 한 번 더 수행합니다.
            if (currentItem != null && currentItem.transform != null)
            {
                // 현재 물건의 이름이 정답 이름과 일치하는지 확인
                if (currentItem.transform.name == correctItemName)
                {
                    return true;
                }
            }
        }
        
        // 물건이 없거나 오답인 경우
        return false;
    }
}