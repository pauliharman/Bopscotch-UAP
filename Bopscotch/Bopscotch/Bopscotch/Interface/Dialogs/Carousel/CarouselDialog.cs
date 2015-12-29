using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselDialog : ButtonDialog
    {
        public delegate void ActionHandler(string selectedButtonCaption);
        private float _rotationStep;
        private float _selectorRotation;
        private float _targetRotation;
        private Scene.ObjectRegistrationHandler _registerObject;
        private Scene.ObjectUnregistrationHandler _unregisterObject;
        private bool _carouselDisabled;

        protected List<ICarouselDialogItem> _items;
        protected Range _itemRenderDepths;
        protected Range _itemScales;

        public float TopYWhenInactive { private get; set; }
        public Vector2 CarouselCenter { private get; set; }
        public Vector2 CarouselRadii { private get; set; }
        public float RotationSpeedInDegrees { private get; set; }

        public ActionHandler ActionButtonPressHandler { private get; set; }

        public string Selection { get { return _items[SelectedItem].SelectionValue; } }

        protected int SelectedItem { get; private set; }
        protected bool Rotating { get; private set; }

        protected string AButtonPressedValue { private get; set; }
        protected string BButtonPressedValue { private get; set; }
        protected string XButtonPressedValue { private get; set; }
        protected string YButtonPressedValue { private get; set; }

        protected string DPadMessageText { private get; set; }
        protected float DPadMessageYOffset { private get; set; }

        public bool CarouselDisabled
        {
            get { return _carouselDisabled; }
            set { _carouselDisabled = value; foreach (ICarouselDialogItem i in _items) { i.Active = !value; } }
        }

        public override bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; if (_items != null) { for (int i = 0; i < _items.Count; i++) { _items[i].Visible = value; } } }
        }

        public CarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base()
        {
            SelectedItem = 0;
            Rotating = false;

            _selectorRotation = 0.0f;
            _targetRotation = _selectorRotation;
            _carouselDisabled = false;

            _items = new List<ICarouselDialogItem>();
            _registerObject = registrationHandler;
            _unregisterObject = unregistrationHandler;

            RotationSpeedInDegrees = Default_Rotation_Speed_In_Degrees;

            AButtonPressedValue = "Select";
            BButtonPressedValue = "Back";
            XButtonPressedValue = "";
            YButtonPressedValue = "";

            DPadMessageText = Default_DPad_Message_Text;
            DPadMessageYOffset = Default_DPad_Message_Y_Offset;

            _defaultButtonCaption = "";
            _cancelButtonCaption = "Back";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(WorldPosition.X, TopYWhenInactive);
        }

        protected void AddItem(ICarouselDialogItem item)
        {
            if (!_items.Contains(item))
            {
                item.CarouselCenter = CarouselCenter;
                item.CarouselRadii = CarouselRadii;
                item.DepthRange = _itemRenderDepths;
                item.ScaleRange = _itemScales;

                _items.Add(item);
                _registerObject(item);
            }
        }

        protected void FlushItems()
        {
            foreach (ICarouselDialogItem item in _items) { _unregisterObject(item); }
            _items.Clear();
        }

        protected void InitializeForSpin()
        {
            _rotationStep = MathHelper.TwoPi / _items.Count;

            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].AngleOffsetAtZeroRotation = MathHelper.PiOver2 + (i * _rotationStep);
            }
        }

        protected void SetInitialSelection(string selection)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].SelectionValue == selection) { SetInitialSelection(i); break; }
            }
        }

        protected void SetInitialSelection(int selection)
        {
            SelectedItem = selection;
            _targetRotation = -(_rotationStep * selection);
            _selectorRotation = -(_rotationStep * selection);
        }

        private void UpdateItemPositions()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].PositionRelativeToDialog(WorldPosition, _selectorRotation);
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (Rotating) { UpdateRotation(); }

            UpdateItemPositions();
        }

        protected override void CheckForAndHandleInputFromSingleSource(InputProcessorBase inputSource, int millisecondsSinceLastUpdate)
        {
            if (!Rotating)
            {
                if ((!_carouselDisabled) && (inputSource.MoveLeft)) { _targetRotation += _rotationStep; Rotating = true; }
                else if ((!_carouselDisabled) && (inputSource.MoveRight)) { _targetRotation -= _rotationStep; Rotating = true; }

                if (Rotating) { SoundEffectManager.PlayEffect("carousel-spin"); }
            }

            if (!Rotating)
            {
                CheckMasterSelectActivation(inputSource);

                CheckForButtonAction(inputSource.BButtonPressed, BButtonPressedValue);
                CheckForButtonAction(inputSource.XButtonPressed, XButtonPressedValue);
                CheckForButtonAction(inputSource.YButtonPressed, YButtonPressedValue);
            }
        }

        protected virtual void CheckMasterSelectActivation(InputProcessorBase inputSource)
        {
            CheckForButtonAction(inputSource.AButtonPressed, AButtonPressedValue);
            CheckForButtonAction(inputSource.StartButtonPressed, AButtonPressedValue);
        }

        private void CheckForButtonAction(bool buttonIsPressed, string actionValue)
        {
            if ((buttonIsPressed) && (!string.IsNullOrEmpty(actionValue)))
            {
                _activeButtonCaption = actionValue;
                ActionButtonPressHandler(_activeButtonCaption);
                if (!string.IsNullOrEmpty(ActivateSelectionSoundEffectName)) { SoundEffectManager.PlayEffect(ActivateSelectionSoundEffectName); }
            }
        }

        private void UpdateRotation()
        {
            if (StepWillEndRotation) { HandleRotationComplete(); }
            else { _selectorRotation += (MathHelper.ToRadians(RotationSpeedInDegrees) * Math.Sign(_targetRotation - _selectorRotation)); }
        }

        private bool StepWillEndRotation
        {
            get
            {
                if ((_targetRotation < _selectorRotation) && (_targetRotation >= _selectorRotation - MathHelper.ToRadians(RotationSpeedInDegrees)))
                {
                    return true;
                }
                else if ((_targetRotation > _selectorRotation) && (_targetRotation <= _selectorRotation + MathHelper.ToRadians(RotationSpeedInDegrees)))
                {
                    return true;
                }

                return false;
            }
        }

        protected virtual void HandleRotationComplete()
        {
            SelectedItem = (SelectedItem - Math.Sign(_targetRotation - _selectorRotation) + _items.Count) % _items.Count;
            _targetRotation = Utility.RectifyAngle(_targetRotation);

            if ((_targetRotation < Rotation_Zero_Point_Tolerance) || (_targetRotation > MathHelper.TwoPi - Rotation_Zero_Point_Tolerance))
            {
                _targetRotation = 0.0f;
            }

            _selectorRotation = _targetRotation;
            Rotating = false;
        }

        public void ClearLastSelection()
        {
            SelectedItem = 0;
            _selectorRotation = 0.0f;
            _targetRotation = 0.0f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawDpadMessage(spriteBatch);
        }

        protected virtual void DrawDpadMessage(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(DPadMessageText))
            {
                Vector2 textCenter = CarouselCenter + new Vector2(0.0f, WorldPosition.Y + DPadMessageYOffset);
                float buttonIconSpaceWidth = ((TextureManager.Textures[DPad_Texture].Bounds.Width / 2.0f) * DPad_Message_Scale) + Dpad_Text_X_Margin;
                textCenter.X += (buttonIconSpaceWidth / 2.0f);

                TextWriter.Write(DPadMessageText, spriteBatch,textCenter, Color.White, Color.Black, 3.0f, DPad_Message_Scale, 0.05f, TextWriter.Alignment.Center);

                textCenter.X -= ((TextWriter.LastTextArea.Width / 2.0f) + buttonIconSpaceWidth);
                spriteBatch.Draw(
                    TextureManager.Textures[DPad_Texture],
                    textCenter + new Vector2(0.0f, 10.0f),
                    new Rectangle(0, 0, TextureManager.Textures[DPad_Texture].Bounds.Width / 2, TextureManager.Textures[DPad_Texture].Bounds.Height),
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    DPad_Message_Scale,
                    SpriteEffects.None,
                    0.05f);
            }
        }

        private const float Default_Rotation_Speed_In_Degrees = 5.0f;
        private const float Rotation_Zero_Point_Tolerance = 0.0001f;

        private const string Default_DPad_Message_Text = "Change selection";
        private const float Default_DPad_Message_Y_Offset = 80.0f;

        protected const string DPad_Texture = "icon-dpad";
        protected const float DPad_Message_Scale = 0.65f;
        protected const float Dpad_Text_X_Margin = 10.0f;
    }
}
