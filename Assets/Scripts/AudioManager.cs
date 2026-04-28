using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] voiceAClips;
    public AudioClip[] voiceBClips;
    public AudioClip[] voiceCClips;

    public AudioClip bulletSound;

    private int step = 0;

    public bool bulletShoot = false;

    // 0 = no death yet
    // 1 = B died
    // 2 = A died
    // 3 = C died
    // 4 = Self died
    public int characterDied = 0;

    public void PlayNext()
    {
        Debug.Log("Step : " + step);

        // BEFORE SHOOTING
        if (!bulletShoot)
        {
            switch (step)
            {
                case 0: PlayClip(voiceBClips, 0); break;
                case 1: PlayClip(voiceAClips, 0); break;
                case 2: PlayClip(voiceCClips, 0); break;
                case 3: PlayClip(voiceBClips, 1); break;
                case 4: PlayClip(voiceAClips, 1); break;
                case 5: PlayClip(voiceBClips, 2); break;

                default:
                    Debug.Log("Choose who to shoot...");
                    return;
            }
        }

        // 1 = B died
        else if (characterDied == 1)
        {
            switch (step)
            {
                case 6: PlayClip(voiceAClips, 2); break;
                case 7: PlayClip(voiceCClips, 1); break;
                case 8: PlayClip(voiceAClips, 3); break;

                default:
                    Debug.Log("Ending: B Died");
                    return;
            }
        }

        // 2 = A died
        else if (characterDied == 2)
        {
            switch (step)
            {
                case 6: PlayClip(voiceBClips, 3); break;
                case 7: PlayClip(voiceCClips, 2); break;
                case 8: PlayClip(voiceBClips, 4); break;

                default:
                    Debug.Log("Ending: A Died");
                    return;
            }
        }

        // 3 = C died
        else if (characterDied == 3)
        {
            switch (step)
            {
                case 6: PlayClip(voiceAClips, 4); break;
                case 7: PlayClip(voiceBClips, 5); break;
                case 8: PlayClip(voiceAClips, 5); break;

                default:
                    Debug.Log("Ending: C Died");
                    return;
            }
        }

        // 4 = Player shoots self
        else if (characterDied == 4)
        {
            Debug.Log("Ending: You shot yourself");
            return;
        }

        step++;
    }

    void PlayClip(AudioClip[] clips, int index)
    {
        if (index < clips.Length && clips[index] != null)
            audioSource.PlayOneShot(clips[index]);
    }

    public void PlayBullet()
    {
        audioSource.PlayOneShot(bulletSound);
        bulletShoot = true;
    }

    public void ShootB() { characterDied = 1; bulletShoot = true; }
    public void ShootA() { characterDied = 2; bulletShoot = true; }
    public void ShootC() { characterDied = 3; bulletShoot = true; }
    public void ShootSelf() { characterDied = 4; bulletShoot = true; }
}