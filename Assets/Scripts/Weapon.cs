using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    //[SerializeField] private int attackDamage;
    private CapsuleCollider collider;


    private void Start()
    {
        collider = GetComponent<CapsuleCollider>();
        //Debug.Log(collider.transform.position);
        //Debug.Log(collider.radius);
    }
    
    private void Update()
    {

        // Shout out to the internet for the math on this one
        Vector3 direction = new Vector3 { [collider.direction] = 1 };
        float offset = collider.height / 2 - collider.radius;
        Vector3 localPoint0 = collider.center - direction * offset;
        Vector3 localPoint1 = collider.center + direction * offset;
        Vector3 point0 = transform.TransformPoint(localPoint0);
        Vector3 point1 = transform.TransformPoint(localPoint1);
        Vector3 r = transform.TransformVector(collider.radius, collider.radius, collider.radius);
        float radius = Enumerable.Range(0, 3).Select(xyz => xyz == collider.direction ? 0 : r[xyz])
            .Select(Mathf.Abs).Max();


        Collider[] collisions = Physics.OverlapCapsule(point0, point1, radius);
        foreach (Collider c in collisions)
        {
            if (c.tag.Equals("Enemy") || c.tag.Equals("Boss"))
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
