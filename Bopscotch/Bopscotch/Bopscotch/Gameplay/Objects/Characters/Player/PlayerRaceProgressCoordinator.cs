using Microsoft.Xna.Framework;

namespace Bopscotch.Gameplay.Objects.Characters.Player
{
    public class PlayerRaceProgressCoordinator : Data.RacePlayerData
    {
        public Vector2 RestartPosition { get; private set; }
        public bool RestartsFacingLeft { get; private set; }

        public Player Player { private get; set; }
        public override Vector2 PlayerWorldPosition { get { return Player.WorldPosition; } }

        public void Update(int millisecondsSinceLastUpdate)
        {
            TotalRaceTimeElapsedInMilliseconds += millisecondsSinceLastUpdate;
        }

        public void CompleteLap()
        {
            LapsCompleted++;
            SetRestartPoint();
        }

        public void SetRestartPoint()
        {
            LastCheckpointTimeInMilliseconds = TotalRaceTimeElapsedInMilliseconds;
            RestartPosition = new Vector2(Player.WorldPosition.X, Player.WorldPosition.Y + Player.DistanceToGround - Player.CollisionBoundingCircle.Radius);
            RestartsFacingLeft = Player.IsMovingLeft;
        }

        public void ResurrectPlayerAtLastRestartPoint()
        {
            Player.WorldPosition = RestartPosition;
            Player.Mirror = RestartsFacingLeft;
            Player.Activate();
            Player.StartTimerToResurrectionCompleted();
        }
    }
}
