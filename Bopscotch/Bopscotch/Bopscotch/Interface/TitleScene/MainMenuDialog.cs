using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.TitleScene
{
    public class MainMenuDialog : ButtonDialog
    {
        public MainMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Help", new Vector2(Definitions.Left_Button_Column_X, 85), Button.ButtonIcon.Help, Color.DodgerBlue, 0.7f);
            AddButton("Options", new Vector2(Definitions.Left_Button_Column_X, 215), Button.ButtonIcon.Options, Color.DodgerBlue, 0.7f);
            AddButton("Store", new Vector2(Definitions.Right_Button_Column_X, 85), Button.ButtonIcon.Store, Color.DodgerBlue, 0.7f);
            AddButton("Character", new Vector2(Definitions.Right_Button_Column_X, 215), Button.ButtonIcon.Character, Color.DodgerBlue, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Back_Buffer_Center.X, 360), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Quit", new Vector2(Definitions.Back_Buffer_Center.X, 1500), Button.ButtonIcon.None, Color.Transparent);

            SetMovementLinksForButton("Help", "", "Options", "", "Store");
            SetMovementLinksForButton("Options", "Help", "Start!", "", "Character");
            SetMovementLinksForButton("Store", "", "Character", "Help", "");
            SetMovementLinksForButton("Character", "Store", "Start!", "Options", "");
            SetMovementLinksForButton("Start!", "Options", "", "Options", "Character");

            _defaultButtonCaption = "Start!";
            _cancelButtonCaption = "Quit";
        }

        protected override void ActivateButton(string caption)
        {
            if ((caption == "Start!") && (_activeButtonCaption != "Start!") && (_activeButtonCaption != null))
            {
                SetMovementLinksForButton("Start!", _activeButtonCaption, "", "Options", "Character");
            }

            base.ActivateButton(caption);
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 400.0f;
    }
}
