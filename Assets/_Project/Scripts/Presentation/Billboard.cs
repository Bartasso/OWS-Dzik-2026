using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Keeps a label facing the camera.</summary>
    public sealed class Billboard : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_camera == null)
            {
                throw new InvalidOperationException($"{name}: missing Camera reference");
            }
        }

        private void LateUpdate()
        {
            transform.rotation = _camera.transform.rotation;
        }
    }
}
