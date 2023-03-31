using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyAnimationEvents : MonoBehaviour
{
    public VisualEffect attackEffect;
    public void PlayVisualEffect()
    {
        attackEffect.Play();
    }

    public void StopVisualEffect()
    {
        attackEffect.Stop();
        Debug.Log("stop visual effect");
    }
}
