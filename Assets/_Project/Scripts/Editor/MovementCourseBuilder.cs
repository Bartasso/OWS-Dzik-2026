using System;
using UnityEngine;
using static Game.Editor.SceneBuildHelpers;

namespace Game.Editor
{
    /// <summary>
    /// Primitive geometry for testing movement: up-and-down hills of several angles, a walkable and a
    /// too-steep ramp, stairs to a long slide slope, a half-pipe, a low beam to slide under and a wall.
    /// Stays clear of the sandbox's skill check sites and pickups around the origin.
    /// </summary>
    internal static class MovementCourseBuilder
    {
        private const float Thickness = 0.5f;
        private const float Width = 6f;

        // Half-pipe side profile from the flat bottom outwards: each segment is steeper than the last.
        private static readonly float[] HalfPipeAngles = { 8f, 16f, 24f, 32f, 40f, 48f };

        public static void Build(Func<string, Color, Material> getMaterial, Camera camera, Font font)
        {
            Material ramp = getMaterial("Ramp", new Color(0.55f, 0.5f, 0.4f));
            Material block = getMaterial("CourseBlock", new Color(0.5f, 0.5f, 0.55f));
            Material beam = getMaterial("Beam", new Color(0.75f, 0.35f, 0.2f));
            Transform root = new GameObject("Movement Course").transform;

            // Hills along +X: ramp up, plateau, ramp down.
            Hill(root, new Vector3(-40f, 0f, -25f), 15f, 3f, ramp, block, camera, font);
            Hill(root, new Vector3(-40f, 0f, -36f), 25f, 3f, ramp, block, camera, font);

            // Single ramps onto a plateau with a sheer back edge (walking off it tests coyote time).
            RampOntoPlateau(root, new Vector3(-40f, 0f, -14f), 45f, 3f, "45° (walkable)", ramp, block, camera, font);
            RampOntoPlateau(root, new Vector3(-40f, 0f, -4f), 60f, 3f, "60° (too steep)", ramp, block, camera, font);

            // Stairs (0.3 m steps) up to a platform, then a long 10° slope down for sliding.
            const float stepHeight = 0.3f;
            const float stepDepth = 0.5f;
            const int steps = 12;
            float top = steps * stepHeight;
            for (int i = 0; i < steps; i++)
            {
                Box(root, "Step", new Vector3(6f + i * stepDepth, 0f, -33f), new Vector3(stepDepth, (i + 1) * stepHeight, Width), block);
            }

            float platformStart = 6f + steps * stepDepth;
            Box(root, "Slide Platform", new Vector3(platformStart, 0f, -33f), new Vector3(3f, top, Width), block);
            float slopeRun = top / Mathf.Tan(10f * Mathf.Deg2Rad);
            Ramp(root, "Slide Slope", new Vector3(platformStart + 3f + slopeRun, 0f, -30f), Vector3.left, slopeRun, 10f, ramp);
            Sign(root, new Vector3(platformStart, top, -30f), "Stairs + 10° slide slope", camera, font);

            // Low beam: 1.2 m clearance, too low to walk under, enough to slide under.
            const float clearance = 1.2f;
            Box(root, "Beam Post", new Vector3(18f, 0f, -18.2f), new Vector3(0.4f, clearance + 0.4f, 0.4f), beam);
            Box(root, "Beam Post", new Vector3(23.6f, 0f, -18.2f), new Vector3(0.4f, clearance + 0.4f, 0.4f), beam);
            Box(root, "Beam", new Vector3(18f, clearance, -18.2f), new Vector3(6f, 0.4f, 0.4f), beam);
            Sign(root, new Vector3(21f, clearance + 0.4f, -18f), "Slide under (Ctrl)", camera, font);

            HalfPipe(root, new Vector3(0f, 0f, 31f), ramp, block, camera, font);

            // Wall for later wall runs.
            Box(root, "Wall", new Vector3(40f, 0f, -10f), new Vector3(1f, 4f, 25f), block);
        }

        private static void Hill(Transform root, Vector3 start, float angle, float height, Material ramp, Material block, Camera camera, Font font)
        {
            float run = height / Mathf.Tan(angle * Mathf.Deg2Rad);
            const float plateau = 4f;
            Vector3 side = new Vector3(0f, 0f, Width * 0.5f);
            Ramp(root, $"Hill {angle}° Up", start, Vector3.right, run, angle, ramp);
            Box(root, $"Hill {angle}° Top", start + Vector3.right * run - side, new Vector3(plateau, height, Width), block);
            Ramp(root, $"Hill {angle}° Down", start + Vector3.right * (2f * run + plateau), Vector3.left, run, angle, ramp);
            Sign(root, start + Vector3.right * (run + plateau * 0.5f) + Vector3.up * height, $"{angle}° up / down", camera, font);
        }

        private static void RampOntoPlateau(
            Transform root, Vector3 start, float angle, float height, string label, Material ramp, Material block, Camera camera, Font font)
        {
            float run = height / Mathf.Tan(angle * Mathf.Deg2Rad);
            const float plateau = 3f;
            Ramp(root, $"Ramp {angle}°", start, Vector3.right, run, angle, ramp);
            Box(root, $"Ramp {angle}° Top", start + Vector3.right * run - new Vector3(0f, 0f, Width * 0.5f), new Vector3(plateau, height, Width), block);
            Sign(root, start + Vector3.right * (run + plateau * 0.5f) + Vector3.up * height, label, camera, font);
        }

        /// <summary>
        /// Valley across X: flat bottom, sides curving up through steeper and steeper segments to platforms,
        /// for sliding down one side and carrying the speed up the other.
        /// </summary>
        private static void HalfPipe(Transform root, Vector3 center, Material ramp, Material block, Camera camera, Font font)
        {
            const float bottomHalfWidth = 3f;
            const float segmentRun = 2.5f;
            const float length = 14f;
            const float platformWidth = 3f;

            float height = 0f;
            foreach (float side in new[] { 1f, -1f })
            {
                Vector3 outwards = Vector3.right * side;
                Vector3 edge = center + outwards * bottomHalfWidth;
                foreach (float angle in HalfPipeAngles)
                {
                    Ramp(root, $"Half-pipe {angle}°", edge, outwards, segmentRun, angle, ramp, length);
                    edge += outwards * segmentRun + Vector3.up * (segmentRun * Mathf.Tan(angle * Mathf.Deg2Rad));
                }

                height = edge.y;
                Vector3 platformMin = new Vector3(Mathf.Min(edge.x, edge.x + side * platformWidth), 0f, center.z - length * 0.5f);
                Box(root, "Half-pipe Platform", platformMin, new Vector3(platformWidth, height, length), block);
            }

            Sign(root, center + Vector3.up * 0.5f, $"Half-pipe: {HalfPipeAngles[0]}°-{HalfPipeAngles[HalfPipeAngles.Length - 1]}° sides, {height:0.#} m deep", camera, font);
        }

        /// <summary>Slab whose top surface starts at the low edge and rises along a direction.</summary>
        private static void Ramp(Transform root, string name, Vector3 lowEdgeCenter, Vector3 direction, float run, float angle, Material material, float width = Width)
        {
            float radians = angle * Mathf.Deg2Rad;
            float rise = run * Mathf.Tan(radians);
            Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-angle, 0f, 0f);
            Vector3 topCenter = lowEdgeCenter + direction * (run * 0.5f) + Vector3.up * (rise * 0.5f);
            Vector3 center = topCenter - rotation * Vector3.up * (Thickness * 0.5f);

            GameObject slab = Primitive(name, PrimitiveType.Cube, root, center, new Vector3(width, Thickness, run / Mathf.Cos(radians)), material);
            slab.transform.localRotation = rotation;
            slab.isStatic = true;
        }

        /// <summary>Axis-aligned box from its minimum corner and size.</summary>
        private static void Box(Transform root, string name, Vector3 min, Vector3 size, Material material)
        {
            GameObject box = Primitive(name, PrimitiveType.Cube, root, min + size * 0.5f, size, material);
            box.isStatic = true;
        }

        private static void Sign(Transform root, Vector3 position, string text, Camera camera, Font font)
        {
            var anchor = new GameObject($"Sign: {text}").transform;
            anchor.SetParent(root, false);
            anchor.localPosition = position;
            Label(anchor, text, 1.5f, camera, font);
        }
    }
}
