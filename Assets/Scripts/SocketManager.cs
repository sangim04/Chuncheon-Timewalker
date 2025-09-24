using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketManager : MonoBehaviour
{
    public string correctItemName; // 정답 물건의 이름
    private bool isCorrectlyPlaced = false;

    private void OnTriggerEnter(Collider other)
    {
        JulyeonManager.Instance.CheckAnswer();
        if (other.name == correctItemName)
        {
            isCorrectlyPlaced = true;
        }
        else
        {
            isCorrectlyPlaced = false;
        }
    }

    public bool IsCorrect()
    {
        return isCorrectlyPlaced;
    }
}