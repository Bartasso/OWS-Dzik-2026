using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Builds primitive geometry for character controller tests far from anything in the open scene,
    /// and destroys it afterwards. Coordinates passed in are relative to <see cref="Origin"/>.
    /// </summary>
    internal sealed class KccTestScene
    {
        public const float CapsuleRadius = 0.5f;
        public const float CapsuleHeight = 2f;
        public const float Hover = 0.01f;

        public static readonly Vector3 Origin = new Vector3(5000f, 5000f, 5000f);

        private readonly List<GameObject> _created = new List<GameObject>();

        /// <summary>Axis-aligned box given by its minimum corner and size.</summary>
        public GameObject Box(Vector3 min, Vector3 size)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "KccTestBox";
            box.transform.position = Origin + min + size * 0.5f;
            box.transform.localScale = size;
            _created.Add(box);
            return box;
        }

        /// <summary>Flat floor whose top face lies at the given height.</summary>
        public GameObject Floor(float topY = 0f)
        {
            return Box(new Vector3(-20f, topY - 1f, -20f), new Vector3(40f, 1f, 40f));
        }

        /// <summary>
        /// Plane tilted around Z by an angle, rising towards +X, passing through <see cref="Origin"/>.
        /// </summary>
        public GameObject Slope(float angleDegrees)
        {
            GameObject slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = "KccTestSlope";
            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees);
            slope.transform.SetPositionAndRotation(Origin - rotation * new Vector3(0f, 0.5f, 0f), rotation);
            slope.transform.localScale = new Vector3(20f, 1f, 20f);
            _created.Add(slope);
            return slope;
        }

        /// <summary>Capsule character with its feet at the given position.</summary>
        public KCCMovementEngine Character(Vector3 feet)
        {
            var go = new GameObject("KccTestCharacter");
            go.transform.position = Origin + feet;

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = CapsuleRadius;
            capsule.height = CapsuleHeight;
            capsule.center = new Vector3(0f, CapsuleHeight * 0.5f, 0f);
            go.AddComponent<CapsuleColliderCast>();

            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;

            var engine = go.AddComponent<KCCMovementEngine>();
            _created.Add(go);

            Physics.SyncTransforms();

            // Awake does not run for components added in edit mode; teleport sets the same state.
            engine.TeleportPlayer(go.transform.position);
            return engine;
        }

        public void Destroy()
        {
            foreach (GameObject go in _created)
            {
                Object.DestroyImmediate(go);
            }

            _created.Clear();
        }
    }
}
