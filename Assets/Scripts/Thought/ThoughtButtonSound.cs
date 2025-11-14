using UnityEngine;

public class ThoughtButtonSound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public static ThoughtButtonSound Instance;
    private AudioSource audioSource;

    public AudioClip clickSound;   // 버튼 클릭음

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, 0.8f);
    }
}
