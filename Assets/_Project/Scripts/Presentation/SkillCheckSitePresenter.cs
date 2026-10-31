using System;
using Game.Gameplay.SkillChecks;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Greys out a site once it has been attempted.</summary>
    public sealed class SkillCheckSitePresenter : MonoBehaviour
    {
        [SerializeField] private SkillCheckSite _site;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f);

        [Tooltip("Tint property of the renderer's shader: _BaseColor for URP Lit, _Color for the PSX kits.")]
        [SerializeField] private string _colorProperty = "_BaseColor";

        private int _colorId;
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

            _colorId = Shader.PropertyToID(_colorProperty);
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (!_site.enabled || _site.IsLocked == _shownLocked)
            {
                return;
            }

            _shownLocked = _site.IsLocked;
            _block.SetColor(_colorId, _lockedColor);
            _renderer.SetPropertyBlock(_shownLocked ? _block : null);
        }
    }
}
