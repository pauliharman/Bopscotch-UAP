﻿using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;

using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class CharacterSelectionCarouselDialog : AvatarSelectionCarouselDialog
    {
        public CharacterSelectionCarouselDialog(
            Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Character");

            TopYWhenActive = Top_Y_When_Active;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            AddButton("Back", new Vector2(300.0f, 325), Button.ButtonIcon.B, Color.Red, 0.6f);
            AddButton("Edit", new Vector2(Definitions.Back_Buffer_Center.X, 325), Button.ButtonIcon.Y, Color.Gold, 0.6f);
            AddButton("Select", new Vector2(1300.0f, 325), Button.ButtonIcon.A, Color.LawnGreen, 0.6f);

            YButtonPressedValue = "Edit";
        }

        public override void Activate()
        {
            base.Activate();
            SetInitialSelection(Data.Profile.Settings.SelectedAvatarSlot);
        }

        protected override void SelectionHandler(string buttonCaption)
        {
            base.SelectionHandler(buttonCaption);
            DismissWithReturnValue(buttonCaption);
        }

        private const float Top_Y_When_Active = 400.0f;
    }
}
