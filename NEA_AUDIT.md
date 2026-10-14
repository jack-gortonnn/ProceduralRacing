# NEA Complexity Audit — Fix Checklist

Full scan complete across all 26 .cs files. Confirmed with the user:
`TrackGenerator`'s scoring/backtracking/floodfill reachability check is the
marked algorithm (procedural track generation is the NEA's complex-algorithm
centerpiece) — never simplify its logic, only supporting data shapes if
genuinely redundant.

## Fixes to make

- [x] 1. Delete `Track/Generation/Candidate.cs` (dead comment-only file,
      superseded by inline tuple in `TrackGenerator`). No references anywhere.
      Trivial, zero risk.
      DONE — actual path was `ProceduralRacing/Track/Generation/Candidate.cs`
      (nested project folder, not repo root as originally written). No delete
      tool available via MCP; moved to `_deleted_Candidate.cs` outside the
      project folder (excluded from build glob). User should hard-delete
      that file from disk.

- [x] 2. `TrackConfig.RatingBonuses` (static nested
      `Dictionary<TrackDifficulty, Dictionary<int,float>>`) → replace with a
      single `Dictionary<int,float> RatingBonus` set directly per-difficulty
      in `FromDifficulty`, same pattern as the other per-difficulty fields
      (`MaxPieceDifficulty`, `BaseScore`, etc). `GetRatingBonus(int)`
      simplifies to one dictionary lookup — no difficulty keying needed since
      each `TrackConfig` instance is already difficulty-specific. Caller
      (`TrackGenerator.ScorePiece`) unchanged, signature unchanged.
      DONE — actual file is `Track/Data/TrackDifficulty.cs` (TrackConfig is
      defined there, not a separate TrackConfig.cs). Verified against full
      re-read of the file.

- [x] 3. `CarLoadout.Default` — unused static property, delete. Confirmed
      zero references (`GarageScreen.Confirm()` always builds loadouts
      explicitly from panel indices).
      DONE — verified against full re-read of CarLoadout.cs, GarageScreen.cs,
      and Game1.cs.

- [x] 4. `GarageScreen` — collapsed `CycleChassis`/`CycleEngine`/`CycleTyres`
      (three near-identical methods differing only by which index/count they
      touch) into one generic helper:
      `private static void CycleIndex(ref int index, int count, int dir)`.
      Rewired the 6 button lambdas in the constructor to call it directly.
      DONE — verified against full re-read of GarageScreen.cs.

- [x] 5. `LapTimer.Update` — the checkpoint-crossing block has two
      near-identical branches both gated on `_nextCheckpoint == 0`, differing
      only by whether `_started` is true yet (first crossing vs normal lap
      completion) — a "same event, different point in the process" smell.
      Flatten to one flow: always advance the checkpoint on a valid crossing;
      only gate the lap-completion bookkeeping (LastLapTime/LapsCompleted/
      BestLapTime) on `_started`, and set `_started = true` inside that same
      `_nextCheckpoint == 0` block instead of via a separate first-branch.
      Proposed replacement body already drafted (see chat history) — self
      contained to one method, no external callers affected.
      DONE — verified against full re-read of LapTimer.cs. Note:
      `_lastCrossTime = now` now runs once inside the `_nextCheckpoint == 0`
      block instead of being duplicated across both original branches;
      behavior is unchanged since both original branches set it to the same
      value.

- [x] 6. `RaceSetupScreen.Update` seed-input loop — nested-if audit follow-up.
      Two changes to the same block:
      (a) the outer `if (_prevKey.IsKeyUp(key)) { ... }` wrapping both digit
      and backspace checks was converted to a negated guard clause
      (`if (_prevKey.IsKeyDown(key)) continue;`), removing a nesting level
      since the two inner checks are independent siblings, not dependent
      steps.
      (b) `_seedInput += key.ToString().Replace("D", "")` replaced with
      `_seedInput += (char)('0' + (key - Keys.D0))` — the original round-
      tripped through a string (`Keys.D5` → `"D5"` → strip the `D` →
      `"5"`) to extract a digit that's already recoverable by integer
      subtraction, since `Keys.D0`..`Keys.D9` are guaranteed sequential in
      the enum. Comment added explaining the enum-arithmetic assumption.
      Self-contained to one method, no external callers affected.
      DONE — verified against full re-read of RaceSetupScreen.cs.

- [x] 7. Removed `RatingBonus` scoring term entirely (design decision, not a
      complexity fix) — piece-difficulty scoring was judged small enough
      relative to `CloseLoopBonus`/`MaxLateBonus` to be pure tuning rather
      than load-bearing, and is being pulled for now. Removed:
      `TrackConfig.RatingBonus` field, its four per-difficulty initializers
      in `FromDifficulty`, `TrackConfig.GetRatingBonus(int)`, and the
      `score += config.GetRatingBonus(piece.Difficulty)` call (old step 9)
      in `TrackGenerator.ScorePiece` (renumbered the randomness step from
      10 to 9). `TrackPiece.Difficulty` itself is untouched and still used
      by `MaxPieceDifficulty` piece filtering — only the scoring bonus based
      on it was removed.
      DONE — verified against full re-read of TrackDifficulty.cs and
      TrackGenerator.cs. No other references found anywhere else in the
      codebase.

- [x] 8. `TrackGenerator.ScorePiece` — ifs-vs-additions inconsistency fix.
      The method mixed two styles for the same job (compute a bonus/penalty,
      add it to score): `CloseLoopBonus`/`SamePiecePenalty`/`SameTypePenalty`
      were inline `if` statements, while `EarlyBonus`/`LateBonus`/
      `AlignmentBonus` were extracted `Get...` helpers returning a flat
      value. Added `GetCloseLoopBonus`, `GetSamePiecePenalty`,
      `GetSameTypePenalty` next to the existing scoring helpers, each
      returning the bonus/negated-penalty or `0f`, matching the existing
      helpers' signature style. `ScorePiece` now calls all six as flat
      `score += Get...()` lines with zero inline ifs in the scoring body
      (the only remaining `if` is the unrelated step-1 guard clause). Same
      math, same weights, no behavior change — confirmed by construction
      (`GetCloseLoopBonus` returns `bonus`/`0f` matching the old
      `if (...) score += bonus`; the two penalty helpers return
      `-penalty`/`0f` matching the old `score -= penalty`).
      DONE — verified against full re-read of TrackGenerator.cs.

- [x] 9. Removed the redundant `out Connection exit` parameter threaded
      alongside `out PlacedPiece placed` through `TryFindPlacement`,
      `ScorePiece`, and the scoring helpers. `PlacedPiece.ExitConnection`
      already stores the same `Connection` object passed into its
      constructor as `exitConnection` (see `PlacedPiece.cs`), so `exit` was
      always recoverable from `placed` alone — the separate out-parameter
      was pure duplication. Changed:
      - `TryFindPlacement(piece, rotation, flipped, out placed, out exit)`
        → `TryFindPlacement(piece, rotation, flipped, out placed)`. Internal
        local `exit` variable kept inside the method body (still needed
        there — `placed` isn't assigned until validation passes, so the
        method can't yet read `placed.ExitConnection` while validating).
      - `ScorePiece(..., out placed, out exit)` → `ScorePiece(..., out placed)`.
      - `GetManhattanToStart(placed, exit)` → `GetManhattanToStart(placed)`,
        reads `placed.ExitConnection` internally. Same for
        `GetCloseLoopBonus` and `GetAlignmentBonus`.
      - `optionsStack`'s tuple shape `(PlacedPiece Piece, Connection Exit,
        float Score)` → `(PlacedPiece Piece, float Score)`; `Exit` field
        was unused downstream (`AddPiece` already read
        `piece.ExitConnection` directly, never took `Exit` as a parameter).
        `ScoreAllCandidates`'s return type and body updated to match.
      No behavior change — `placement.ExitConnection` and the removed local
      `exit` were always the same object reference at every call site that
      used both.
      DONE — verified against full re-read of TrackGenerator.cs. Confirmed
      via PlacedPiece.cs that ExitConnection stores the constructor's
      exitConnection argument unchanged. No other file references
      TryFindPlacement, ScorePiece, or optionsStack's tuple shape (Track.cs
      only calls BeginTrack()/Update()).

- [x] 10. `TrackGenerator.OccupiesStartWithoutClosing` — same redundant-
      parameter pattern as fix #9, found on a second pass. Took
      `(PlacedPiece candidate, Connection exit)` but its one call site
      (`TryFindPlacement` step 6) always passed the local `exit` that, by
      that point in the method, is already equal to `placement.ExitConnection`
      (both reference the same `Connection` object passed into the
      `PlacedPiece` constructor). Changed to `OccupiesStartWithoutClosing
      (PlacedPiece candidate)`, reading `candidate.ExitConnection` internally.
      No behavior change.
      DONE — verified against full re-read of TrackGenerator.cs.

> Note: fix #10 was found via a second read of TrackGenerator.cs looking
> specifically for the "parameter duplicates what's already on the object"
> pattern from fix #9. The same sweep also flagged two candidates outside
> TrackGenerator, in the Car/ area, which supersede the "no redundant state
> or dead code found" note below for Car.cs/CarCollision.cs/CarAudio.cs:
> - `CarPhysics.Update`'s `forward`/`back`/`left`/`right` parameters never
>   change call-to-call for a given car — `Car` already stores them as
>   fields and passes the same four values every frame. Candidate fix:
>   move them to `CarPhysics`'s constructor instead of per-call parameters.
> - `CarCollision.Update`'s `position`/`rotation`/`collisionScale`
>   parameters are all already available on `Car` (`Position`, `Rotation`,
>   `Chassis.collisionScale`), and `Car.Update` is the only caller. Passing
>   `Car car` instead would let `GetCorners` read them directly — but
>   `Update` also takes `ref Physics.Velocity` for mutation, which doesn't
>   fold into a plain `Car` reference without `Car` exposing a mutable
>   velocity path. Not yet applied — needs a decision on how the velocity
>   mutation should work before changing.

- [x] 11. `CarCollision` — unified car-vs-track with car-vs-car under the
      same pattern (`ResolveCarVsCar`'s shape), resolving the fix #10 note
      above. `CarCollision` is now `static class CarCollision` (was an
      instance created once per `Car` purely to cache `carWidth`/`carHeight`
      — no other per-instance state existed). Changes:
      - `Update(dt, position, rotation, track, collisionScale, ref velocity,
        oobBrakingPower, out isOnTrack)` → `ResolveCarVsTrack(Car car,
        List<PlacedPiece> track, float dt, out bool isOnTrack)`, static,
        matching `ResolveCarVsCar(Car a, Car b)`'s shape. Reads
        `car.Position`/`car.Rotation`/`car.Tyres.oobBrakingPower` directly;
        applies the braking consequence via `car.ApplyCollisionPush(Vector2.Zero,
        velocityDelta)` instead of a `ref Vector2 velocity` parameter — same
        mutation path `ResolveCarVsCar` already uses for pushes/impulses.
      - `SetCarDimensions(width, height)` and the `carWidth`/`carHeight`
        instance fields removed. `Car` now exposes public `CollisionWidth`/
        `CollisionHeight` (computed once in `LoadContent`, same formula as
        before: `sprite.Width/Height * Scale * Chassis.collisionScale`),
        parallel to the existing public `CollisionRadius`. `GetCorners` and
        `GetOverlappingPieces` (private static helpers) now take `Car car`
        and read `car.CollisionWidth`/`CollisionHeight`/`Position`/`Rotation`
        directly instead of four separate loose parameters.
      - `Car.Collision` field removed entirely (no longer instantiated in
        the constructor); `Car.Update` now calls the static
        `CarCollision.ResolveCarVsTrack(this, track, dt, out isOnTrack)`.
      - Bug fix (dormant, no observable effect yet): the old `GetCorners`
        computed `carWidth * scale` where `carWidth` (from
        `SetCarDimensions`) already had `Chassis.collisionScale` baked in,
        and the separately-passed `scale` argument was `Chassis.collisionScale`
        again — collisionScale was applied twice to the fine-grained
        per-pixel hitbox used by `GetCorners`, while `GetOverlappingPieces`'s
        coarse bounding-box filter applied it once, so the two were
        inconsistent whenever `collisionScale != 1`. Currently invisible
        since `Chassis.Basic.collisionScale == 1.0f` is the only chassis
        that exists. Fixed by construction in the rewrite (`CollisionWidth`/
        `CollisionHeight` compute the scale exactly once, used consistently
        by both helpers now).
      This also sets up rotation-on-hit (discussed, not yet built): future
      work would extend `Car.ApplyCollisionPush` with a `rotationDelta`
      parameter and have `ResolveCarVsCar` compute/pass it —
      `ResolveCarVsTrack` is unaffected since off-track braking has no
      reason to spin the car.
      DONE — verified against full re-read of CarCollision.cs and Car.cs.
      No other file references the old `Update`/`SetCarDimensions`/
      `Collision` field/instance shape (GameScreen.cs's
      `CarCollision.ResolveCarVsCar(cars[0], cars[1])` call was already
      static and is unaffected).

- [x] 12. `CarPhysics.ApplyLateralGrip` — removed `speedBlend` (was
      `Clamp(Abs(fwdSpeed) / 5f, 0f, 1f)`, ramping grip-correction strength
      in over the first ~5 units of forward speed). Play-tested via
      temporary pin (`speedBlend = 1f`, full-strength grip regardless of
      speed) at low-speed turns and pull-away-from-standstill — user
      reported it "literally identical," no perceptible difference from the
      original ramped version. Likely redundant with `UpdateSteering`'s own
      `speedEngagement` gate (steering barely engages below ~30 speed
      already), so the extra low-speed easing had nothing left to smooth.
      Removed the variable and its multiplication from the final line;
      `fwdSpeed` dropped from `ApplyLateralGrip`'s signature since
      `speedBlend` was its only use (call site in `Update` updated to
      `ApplyLateralGrip(dt, fwd)`). `dt` and `fwd` still used, kept.
      `ApplyLateralGrip`'s core grip math (side, sideVel, gripBase/
      gripReduction with the NaN-guard comment) is unchanged — user
      separately confirmed via the same play-test method that the grip
      effect itself (not `speedBlend`) is necessary ("like driving with ice
      wheels" without it), so that part was kept as-is.
      DONE — verified against full re-read of CarPhysics.cs.

- [x] 13. `CarPhysics.ApplyLateralGrip` follow-up — the remaining
      `gripBase`/`gripReduction` two-step was itself a mini factory-line:
      `gripBase` existed purely as a NaN-guard for the `Math.Pow` call on
      the next line, not as a separately meaningful quantity, so having it
      as its own named variable added a step without adding a concept.
      First pass inlined the clamp directly into the `Math.Pow` call and
      renamed the result from `gripReduction` to `gripStrength` —
      "reduction" was backwards (it's not measuring how much grip is
      reduced, it's the strength of the correction grip applies to lateral
      velocity). User reviewed the inlined version and asked for `gripBase`
      back as its own named variable (readability preference, not a
      correctness concern) — final shape keeps `gripBase` as a separate
      line with its guard comment, `gripStrength`'s corrected name kept.
      `ApplyLateralGrip` is now four lines: the sideways axis, sideways
      velocity, grip strength (with its NaN-guard on the line above), and
      the apply. Same math throughout, no behavior change.
      DONE — verified against full re-read of CarPhysics.cs.

- [x] 14. `CarPhysics.UpdateSteering` — was the only helper in the class
      still doing its own input polling. Every other helper called from
      `Update` (`UpdateVelocity`, `ApplyLateralGrip`, `UpdateGearAndRPM`)
      only receives already-decided values (`isBrake`, `fwdSpeed`, `dt`,
      `fwd`) — `Update` polls `KeyboardState` once up front
      (`IsThrottle = kb.IsKeyDown(forward)`, `bool isBrake = ...`) and hands
      the results down. `UpdateSteering` broke that pattern: it still took
      `KeyboardState kb, Keys left, Keys right` and computed `turnInput`
      internally, making it the only helper aware that keyboard input
      exists at all. Moved the `turnInput` computation
      (`(kb.IsKeyDown(right) ? 1f : 0f) - (kb.IsKeyDown(left) ? 1f : 0f)`)
      up into `Update`, alongside where `IsThrottle`/`isBrake` are already
      decided. `UpdateSteering`'s signature changed from `(dt, kb, left,
      right, ref rot, fwdSpeed)` to `(dt, turnInput, ref rot, fwdSpeed)` —
      it now only sees a plain float, same as every other helper only sees
      decided values. The `if (fwdSpeed < -0.5f) turnInput *= -1f;`
      reverse-steering flip stays inside `UpdateSteering` unchanged (it's
      value-type reassignment of the local parameter copy, doesn't affect
      `Update`'s `turnInput`). No behavior change — same computation, same
      order, just relocated to match where the class already decides
      input-derived values once and threads decisions downward from there.
      DONE — verified against full re-read of CarPhysics.cs.

- [x] 15. `CarPhysics.Update` — removed both `ref` parameters
      (`ref Vector2 pos, ref float rot`). `CarPhysics` already owned
      `Velocity` as a plain field with every helper reading/writing it
      directly, but `Position`/`Rotation` were owned by `Car` and reached
      into via `ref` each frame — an inconsistency raised directly ("why
      are we passing in ref rot"). Made `CarPhysics` own `Position` and
      `Rotation` too, as public fields matching `Velocity`'s pattern:
      - `CarPhysics` gained `public Vector2 Position;` and
        `public float Rotation;`. Constructor signature changed from
        `(Chassis, Engine, Tyres)` to `(Vector2 startPosition, Chassis,
        Engine, Tyres)`, setting `Position = startPosition`.
      - `Update`'s signature dropped `ref Vector2 pos, ref float rot`
        entirely — now `(dt, kb, forward, back, left, right)`. Reads
        `Rotation` directly for the `fwd` vector, writes `Position +=
        Velocity * dt` directly at the end (was `pos += ...`).
      - `UpdateSteering` dropped its own `ref float rot` parameter too and
        writes `Rotation` directly (was `rot += ...`), matching how
        `UpdateVelocity` already wrote `Velocity` directly with no `ref`.
      - `Car.Position`/`Car.Rotation` changed from plain public fields to
        pass-through properties (`public Vector2 Position => Physics.Position;`,
        same pattern already used for `Velocity`/`RPM`/`Gear`). `Car`'s
        constructor no longer sets `Position` itself — passes `startPos`
        into `CarPhysics`'s constructor instead. `Car.Update`'s call to
        `Physics.Update` dropped both `ref` arguments. `ResetCar` and
        `ApplyCollisionPush`, which used to assign `Position`/`Rotation`
        directly as fields, now write through `Physics.Position =`/
        `Physics.Rotation =`/`Physics.Position +=` since the `Car`-level
        properties are read-only.
      Checked every external reader of `car.Position`/`car.Rotation`
      (`Camera.Update`, `CarCollision`'s `ResolveCarVsTrack`/`ResolveCarVsCar`
      via `GetCorners`, `GameScreen`'s `_lastCarPositions[i] = cars[i].Position`
      and lap-timer calls) — all read-only, all unaffected since they just
      see the same values through the (now computed) property. No behavior
      change; `Position`/`Rotation` are now genuinely owned by `CarPhysics`
      instead of being borrowed via `ref` from `Car` each frame.
      DONE — verified against full re-read of CarPhysics.cs, Car.cs, and
      Camera.cs. GameScreen.cs and CarCollision.cs confirmed unaffected
      from earlier full re-reads this session (read-only `car.Position`/
      `car.Rotation` usage, no direct field writes anywhere outside Car.cs).

- [x] 16. `CarPhysics.IsBrake` — promoted from a local `bool isBrake` in
      `Update` to a public property (`{ get; private set; }`), matching
      `IsThrottle`'s pattern exactly, once it had a genuine external
      consumer to justify it (mirrors why `IsThrottle` is a property in the
      first place: `Car.Update` passes it into `Audio.Update` for engine
      sound shaping). Changes:
      - `CarPhysics` gained `public bool IsBrake { get; private set; }`.
        `Update` sets `IsBrake = kb.IsKeyDown(back)` (was local `bool
        isBrake = ...`). `UpdateVelocity` dropped its `bool isBrake`
        parameter and reads the `IsBrake` property directly instead,
        matching how it already read `IsThrottle` directly with no
        parameter.
      - `CarAudio.Update` gained a `bool isBrake` parameter and a new
        `wasBraking` field (edge-detection, same debounce shape as the
        existing `shifting = gear != prevGear` check) so a brake cue plays
        once per press rather than every frame the key is held. Wired
        through `Car.Update`'s call:
        `Audio.Update(dt, Physics.RPM, Engine.rpmMax, Physics.IsThrottle,
        Physics.IsBrake, Physics.Gear, oldGear)`.
      - No `brake.wav` asset exists and none could be generated/sourced
        for this session — the brake cue temporarily reuses the already-
        loaded `shiftDown` `SoundEffect` (same instance used for downshift
        sounds), called out with a `// TEMP` comment at the call site. No
        new `Content.mgcb` entry needed since no new file is loaded. When a
        real brake sound is added: new `SoundEffect brakeSound` field,
        loaded in `LoadContent` alongside `shiftDown`, swap the one
        `PlaySound(shiftDown, ...)` call in the brake-cue block to
        `PlaySound(brakeSound, ...)`.
      DONE — verified against full re-read of CarPhysics.cs, CarAudio.cs,
      and Car.cs.

## Reviewed, no change needed (kept for reference / writeup justification)

- `GameScreen`'s 8 parallel per-car arrays (PenaltyState, raceFinished[],
  finishPlace[], finishTime[], _lastCarPositions[], cars[], cameras[],
  lapTimers[]) — smells like redundant state but isn't worth a `PlayerState`
  wrapper class for 1–2 players; all writes happen once per frame in one
  loop, low divergence risk. Worth a sentence in the writeup justifying the
  decision, not a code change.
- `TrackGenerator.optionsStack` vs `Track` — looks like duplicated state but
  is correctly separated: committed track vs backtracking search frontier.
  Keeping search-scratch data off `PlacedPiece` is the right call (avoids
  mutable scratch fields on a supposedly-finished object).
- `PieceLibrary.PrecomputeUniqueTransforms` — real optimisation (dedupes
  symmetric piece transforms before scoring), not decoration.
- `CarPhysics` — flat, well-decomposed, no smells found.
- `Chassis`/`Engine`/`Tyres` — look mergeable (same shape: ID, DisplayName,
  Price, static Basic/All) but the split is load-bearing: `CarPhysics`'s
  constructor takes three distinct typed parameters, a compile-time
  guarantee against e.g. passing two engines where an engine+tyres are
  expected. Merging into one `Part` type + enum tag would trade that for a
  runtime check — explicitly against the "don't trade compile-time for
  runtime" rule. Keep as is.
- `Screen` interface — minimal two-method interface, does real work enabling
  screen-switching in `Game1`. Not decorative.
- `CarLoadout` struct itself (minus `Default`) — correctly-sized transport
  type between `GarageScreen` and `Game1.StartGame`.
- `Interface.cs`, `Camera.cs`, `Button.cs` — all flat, single-responsibility
  utility/UI classes, no smells.
- `RaceSetupScreen.cs` — `_customMode` bool gating quick-race vs custom-race
  flow is legitimate UI state; difficulty-button closures use the correct
  per-item local-copy pattern (positive contrast to what GarageScreen had
  before fix #4 — worth noting in the writeup).
- `Track.cs` — thin coordinator (Grid/TrackGenerator/TrackInfo), InitGenerator
  correctly reused by constructor and Reset. No smells.
- `PlacedPiece.cs` — `entryConnection ?? TransformedConnections[0]` default is
  a deliberate, documented special case for the start piece, not scratch
  state. Fine as is.
- `Connections.cs` — `Connection` (piece-relative, static piece data) vs
  `WorldConnection` (absolute-grid, used during generation search) look
  mergeable but represent a real domain boundary. Keep separate.
- `Car.cs`, `CarAudio.cs`, `CarCollision.cs` — all well-decomposed,
  single-responsibility, no redundant state or dead code found.

## Scan status: COMPLETE
All 26 .cs files in the repo have been read and reviewed (25 remain after
Candidate.cs's removal). Fixes #1-16 are all verified DONE against the live
files — the note that used to be here (claiming #1/#2/#3/#5 were still
outstanding) was stale and wrong; independently re-verified all four against
TrackDifficulty.cs, Car.cs, LapTimer.cs, and the moved _deleted_Candidate.cs
during the session 2 correctness pass below.

## Session 2 continued — feature: qualifying shootout (2026-07-27)

Added a 1-lap qualifying session before the race that sets grid order by lap
time, per user request. Design confirmed with user: both cars qualify
simultaneously (not sequentially); each player's viewport renders the other
car at 50% alpha (ghost) for a live pace comparison.

Files touched:
- `Car/Car.cs` — `Draw(SpriteBatch)` → `Draw(SpriteBatch, float alpha = 1f)`
  (default-param, backward compatible); `ResetCar()` →
  `ResetCar(Vector2 position)` (confirmed zero callers before this change, so
  safe to reshape).
- `Screens/GameScreen.cs` — new `Phase` enum (`Qualifying`/`Racing`);
  `QualifyingLaps = 1` alongside existing `TotalLaps = 3`; new `qualified[]`/
  `qualifyingTime[]`/`gridPosition[]` arrays; hoisted `startPos` and
  `spawnOffsets` from constructor locals to fields so `StartRaceFromQualifying`
  can reuse them; per-car lap-tracking in `Update` now branches on `phase`
  (qualifying records a snapshot time once `QualifyingLaps` is hit, racing
  keeps its existing `TotalLaps`/`raceFinished`/`finishTime` logic
  untouched); new `StartRaceFromQualifying()` ranks cars by qualifying time,
  repositions them onto `spawnOffsets` by rank (not player index), resets
  `lapTimers`/`penalties`/`raceTime`/`countdown`/`isRacing` so the race times
  itself independently of qualifying; `DrawPlayerViewport` draws non-own cars
  at 50% alpha during qualifying, and swaps the lap-status HUD line between
  "QUALIFYING - LAP 1/1" / "QUALIFIED - {time}" and the existing race labels;
  added a "Grid - P{n}" line during the race and an "Opponent - {time}" line
  during qualifying (same Y-slot, mutually exclusive by phase, no HUD
  collision with the existing 8 label rows).

Design choices made without a separate sign-off round (stated to user,
consistent with what they asked for):
- Off-track penalties apply during qualifying the same as the race (reuses
  the existing `PenaltyState` machinery unchanged - the penalty block's
  `!raceFinished[i]` gate is already true throughout qualifying, so no new
  code was needed for this to work).
- A car keeps driving normally after banking its own qualifying lap (no
  "parked/waiting" state) while the other player finishes theirs.

Correction (user feedback, applied): car-vs-car collision must NOT apply
during qualifying - cars should be able to freely overlap while sharing the
track for the ghost-comparison lap. `CarCollision.ResolveCarVsCar` is now
gated on `phase == Phase.Racing` in addition to the existing
`playerCount == 2 && isRacing` check; still applies normally once the real
race starts. Self-contained one-line gate, no other caller of
`ResolveCarVsCar` exists.

Known minor quirk, not fixed (cosmetic only, judged not worth extra state):
once a car is `qualified[i]`, its `qualifyingTime[i]` is a locked-in
snapshot, but its `PenaltyState.Count`/`TotalSeconds` keep accumulating if it
keeps driving off-track while waiting on the other player - the on-screen
"Penalties" counter can tick up past what was actually charged to their
qualifying time. Doesn't affect the recorded time or grid order, only the
HUD counter's post-qualifying accuracy for that brief waiting window.

NOT YET PLAYTESTED (no way to run the game from this tool) - please verify:
- Qualifying starts immediately (3-2-1-GO), both cars visible, ghost alpha
  looks right on each side of the split screen.
- Grid reorders correctly when player 2 qualifies faster than player 1 (i.e.
  confirm it's ranked by time, not just always keeping player 1 on pole).
- Countdown/HUD cleanly transitions from "QUALIFIED" labels into the normal
  race HUD with no leftover qualifying state visible.
- `Color * float` (used for the ghost alpha) fades as expected given the car
  sprite's actual alpha-blend/premultiply setup - flagging since this wasn't
  verified against the actual texture, just standard MonoGame `SpriteBatch`
  default `BlendState.AlphaBlend` behavior.

## Workflow notes for next session

- Repo lives at
  `C:\Users\jackg\source\repos\jack-gortonnn\ProceduralRacing` and is only
  reachable via the `Visual_Studio_2026:*` MCP tools (read_text_file,
  edit_file, write_file, directory_tree, etc) — NOT via str_replace/
  create_file/bash_tool, which operate on a separate sandboxed filesystem
  and will silently write to the wrong place without erroring loudly.
- Before any edit: re-read the target file fresh with
  `Visual_Studio_2026:read_text_file` (line numbers from earlier in a
  conversation are not reliable).
- One targeted edit per `Visual_Studio_2026:edit_file` call, anchored on a
  short unique old_str/oldText.
- After any nontrivial edit, re-read the full file end-to-end before
  considering it done.

## Session 2 — correctness audit (2026-07-27)

New pass focused on correctness (not just complexity), independent of the
session 1 sweep above. Findings below, pending sign-off before any edits:

- [x] `CarPhysics.UpdateGearAndRPM` (`Car/Properties/CarPhysics.cs`) —
      off-by-one, top gear (5th, for the 5-gear Basic engine) is
      mathematically unreachable. `totalGears = gearCount - 1 = 4`;
      `gearProgress` correctly ranges 0..4 across the speed range, but
      `Gear = 1 + Math.Min((int)gearProgress, totalGears - 1)` clamps the
      index to 3, capping `Gear` at 4. Verified numerically:
      `MathHelper.Clamp` pins `speedRatio` to exactly 1.0 at practical top
      speed, `Math.Pow(1f, x) == 1f` exactly (no float overshoot to guard
      against), so the `- 1` in the clamp isn't protecting against
      anything — it just cuts off the last gear. Gear 5 is unreachable for
      roughly the top 16% of the speed range (ratio gtr 0.84 at
      gearCurve=0.6), not just an exact-boundary case. Fix is
      `totalGears - 1` -> `totalGears`; self-contained to this private
      method, no external callers (confirmed — only `Update` calls it).
      DONE — verified against full re-read of CarPhysics.cs.

- [x] `GameScreen.Update` (`Screens/GameScreen.cs`) — `LapTimer
      .BuildCheckpoints` re-runs every tick for the whole race, not once
      when the track completes. `Settings.Generation.SecondsPerStep =
      0.00001f` means the `if (timer >= SecondsPerStep)` block fires
      essentially every frame, and the `if (track.IsComplete) foreach(...)
      BuildCheckpoints(...)` check inside it has no "already built" guard
      — `track.IsComplete` stays true forever once the track finishes, so
      it keeps re-firing for the rest of the race. Not an observable
      gameplay bug (same checkpoints recomputed each time, same order) but
      wasted work every frame, every player, for the whole race. Fix: gate
      with a one-time `checkpointsBuilt` bool in `GameScreen`;
      self-contained to that file (confirmed — `BuildCheckpoints` has no
      other callers).
      DONE — added `checkpointsBuilt` field, guarded the rebuild block,
      verified against full re-read of GameScreen.cs.

- [x] `Button.Update` (`UI/Button.cs`) — `OnClick` fires every frame the
      mouse is held down while hovering, not once per press — no
      edge-detection, unlike the debounce pattern already used elsewhere
      (`RaceSetupScreen`'s key input, `CarAudio`'s `wasBraking` check).
      Currently invisible for `GarageScreen`'s cycle buttons because
      `Chassis.All`/`Engine.All`/`Tyres.All` each have exactly one entry
      (`(0 + 1) % 1 == 0`, repeat-firing lands on the same index) — but
      will cause runaway cycling the moment a second chassis/engine/tyre
      option is added, which the `Price` field and `All` array shape
      clearly anticipate. BEHAVIOR CHANGE, not a pure shape change — it
      changes when `OnClick` fires for every `Button` in the game (12 call
      sites across `GarageScreen` and `RaceSetupScreen`). Needs sign-off
      before touching.
      DONE (user signed off) — added `_wasPressed` field, fires only on
      the not-pressed -> pressed edge while hovering, matching the
      debounce pattern already used elsewhere. Verified against full
      re-read of Button.cs. All 12 call sites just call `Update()` once
      per frame per screen loop and don't depend on repeat-fire, so no
      behavior other than the fix itself changes.

- Solo mode (`playerCount == 1`) confirmed dead by user — pruned. Removed:
  `GarageScreen`'s `panelWidth`/`Label` ternaries (now fixed 2-player
  values) and the pointless `playerCount == 2 ? 860 : 860` continue-button
  ternary; `GameScreen`'s single-viewport branch in `GetViewport`, the
  `if (playerCount == 2)` guards around `SetAudioPan` and
  `DrawSplitDivider`. Left the `playerCount`/`_playerCount` fields
  themselves in place (still legitimately used for array sizing and loop
  bounds, correctly always 2) and left `GameScreen`'s
  `if (playerCount == 2 && isRacing) CarCollision.ResolveCarVsCar(...)`
  guard as-is (a real bounds-guard against `cars[0]`/`cars[1]`, not dead).
  Did not touch `GarageScreen`'s or `GameScreen`'s constructor signatures
  — only one caller each (`Game1.GoTo1v1Garage`/`Game1.StartGame`), both
  already always pass 2, so no cross-file signature change was needed to
  remove the dead branches. Verified against full re-read of both files.

- [x] `CarPhysics.UpdateGearAndRPM` follow-up (`Car/Properties/CarPhysics.cs`)
      — the gear-index fix above was correct but incomplete: it exposed a
      second, deeper bug reported by the user ("5th gear stays at ~800rpm").
      `speedRatio` was `MathHelper.Clamp`'d to `[0,1]`; once real speed
      crossed 90% of true maxSpeed, `speedRatio` (and everything derived
      from it — `curvedRatio`, `gearProgress`) froze at a constant, and
      `withinGear = gearProgress % 1f` landed on exactly `4.0 % 1 = 0` at
      that frozen point — pinning RPM at `rpmIdle` for the *entire* top
      ~10% of the speed range, not just a boundary instant. This bug
      predates the gear-index fix (it used to freeze gear 4's RPM instead,
      just less noticeably since gear 4 still had real variation below the
      freeze point) — the index fix just moved the freeze onto gear 5's
      whole range, making it obvious. Root cause: `speedRatio`'s upper
      clamp was unnecessary (`Math.Abs(fwdSpeed) / (maxSpeed *
      practicalTopSpeedFraction)` is already non-negative by construction)
      and actively harmful — it destroyed the speed information needed to
      keep `gearProgress` (and thus `withinGear`) moving for real speeds
      between the practical (90%) and true (100%+) top speed. Fix: removed
      the clamp entirely; `Gear`'s own `Math.Min((int)gearProgress,
      totalGears)` already caps the gear number regardless of how high
      `gearProgress` climbs, so gear selection is unaffected at the top
      end and completely unchanged below 90% speed (gears 1-4 never
      touched the old clamp boundary anyway). BEHAVIOR CHANGE (RPM/gear
      values for the top ~10% of the speed range now actually vary,
      matching the sawtooth pattern used everywhere else) — user confirmed
      the symptom and expected result directly, so applied without a
      separate sign-off round.
      DONE — verified against full re-read of CarPhysics.cs. Simulated the
      boundary numerically: at fwdSpeed = 0.9*maxSpeed exactly,
      gearProgress = 4.0, Gear = 5, withinGear = 0 (RPM starts at idle
      right as you shift into 5th — correct sawtooth entry, not a freeze);
      above that, gearProgress keeps climbing (e.g. ~4.77 at true
      maxSpeed), giving withinGear ~0.77 and RPM climbing toward rpmMax as
      expected.
