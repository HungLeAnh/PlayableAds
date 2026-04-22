using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource audioSource;
    public AudioClip letterSelectedClip;
    public AudioClip wordFoundClip;
    public AudioClip wrongLetterClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayLetterSelected() { PlaySound(letterSelectedClip); }
    public void PlayWordFound() { PlaySound(wordFoundClip); }
    public void PlayWrongLetter() { PlaySound(wrongLetterClip); }
    public void PlayWin() { PlaySound(winClip); }
    public void PlayLose() { PlaySound(loseClip); }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
