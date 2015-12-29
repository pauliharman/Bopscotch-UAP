using System;

using Microsoft.Xna.Framework;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Input;

namespace Bopscotch.Scenes.Gameplay.Survival
{
    public class SurvivalGameplayScene : StorableScene
    {
        private SurvivalSubScene _gameplayContainer;

        public SurvivalGameplayScene()
            : base("survival-play-scene", Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            _gameplayContainer = new SurvivalSubScene();
            _gameplayContainer.DeactivationHandler = SubsceneDeactivationHandler;
            RegisterGameObject(_gameplayContainer);
        }

        public void SubsceneDeactivationHandler(Type nextSceneType)
        {
            if (nextSceneType != this.GetType()) { Data.Profile.PauseOnSceneActivation = false; }
            NextSceneType = nextSceneType;
            Deactivate();
        }

        protected override void CompleteDeactivation()
        {
            if (_nextSceneType != typeof(SurvivalGameplayScene)) { MusicManager.StopMusic(); ControllerPool.SetControllersToMenuMode(); }
            base.CompleteDeactivation();
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);
            InitializeGameObjects();
        }

        public override void Activate()
        {
            _gameplayContainer.BufferArea = CreateDisplayArea();
            _gameplayContainer.CameraOverspillMargin = Vector2.Zero;
            _gameplayContainer.SafeAreaOuterLimits = new Rectangle(0, 0, Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height);

            ControllerPool.SetControllersToGameplayMode();

            base.Activate();
            _gameplayContainer.Activate();
        }

        private Rectangle CreateDisplayArea()
        {
            //float x = Definitions.Back_Buffer_Width * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float y = Definitions.Back_Buffer_Height * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float width = (Definitions.Back_Buffer_Width * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - x;
            //float height = (Definitions.Back_Buffer_Height * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - y;

            //return new Rectangle((int)(x + Data.Profile.Settings.DisplaySafeAreaTopLeft.X), (int)(y + Data.Profile.Settings.DisplaySafeAreaTopLeft.Y), (int)width, (int)height);

            return ScaledBufferFrame;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _gameplayContainer.Update(MillisecondsSinceLastUpdate);
        }

        protected override void BeginRender()
        {
            _gameplayContainer.RenderContentToBackBuffer(SpriteBatch);

            base.BeginRender();
        }

        protected override void HandleBackButtonPress()
        {
            _gameplayContainer.SceneIsDeactivating = (CurrentState == Status.Deactivating);
            _gameplayContainer.HandlebackButtonPress();
        }
    }
}
