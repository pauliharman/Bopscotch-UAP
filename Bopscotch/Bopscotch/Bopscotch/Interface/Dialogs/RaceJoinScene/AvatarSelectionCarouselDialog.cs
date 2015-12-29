using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Data.Avatar;
using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;
using Bopscotch.Scenes.Gameplay.Race;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class AvatarSelectionCarouselDialog : CarouselDialog
    {
        private RaceStartScene.AvatarSelectionButtonHandler _sendButtonActionToScene;
        private InputProcessorBase _inputProcessor;

        public string SelectedSkin { get; private set; }

        public AvatarSelectionCarouselDialog(InputProcessorBase inputProcessor, RaceStartScene.AvatarSelectionButtonHandler sceneButtonHandler,
            Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            ActionButtonPressHandler = SelectionHandler;
            InputSources.Add(inputProcessor);

            Height = Dialog_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            _inputProcessor = inputProcessor;
            _sendButtonActionToScene = sceneButtonHandler;
            _captionsForButtonsNotActivatedByGamepadStartButton.Add("Back");

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 325), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);
            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 325), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);
        }

        private void SelectionHandler(string buttonCaption)
        {
            if (buttonCaption == "Select") 
            { 
                SelectedSkin = Selection; 
                SetNonBackButtonsEnabledState(false);

                ActivateButton("Back");
            }

            _sendButtonActionToScene(buttonCaption, this);
        }

        private void SetNonBackButtonsEnabledState(bool enableButtons)
        {
            _buttons["Select"].Disabled = !enableButtons;
            _buttons["previous"].Disabled = !enableButtons;
            _buttons["next"].Disabled = !enableButtons;
        }

        public override void Activate()
        {
            FlushItems();

            base.Activate();

            AddAvatar("player-dusty");
            AddAvatar("player-dustin");
            AddAvatar("player-dustina");
            AddAvatar("player-dasuto");

            SelectedSkin = "";
            SetNonBackButtonsEnabledState(true);
            ActivateButton("Select");

            InitializeForSpin();
        }

        private void AddAvatar(string skinName)
        {
            CarouselAvatar avatar = new CarouselAvatar(skinName);
            avatar.CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Front);
            avatar.SkinBones(SkeletonDataManager.Skins[string.Concat(skinName, "-dialog")]);
            avatar.RenderLayer = RenderLayer;
            avatar.StartRestingAnimationSequence();

            AddItem(avatar);
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            _inputProcessor.Update(millisecondsSinceLastUpdate);
            base.Update(millisecondsSinceLastUpdate);
        }

        private const float Carousel_Center_Y = 160.0f;
        private const float Carousel_Horizontal_Radius = 300.0f;
        private const float Carousel_Vertical_Radius = 50.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        protected const int Dialog_Height = 400;
        protected const float Dialog_Margin = 30.0f;
    }
}
