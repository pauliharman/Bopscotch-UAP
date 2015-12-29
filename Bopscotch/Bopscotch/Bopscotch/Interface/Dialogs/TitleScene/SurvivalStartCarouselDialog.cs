using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class SurvivalStartCarouselDialog : AreaSelectionCarouselDialog
    {
        private bool _upHeld;
        private bool _downHeld;

        public SurvivalStartCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Area and Level");

            Height = Dialog_Height;
            TopYWhenActive = 300;
            TopYWhenInactive = Definitions.Back_Buffer_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
        }

        protected override void CreateButtons()
        {
            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 470), Button.ButtonIcon.B, Color.Red, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Right_Button_Column_X, 470), Button.ButtonIcon.A, Color.LawnGreen, 0.7f);

            AButtonPressedValue = "Start!";

            base.CreateButtons();
        }

        protected override void CheckMasterSelectActivation(InputProcessorBase inputSource)
        {
            if (!_buttons["Start!"].Disabled) { base.CheckMasterSelectActivation(inputSource); }
        }

        protected override void HandleNonSpinButtonAction(string buttonCaption)
        {
            DismissWithReturnValue(buttonCaption);
        }

        private void AttemptToStepSelectedLevel(int stepDirection, string stepButtonCaption, bool stepNotPossible)
        {
            if (!stepNotPossible)
            {
                Data.Profile.CurrentAreaData.UpdateLevelSelection(stepDirection);
                SetupStartButtonDependentOnAreaLockStatus();
            }
        }

        private void SetupStartButtonDependentOnAreaLockStatus()
        {
            _buttons["Start!"].Disabled = ((bool)_dataSource[SelectedItem].Attribute("locked"));
        }

        public override void Activate()
        {
            InputSources = new List<InputProcessorBase>();
            InputSources.Add(ControllerPool.Controllers.PlayerOne);

            base.Activate();

            SetInitialSelection(Data.Profile.CurrentAreaData.Name);
            SetupStartButtonDependentOnAreaLockStatus();

            _upHeld = false;
            _downHeld = false;
        }

        protected override void HandleDialogExitCompletion()
        {
            Data.Profile.Save();
            
            base.HandleDialogExitCompletion();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (!InputSources[0].IsAvailable) { Cancel(); }
        }

        protected override void CheckForAndHandleInputFromSingleSource(InputProcessorBase inputSource, int millisecondsSinceLastUpdate)
        {
            base.CheckForAndHandleInputFromSingleSource(inputSource, millisecondsSinceLastUpdate);

            if (inputSource.MoveDown)
            {
                if (!_downHeld) { AttemptLevelStep(-1); _downHeld = true; }
            }
            else
            {
                _downHeld = false;
            }

            if (inputSource.MoveUp)
            {
                if (!_upHeld) { AttemptLevelStep(1); _upHeld = true; }
            }
            else
            {
                _upHeld = false;
            }
        }

        private void AttemptLevelStep(int direction)
        {
            int currentSelection = Data.Profile.CurrentAreaData.LastSelectedLevel;

            Data.Profile.CurrentAreaData.UpdateLevelSelection(direction);

            if (currentSelection != Data.Profile.CurrentAreaData.LastSelectedLevel)
            {
                _textTransitionTimer.NextActionDuration = Text_Fade_Duration_In_Milliseconds;
                SoundEffectManager.PlayEffect("menu-move");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!(bool)_dataSource[SelectedItem].Attribute("locked")) { DrawCurrentLevelSelection(spriteBatch); }
        }

        protected override void DrawAreaDetails(SpriteBatch spriteBatch)
        {
            string areaText = _dataSource[SelectedItem].Attribute("name").Value;

            if ((bool)_dataSource[SelectedItem].Attribute("locked"))
            {
                areaText = string.Concat(areaText, " (", Translator.Translation("locked"), ")");
            }
            else if (_dataSource[SelectedItem].Attribute("difficulty").Value != "n/a")
            {
                areaText = string.Concat(areaText, " (", Translator.Translation(_dataSource[SelectedItem].Attribute("difficulty").Value), ")");
            }

            TextWriter.Write(areaText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 250.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Center);
        }

        private void DrawCurrentLevelSelection(SpriteBatch spriteBatch)
        {
            string levelText = string.Format(Translator.Translation("Level: {0} ({1} unlocked)"),
                Data.Profile.CurrentAreaData.LastSelectedLevel + 1,
                Data.Profile.CurrentAreaData.UnlockedLevelCount);

            TextWriter.Write(levelText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 305.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Center);
        }

        protected override void HandleRotationComplete()
        {
            base.HandleRotationComplete();

            Data.Profile.CurrentAreaName = Selection;
            SetupStartButtonDependentOnAreaLockStatus();

            _textTransitionTimer.NextActionDuration = Text_Fade_Duration_In_Milliseconds;
        }

        protected override void DrawDpadMessage(SpriteBatch spriteBatch)
        {
            DrawMessage(spriteBatch, "Change Area", TextWriter.Alignment.Right, 0);
            DrawMessage(spriteBatch, "Select Level", TextWriter.Alignment.Left, TextureManager.Textures[DPad_Texture].Bounds.Width / 2);
        }

        private void DrawMessage(SpriteBatch spriteBatch, string message, TextWriter.Alignment alignment, int sourceTextureOffset)
        {
            float buttonIconSpaceWidth = ((TextureManager.Textures[DPad_Texture].Bounds.Width / 2.0f) * DPad_Message_Scale) + Dpad_Text_X_Margin;

            Vector2 textOrigin = new Vector2(
                Definitions.Back_Buffer_Center.X + (alignment == TextWriter.Alignment.Left ? 20.0f + buttonIconSpaceWidth : -20.0f),
                WorldPosition.Y + Carousel_Center_Y + 180.0f);

            TextWriter.Write(message, spriteBatch, textOrigin, Color.White, Color.Black, 3.0f, DPad_Message_Scale, 0.05f, alignment);

            Vector2 buttonOrigin = new Vector2(textOrigin.X - buttonIconSpaceWidth, textOrigin.Y + 10.0f);
            if (alignment == TextWriter.Alignment.Right) { buttonOrigin.X -= TextWriter.LastTextArea.Width; }

            spriteBatch.Draw(
                TextureManager.Textures[DPad_Texture],
                buttonOrigin,
                new Rectangle(sourceTextureOffset, 0, TextureManager.Textures[DPad_Texture].Bounds.Width / 2, TextureManager.Textures[DPad_Texture].Bounds.Height),
                Color.White,
                0.0f,
                Vector2.Zero,
                DPad_Message_Scale,
                SpriteEffects.None,
                0.05f);
        }

        private const float Carousel_Center_Y = 180.0f;
        private const float Carousel_Horizontal_Radius = 290.0f;
        private const float Carousel_Vertical_Radius = 20.0f;
        private const int Dialog_Height = 540;
        private const int Text_Fade_Duration_In_Milliseconds = 150;
    }
}
