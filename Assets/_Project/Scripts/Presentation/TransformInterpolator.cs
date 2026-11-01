using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Smooths a visual child of a body that Gameplay moves in the fixed tick: renders between the last
    /// two tick positions, one tick behind. Without it, motion at the physics rate judders on faster screens.
    /// </summary>
    // Runs after Gameplay so FixedUpdate samples the body after it moved and Update sees this frame's facing.
    [DefaultExecutionOrder(1000)]
    public sealed class TransformInterpolator : MonoBehaviour
    {
        [Tooltip("Transform moved by the simulation, usually the parent.")]
        [SerializeField] private Transform _body;

        [Tooltip("A larger jump in one tick is a teleport and is not smoothed.")]
        [SerializeField] private float _teleportDistance = 3f;

        private Vector3 _localOffset;
        private Vector3 _previous;
        private Vector3 _current;

        private void Awake()
        {
            if (_body == null)
            {
                throw new InvalidOperationException($"{name}: missing body Transform reference");
            }

            _localOffset = Quaternion.Inverse(_body.rotation) * (transform.position - _body.position);
        }

        private void OnEnable()
        {
            _previous = _body.position;
            _current = _previous;
        }

        private void FixedUpdate()
        {
            _previous = _current;
            _current = _body.position;
            if ((_current - _previous).sqrMagnitude > _teleportDistance * _teleportDistance)
            {
                _previous = _current;
            }
        }

        private void Update()
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            transform.position = Vector3.Lerp(_previous, _current, alpha) + _body.rotation * _localOffset;
        }
    }
}
