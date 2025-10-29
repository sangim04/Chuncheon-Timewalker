using UnityEngine;

public class TeleportButton : MonoBehaviour
{
    public RoadView mover;   
    public Transform target;     

    public void TeleportWithCurve()
    {
        mover.MoveTo(target.position);

        Debug.Log("´­¸²");
    }
}
