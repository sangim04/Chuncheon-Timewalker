using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketChecker : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        socket.selectEntered.AddListener(OnObjectPlaced);
    }

    private void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnObjectPlaced);
    }

    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        // 소켓 이름
        string socketName = socket.gameObject.name;

        // 들어온 오브젝트 이름
        string objectName = args.interactableObject.transform.name;

        // 이름 규칙 검사
        if (objectName + "Socket" == socketName)
        {
            Debug.Log($"성공! {objectName} 이(가) {socketName}에 올바르게 들어갔습니다.");
        }
        else
        {
            Debug.Log($"실패... {objectName} 은(는) {socketName}에 들어갈 수 없습니다.");
        }
    }
}

