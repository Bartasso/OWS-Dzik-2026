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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Default values for physics queries.
    /// </summary>
    public static class RaycastHelperConstants
    {
        public const int DefaultLayerMask = ~0;

        public const QueryTriggerInteraction DefaultQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
    }

    /// <summary>
    /// Physics queries against the shape of a character. Implementations must not allocate:
    /// hits are returned as plain <see cref="RaycastHit"/> values.
    /// </summary>
    public interface IColliderCast
    {
        /// <summary>
        /// Lowest point of the shape at a given position and rotation.
        /// </summary>
        Vector3 GetBottom(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Casts the shape along a direction and returns the closest hit that is not the shape itself.
        /// Without a hit, <paramref name="hit"/> is default with an infinite distance.
        /// </summary>
        bool CastSelf(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            out RaycastHit hit,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.01f);

        /// <summary>
        /// Offset that pushes the shape out of everything it overlaps, clamped to a maximum distance.
        /// </summary>
        Vector3 PushOutOverlapping(
            Vector3 position,
            Quaternion rotation,
            float maxDistance,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.0f);

        /// <summary>
        /// Plain raycast from a point.
        /// </summary>
        bool DoRaycastInDirection(
            Vector3 source,
            Vector3 direction,
            float distance,
            out RaycastHit stepHit,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction);
    }
}
