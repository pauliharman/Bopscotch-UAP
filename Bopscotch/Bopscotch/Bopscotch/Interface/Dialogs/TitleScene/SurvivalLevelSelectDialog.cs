using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class SurvivalLevelSelectDialog : ButtonDialog
    {
        private Timer _thumbnailTransitionTimer;
        private bool _thumbnailIsFadingOut;
        private int _thumbnailStepDirection;
        private Color _thumbnailTargetTint;

        public SurvivalLevelSelectDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = 300;

            AddButton("Start!", new Vector2(Definitions.Left_Button_Column_X, 500), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Back", new Vector2(Definitions.Right_Button_Column_X, 500), Button.ButtonIcon.Back, Color.DodgerBlue);

            AddIconButton("prev-area", new Vector2(Definitions.Back_Buffer_Center.X - 200, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next-area", new Vector2(Definitions.Back_Buffer_Center.X + 200, 175), Button.ButtonIcon.Next, Color.DodgerBlue);
            AddIconButton("prev-level", new Vector2(Definitions.Back_Buffer_Center.X - 200, 375), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next-level", new Vector2(Definitions.Back_Buffer_Center.X + 200, 375), Button.ButtonIcon.Next, Color.DodgerBlue);

            SetMovementLinksForButton("prev-area", "", "prev-level", "", "next-area");
            SetMovementLinksForButton("next-area", "", "next-level", "prev-area", "");
            SetMovementLinksForButton("prev-level", "prev-area", "Start!", "", "next-level");
            SetMovementLinksForButton("next-level", "next-area", "Back", "prev-level", "");
            SetMovementLinksForButton("Start!", "prev-level", "", "", "Back");
            SetMovementLinksForButton("Back", "next-level", "", "Start!", "");

            _defaultButtonCaption = "Start!";
            _cancelButtonCaption = "Back";

            _thumbnailTransitionTimer = new Timer("", ThumbnailTransitionHandler);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_thumbnailTransitionTimer.Tick);

            _boxCaption = "Select Area and Level";
        }

        public override void Activate()
        {
            base.Activate();

            _thumbnailIsFadingOut = true;
            _thumbnailTransitionTimer.NextActionDuration = Thumbnail_Fade_Duration_In_Milliseconds;
        }

        private void ThumbnailTransitionHandler()
        {
            if (_thumbnailIsFadingOut)
            {
                Profile.StepSelectedArea(_thumbnailStepDirection);
                if (Profile.AreaData.Locked) { SetPresentationForLockedLevel(); }
                else { SetPresentationForUnlockedLevel(); }
                _thumbnailTransitionTimer.NextActionDuration = Thumbnail_Fade_Duration_In_Milliseconds;
            }

            _thumbnailIsFadingOut = !_thumbnailIsFadingOut;
        }

        private void SetPresentationForLockedLevel()
        {
            _thumbnailTargetTint = Color.Gray;
            _buttons["prev-level"].Disabled = true;
            _buttons["next-level"].Disabled = true;
            _buttons["Start!"].Disabled = true;
        }

        private void SetPresentationForUnlockedLevel()
        {
            _thumbnailTargetTint = Color.White;
            _buttons["prev-level"].Disabled = false;
            _buttons["next-level"].Disabled = false;
            _buttons["Start!"].Disabled = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawCurrentAreaSelection(spriteBatch);
            DrawCurrentLevelSelection(spriteBatch);
        }

        private void DrawCurrentAreaSelection(SpriteBatch spriteBatch)
        {
            Texture2D areaThumbnail = TextureManager.Textures[Profile.AreaData.SelectionTexture];

            spriteBatch.Draw(areaThumbnail, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 175.0f), null, ThumbnailTint, 0.0f,
                new Vector2(areaThumbnail.Width, areaThumbnail.Height) / 2.0f, Definitions.Background_Texture_Thumbnail_Scale, SpriteEffects.None, 
                Component_Render_Depth);

            spriteBatch.Draw(TextureManager.Textures[Lock_Texture], new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 175.0f), null,
                LockTint, 0.0f, new Vector2(TextureManager.Textures[Lock_Texture].Width, TextureManager.Textures[Lock_Texture].Height) / 2.0f, 1.0f,
                SpriteEffects.None, Component_Render_Depth - 0.001f);

            TextWriter.Write(Translator.Translation(Profile.AreaData.Name), spriteBatch,
                new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 225.0f),
                _thumbnailTargetTint, Color.Black, 3.0f, Component_Render_Depth, TextWriter.Alignment.Center);
        }

        private Color ThumbnailTint
        {
            get
            {
                if (_thumbnailIsFadingOut)
                {
                    if (_thumbnailTransitionTimer.CurrentActionProgress == 1.0f) { return _thumbnailTargetTint; }
                    else { return Color.Lerp(_thumbnailTargetTint, Color.Transparent, _thumbnailTransitionTimer.CurrentActionProgress); }
                }
                else
                {
                    return Color.Lerp(Color.Transparent, _thumbnailTargetTint, _thumbnailTransitionTimer.CurrentActionProgress);
                }
            }
        }

        private Color LockTint
        {
            get
            {
                if (Profile.AreaData.Locked)
                {
                    if (_thumbnailIsFadingOut)
                    {
                        if (_thumbnailTransitionTimer.CurrentActionProgress == 1.0f) { return Color.White; }
                        else { return Color.Lerp(Color.White, Color.Transparent, _thumbnailTransitionTimer.CurrentActionProgress); }
                    }
                    else
                    {
                        return Color.Lerp(Color.Transparent, Color.White, _thumbnailTransitionTimer.CurrentActionProgress);
                    }
                }

                return Color.Transparent;
            }
        }

        private void DrawCurrentLevelSelection(SpriteBatch spriteBatch)
        {
            TextWriter.Write((Profile.AreaData.LastSelectedLevel + 1).ToString(), spriteBatch,
                new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 325.0f), _thumbnailTargetTint, Color.Black, 3.0f,
                Component_Render_Depth, TextWriter.Alignment.Center);
        }

        protected override void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            if (inputSource.SelectionTriggered)
            {
                if ((inputSource.SelectionLocation != Vector2.Zero) || (_activeButtonCaption == "Start!") || (_activeButtonCaption == "Back"))
                {
                    base.CheckForAndHandleSelection(inputSource);
                }
                else
                {
                    HandleStepSelection(_activeButtonCaption);
                }
            }
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            if ((buttonCaption == "Start!") || (buttonCaption == "Back")) { return base.HandleButtonTouch(buttonCaption); }

            HandleStepSelection(buttonCaption);
            return false;
        }

        private void HandleStepSelection(string stepButtonCaption)
        {
            switch (stepButtonCaption)
            {
                case "prev-area":
                    if (Profile.MoreAreasExistInStepDirection(-1))
                    {
                        _thumbnailStepDirection = -1;
                        _thumbnailTransitionTimer.NextActionDuration = Thumbnail_Fade_Duration_In_Milliseconds;
                    }
                    break;
                case "next-area":
                    if (Profile.MoreAreasExistInStepDirection(1))
                    {
                        _thumbnailStepDirection = 1;
                        _thumbnailTransitionTimer.NextActionDuration = Thumbnail_Fade_Duration_In_Milliseconds;
                    }
                    break;
                case "prev-level":
                    Profile.AreaData.SelectPreviousLevel();
                    break;
                case "next-level": Profile.AreaData.SelectNextLevel();
                    break;
            }
        }

        private const int Dialog_Height = 580;
        private const float Component_Render_Depth = 0.141f;

        private const int Thumbnail_Fade_Duration_In_Milliseconds = 120;

        private const string Lock_Texture = "icon-locked";
    }
}
