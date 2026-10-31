using System;
using Game.Gameplay;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Floating damage numbers from a fixed set of TextMeshes created at start (oldest is reused when full).
    /// Number strings are cached, so a hit doesn't allocate.
    /// </summary>
    public sealed class DamageNumbers : MonoBehaviour
    {
        private const int CachedNumbers = 10000;

        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private Camera _camera;
        [SerializeField] private TextMesh _prefab;
        [SerializeField] private int _capacity = 64;
        [SerializeField] private float _lifetime = 0.8f;
        [SerializeField] private float _riseSpeed = 1.5f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _critColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private float _critScale = 1.6f;

        private readonly string[] _numberCache = new string[CachedNumbers];
        private TextMesh[] _texts;
        private float[] _ages;
        private Vector3[] _baseScales;
        private int _next;
        private CombatEvents _events;

        private void Awake()
        {
            if (_bootstrap == null || _camera == null || _prefab == null)
            {
                throw new InvalidOperationException($"{name}: missing GameBootstrap, Camera or TextMesh prefab reference");
            }

            _texts = new TextMesh[_capacity];
            _ages = new float[_capacity];
            _baseScales = new Vector3[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                _texts[i] = Instantiate(_prefab, transform);
                _baseScales[i] = _texts[i].transform.localScale;
                _texts[i].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            RunContext context = _bootstrap.Context ?? throw new InvalidOperationException($"{name}: bootstrap has no run context");
            _events = context.Events;
            _events.DamageDealt += OnDamageDealt;
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.DamageDealt -= OnDamageDealt;
            }
        }

        private void OnDamageDealt(DamageDealtEvent e)
        {
            int slot = _next;
            _next = (_next + 1) % _capacity;

            TextMesh text = _texts[slot];
            text.text = NumberText(Mathf.CeilToInt(e.Amount));
            text.color = e.IsCrit ? _critColor : _normalColor;
            text.transform.localScale = _baseScales[slot] * (e.IsCrit ? _critScale : 1f);
            text.transform.position = e.Position + Vector3.up * 0.5f;
            text.gameObject.SetActive(true);
            _ages[slot] = 0f;
        }

        private void LateUpdate()
        {
            Quaternion facing = _camera.transform.rotation;
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < _capacity; i++)
            {
                TextMesh text = _texts[i];
                if (!text.gameObject.activeSelf)
                {
                    continue;
                }

                _ages[i] += deltaTime;
                if (_ages[i] >= _lifetime)
                {
                    text.gameObject.SetActive(false);
                    continue;
                }

                Transform t = text.transform;
                t.SetPositionAndRotation(t.position + Vector3.up * (_riseSpeed * deltaTime), facing);
                Color color = text.color;
                color.a = 1f - _ages[i] / _lifetime;
                text.color = color;
            }
        }

        private string NumberText(int value)
        {
            if (value < 0 || value >= CachedNumbers)
            {
                return value.ToString();
            }

            return _numberCache[value] ??= value.ToString();
        }
    }
}
