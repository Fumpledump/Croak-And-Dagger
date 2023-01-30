using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //HOW TO USE:
    //There is a Sound Manager Object in the world. It has an SFX array in which you can add sounds to.
    //Any script in the game can just type:
    //  SoundManager.instance.PlaySound(*sound index*) <- Here you use the index from the sfx array
    
    public static SoundManager Instance;
    public AudioClip[] sfx;

    [SerializeField] private AudioSource musicSource, effectsSource;

    //Singleton sound manager. A single sound manager will be created in the game
    //and will manage both music and effects for the game.
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(int clipNum)
    {
        effectsSource.PlayOneShot(sfx[clipNum]);
    }
}
