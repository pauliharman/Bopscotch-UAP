using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Input;

namespace Bopscotch
{
    public class Game1 : GameBase
    {
        public Game1()
            : base(1440,900, true)
            //: base(1200, 675, false)
            //: base(1280, 768, false)
            //: base(1600, 900, false)
            //: base(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, true)
        {
            Guide.SimulateTrialMode = false;

            ControllerPool.CreateForGame(this);
            EnsureAllContentIsVisible = true;
        }

        protected override void Initialize()
        {
            Definitions.IsWideScreen = false;

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

            SceneTransitionCrossFadeTextureName = "pixel";

            SceneBackBufferArea = new Microsoft.Xna.Framework.Rectangle(0, 0, 1440,900);
            //SceneBackBufferArea = new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            //SceneBackBufferArea = new Microsoft.Xna.Framework.Rectangle(0, 0, 1280, 768);
            //SceneBackBufferArea = new Microsoft.Xna.Framework.Rectangle(0, 0, 1200, 675);
            
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
