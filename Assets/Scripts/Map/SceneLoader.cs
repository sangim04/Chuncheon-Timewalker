using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Map SpawnPoint")]
    public Transform mapSpawnPoint;

    [Header("로드할 씬 이름")]
    public string sceneName;

    public void LoadScene()
    {
        if (sceneName == "Map" && mapSpawnPoint != null)
        {
            SpawnManager.Instance.pendingSpawnPosition = mapSpawnPoint.position;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Map" &&
            SpawnManager.Instance.pendingSpawnPosition != Vector3.zero)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();

                if (controller != null) controller.enabled = false;

                player.transform.position = SpawnManager.Instance.pendingSpawnPosition;

                if (controller != null) controller.enabled = true;
            }

            SpawnManager.Instance.pendingSpawnPosition = Vector3.zero;
        }
    }
}
