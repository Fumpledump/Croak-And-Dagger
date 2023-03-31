using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    private Camera currentCamera;
    // Start is called before the first frame update
    void Start()
    {
        currentCamera = Camera.main;
    }

    // Update is called once per frame

    private void Update()
    {

    }

    private void LateUpdate()
    {
        if (currentCamera == null || currentCamera.isActiveAndEnabled == false)
        {
            currentCamera = Camera.main;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - currentCamera.transform.position);
    }

}
