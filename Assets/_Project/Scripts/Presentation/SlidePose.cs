using System;
using Game.Gameplay.Player;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Leans the character's pose back while it slides; the pivot is the pose's origin (the feet).</summary>
    public sealed class SlidePose : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;

        [Tooltip("Degrees the pose leans back while sliding.")]
        [SerializeField] private float _leanDegrees = 30f;

        [Tooltip("Degrees per second towards the target lean.")]
        [SerializeField] private float _leanSpeed = 240f;

        private float _lean;

        private void Awake()
        {
            if (_motor == null)
            {
                throw new InvalidOperationException($"{name}: missing PlayerMotor reference");
            }
        }

        private void Update()
        {
            float target = _motor.IsSliding ? _leanDegrees : 0f;
            _lean = Mathf.MoveTowards(_lean, target, _leanSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(-_lean, 0f, 0f);
        }
    }
}
