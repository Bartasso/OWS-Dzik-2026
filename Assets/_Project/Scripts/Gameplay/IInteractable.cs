using UnityEngine;

namespace Game.Gameplay
{
    public interface IInteractable
    {
        Vector3 Position { get; }

        bool CanInteract { get; }

        /// <summary>Shown next to the E prompt.</summary>
        string Prompt { get; }

        void Interact();
    }
}
