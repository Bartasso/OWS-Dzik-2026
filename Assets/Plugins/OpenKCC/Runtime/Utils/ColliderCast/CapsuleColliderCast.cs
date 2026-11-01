// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// Allocation-free physics queries for a character shaped by its <see cref="CapsuleCollider"/>.
    /// The collider is the single source of the dimensions, in the editor and in builds alike.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleColliderCast : MonoBehaviour, IColliderCast
    {
        /// <summary>
        /// Maximum number of hits or overlaps considered per query.
        /// </summary>
        public const int QueryCacheSize = 25;

        private readonly Collider[] _overlapCache = new Collider[QueryCacheSize];
        private readonly RaycastHit[] _hitCache = new RaycastHit[QueryCacheSize];
        private CapsuleCollider _capsule;

        /// <summary>
        /// Capsule that defines the shape of the character.
        /// </summary>
        public CapsuleCollider Capsule
        {
            get
            {
                if (_capsule == null)
                {
                    _capsule = GetComponent<CapsuleCollider>();
                    if (_capsule == null)
                    {
                        throw new InvalidOperationException($"{name}: missing CapsuleCollider");
                    }
                }

                return _capsule;
            }
        }

        /// <inheritdoc/>
        public bool CastSelf(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            out RaycastHit hit,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.01f)
        {
            GetCapsule(position, rotation, -skinWidth, out Vector3 top, out Vector3 bottom, out float radius);
            int count = Physics.CapsuleCastNonAlloc(top, bottom, radius, direction, _hitCache, distance + skinWidth, layerMask, queryTriggerInteraction);

            hit = default;
            hit.distance = Mathf.Infinity;
            bool hitSomething = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit candidate = _hitCache[i];
                if (candidate.collider.transform == transform)
                {
                    continue;
                }

                float candidateDistance = Mathf.Max(candidate.distance - skinWidth, 0);
                if (candidateDistance < hit.distance)
                {
                    hit = candidate;
                    hit.distance = candidateDistance;
                }

                hitSomething = true;
            }

            return hitSomething;
        }

        /// <inheritdoc/>
        public Vector3 PushOutOverlapping(
            Vector3 position,
            Quaternion rotation,
            float maxDistance,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.0f)
        {
            GetCapsule(position, rotation, -skinWidth, out Vector3 top, out Vector3 bottom, out float radius);
            int count = Physics.OverlapCapsuleNonAlloc(top, bottom, radius, _overlapCache, layerMask, queryTriggerInteraction);

            Vector3 pushed = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                Collider overlap = _overlapCache[i];
                if (overlap.transform == transform)
                {
                    continue;
                }

                Transform other = overlap.transform;
                Physics.ComputePenetration(
                    Capsule, position, rotation,
                    overlap, other.position, other.rotation,
                    out Vector3 direction, out float distance);

                Vector3 push = direction.normalized * distance;
                pushed += push;
                position += push;
            }

            return Vector3.ClampMagnitude(pushed, maxDistance);
        }

        /// <inheritdoc/>
        public bool DoRaycastInDirection(
            Vector3 source,
            Vector3 direction,
            float distance,
            out RaycastHit stepHit,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction)
        {
            return Physics.Raycast(source, direction, out stepHit, distance, layerMask, queryTriggerInteraction);
        }

        /// <inheritdoc/>
        public Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            GetCapsule(position, rotation, 0f, out _, out Vector3 bottom, out float radius);
            return bottom + radius * (rotation * -GetCapsuleRelativeUp(Capsule.direction));
        }

        /// <summary>
        /// Centres of the capsule's end spheres and its radius, grown by a radius modifier.
        /// </summary>
        public void GetCapsule(Vector3 position, Quaternion rotation, float radiusMod, out Vector3 top, out Vector3 bottom, out float radius)
        {
            CapsuleCollider capsule = Capsule;
            Vector3 center = rotation * capsule.center + position;
            radius = capsule.radius + radiusMod;
            float height = capsule.height + radiusMod * 2;
            Vector3 up = rotation * GetCapsuleRelativeUp(capsule.direction);
            float halfSegment = Mathf.Max(height / 2 - radius, 0f);
            bottom = center - up * halfSegment;
            top = center + up * halfSegment;
        }

        /// <summary>
        /// Local axis of a <see cref="CapsuleCollider.direction"/> value (0 = X, 1 = Y, 2 = Z).
        /// </summary>
        public static Vector3 GetCapsuleRelativeUp(int direction)
        {
            switch (direction)
            {
                case 0:
                    return Vector3.right;
                case 2:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }
    }
}
