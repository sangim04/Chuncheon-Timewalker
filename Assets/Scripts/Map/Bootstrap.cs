using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public GameObject spawnManagerPrefab;

    void Awake()
    {
        if (SpawnManager.Instance == null)
            Instantiate(spawnManagerPrefab);

        SceneManager.LoadScene("Map");
    }
}
