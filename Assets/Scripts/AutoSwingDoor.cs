
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace UdonSharp.Examples.Utilities
{
    /// <summary>
    /// Swings a door open as a player approaches/passes through a trigger volume,
    /// and closes it again after everyone has left. Local-only (not networked) -
    /// each client animates the door based on its own view of nearby players.
    /// Requires a Collider (isTrigger = true) on this same GameObject.
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
        public float openAngle = -100f;

        [Tooltip("Degrees per second the door swings")]
        public float swingSpeed = 180f;

        [Tooltip("Seconds after the last player leaves the trigger before the door swings shut")]
        public float closeDelay = 1.5f;

        private Vector3[] _localOffsets;
        private Quaternion[] _localRotations;
        private float _currentAngle;
        private float _targetAngle;
        private int _playersInside;
        private float _closeTimer;
        private bool _pendingClose;

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

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            _playersInside++;
            _targetAngle = openAngle;
            _pendingClose = false;
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            _playersInside = _playersInside > 0 ? _playersInside - 1 : 0;
            if (_playersInside == 0)
            {
                _pendingClose = true;
                _closeTimer = closeDelay;
            }
        }

        void Update()
        {
            if (_pendingClose)
            {
                _closeTimer -= Time.deltaTime;
                if (_closeTimer <= 0f)
                {
                    _targetAngle = 0f;
                    _pendingClose = false;
                }
            }

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
