using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Feeds an Animator float with the horizontal speed this object moves at, so walk and run clips
    /// follow the movement that Gameplay drives. Reads the transform only.
    /// </summary>
    public sealed class LocomotionAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _speedParameter = "Speed";

        [Tooltip("Parameter value per metre a second, e.g. 1 / the clip's own walk speed.")]
        [SerializeField] private float _valuePerMetrePerSecond = 1f;

        [Tooltip("A jump larger than this in one frame is a teleport (pool respawn), not movement.")]
        [SerializeField] private float _teleportDistance = 2f;

        [SerializeField] private float _damping = 0.1f;

        private int _parameter;
        private Vector3 _lastPosition;

        private void Awake()
        {
            if (_animator == null)
            {
                throw new InvalidOperationException($"{name}: missing Animator reference");
            }

            _parameter = Animator.StringToHash(_speedParameter);
        }

        private void OnEnable()
        {
            _lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            Vector3 position = transform.position;
            Vector3 delta = position - _lastPosition;
            _lastPosition = position;
            delta.y = 0f;

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            float distance = delta.magnitude;
            float speed = distance > _teleportDistance ? 0f : distance / deltaTime;
            _animator.SetFloat(_parameter, speed * _valuePerMetrePerSecond, _damping, deltaTime);
        }
    }
}
