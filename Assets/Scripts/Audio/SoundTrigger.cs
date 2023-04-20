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
            AudioManager.instance.SetActiveSong(stopSong, false);
            AudioManager.instance.Play(startSong);
            AudioManager.instance.SetActiveSong(startSong, true);
        }
    }

}
