using UnityEngine;

public class QuizSound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public static QuizSound Instance;
    private AudioSource audioSource;

    public AudioClip nextPage;
    public AudioClip closeBook;
    public AudioClip buttonPush;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayNext()
    {
        if (nextPage != null)
            audioSource.PlayOneShot(nextPage, 0.8f);
    }

    public void PlayCLose()
    {
        if (closeBook != null)
            audioSource.PlayOneShot(closeBook, 0.8f);
    }

    public void PlayButton()
    {
        if (buttonPush != null)
            audioSource.PlayOneShot(buttonPush, 0.8f);
    }
}

