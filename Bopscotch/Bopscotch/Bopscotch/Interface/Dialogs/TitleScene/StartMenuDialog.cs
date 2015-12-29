using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class StartMenuDialog : ButtonDialog
    {
        public StartMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Adventure", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Adventure, Color.LawnGreen);
            AddButton("Race", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Race, Color.LawnGreen);
            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 320.0f), Button.ButtonIcon.B, Color.Red, 0.7f);

            SetMovementLinksForButton("Adventure", "", "Back", "", "Race");
            SetMovementLinksForButton("Race", "", "Back", "Adventure", "");
            SetMovementLinksForButton("Back", "", "", "Adventure", "Race");

            _defaultButtonCaption = "Race";
            _cancelButtonCaption = "Back";

            _boxCaption = "Select Game Mode";
        }

        protected override void ActivateButton(string caption)
        {
            if ((caption == "Back") && (_activeButtonCaption != "Back") && (_activeButtonCaption != null))
            {
                SetMovementLinksForButton("Back", _activeButtonCaption, "", "Adventure", "Race");
            }

            base.ActivateButton(caption);
        }

        private const int Dialog_Height = 420;
        private const float Top_Y_When_Active = 400.0f;
    }
}
