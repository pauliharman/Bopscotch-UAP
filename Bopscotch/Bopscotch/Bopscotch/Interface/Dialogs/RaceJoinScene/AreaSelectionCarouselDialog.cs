using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;

using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class AreaSelectionCarouselDialog : CarouselDialog
    {
        public AreaSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Course");

            Height = Dialog_Height;
            TopYWhenInactive = Definitions.Back_Buffer_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            RotationSpeedInDegrees = Rotation_Speed_In_Degrees;
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            InputSources.Add(KeyboardInputProcessor.CreateForPlayerOne());
            InputSources.Add(KeyboardInputProcessor.CreateForPlayerTwo());

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 305), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 305), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 625), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);
            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 625), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);

            _cancelButtonCaption = "Back";
        }

        public override void Activate()
        {
            FlushItems();
            Visible = true;

            base.Activate();

            AddArea("Hilltops");
            AddArea("Tutorial");

            ActivateButton("Select");
            InitializeForSpin();
        }

        private void AddArea(string areaName)
        {
            CarouselFlatImage area = new CarouselFlatImage(areaName, Data.Profile.AreaSelectionTexture(areaName));
            area.RenderLayer = RenderLayer;
            area.MasterScale = Definitions.Background_Texture_Thumbnail_Scale;

            AddItem(area);
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < InputSources.Count; i++) { InputSources[i].Update(millisecondsSinceLastUpdate); }
            base.Update(millisecondsSinceLastUpdate);
        }

        protected override void HandleDialogExitCompletion()
        {
            FlushItems();
            base.HandleDialogExitCompletion();
        }

        private const float Carousel_Center_Y = 280.0f;
        private const float Carousel_Horizontal_Radius = 275.0f;
        private const float Carousel_Vertical_Radius = 90.0f;
        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;
        private const int Dialog_Height = 700;
        private const float Rotation_Speed_In_Degrees = 10.0f;
    }
}
