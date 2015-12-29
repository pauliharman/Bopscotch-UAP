using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public class CheckpointFlag : Flag
    {
        public int CheckpointIndex { get; private set; }

        public CheckpointFlag(int checkpointIndex)
            : base()
        {
            TextureReference = Texture_Name;
            Texture = TextureManager.Textures[Texture_Name];

            CheckpointIndex = checkpointIndex;

            SetFrameAndAnimation();
        }

        private const string Texture_Name = "flag-checkpoint";

        public const string Data_Node_Name = "checkpoint-flag";
    }
}
