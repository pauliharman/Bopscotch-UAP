using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

using Bopscotch.Input;

// TODO: What happens if a controller is unplugged and permanently lost? IE: Controller 1 is unplugged while 2 and 3 are playing,
// 3 is then unplugged and plugs back in as 1 => 3 is permanently lost... need to transfer control or act accordingly

namespace Bopscotch.Interface.Dialogs
{
    public class ControllerUnpluggedDialog : ButtonDialog
    {
        private string _message;
        private bool _inputSourcesAreConnected;

        private bool AnyInputSourcesConnected
        {
            get
            {
                if (!Data.Profile.PlayingRaceMode) { return ControllerPool.Controllers.PlayerOne.IsAvailable; }
                else { return (ControllerPool.Controllers.PlayerOne.IsAvailable || ControllerPool.Controllers.PlayerTwo.IsAvailable); }
            }
        }

        public ControllerUnpluggedDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 320.0f), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);

            _defaultButtonCaption = "Back";
            _cancelButtonCaption = "Back";

            _boxCaption = "Controller Unplugged";
        }

        public void SetInputSourcesToPlayerControllers()
        {
            InputSources = new List<InputProcessorBase>();
            InputSources.Add(ControllerPool.Controllers.PlayerOne);

            if (Data.Profile.PlayingRaceMode) { InputSources.Add(ControllerPool.Controllers.PlayerTwo); }

            _inputSourcesAreConnected = AnyInputSourcesConnected;
        }

        public override void Activate()
        {
            ControllerPool.SetControllersToMenuMode();

            if (Data.Profile.PlayingRaceMode) { _message = Translator.Translation("Please make sure both controllers are plugged in"); }
            else { _message = Translator.Translation("Please make sure your controller is plugged in"); }

            base.Activate();
        }

        protected override void Dismiss()
        {
            base.Dismiss();

            ControllerPool.SetControllersToGameplayMode();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (_inputSourcesAreConnected != AnyInputSourcesConnected)
            {
                HandleConnectionStateChange();
                _inputSourcesAreConnected = AnyInputSourcesConnected;
            }
        }

        private void HandleConnectionStateChange()
        {
            if (AnyInputSourcesConnected) { SetInputSourcesToPlayerControllers(); }
            else { InputSources = ControllerPool.Controllers.All; }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            TextWriter.Write(_message, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 150.0f), Color.White, Color.Black, 3.0f, 0.65f,
                0.1f, TextWriter.Alignment.Center);
        }

        private const int Dialog_Height = 420;
        private const float Top_Y_When_Active = 400.0f;
    }
}
