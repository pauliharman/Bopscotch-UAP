using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;
using Bopscotch.Interface.Dialogs.RaceJoinScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceStartScene : StaticSceneBase
    {
        private PlayerTwoStartDialog _playerTwoStartDialog;
        private PlayerOneAvatarCarousel _playerOneAvatarCarousel;
        private PlayerTwoAvatarCarousel _playerTwoAvatarCarousel;
        private CourseSelectionCarouselDialog _areaCarousel;

        private AnimationController _animationController;

        public RaceStartScene()
            : base()
        {
            _backgroundTextureName = Background_Texture_Name;

            _animationController = new AnimationController();

            CreateSupportingDialogs();
        }

        private void CreateSupportingDialogs()
        {
            _playerTwoStartDialog = new PlayerTwoStartDialog() { ExitCallback = HandlePlayerTwoStartDialogAction };
            RegisterGameObject(_playerTwoStartDialog);
        }

        private void HandlePlayerTwoStartDialogAction(string buttonCaption)
        {
            switch (buttonCaption)
            {
                case "Back":
                    _playerOneAvatarCarousel.Cancel(); 
                    ReturnToTitleScene();
                    break;
                case "Start":
                    ControllerPool.SetPlayerTwoController(_playerTwoStartDialog.ActuatingController);
                    _playerTwoAvatarCarousel.Activate();
                    break;
            }
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);

            _playerOneAvatarCarousel = new PlayerOneAvatarCarousel(HandleAvatarSelectionAction, RegisterGameObject, UnregisterGameObject) 
                { ExitCallback = HandleAvatarSelectorDismissComplete };

            _playerTwoAvatarCarousel = new PlayerTwoAvatarCarousel(HandleAvatarSelectionAction, RegisterGameObject, UnregisterGameObject);

            _areaCarousel = new CourseSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject) 
                { 
                    ActionButtonPressHandler = HandleAreaSelectionAction,
                    ExitCallback = HandleAreaSelectorDismissComplete
                };
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            if (toUnregister is IAnimated) { _animationController.RemoveAnimatedObject((IAnimated)toUnregister); }
            base.RegisterGameObject(toUnregister);
        }

        private void HandleAvatarSelectionAction(string buttonCaption, AvatarSelectionCarouselDialog sender)
        {
            switch (buttonCaption)
            {
                case "Back":
                    DismissAvatarSelectors();
                    if (_playerTwoStartDialog.Active) { _playerTwoStartDialog.Cancel(); }
                    ReturnToTitleScene(); 
                    break;
                case "Select":
                    if (sender == _playerOneAvatarCarousel)
                    {
                        HandleAvatarSelection(RaceGameplayScene.Player_One_Avatar_Skin_Parameter, _playerOneAvatarCarousel.SelectedAvatarSkinSlot);
                    }
                    if (sender == _playerTwoAvatarCarousel)
                    {
                        HandleAvatarSelection(RaceGameplayScene.Player_Two_Avatar_Skin_Parameter, _playerTwoAvatarCarousel.SelectedAvatarSkinSlot);
                    }
                    break;
            }
        }

        private void ReturnToTitleScene()
        {
            NextSceneParameters.Clear();
            NextSceneParameters.Set("music-already-running", true);
            NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "start");
            NextSceneType = typeof(TitleScene);
            Deactivate();
        }

        protected override void CompleteDeactivation()
        {
            if (!NextSceneParameters.Get<bool>("music-already-running")) { MusicManager.StopMusic(); }
            base.CompleteDeactivation();
        }

        private void DismissAvatarSelectors()
        {
            _playerOneAvatarCarousel.Cancel();
            _playerTwoAvatarCarousel.Cancel();
        }

        private void HandleAvatarSelection(string parameterName, int parameterValue)
        {
            NextSceneParameters.Set(parameterName, parameterValue);

            if ((_playerOneAvatarCarousel.SelectedAvatarSkinSlot > -1) && (_playerTwoAvatarCarousel.SelectedAvatarSkinSlot > -1))
            {
                DismissAvatarSelectors();
            }
        }

        private void HandleAvatarSelectorDismissComplete(string buttonCaption)
        {
            if ((_playerOneAvatarCarousel.SelectedAvatarSkinSlot > -1) && (_playerTwoAvatarCarousel.SelectedAvatarSkinSlot > -1))
            { 
                _areaCarousel.Activate(); 
            }
        }

        private void HandleAreaSelectionAction(string buttonCaption)
        {
            if (buttonCaption == "Select")
            {
                var areaData = (from el in Data.Profile.SimpleAreaData where el.Attribute("name").Value == _areaCarousel.Selection select el).First();
                NextSceneParameters.Set(RaceGameplayScene.Course_Area_Parameter, _areaCarousel.Selection);
                NextSceneParameters.Set(RaceGameplayScene.Course_Speed_Parameter, (int)areaData.Attribute("speed"));

                NextSceneType = typeof(RaceGameplayScene);
            }

            _areaCarousel.DismissWithReturnValue(buttonCaption);
        }

        private void HandleAreaSelectorDismissComplete(string buttonCaption)
        {
            switch (buttonCaption)
            {
                case "Back": ReturnToTitleScene(); break;
                case "Select": Deactivate(); break;
            }
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            _playerOneAvatarCarousel.Activate();

            _playerTwoStartDialog.InputSources = ControllerPool.Controllers.AllButPlayerOne;
            _playerTwoStartDialog.Activate();

            _playerTwoAvatarCarousel.Visible = false;
            _areaCarousel.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _animationController.Update(MillisecondsSinceLastUpdate);

            _playerTwoStartDialog.Update(MillisecondsSinceLastUpdate);

            _playerOneAvatarCarousel.Update(MillisecondsSinceLastUpdate);
            _playerTwoAvatarCarousel.Update(MillisecondsSinceLastUpdate);
            _areaCarousel.Update(MillisecondsSinceLastUpdate);
        }

        protected override void HandleBackButtonPress()
        {
            if (_playerOneAvatarCarousel.Active) { DismissAvatarSelectors(); }
            if (_areaCarousel.Active) { _areaCarousel.DismissWithReturnValue(""); }

            ReturnToTitleScene(); 

            base.HandleBackButtonPress();
        }

        private const string Background_Texture_Name = "background-2";
    }
}
