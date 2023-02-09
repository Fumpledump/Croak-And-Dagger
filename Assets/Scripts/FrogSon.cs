using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSon : MonoBehaviour
{
    // Can be divided into mesh, renderer etc
    public GameObject weaponObject;
    public int attackDamage;
    // (-100 , 100)
    public int relationshipValue;

    // Gravity
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_verticalVelocity < 0.0f)
        {
            _verticalVelocity = -2f;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }

        //_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
             //new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    void SwitchWeapons()
    {

    }
}
