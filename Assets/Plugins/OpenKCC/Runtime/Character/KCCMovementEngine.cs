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
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Moves a kinematic capsule through the scene: pushes out of overlaps, slides along surfaces,
    /// steps up and snaps down, and tracks the grounded state. Allocation-free per tick; the caller
    /// owns the tick and passes its delta time explicitly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IColliderCast))]
    public class KCCMovementEngine : MonoBehaviour, IKCCConfig
    {
        /// <summary>
        /// Default value for distance from ground player is considered grounded.
        /// </summary>
        public const float DefaultGroundedDistance = 0.05f;

        /// <summary>
        /// Default value for distance to check for ground below player.
        /// </summary>
        public const float DefaultGroundCheckDistance = 0.25f;

        /// <summary>
        /// Default maximum number of bounces for computing player movement.
        /// </summary>
        public const int DefaultMaxBounces = 5;

        /// <summary>
        /// Default expected depth of a step for walking up staircases.
        /// </summary>
        public const float DefaultStepUpDepth = 0.1f;

        /// <summary>
        /// Default angle power for bouncing and sliding off surfaces.
        /// </summary>
        public const float DefaultAnglePower = 2.0f;

        /// <summary>
        /// Default max push speed when moving the player.
        /// </summary>
        public const float DefaultMaxPushSpeed = 100.0f;

        /// <summary>
        /// Max launch speed of player form moving ground.
        /// </summary>
        public const float DefaultMaxLaunchVelocity = 2.0f;

        /// <summary>
        /// Bounces one move can produce: up to MaxBounces + 1 bounces plus the closing Stop.
        /// Two moves per tick fit without the bounce buffer growing.
        /// </summary>
        private const int BounceBufferCapacity = (DefaultMaxBounces + 2) * 2;

        /// <summary>
        /// Height of a step that the player can climb up.
        /// </summary>
        [Tooltip("Height of a step that the player can climb up.")]
        public float stepHeight = 0.35f;

        /// <summary>
        /// Max angle the player can walk up before slipping.
        /// </summary>
        [Tooltip("Max angle the player can walk up before slipping.")]
        public float maxWalkAngle = 60.0f;

        /// <summary>
        /// Skin width for player collisions.
        /// </summary>
        [Tooltip("Skin width for player collisions.")]
        public float skinWidth = 0.01f;

        /// <summary>
        /// Layermask for computing player collisions.
        /// </summary>
        [Tooltip("Layermask for computing player collisions.")]
        public LayerMask layerMask = RaycastHelperConstants.DefaultLayerMask;

        /// <summary>
        /// Approximated world velocity of the player.
        /// </summary>
        [NonSerialized]
        public SmoothedVector worldVelocity = new SmoothedVector(10);

        private readonly List<KCCBounce> _bounces = new List<KCCBounce>(BounceBufferCapacity);
        private readonly List<IOnPlayerTeleport> _teleportListeners = new List<IOnPlayerTeleport>();
        private IColliderCast _colliderCast;
        private Vector3 _previousPosition;

        // Components of the floor, looked up only when the floor changes.
        private GameObject _floor;
        private IMovingGround _floorMovingGround;
        private Rigidbody _floorRigidbody;

        /// <inheritdoc/>
        public bool CanSnapUp => GroundedState.OnGround;

        /// <summary>
        /// Snap down distance for player snapping down.
        /// </summary>
        public virtual float SnapDown => stepHeight * 2f;

        /// <summary>
        /// Max default launch velocity for the player from unlabeled
        /// surfaces.
        /// </summary>
        public virtual float MaxDefaultLaunchVelocity => DefaultMaxLaunchVelocity;

        /// <summary>
        /// Maximum speed at which the player can snap down surfaces.
        /// </summary>
        public virtual float MaxSnapDownSpeed => 5.0f;

        /// <summary>
        /// Upwards direction for the KCC Movement engine.
        /// </summary>
        public virtual Vector3 Up => Vector3.up;

        /// <inheritdoc/>
        public virtual int MaxBounces => DefaultMaxBounces;

        /// <inheritdoc/>
        public virtual float VerticalSnapUp => stepHeight;

        /// <inheritdoc/>
        public virtual float StepUpDepth => DefaultStepUpDepth;

        /// <inheritdoc/>
        public virtual float AnglePower => DefaultAnglePower;

        /// <inheritdoc/>
        public virtual float SkinWidth => skinWidth;

        /// <inheritdoc/>
        public virtual LayerMask LayerMask => layerMask;

        /// <summary>
        /// Max push speed of the player in units per second when pushing
        /// out of overlapping objects.
        /// </summary>
        public float MaxPushSpeed => DefaultMaxPushSpeed;

        /// <summary>
        /// Collider cast for player movement.
        /// </summary>
        public IColliderCast ColliderCast
        {
            get
            {
                if (_colliderCast == null)
                {
                    _colliderCast = GetComponent<IColliderCast>();
                    if (_colliderCast == null)
                    {
                        throw new InvalidOperationException($"{name}: missing IColliderCast component");
                    }
                }

                return _colliderCast;
            }
        }

        /// <summary>
        /// Distance to ground at which player is considered grounded.
        /// </summary>
        public virtual float GroundedDistance => DefaultGroundedDistance;

        /// <summary>
        /// Distance to check player distance to ground.
        /// </summary>
        public virtual float GroundCheckDistance => DefaultGroundCheckDistance;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees).
        /// </summary>
        public virtual float MaxWalkAngle => maxWalkAngle;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        public RelativeParentConfig RelativeParentConfig { get; protected set; } = new RelativeParentConfig();

        /// <summary>
        /// Current grounded state of the character.
        /// </summary>
        public KCCGroundedState GroundedState { get; protected set; }

        /// <summary>
        /// Bounces of the last <see cref="MovePlayer(float, Vector3)"/> call. Reused every tick:
        /// read it before the next move and index it instead of using foreach.
        /// </summary>
        public IReadOnlyList<KCCBounce> LastBounces => _bounces;

        /// <summary>
        /// Setup and configure the movement engine.
        /// </summary>
        public void Awake()
        {
            ResetVelocityTracking();
        }

        /// <summary>
        /// Is the a movement vector is moving in the direction
        /// upwards relative to player direction.
        /// </summary>
        public bool MovingUp(Vector3 move)
        {
            return Vector3.Dot(move, Up) > 0;
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standing on</returns>
        public virtual Vector3 GetGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;
            if (_floorMovingGround != null)
            {
                if (_floorMovingGround.AvoidTransferMomentum())
                {
                    return Vector3.zero;
                }

                // Weight movement of ground by ground movement weight
                groundVelocity = _floorMovingGround.GetVelocityAtPoint(GroundedState.GroundHitPosition);
                float velocityWeight =
                    _floorMovingGround.GetMovementWeight(GroundedState.GroundHitPosition, groundVelocity);
                float transferWeight =
                    _floorMovingGround.GetTransferMomentumWeight(GroundedState.GroundHitPosition, groundVelocity);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }
            else if (_floorRigidbody != null && !_floorRigidbody.isKinematic)
            {
                Vector3 groundVel = _floorRigidbody.GetPointVelocity(GroundedState.GroundHitPosition);
                float velocity = Mathf.Min(groundVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = groundVel.normalized * velocity;
            }
            else if (GroundedState.StandingOnGround)
            {
                Vector3 avgVel = worldVelocity.Average();
                float velocity = Mathf.Min(avgVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = avgVel.normalized * velocity;
            }

            return groundVelocity;
        }

        /// <summary>
        /// Have the player to visually move with the ground.
        /// </summary>
        public void Update()
        {
            RelativeParentConfig.FollowGround(transform);
        }

        /// <summary>
        /// Applies one movement for this tick: follows moving ground, pushes out of overlaps,
        /// moves with bounces, snaps down and updates the grounded state.
        /// </summary>
        /// <param name="deltaTime">Duration of the tick in seconds.</param>
        /// <param name="move">Desired displacement in world space for this tick.</param>
        public void MovePlayer(float deltaTime, Vector3 move)
        {
            MovePlayerInternal(deltaTime, move, Vector3.zero, 1);
        }

        /// <summary>
        /// Same as <see cref="MovePlayer(float, Vector3)"/> but with two moves resolved one after
        /// the other, e.g. a ground-projected walk and a separate vertical velocity.
        /// </summary>
        public void MovePlayer(float deltaTime, Vector3 firstMove, Vector3 secondMove)
        {
            MovePlayerInternal(deltaTime, firstMove, secondMove, 2);
        }

        /// <summary>
        /// Displacement one move of the last <see cref="MovePlayer(float, Vector3)"/> call actually achieved,
        /// after bounces and step-ups; excludes push-out, snap-down and moving ground.
        /// </summary>
        /// <param name="moveIndex">0 for the first move, 1 for the second.</param>
        public Vector3 GetMoveDisplacement(int moveIndex)
        {
            int start = 0;
            for (int i = 0; i < _bounces.Count; i++)
            {
                if (_bounces[i].action != KCCUtils.MovementAction.Stop)
                {
                    continue;
                }

                if (moveIndex == 0)
                {
                    return _bounces[i].finalPosition - _bounces[start].initialPosition;
                }

                moveIndex--;
                start = i + 1;
            }

            throw new ArgumentOutOfRangeException(nameof(moveIndex), $"{name}: last tick had no move with this index");
        }

        /// <summary>
        /// Teleport player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public virtual void TeleportPlayer(Vector3 position)
        {
            RelativeParentConfig.Reset();
            transform.position = position;
            ResetVelocityTracking();

            GetComponents(_teleportListeners);
            for (int i = 0; i < _teleportListeners.Count; i++)
            {
                _teleportListeners[i].OnPlayerTeleport(position, transform.rotation);
            }
        }

        /// <summary>
        /// Get the movement of the player projected onto the plane
        /// they are standing on if they are not falling.
        /// </summary>
        /// <param name="movement">How the player is attempting to move.</param>
        /// <returns>Projected movement onto the plane the player is standing on.</returns>
        public virtual Vector3 GetProjectedMovement(Vector3 movement)
        {
            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (GroundedState.StandingOnGround && !GroundedState.Sliding)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, GroundedState.SurfaceNormal).normalized * movement.magnitude;
                if (projectedMovement.magnitude + KCCUtils.Epsilon >= movement.magnitude)
                {
                    movement = projectedMovement;
                }
            }

            return movement;
        }

        /// <summary>
        /// Update the current grounded state of this kinematic character controller.
        /// </summary>
        public virtual KCCGroundedState CheckGrounded(bool snappedUp, bool snappedDown)
        {
            Vector3 groundCheckPos = transform.position;

            // If snapped up, use the snapped position to check grounded
            if (snappedUp || snappedDown)
            {
                Vector3 snapDelta = KCCUtils.GetSnapDelta(
                    transform.position,
                    transform.rotation,
                    -Up,
                    SnapDown,
                    ColliderCast,
                    LayerMask,
                    SkinWidth);
                groundCheckPos += snapDelta;
            }

            bool didHit = ColliderCast.CastSelf(
                groundCheckPos,
                transform.rotation,
                -Up,
                GroundCheckDistance,
                out RaycastHit hit,
                LayerMask,
                skinWidth: SkinWidth);

            Vector3 normal = hit.normal;
            if (snappedUp)
            {
                normal = GroundedState.SurfaceNormal;
            }
            else if (snappedDown)
            {
                // Check if we're walking down stairs
                bool overrideNormal = ColliderCast.DoRaycastInDirection(
                    transform.position + SkinWidth * Up,
                    -Up,
                    GroundCheckDistance + SkinWidth,
                    out RaycastHit stepHit,
                    LayerMask);
                if (overrideNormal)
                {
                    normal = stepHit.normal;
                }
            }

            GameObject floor = didHit ? hit.collider.gameObject : null;
            UpdateFloorComponents(floor);

            GroundedState = new KCCGroundedState(
                distanceToGround: hit.distance,
                onGround: didHit,
                angle: Vector3.Angle(normal, Up),
                surfaceNormal: normal,
                groundHitPosition: hit.point,
                floor: floor,
                groundedDistance: GroundedDistance,
                maxWalkAngle: MaxWalkAngle);

            return GroundedState;
        }

        /// <summary>
        /// Should the player snap down after a movement.
        /// </summary>
        /// <param name="snappedUp">Did the player snap up during their movement.</param>
        /// <param name="anyMoveUp">Was any of the moves of this tick directed upwards.</param>
        /// <returns>True if the player should snap down, false otherwise.</returns>
        protected virtual bool ShouldSnapDown(bool snappedUp, bool anyMoveUp)
        {
            return !snappedUp &&
                GroundedState.StandingOnGround &&
                !GroundedState.Sliding &&
                !anyMoveUp;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        protected virtual void SnapPlayerDown(float deltaTime)
        {
            Vector3 delta = KCCUtils.GetSnapDelta(
                transform.position,
                transform.rotation,
                -Up,
                SnapDown,
                ColliderCast,
                LayerMask,
                SkinWidth);
            transform.position += Vector3.ClampMagnitude(delta, MaxSnapDownSpeed * deltaTime);
        }

        private void MovePlayerInternal(float deltaTime, Vector3 firstMove, Vector3 secondMove, int moveCount)
        {
            if (deltaTime <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, $"{name}: deltaTime must be positive");
            }

            RelativeParentConfig.FollowGround(transform);
            worldVelocity.AddSample((transform.position - _previousPosition) / deltaTime);

            Vector3 start = transform.position;

            // Push player out of overlapping objects
            transform.position += ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                MaxPushSpeed * deltaTime,
                LayerMask,
                QueryTriggerInteraction.Ignore,
                SkinWidth / 2);

            _bounces.Clear();
            ApplyMove(firstMove);
            if (moveCount > 1)
            {
                ApplyMove(secondMove);
            }

            bool snappedUp = false;
            for (int i = 0; i < _bounces.Count; i++)
            {
                if (_bounces[i].action == KCCUtils.MovementAction.SnapUp)
                {
                    snappedUp = true;
                    break;
                }
            }

            bool anyMoveUp = MovingUp(firstMove) || (moveCount > 1 && MovingUp(secondMove));

            // Only snap down if the player was grounded before they started
            // moving and are not currently trying to move upwards.
            bool snappedDown = ShouldSnapDown(snappedUp, anyMoveUp);
            if (snappedDown)
            {
                SnapPlayerDown(deltaTime);
            }

            CheckGrounded(snappedUp, snappedDown);

            Vector3 delta = transform.position - start;
            transform.position += RelativeParentConfig.UpdateMovingGround(
                start, transform.rotation, GroundedState, _floorMovingGround, delta, deltaTime);
            _previousPosition = transform.position;
        }

        private void ApplyMove(Vector3 move)
        {
            KCCUtils.GetBounces(transform.position, move, transform.rotation, this, _bounces);

            // GetBounces always ends with a Stop bounce holding the final position.
            transform.position = _bounces[_bounces.Count - 1].finalPosition;
        }

        private void UpdateFloorComponents(GameObject floor)
        {
            // Reference comparison: a destroyed floor equals null under Unity's ==, which would keep
            // its stale components cached.
            if (ReferenceEquals(floor, _floor))
            {
                return;
            }

            _floor = floor;
            _floorMovingGround = null;
            _floorRigidbody = null;
            if (floor != null)
            {
                floor.TryGetComponent(out _floorMovingGround);
                floor.TryGetComponent(out _floorRigidbody);
            }
        }

        private void ResetVelocityTracking()
        {
            _previousPosition = transform.position;
            worldVelocity.Clear();
        }
    }
}
