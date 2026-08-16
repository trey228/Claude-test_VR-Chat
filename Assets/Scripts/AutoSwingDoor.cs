
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace UdonSharp.Examples.Utilities
{
    /// <summary>
    /// Swings a door open when a player clicks/interacts with it, holds it open
    /// for a set duration, then swings it shut again. Interacting again while
    /// open resets the open timer. Local-only (not networked) - each client
    /// animates the door based on its own interaction.
    /// Requires a Collider on this same GameObject for Interact to register.
    /// </summary>
    [AddComponentMenu("Udon Sharp/Utilities/Auto Swing Door")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class AutoSwingDoor : UdonSharpBehaviour
    {
        [Tooltip("World-space point the door rotates around (the hinge)")]
        public Transform hinge;

        [Tooltip("All door pieces (leaf, handle, lock hardware) that swing together")]
        public Transform[] doorParts;

        [Tooltip("Angle in degrees the door swings open around the hinge's up axis; sign controls direction")]
        public float openAngle = -90f;

        [Tooltip("Degrees per second the door swings")]
        public float swingSpeed = 180f;

        [Tooltip("Seconds the door stays open after being clicked before it swings shut")]
        public float openDuration = 15f;

        private Vector3[] _localOffsets;
        private Quaternion[] _localRotations;
        private float _currentAngle;
        private float _targetAngle;
        private int _pendingCloseCount;

        void Start()
        {
            int count = doorParts.Length;
            _localOffsets = new Vector3[count];
            _localRotations = new Quaternion[count];
            for (int i = 0; i < count; i++)
            {
                Transform part = doorParts[i];
                _localOffsets[i] = part.position - hinge.position;
                _localRotations[i] = part.rotation;
            }
        }

        public override void Interact()
        {
            _targetAngle = openAngle;
            _pendingCloseCount++;
            SendCustomEventDelayedSeconds(nameof(_CloseDoor), openDuration);
        }

        public void _CloseDoor()
        {
            if (_pendingCloseCount > 0)
                _pendingCloseCount--;

            if (_pendingCloseCount == 0)
                _targetAngle = 0f;
        }

        void Update()
        {
            if (!Mathf.Approximately(_currentAngle, _targetAngle))
            {
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, swingSpeed * Time.deltaTime);
                Quaternion swing = Quaternion.AngleAxis(_currentAngle, Vector3.up);
                for (int i = 0; i < doorParts.Length; i++)
                {
                    Transform part = doorParts[i];
                    part.position = hinge.position + swing * _localOffsets[i];
                    part.rotation = swing * _localRotations[i];
                }
            }
        }
    }
}
