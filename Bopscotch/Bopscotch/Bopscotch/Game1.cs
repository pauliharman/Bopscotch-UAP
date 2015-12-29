using System;

using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Input;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace Bopscotch
{
    public class Game1 : GameBase
    {
        public Game1()
            : base(Orientation.Landscape)
        {
            ControllerPool.CreateForGame(this);
            EnsureAllContentIsVisible = true;
        }

        protected override void Initialize()
        {
            AddScene(new Scenes.NonGame.LoadingScene());
            AddScene(new Scenes.NonGame.TitleScene());
            AddScene(new Scenes.NonGame.CreditsScene());
            AddScene(new Scenes.NonGame.DisplayCalibrationScene());
            AddScene(new Scenes.NonGame.AvatarCustomisationScene());
            AddScene(new Scenes.Gameplay.Survival.SurvivalGameplayScene());
            AddScene(new Scenes.Gameplay.Survival.SurvivalAreaCompleteScene());
            AddScene(new Scenes.Gameplay.Race.RaceStartScene());
            AddScene(new Scenes.Gameplay.Race.RaceGameplayScene());
            AddScene(new Scenes.Gameplay.Race.RaceFinishScene());

            base.Initialize();

            DisplayInformation di = DisplayInformation.GetForCurrentView();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isFullScreen = view.TryEnterFullScreenMode();
            Rect screenSize = view.VisibleBounds;

            int screenWidth = (int)(screenSize.Width * di.RawPixelsPerViewPixel);
            int width = (int)((screenSize.Height * di.RawPixelsPerViewPixel * 16.0) / 9.0);
            int screenHeight = (int)(screenSize.Height * di.RawPixelsPerViewPixel);

            SetResolutionMetrics(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height, ScalingAxis.X, screenWidth, screenHeight);

            SceneTransitionCrossFadeTextureName = "pixel";

            StartInitialScene(typeof(Scenes.NonGame.LoadingScene));
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            TextureManager.AddTexture("leda-logo", Content.Load<Texture2D>("Textures\\leda-logo"));
            TextureManager.AddTexture("pixel", Content.Load<Texture2D>("Textures\\WhitePixel"));
            TextureManager.AddTexture("load-spinner", Content.Load<Texture2D>("Textures\\load-spinner"));
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            MusicManager.StopMusic();

            base.OnExiting(sender, args);
        }

        private const float Widescreen_Ratio_Threshold = 1.5f;
    }
}
