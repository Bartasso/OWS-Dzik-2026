# OpenKCC (vendored core)

Source: https://github.com/nicholas-maltbie/OpenKCC, commit `a1a30ed` (2023-09-14), package `com.nickmaltbie.openkcc` 1.5.0.
MIT, see `LICENSE.txt`.

Only the collision core is kept: movement engine, capsule cast, grounded state, moving ground and utils.
Camera, UI, input, animation/foot IK, samples, the author's tests (they need Moq), other collider shapes,
debug helpers and TestUtilsUnity are left out.

## Local changes

- `KCCMovementEngine.MovePlayer(deltaTime, move[, secondMove])`: allocation-free, explicit delta time
  (no static time service), bounces written to a reused buffer exposed as `LastBounces`.
- Floor components (`IMovingGround`, `Rigidbody`) looked up once when the floor changes, not every tick;
  no `?.` / `??` on Unity objects.
- `IColliderCast` returns plain `RaycastHit` through `out`; no LINQ, no wrapper classes.
- `CapsuleColliderCast` always reads dimensions from its `CapsuleCollider` (upstream used stale cached
  values in builds).
- `KCCUtils.GetBounces` fills a list instead of a `yield` iterator; `KCCBounce` is a struct; step-up uses
  the configured up direction instead of `Vector3.up`.
- `IKCCGrounded` removed: passing `KCCGroundedState` through it boxed every tick.
- `TeleportPlayer` resets velocity tracking, so a teleport no longer counts as a huge velocity sample.
- Default `RelativeParentConfig` instead of `RelativeParentConfigWithFeet` (foot IK not vendored).

Regression scenarios: `Assets/_Project/Tests/EditMode/KccMovementEngineTests.cs`.
