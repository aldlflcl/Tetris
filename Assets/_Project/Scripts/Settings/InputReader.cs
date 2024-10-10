using System;
using Tetris.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tetris.GamePlay
{
    [CreateAssetMenu(menuName = "Create Input Reader", fileName = "Input Reader")]
    public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public event Action<Vector2Int> Move;
        public event Action<bool> Rotate;
        public event Action HardDrop;

        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
        }

        public void EnableInputActions()
        {
            _inputActions.Enable();
        }


        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                var readValue = context.ReadValue<Vector2>();
                var adjustedValue = readValue.ToVector2Int();
                Move?.Invoke(adjustedValue);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                Move?.Invoke(Vector2Int.zero);
            }
        }


        public void OnLook(InputAction.CallbackContext context)
        { }

        public void OnFire(InputAction.CallbackContext context)
        { }

        public void OnRotationLeft(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                Rotate?.Invoke(false);
            }
        }

        public void OnRotationRight(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                Rotate?.Invoke(true);
            }
        }

        public void OnHardDrop(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                HardDrop?.Invoke();
            }
        }
    }
}