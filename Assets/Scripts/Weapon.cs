using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    //[SerializeField] private int attackDamage;
    private CapsuleCollider collider;


    private void Start()
    {
        collider = GetComponent<CapsuleCollider>();
    }
    
    private void Update()
    {
        Collider[] collisions = Physics.OverlapCapsule(collider.transform.position, collider.transform.position, collider.radius);
        foreach (Collider c in collisions)
        {
            if (c.tag.Equals("Enemy"))
            {
                parent.GetComponent<FrogCharacter>().CheckHit(c.gameObject);
            }
        }
    }


    /*
    private void OnCollisionEnter(Collision collision)
    {
        GameObject parent = collision.gameObject.transform.parent.gameObject;

        //Debug.Log("test: "+ parent.tag);
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("omg enemy");
            collision.gameObject.GetComponent<Animator>().SetBool("Hit", true);
            collision.gameObject.GetComponent<Enemy>().GetHit(attackDamage);
        }
    }*/

    
}
