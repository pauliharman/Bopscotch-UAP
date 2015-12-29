using Microsoft.Xna.Framework;

using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Data
{
    public abstract class RacePlayerCommunicationData
    {
        public int TotalRaceTimeElapsedInMilliseconds { get; protected set; }
        public int LapsCompleted { get; protected set; }
        public int LastCheckpointTimeInMilliseconds { get; protected set; }
        public int LastCheckpointIndex { get; protected set; }

        public Definitions.PowerUp LastAttackPowerUp { get; protected set; }
        public int LastAttackPowerUpTimeInMilliseconds { get; protected set; }

        public abstract Vector2 PlayerWorldPosition { get; }

        public RacePlayerCommunicationData()
            : base()
        {
            TotalRaceTimeElapsedInMilliseconds = 0;
            LastCheckpointIndex = -1;
        }
    }
}
