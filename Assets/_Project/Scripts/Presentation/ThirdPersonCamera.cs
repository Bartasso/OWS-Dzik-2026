using System;
using Game.Gameplay.Player;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Over-the-shoulder camera that follows the player's aim. No collision yet.</summary>
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private PlayerAim _aim;

        [Tooltip("What the camera orbits: the player's interpolated visual, not the simulated body.")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _distance = 6f;

        [Tooltip("Height of the orbit pivot above the player's feet.")]
        [SerializeField] private float _pivotHeight = 1.8f;

        [Tooltip("Sideways offset; positive puts the player on the left of the screen.")]
        [SerializeField] private float _shoulderOffset = 0.8f;

        private void Awake()
        {
            if (_aim == null)
            {
                throw new InvalidOperationException($"{name}: missing PlayerAim reference");
            }

            if (_target == null)
            {
                throw new InvalidOperationException($"{name}: missing target Transform reference");
            }
        }

        private void LateUpdate()
        {
            Quaternion rotation = Quaternion.Euler(_aim.Pitch, _aim.Yaw, 0f);
            Vector3 pivot = _target.position + Vector3.up * _pivotHeight;
            transform.SetPositionAndRotation(pivot + rotation * new Vector3(_shoulderOffset, 0f, -_distance), rotation);
        }
    }
}
