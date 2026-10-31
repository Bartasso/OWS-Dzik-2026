using System;
using System.Collections.Generic;
using Game.Gameplay.Commands;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>Picks the nearest interactable within GameBalance's interact radius.</summary>
    public sealed class Interactor : MonoBehaviour
    {
        private RunContext _context;
        private IReadOnlyList<IInteractable> _interactables;

        public IInteractable Current { get; private set; }

        public void Init(RunContext context, IReadOnlyList<IInteractable> interactables)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _interactables = interactables ?? throw new ArgumentNullException(nameof(interactables));
            enabled = true;
        }

        public void Apply(in InteractCommand command)
        {
            if (_context.Pause.IsPaused || Current == null)
            {
                return;
            }

            Current.Interact();
        }

        private void Update()
        {
            Current = null;
            if (_context.Pause.IsPaused)
            {
                return;
            }

            float radius = _context.Balance.InteractRadius;
            float bestDistanceSq = radius * radius;
            Vector3 position = transform.position;
            for (int i = 0; i < _interactables.Count; i++)
            {
                IInteractable interactable = _interactables[i];
                if (!interactable.CanInteract)
                {
                    continue;
                }

                Vector3 offset = interactable.Position - position;
                offset.y = 0f;
                float distanceSq = offset.sqrMagnitude;
                if (distanceSq <= bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    Current = interactable;
                }
            }
        }
    }
}
