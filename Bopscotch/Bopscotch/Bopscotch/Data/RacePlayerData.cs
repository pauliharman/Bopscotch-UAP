﻿using Microsoft.Xna.Framework;

namespace Bopscotch.Data
{
    public abstract class RacePlayerData
    {
        public int TotalRaceTimeElapsedInMilliseconds { get; protected set; }
        public int LapsCompleted { get; protected set; }
        public int LastCheckpointTimeInMilliseconds { get; protected set; }
        public int LastCheckpointIndex { get; protected set; }

        public abstract Vector2 PlayerWorldPosition { get; }

        public RacePlayerData()
            : base()
        {
            TotalRaceTimeElapsedInMilliseconds = 0;
            LastCheckpointIndex = -1;
        }
    }
}
