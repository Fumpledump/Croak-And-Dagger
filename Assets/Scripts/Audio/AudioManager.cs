using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour, IDataPersistence
{
    public Sound[] sounds;

    public static AudioManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        AudioListener.volume = PlayerPrefs.GetFloat("Music");
    }

    private void Start()
    {
        //Play("LevelOneSong");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void StopSounds()
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            sounds[i].source.Stop();
        }
    }

    public string ActiveSong()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].currentActive)
            {
                return sounds[i].name;
            }
        }
        return sounds[0].name;
    }

    public void SetActiveSong(string name, bool active)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.currentActive = active;
    }

    public void LoadData(GameData data)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (data.songStates.ContainsKey(sounds[i].name))
            {
                sounds[i].currentActive = data.songStates[sounds[i].name];
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (data.songStates.ContainsKey(sounds[i].name))
            {
                data.songStates.Remove(sounds[i].name);
            }
            data.songStates.Add(sounds[i].name, sounds[i].currentActive);
        }

    }
}
