using Cinemachine;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TargetLock : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera followCam;
    [SerializeField] private CinemachineVirtualCamera targetCam;
    [SerializeField] private Camera mainCam;

    [SerializeField] public GameObject target;
    [SerializeField] private GameObject targetSwitcher;
    [SerializeField] private Image lockImage;
    [SerializeField] private StarterAssetsInputs input;

    [SerializeField] private Transform player;
    [SerializeField] private Transform targetCamHelper;

    [SerializeField] private float switchSensitivity;
    [SerializeField] private float switchCooldown;
    [SerializeField] private float timeSinceLastSwitch;
    [SerializeField] private bool justSwitched;
    [SerializeField] private TargetSwitch switcher;

    void Start()
    {
        switcher = targetSwitcher.GetComponent<TargetSwitch>();
    }
    void Update()
    {
        CheckCameraSwitch();
        

        if (lockImage.IsActive())
        {
            MoveLockOnCamera();
            MoveLockOnSprite();
        }

        if (justSwitched)
        {
            if(timeSinceLastSwitch < switchCooldown) timeSinceLastSwitch += Time.deltaTime;
            else 
            {
                justSwitched = false;
            }
        }
    }

    private void CheckTargetAlive()
    {
        if(target.transform.parent.GetComponent<Enemy>().health <= 0)
        {
            if (!FindEnemy()) ToggleCamSwitch();
        }
    }

    
    private void CheckCameraSwitch()
    {
        // if user presses the lock on toggle button, switch used camera
        if (input.lockOnEnemy)
        {
            input.lockOnEnemy = false;

            ToggleCamSwitch();
        }

        // if the current target is dead, either find a new target or stop locking on
        if (target == null || !targetCam.gameObject.activeSelf) return;
        Enemy enemy = target.transform.parent.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (enemy.health <= 0)
            {
                if (!FindEnemy()) ToggleCamSwitch();
                else targetCam.LookAt = target.transform;
            }
        }

    }

    bool FindEnemy()
    {
        try{ target = switcher.GetTarget("center").gameObject; }
        catch { return false; }
        return true;
    }

    // Called when user toggles the target lock
    // Disables the current camera and enables the other one
    // Ex: If the freecam is active and the user turns on target lock, disable freecam and enable target cam
    private void ToggleCamSwitch()
    {
        if (followCam.isActiveAndEnabled)
        {
            if (!FindEnemy()) return;
            input.lockedOn = true;
            targetCam.LookAt = target.transform;
            followCam.gameObject.SetActive(false);
            targetCam.gameObject.SetActive(true);
            lockImage.gameObject.SetActive(true);
        }
        else
        {
            target = null;
            switcher.ClearTarget();
            targetCam.LookAt = null;
            input.lockedOn = false;
            targetCam.gameObject.SetActive(false);
            lockImage.gameObject.SetActive(false);
            followCam.gameObject.SetActive(true);
        }
    }

    // moves the lock on camera to look at the target while behind the player's head
    private void MoveLockOnCamera()
    {
        if (justSwitched) { MoveCamera(); return; }

        if (MathF.Abs(input.look.x) > switchSensitivity)
        {
            if (input.look.x < 0 && switcher.leftCollider != null) // left
            {
                target = switcher.GetTarget("left").gameObject;
            }
            else if (input.look.x > 0 && switcher.rightCollider != null) // right
            {
                target = switcher.GetTarget("right").gameObject;
            }
            else return;

            targetCam.LookAt = target.transform;
            justSwitched = true; timeSinceLastSwitch = 0;
        }
        else if(MathF.Abs(input.look.y) > switchSensitivity)
        {
            if (input.look.y < 0 && switcher.upCollider != null) // up
            {
                target = switcher.GetTarget("up").gameObject;
            }
            else if (input.look.y > 0 && switcher.downCollider != null) // down
            {
                target = switcher.GetTarget("down").gameObject;
            }
            else return;

            targetCam.LookAt = target.transform;
            justSwitched = true; timeSinceLastSwitch = 0;
        }

        MoveCamera();
    }
    private void MoveCamera()
    {
        //TODO: Make Target Transition smoother

        targetCamHelper.LookAt(target.transform);

        // normalized vector for the distance between the player and the target
        Vector3 btwn = (player.position - target.transform.position).normalized;
        Vector3 testBtwn = btwn * 8;

        // offsets the current position to behind the players head
        //targetCamHelper.position = new Vector3(player.position.x + (btwn.x * 12), player.position.y + 2, player.position.z + (btwn.z * 12));
        targetCamHelper.position = new Vector3(player.position.x + (testBtwn.x), player.position.y + 2, player.position.z + (testBtwn.z));
    }

    // move the target lock sprite to the object being targeted
    private void MoveLockOnSprite()
    {
        lockImage.gameObject.transform.position = mainCam.WorldToScreenPoint(target.transform.position);
    }

    /*private Vector3 GetEnemyMidpoint()
    {
        float targetHeight = target.GetComponent<CapsuleCollider>().height;
        Vector3 targetPos = target.transform.position;
        return new Vector3(targetPos.x, targetPos.y + targetHeight/2, targetPos.z);
    }*/
}