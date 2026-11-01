using System;
using Game.Gameplay.Player;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Feeds an Animator bool with whether the player is sliding (crouch clip for now).</summary>
    public sealed class SlideAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Animator _animator;
        [SerializeField] private string _slidingParameter = "Sliding";

        private int _parameter;

        private void Awake()
        {
            if (_motor == null)
            {
                throw new InvalidOperationException($"{name}: missing PlayerMotor reference");
            }

            if (_animator == null)
            {
                throw new InvalidOperationException($"{name}: missing Animator reference");
            }

            _parameter = Animator.StringToHash(_slidingParameter);
        }

        private void Update()
        {
            _animator.SetBool(_parameter, _motor.IsSliding);
        }
    }
}
