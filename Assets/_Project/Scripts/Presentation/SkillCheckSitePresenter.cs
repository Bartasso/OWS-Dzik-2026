using System;
using Game.Gameplay.SkillChecks;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Greys out a site once it has been attempted.</summary>
    public sealed class SkillCheckSitePresenter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private SkillCheckSite _site;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f);

        private MaterialPropertyBlock _block;
        private bool _shownLocked;

        private void Awake()
        {
            if (_site == null)
            {
                throw new InvalidOperationException($"{name}: missing SkillCheckSite reference");
            }

            if (_renderer == null)
            {
                throw new InvalidOperationException($"{name}: missing Renderer reference");
            }

            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (!_site.enabled || _site.IsLocked == _shownLocked)
            {
                return;
            }

            _shownLocked = _site.IsLocked;
            _block.SetColor(BaseColorId, _lockedColor);
            _renderer.SetPropertyBlock(_shownLocked ? _block : null);
        }
    }
}
