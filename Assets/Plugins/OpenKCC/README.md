# OpenKCC (vendored core)

Source: https://github.com/nicholas-maltbie/OpenKCC, commit `a1a30ed` (2023-09-14), package `com.nickmaltbie.openkcc` 1.5.0.
`TestUtils/` comes from https://github.com/nicholas-maltbie/TestUtilsUnity (`IUnityService`, `UnityService` only).
Both are MIT, see `LICENSE.txt` and `TestUtils/LICENSE.txt`.

Only the collision core is copied (movement engine, collider casts, grounded state, moving ground, utils).
Camera, UI, input, animation/foot IK, samples and the author's tests (they need Moq) are left out.

## Local changes

- `KCCMovementEngine`: default `RelativeParentConfig` instead of `RelativeParentConfigWithFeet` (foot IK not vendored).
- `KCCMovementEngine.worldVelocity`: `[NonSerialized]` (Unity 6 serialization analyzer, UAC1001).
- `ColliderConfiguration`: no longer derives from `PropertyAttribute` (its drawer is not vendored; UAC1002).

## Known issues to fix before building movement on it

- `MovePlayer` allocates 2.5–5 KB per tick (LINQ, `yield` iterators, `params` array).
- Static `UnityService.Instance` for time; `deltaTime` and `fixedDeltaTime` are mixed.
- `?.` on Unity objects and `GetComponent` in the hot path.
