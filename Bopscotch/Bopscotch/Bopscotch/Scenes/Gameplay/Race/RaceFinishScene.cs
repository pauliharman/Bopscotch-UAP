using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Interface.Dialogs.RaceFinishScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceFinishScene : MenuDialogScene
    {
        private AnimationController _animationController;

        public RaceFinishScene()
            : base()
        {
            _animationController = new AnimationController();

            _dialogs.Add("results", new ResultsDialog(RegisterGameObject) { ExitCallback = ReturnToTitleScene } );

            _backgroundTextureName = Background_Texture_Name;
        }

        private void ReturnToTitleScene(string buttonCaption)
        {
            NextSceneParameters.Clear();
            NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "start");
            NextSceneType = typeof(TitleScene);
            Deactivate();
        }

        public override void HandleAssetLoadCompletion(System.Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);

            ((ResultsDialog)_dialogs["results"]).InitializeComponents();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            base.Activate();

            ((ResultsDialog)_dialogs["results"]).Outcome = NextSceneParameters.Get<Definitions.RaceOutcome>(Outcome_Parameter_Name);
            ((ResultsDialog)_dialogs["results"]).PlayerOneSkinSlotIndex = NextSceneParameters.Get<int>(RaceGameplayScene.Player_One_Avatar_Skin_Parameter);
            ((ResultsDialog)_dialogs["results"]).PlayerTwoSkinSlotIndex = NextSceneParameters.Get<int>(RaceGameplayScene.Player_Two_Avatar_Skin_Parameter);
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            ActivateDialog("results");
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);

            base.Update(gameTime);
        }

        private const string Background_Texture_Name = "background-2";

        public const string Outcome_Parameter_Name = "race-outcome";
    }
}