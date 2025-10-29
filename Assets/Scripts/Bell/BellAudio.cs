using UnityEngine;

public class BellAudio : MonoBehaviour
{
    public float speed = 0;
    void Start()
    {
        
    }
    
    void Update()
    {
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }
}
