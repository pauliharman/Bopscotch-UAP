//using System;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using Leda.Core;
//using Leda.Core.Gamestate_Management;
//using Leda.Core.Asset_Management;
//using Leda.Core.Game_Objects.Controllers;

//using Bopscotch.Scenes.RaceMode;

//namespace Bopscotch.Tests
//{
//    public class RaceTestScene : Scene
//    {
//        private SinglePlayerSubScene _playerOneSubScene;
//        private SinglePlayerSubScene _playerTwoSubScene;


//        public RaceTestScene()
//            : base(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
//        {
//            Data.GlobalData.CurrentGame.SetForNewGame();

//            _playerOneSubScene = new SinglePlayerSubScene(Point.Zero);
//            RegisterGameObject(_playerOneSubScene);

//            _playerTwoSubScene = new SinglePlayerSubScene(Point.Zero);
//            RegisterGameObject(_playerTwoSubScene);
//        }

//        public override void Initialize()
//        {
//            base.Initialize();

//            Rectangle playerOneDisplayArea = new Rectangle(0, 0, ScaledBufferFrame.Width + (ScaledBufferFrame.X * 2), ScaledBufferFrame.Height / 2);
//            playerOneDisplayArea.X = (Definitions.Back_Buffer_Width - playerOneDisplayArea.Width) / 2;
//            playerOneDisplayArea.Y = Split_Margin;
//            _playerOneSubScene.DisplayArea = playerOneDisplayArea;

//            Rectangle playerTwoDisplayArea = new Rectangle(0, 0, ScaledBufferFrame.Width + (ScaledBufferFrame.X * 2), ScaledBufferFrame.Height / 2);
//            playerTwoDisplayArea.X = (Definitions.Back_Buffer_Width - playerTwoDisplayArea.Width) / 2;
//            playerTwoDisplayArea.Y = playerOneDisplayArea.Y + playerOneDisplayArea.Height + Split_Margin;
//            _playerTwoSubScene.DisplayArea = playerTwoDisplayArea;

//        }

//        public override void Activate()
//        {
//            base.Activate();
//            _playerOneSubScene.Activate();
//            _playerTwoSubScene.Activate();
//        }

//        public override void Update(GameTime gameTime)
//        {
//            base.Update(gameTime);
            
//            _playerOneSubScene.Update(MillisecondsSinceLastUpdate, SpriteBatch);
//            _playerTwoSubScene.Update(MillisecondsSinceLastUpdate, SpriteBatch);
//        }

//        protected override void HandleBackButtonPress()
//        {
//            Game.Exit();
//        }

//        private const int Split_Margin = 10;
//    }
//}
