using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Animation;
using Leda.Core.Animation.Skeletons;

using Bopscotch.Interface.Dialogs.SurvivalGameplayScene;

namespace Bopscotch.Tests
{
    public class TestScene : StorableScene
    {
        private PauseDialog _pauseDialog;

        private MotionController _motionController;

        public TestScene()
            : base("Test-Scene", Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            _pauseDialog = new PauseDialog();
            RegisterGameObject(_pauseDialog);

            _motionController = new MotionController();
            _motionController.AddMobileObject(_pauseDialog);
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            _pauseDialog.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _motionController.Update(MillisecondsSinceLastUpdate);
        }

        protected override void Render()
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(TextureManager.Textures["background-1"], Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.9f);
            SpriteBatch.End();

            base.Render();

            SpriteBatch.Begin();
            TextWriter.Write("Testing...", SpriteBatch, Vector2.Zero, Color.White, 0.1f, TextWriter.Alignment.Left);
            SpriteBatch.End();
        }

        protected override void HandleBackButtonPress()
        {
            Game.Exit();
        }
    }
}
