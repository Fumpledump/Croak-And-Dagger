using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool dash;
		public bool pAttack;
		public bool hAttack;
		public bool lockOnEnemy;
		public bool lockedOn;
		public bool interact;
		public bool reportTongueChange;
		public bool cheat;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnDash(InputValue value)
		{
			DashInput(value.isPressed);
		}
		public void OnPAttack(InputValue value)
		{
			PAttackInput(value.isPressed);
		}

		public void OnHAttack(InputValue value)
		{
			HAttackInput(value.isPressed);
		}

		public void OnLockOnEnemy(InputValue value)
		{
			LockOnEnemyInput(value.isPressed);
		}
		public void OnTongue(InputValue value)
		{
            //this gets set back to false in FrogCharacter.cs once it's been handled (hence "report" rather than just storing the actual bool value)
            //it's kind of cursed and I am truly sorry but this is the only way I could figure out how to detect a button being held/released???
            reportTongueChange = true;
		}


		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnCheat(InputValue value)
		{
			CheatInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void DashInput(bool newDashState)
		{
			dash = newDashState;
		}

		public void PAttackInput(bool newAttackState)
		{
			pAttack = newAttackState;
		}

		public void HAttackInput(bool newHeavyAttackState)
		{
			hAttack = newHeavyAttackState;
		}

		public void LockOnEnemyInput(bool newLockState)
		{
			lockOnEnemy = newLockState;
		}
		public void InteractInput(bool newInteraction)
		{
			interact = newInteraction;
		}
		public void CheatInput(bool newInteraction)
		{
			cheat = newInteraction;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}