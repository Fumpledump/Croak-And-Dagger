using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public string stopSong;
    public string startSong;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            AudioManager.instance.Stop(stopSong);
            AudioManager.instance.Play(startSong);
        }
    }

}
