using System.Collections.Generic;
using Microsoft.Xna.Framework;

// Times laps using one checkpoint line at the exit of every track piece. Checkpoint 0 is the start/finish line.
// Cutting the track: the car is allowed to miss a few pieces (Settings.Race.SkipGrace) - e.g. clipping a corner
// and going from piece 1 straight to piece 3 is fine. Skipping more than that invalidates the lap straight away,
// so the player is told immediately rather than finding out at the finish line.

public enum LapEvent { None, LapCompleted, LapInvalidated }

public class LapTimer
{
    private readonly List<Vector2> checkpointPositions = new();
    private readonly List<Vector2> checkpointDirections = new();
    private float checkpointRadius;

    private int nextCheckpoint;
    private bool started;

    public double CurrentLapTime { get; private set; }
    public double LastLapTime { get; private set; }
    public double BestLapTime { get; private set; }       // 0 = no valid lap yet
    public double LastLapDelta { get; private set; }      // last lap minus the previous best (negative = improved)
    public bool LastLapWasBest { get; private set; }
    public bool LastLapValid { get; private set; } = true;
    public bool CurrentLapValid { get; private set; } = true;
    public int LapsCompleted { get; private set; }
    public LapEvent LastEvent { get; private set; }       // what happened this frame, for the UI to react to

    public void BuildCheckpoints(List<PlacedPiece> track, int tileSize)
    { // Places a checkpoint line at the exit edge of every piece, facing the direction of travel
        checkpointPositions.Clear();
        checkpointDirections.Clear();
        checkpointRadius = tileSize * 0.5f;

        foreach (var piece in track)
        {
            var exit = piece.ExitConnection;
            Vector2 position = ((piece.GridPosition + exit.Position).ToVector2() + exit.Direction.ToVector2() * 0.5f + new Vector2(0.5f)) * tileSize;

            checkpointPositions.Add(position);
            checkpointDirections.Add(exit.Direction.ToVector2());
        }
    }

    public void Update(float dt, Vector2 carPos, Vector2 carLastPos)
    {
        LastEvent = LapEvent.None;
        if (checkpointPositions.Count == 0) return;

        if (started) CurrentLapTime += dt;

        for (int i = 0; i < checkpointPositions.Count; i++)
        {
            if (!CrossedForwards(i, carLastPos, carPos)) continue;

            if (i == 0)
                CrossFinishLine();
            else if (started && i >= nextCheckpoint)
            {
                CheckSkipped(i - nextCheckpoint);
                nextCheckpoint = i + 1;
            }
        }
    }

    private bool CrossedForwards(int i, Vector2 from, Vector2 to)
    { // True if the car moved from behind the line to in front of it this frame, close to the line's centre
        Vector2 point = checkpointPositions[i], dir = checkpointDirections[i];
        return Vector2.Dot(from - point, dir) < 0
            && Vector2.Dot(to - point, dir) >= 0
            && Vector2.Distance(to, point) <= checkpointRadius;
    }

    public void InvalidateLap()
    { // Called from outside, e.g. when the car exceeds track limits in time trial
        if (!started || !CurrentLapValid) return;
        CurrentLapValid = false;
        LastEvent = LapEvent.LapInvalidated;
    }

    private void CheckSkipped(int skipped)
    {
        if (skipped > Settings.Race.SkipGrace && CurrentLapValid)
        {
            CurrentLapValid = false;
            LastEvent = LapEvent.LapInvalidated;
        }
    }

    private void CrossFinishLine()
    {
        if (started)
        {
            // Pieces between the last checkpoint hit and the finish line also count as skipped
            CheckSkipped(checkpointPositions.Count - nextCheckpoint);

            LastLapTime = CurrentLapTime;
            LastLapValid = CurrentLapValid;
            LastLapDelta = BestLapTime > 0 ? LastLapTime - BestLapTime : 0;
            LastLapWasBest = CurrentLapValid && (BestLapTime == 0 || LastLapTime < BestLapTime);
            if (LastLapWasBest) BestLapTime = LastLapTime;

            LapsCompleted++;
            if (LastEvent == LapEvent.None) LastEvent = LapEvent.LapCompleted;
        }

        started = true;
        CurrentLapTime = 0;
        CurrentLapValid = true;
        nextCheckpoint = 1;
    }
}
