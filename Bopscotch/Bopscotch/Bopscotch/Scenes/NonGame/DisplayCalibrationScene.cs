
using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;
using Bopscotch.Input;

namespace Bopscotch.Scenes.NonGame
{
    public class DisplayCalibrationScene : ContentSceneWithBackDialog
    {
        public DisplayCalibrationScene()
            : base()
        {
            _backgroundTextureName = Background_Texture_Name;
            _maintainsTitleSceneMusic = false;
            _contentFileName = Display_Content_Elements_File;
        }

        public override void Activate()
        {
            if (!NextSceneParameters.Get<bool>("button-caption")) { BackButtonCaption = "OK"; }

            foreach (InputProcessorBase processor in _inputProcessors) { processor.AllowDirectionalRepeat = true; }

            MusicManager.StopMusic();
            base.Activate();
        }

        protected override void CompleteDeactivation()
        {
            Data.Profile.Save();

            foreach (InputProcessorBase processor in _inputProcessors) { processor.AllowDirectionalRepeat = false; }

            base.CompleteDeactivation();
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput();

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            for (int i = 0; i < _inputProcessors.Count; i++)
            {
                if (_inputProcessors[i].MoveUp) { UpdateSafeAreaSize(-1); break; }
                if (_inputProcessors[i].MoveDown) { UpdateSafeAreaSize(1); break; }
                if (_inputProcessors[i].MoveLeft) { UpdateSafeAreaLeft(-1); break; }
                if (_inputProcessors[i].MoveRight) { UpdateSafeAreaLeft(1); break; }
            }
        }

        private void UpdateSafeAreaSize(int direction)
        {
            Data.Profile.Settings.DisplaySafeAreaFraction = MathHelper.Clamp(
                Data.Profile.Settings.DisplaySafeAreaFraction + (direction * Boundary_Size_Change_Rate),
                0.0f,
                Data.PCSettings.Default_Display_Safe_Area_Fraction);

            SetBufferFrame();
        }

        private void UpdateSafeAreaLeft(int direction)
        {
            Data.Profile.Settings.DisplaySafeAreaTopLeft = new Vector2(
                MathHelper.Clamp(
                    Data.Profile.Settings.DisplaySafeAreaTopLeft.X + (direction * Boundary_Position_Change_Rate),
                    -Maximum_Left_Offset,
                    Maximum_Left_Offset),
                0.0f);

            SetBufferFrame();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (CurrentState == Status.Active) { DrawSafeAreaBoundaries(); }
        }

        private void DrawSafeAreaBoundaries()
        {
            Vector2 topLeft = new Vector2(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height) * Data.Profile.Settings.DisplaySafeAreaFraction;
            Vector2 bottomRight = new Vector2(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height) - topLeft;

            topLeft += Data.Profile.Settings.DisplaySafeAreaTopLeft;
            bottomRight += Data.Profile.Settings.DisplaySafeAreaTopLeft;

            SpriteBatch.Begin();
            RenderTools.Line(SpriteBatch, TextureManager.Textures["pixel"], topLeft, new Vector2(bottomRight.X, topLeft.Y), 1.0f, Color.White, 0.5f);
            RenderTools.Line(SpriteBatch, TextureManager.Textures["pixel"], topLeft, new Vector2(topLeft.X, bottomRight.Y), 1.0f, Color.White, 0.5f);
            RenderTools.Line(SpriteBatch, TextureManager.Textures["pixel"], bottomRight, new Vector2(bottomRight.X, topLeft.Y), 1.0f, Color.White, 0.5f);
            RenderTools.Line(SpriteBatch, TextureManager.Textures["pixel"], bottomRight, new Vector2(topLeft.X, bottomRight.Y), 1.0f, Color.White, 0.5f);
            SpriteBatch.End();
        }

        private const string Background_Texture_Name = "background-4";
        private const string Display_Content_Elements_File = "Content/Files/Content/{0}/display.xml";

        private const float Boundary_Size_Change_Rate = 0.001f;
        private const float Boundary_Position_Change_Rate = 2.0f;
        private const float Maximum_Left_Offset = 300.0f;
    }
}
