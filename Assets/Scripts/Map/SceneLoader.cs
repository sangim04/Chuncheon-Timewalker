using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName; 

    void OnMouseDown()
    {
        SceneManager.LoadScene(sceneName);
    }
}
