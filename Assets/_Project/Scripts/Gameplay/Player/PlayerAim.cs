using Game.Gameplay.Commands;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>
    /// Where the player looks: gameplay state (movement and shots use it), the camera only follows it.
    /// </summary>
    public sealed class PlayerAim : MonoBehaviour
    {
        [Tooltip("Degrees; negative looks up.")]
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 70f;

        public float Yaw { get; private set; }

        public float Pitch { get; private set; }

        public Quaternion YawRotation => Quaternion.Euler(0f, Yaw, 0f);

        /// <summary>Horizontal facing; shots fly flat along it.</summary>
        public Vector3 Forward => YawRotation * Vector3.forward;

        public void Init()
        {
            Yaw = transform.eulerAngles.y;
            Pitch = 0f;
        }

        public void Apply(in AimCommand command)
        {
            Yaw = Mathf.Repeat(Yaw + command.DeltaDegrees.x, 360f);
            Pitch = Mathf.Clamp(Pitch - command.DeltaDegrees.y, _minPitch, _maxPitch);
        }
    }
}
