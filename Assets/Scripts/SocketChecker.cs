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
        string socketName = socket.gameObject.name;
        string objectName = args.interactableObject.transform.name;

        if (objectName + "Socket" == socketName)
        {
            Debug.Log($"성공! {objectName} - {socketName}");
        }
        else
        {
            Debug.Log($"실패! {objectName} - {socketName}");
        }
    }
}

