using System;
using System.Text;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using Game.Gameplay.Combat;
using Game.Gameplay.Enemies;
using Game.Gameplay.Player;
using Game.Gameplay.SkillChecks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Sandbox
{
    /// <summary>
    /// Debug overlay (IMGUI) for the sandbox: stats, owned items, skill check results, test buttons.
    /// Allocates strings every frame; it's a test tool, not part of the game.
    /// </summary>
    public sealed class SandboxDebugPanel : MonoBehaviour
    {
        private const float PanelWidth = 360f;

        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private SandboxSettings _settings;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerShooter _playerShooter;
        [SerializeField] private Interactor _interactor;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private ProjectileSystem _projectiles;

        [Tooltip("Seconds the last skill check result stays on screen.")]
        [SerializeField] private float _bannerSeconds = 4f;

        private readonly StringBuilder _text = new StringBuilder(2048);
        private RunContext _context;
        private bool _panelVisible = true;
        private string _banner;
        private float _bannerRemaining;
        private string _procResult = "-";
        private Vector2 _scroll;
        private GUIStyle _bannerStyle;
        private GUIStyle _centerStyle;

        private void Awake()
        {
            if (_bootstrap == null || _settings == null || _playerHealth == null || _playerShooter == null
                || _interactor == null || _enemySpawner == null || _projectiles == null)
            {
                throw new InvalidOperationException($"{name}: missing scene reference, rebuild the sandbox scene");
            }
        }

        private void Start()
        {
            _context = _bootstrap.Context ?? throw new InvalidOperationException($"{name}: bootstrap has no run context");
            ValidationReport report = ValidationLog.ValidateAndLog(_settings);
            if (report.HasErrors)
            {
                throw new InvalidOperationException($"{name}: {_settings.name} is invalid, see the console");
            }

            _context.Events.SkillCheckResolved += OnSkillCheckResolved;
            SetCursorLocked(true);
        }

        private void OnDestroy()
        {
            if (_context != null)
            {
                _context.Events.SkillCheckResolved -= OnSkillCheckResolved;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                _panelVisible = !_panelVisible;
            }

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (_bannerRemaining > 0f)
            {
                _bannerRemaining -= Time.deltaTime;
            }
        }

        private void OnSkillCheckResolved(SkillCheckResolvedEvent e)
        {
            SkillCheckOutcome outcome = e.Outcome;
            SkillCheckRequest request = outcome.Request;
            SkillCheckResult result = outcome.Result;
            int proficiency = request.IsProficient ? _context.Balance.ToSkillCheckRules().ProficiencyBonus : 0;

            string verdict;
            if (result.Nat20)
            {
                verdict = $"NATURALNE 20! Sukces, nagroda x{outcome.RewardCopies}";
            }
            else if (result.Nat1)
            {
                verdict = "NATURALNA 1! Katastrofa";
            }
            else
            {
                verdict = result.Success ? "SUKCES" : "PORAŻKA";
            }

            string mode = request.HasAdvantage && !request.HasDisadvantage ? " (advantage)"
                : request.HasDisadvantage && !request.HasAdvantage ? " (disadvantage)" : "";
            string reroll = result.WasRerolled ? " (przerzut z LCK!)" : "";
            string reward = result.Success ? $"\nNagroda: {e.Site.Reward.DisplayName}" : "";

            _banner = $"{e.Site.DisplayName}: {e.Site.Skill.DisplayName} ({e.Site.Skill.Ability}){mode}\n"
                + $"k20 = {result.Natural}{reroll}  + cecha {request.AbilityModifier}  + biegłość {proficiency}  + bonus {request.Bonus}"
                + $"  = {result.Total}  vs DC {request.Dc}\n{verdict}{reward}";
            _bannerRemaining = _bannerSeconds;
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawCenter();
            if (_panelVisible)
            {
                DrawPanel();
            }
        }

        private void DrawCenter()
        {
            float w = Screen.width;
            float h = Screen.height;
            GUI.Label(new Rect(w / 2f - 10f, h / 2f - 12f, 20f, 24f), "+", _centerStyle);

            IInteractable current = _interactor.Current;
            if (current != null)
            {
                GUI.Label(new Rect(0f, h * 0.62f, w, 30f), $"[E] {current.Prompt}", _centerStyle);
            }

            if (_bannerRemaining > 0f && _banner != null)
            {
                GUI.Box(new Rect(w / 2f - 380f, h * 0.15f, 760f, 110f), _banner, _bannerStyle);
            }

            if (_playerHealth.IsDead)
            {
                GUI.Box(new Rect(w / 2f - 200f, h / 2f - 40f, 400f, 80f), "Albert padł.\nF5: nowy run", _bannerStyle);
            }
        }

        private void DrawPanel()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, PanelWidth, Screen.height - 20f), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);

            GUILayout.Label(BuildStatusText());

            GUILayout.Label("Test:");
            foreach (TalentDef talent in _settings.TestTalents)
            {
                if (GUILayout.Button($"Talent: {talent.DisplayName} (x{_context.Loadout.GetStacks(talent)})"))
                {
                    _context.Loadout.Add(talent);
                }
            }

            if (GUILayout.Button("Wywołaj trigger BossKilled"))
            {
                _context.Loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));
            }

            if (GUILayout.Button("Przyzwij bossa"))
            {
                _enemySpawner.SpawnBoss();
            }

            if (GUILayout.Button($"Rzuć proc {_settings.TestProcChance:P0} (wynik: {_procResult})"))
            {
                _procResult = _context.Loadout.RollProc(_settings.TestProcChance) ? "TAK" : "nie";
            }

            _playerHealth.Invulnerable = GUILayout.Toggle(_playerHealth.Invulnerable, "Nieśmiertelność");

            GUILayout.Label("WASD ruch, mysz celowanie, LPM strzał, E interakcja\nTab kursor, F1 panel, F5 nowy run");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string BuildStatusText()
        {
            StatSheet stats = _context.Stats;
            GameBalance balance = _context.Balance;
            WeaponDef weapon = _playerShooter.Weapon;
            Ability scaling = _context.Scaling.Resolve(weapon.Category, weapon.ScalingAbility);

            _text.Clear();
            _text.AppendLine($"{_context.Character.DisplayName}   seed {_context.Rng.Seed}");
            _text.AppendLine($"HP {_playerHealth.Current:0}/{_playerHealth.Max:0}{(_context.Pause.IsPaused ? "   [PAUZA]" : "")}");
            _text.AppendLine($"Wrogowie {_enemySpawner.Alive}   zabici {_enemySpawner.Kills}   pociski {_projectiles.ActiveCount}");
            _text.AppendLine();
            _text.AppendLine($"{weapon.DisplayName}: {weapon.Category} skaluje z {scaling}");
            _text.AppendLine($"Obrażenia {DamageCalculator.BaseDamage(weapon, stats, _context.Scaling, balance):0.#}"
                + $"   krytyk {DamageCalculator.CritChance(stats, balance):P1} x{DamageCalculator.CritMultiplier(stats, balance):0.##}");
            _text.AppendLine($"Odstęp strzałów {_playerShooter.FireInterval:0.###} s");
            _text.AppendLine();

            foreach (StatId stat in (StatId[])Enum.GetValues(typeof(StatId)))
            {
                _text.AppendLine(StatIds.IsInteger(stat) ? $"{stat}: {stats.GetInt(stat)}" : $"{stat}: {stats.Get(stat):0.###}");
            }

            _text.AppendLine();
            _text.AppendLine("Posiadane:");
            if (_context.Loadout.Owned.Count == 0)
            {
                _text.AppendLine("  (nic)");
            }

            foreach (EffectDefinition owned in _context.Loadout.Owned)
            {
                _text.AppendLine($"  {owned.DisplayName} x{_context.Loadout.GetStacks(owned)}");
            }

            return _text.ToString();
        }

        private void EnsureStyles()
        {
            if (_bannerStyle != null)
            {
                return;
            }

            _bannerStyle = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _centerStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
