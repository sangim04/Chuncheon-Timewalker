using System;
using Unity.VisualScripting;
using UnityEngine;

public class BellCollision : MonoBehaviour
{
    public GameObject bellAudio;
    private GameObject bell;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (bell == null)
        {
            bell = Instantiate(bellAudio, transform.position, Quaternion.identity);
            Destroy(bell, 20);
        }
    }
}
