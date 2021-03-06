using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;

    public enum Sounds
    {
        None,
        Money,
        Forbidden,
        Logging,
        Knock,
        Scratch,
        Whoosh,
        Fire,
        Ping,
        Hit,
    }

    public AudioSource soundSource3d;
    public AudioSource soundSource2d;
    public AudioSource musicSource;
    public int sources = 20;

    public AudioClip[] moneySounds;
    public AudioClip[] forbiddenSounds;
    public AudioClip[] loggingSounds;
    public AudioClip[] knockSounds;
    public AudioClip[] scratchSounds;
    public AudioClip[] whooshSounds;
    public AudioClip[] fireSounds;
    public AudioClip[] pingSounds;
    public AudioClip[] hitSounds;


    List<AudioSource> soundSources3d;
    List<AudioSource> soundSources2d;
    int soundSourceIndex3d = 0;
    int soundSourceIndex2d = 0;

    private void Awake()
    {
        instance = this;
        soundSources3d = new List<AudioSource>(sources);
        soundSources3d.Add(soundSource3d);
        for (int i = 1; i < sources; i++)
        {
            soundSources3d.Add(Instantiate<AudioSource>(soundSource3d, Vector3.zero, Quaternion.identity, transform));
        }
        soundSources2d = new List<AudioSource>(sources);
        soundSources2d.Add(soundSource2d);
        for (int i = 1; i < sources; i++)
        {
            soundSources2d.Add(Instantiate<AudioSource>(soundSource2d, Vector3.zero, Quaternion.identity, transform));
        }
    }

    public static void PlaySound(Sounds sound, Vector3 pos)
    {
        instance.PlaySound3d(sound, pos);
    }

    public static void PlaySound(Sounds sound, float delay = 0f)
    {
        if (delay > 0f)
            instance.PlaySound2dDelayed(sound, delay);
        else
            instance.PlaySound2d(sound);
    }

    public void PlaySound3d(Sounds sound, Vector3 pos)
    {
        var source = soundSources3d[++soundSourceIndex3d % sources];
        source.transform.position = pos;
        var clip = GetClip(sound);
        if (clip)
        {
            source.pitch = Random.Range(0.9f, 1.15f);
            source.PlayOneShot(clip);
        }
    }

    public void PlaySound2d(Sounds sound)
    {
        var source = soundSources2d[++soundSourceIndex2d % sources];
        var clip = GetClip(sound);
        if (clip)
        {
            source.pitch = Random.Range(0.9f, 1.15f);
            source.PlayOneShot(clip);
        }
    }

    public void PlaySound2dDelayed(Sounds sound, float delay)
    {
        var source = soundSources2d[++soundSourceIndex2d % sources];
        var clip = GetClip(sound);
        if (clip)
        {
            source.pitch = Random.Range(0.9f, 1.15f);
            source.clip = clip;
            source.PlayDelayed(delay);
        }
    }

    AudioClip GetClip(Sounds sound)
    {
        return sound switch
        {
            Sounds.None => null,
            Sounds.Money => moneySounds[Random.Range(0, moneySounds.Length)],
            Sounds.Forbidden => forbiddenSounds[Random.Range(0, forbiddenSounds.Length)],
            Sounds.Logging => loggingSounds[Random.Range(0, loggingSounds.Length)],
            Sounds.Knock => knockSounds[Random.Range(0, knockSounds.Length)],
            Sounds.Scratch => scratchSounds[Random.Range(0, scratchSounds.Length)],
            Sounds.Whoosh => whooshSounds[Random.Range(0, whooshSounds.Length)],
            Sounds.Fire => fireSounds[Random.Range(0, fireSounds.Length)],
            Sounds.Ping => pingSounds[Random.Range(0, pingSounds.Length)],
            Sounds.Hit => hitSounds[Random.Range(0, hitSounds.Length)],
            _ => throw new System.NotImplementedException(),
        };
    }

    public static void StopMusic()
    {
        instance.musicSource.mute = true;
    }

    public void Knock()
    {
        PlaySound2d(Sounds.Knock);
    }
}
