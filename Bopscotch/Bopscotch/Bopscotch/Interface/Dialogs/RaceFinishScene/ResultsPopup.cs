using Microsoft.Xna.Framework;

using Bopscotch.Effects.Popups;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsPopup : PopupRequiringDismissal
    {
        public int PlayerOneSkinSlotIndex { private get; set; }
        public int PlayerTwoSkinSlotIndex { private get; set; }
        public Definitions.RaceOutcome Outcome { private get; set; }

        public float ParentDialogY { set { DisplayPosition = new Vector2(Definitions.Back_Buffer_Center.X, value + Offset_From_Dialog_Top); } }

        public ResultsPopup()
            : base()
        {
            RenderDepth = 0.05f;
        }

        public override void Activate()
        {
            base.Activate();

            switch (Outcome)
            {
                case Definitions.RaceOutcome.PlayerOneWin:
                    TextureReference = Player_One_Win_Texture;
                    break;
                case Definitions.RaceOutcome.PlayerTwoWin:
                    TextureReference = Player_Two_Win_Texture;
                    break;
                case Definitions.RaceOutcome.Draw:
                    TextureReference = Draw_Texture;
                    break;
                case Definitions.RaceOutcome.Incomplete:
                    TextureReference = Incomplete_Texture;
                    break;
            }
        }

        private const float Offset_From_Dialog_Top = 200.0f;
        private const string Player_One_Win_Texture = "popup-race-1up-win";
        private const string Player_Two_Win_Texture = "popup-race-2up-win";
        private const string Draw_Texture = "popup-race-draw";
        private const string Incomplete_Texture = "popup-race-not-finished";
    }
}
