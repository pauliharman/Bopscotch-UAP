using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;

using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class CourseSelectionCarouselDialog : AreaSelectionCarouselDialog
    {
        public CourseSelectionCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Course");

            Height = Dialog_Height;
            TopYWhenInactive = Definitions.Back_Buffer_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);

            DPadMessageYOffset = 230.0f;

            registrationHandler(this);

            InputSources = new List<InputProcessorBase>();
            InputSources.Add(null);
            InputSources.Add(null);
        }

        protected override void CreateButtons()
        {
            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 625), Button.ButtonIcon.B, Color.Red, 0.7f);
            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 625), Button.ButtonIcon.A, Color.LawnGreen, 0.7f);

            base.CreateButtons();
        }

        public override void Activate()
        {
            base.Activate();

            InputSources[0] = ControllerPool.Controllers.PlayerOne;
            InputSources[1] = ControllerPool.Controllers.PlayerTwo;

            ActivateButton("Select");
        }

        protected override void GetAreaData()
        {
            base.GetAreaData();

            for (int i = _dataSource.Count - 1; i >= 0; i--)
            {
                if ((bool)_dataSource[i].Attribute("no-race")) { _dataSource.RemoveAt(i); }
                else { _dataSource[i].SetAttributeValue("locked", false); }
            }
        }

        protected override void DrawAreaDetails(SpriteBatch spriteBatch)
        {
            string areaText = _dataSource[SelectedItem].Attribute("name").Value;

            if ((bool)_dataSource[SelectedItem].Attribute("locked"))
            {
                areaText = string.Concat(areaText, " (", Translator.Translation("locked"), ")");
            }

            TextWriter.Write(areaText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 425.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.1f, TextWriter.Alignment.Center);
        }

        protected override void HandleRotationComplete()
        {
            base.HandleRotationComplete();

            for (int i = 0; i < _dataSource.Count; i++)
            {
                if (_dataSource[i].Attribute("name").Value == Selection) 
                { 
                    _buttons["Select"].Disabled = (bool)_dataSource[i].Attribute("locked"); 
                }
            }
        }

        private const float Carousel_Center_Y = 280.0f;
        private const float Carousel_Horizontal_Radius = 290.0f;
        private const float Carousel_Vertical_Radius = 90.0f;
        private const int Dialog_Height = 700;
    }
}
