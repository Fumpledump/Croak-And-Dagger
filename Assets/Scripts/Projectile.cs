using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        FrogCharacter player = collision.gameObject.GetComponent<FrogCharacter>();
        if(player != null)
        {
            player.TakeDamage(10);
            GameManager.instance.hudUpdate = true;
        }
        Destroy(gameObject);
    }
}
