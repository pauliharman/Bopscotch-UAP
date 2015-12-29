using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core;

using Bopscotch.Input;

namespace Bopscotch.Scenes.BaseClasses
{
    public abstract class StaticSceneBase : Scene
    {
        protected List<Input.InputProcessorBase> _inputProcessors;
        protected string _backgroundTextureName;

        public StaticSceneBase()
            : base(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            _inputProcessors = ControllerPool.Controllers.All;

            DoNotUseBackBuffer = false;
        }

        public override void Activate()
        {
            //SetBufferFrame();

            base.Activate();
        }

        protected void SetBufferFrame()
        {
            Vector2 topLeft = new Vector2(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height) * Data.Profile.Settings.DisplaySafeAreaFraction;
            Vector2 bottomRight = new Vector2(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height) - topLeft;

            topLeft += Data.Profile.Settings.DisplaySafeAreaTopLeft;
            bottomRight += Data.Profile.Settings.DisplaySafeAreaTopLeft;

            ScaledBufferFrame = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            RegisterGameObject(new Bopscotch.Gameplay.Objects.Environment.Background() { TextureReference = _backgroundTextureName });
        }
    }
}
