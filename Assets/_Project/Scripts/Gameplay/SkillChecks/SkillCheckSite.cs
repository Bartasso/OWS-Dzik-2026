using System;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.SkillChecks
{
    public sealed class SkillCheckSite : MonoBehaviour, IInteractable
    {
        [SerializeField] private SkillCheckSiteDef _definition;

        private RunContext _context;
        private SkillCheckSiteState _state;
        private float _pauseRemaining;
        private bool _holdingPause;

        public SkillCheckSiteDef Definition => _definition;

        public bool IsLocked => _state.IsLocked;

        public Vector3 Position => transform.position;

        public bool CanInteract => !_state.IsLocked;

        public string Prompt => _definition.DisplayName;

        public void Init(RunContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _state = new SkillCheckSiteState(_definition);
            enabled = true;
        }

        public void Interact()
        {
            SkillCheckOutcome outcome = _state.Attempt(
                _context.Stats, _context.Character, _context.SkillCheck, _context.Loadout, _context.Balance.Nat20RewardCopies);
            _context.Events.RaiseSkillCheckResolved(new SkillCheckResolvedEvent(_definition, outcome));

            _context.Pause.Push();
            _holdingPause = true;
            _pauseRemaining = _context.Balance.SkillCheckPauseSeconds;
        }

        private void Awake()
        {
            if (_definition == null)
            {
                throw new InvalidOperationException($"{name}: missing SkillCheckSiteDef reference");
            }
        }

        private void Update()
        {
            if (!_holdingPause)
            {
                return;
            }

            _pauseRemaining -= Time.deltaTime;
            if (_pauseRemaining <= 0f)
            {
                _holdingPause = false;
                _context.Pause.Pop();
            }
        }
    }
}
