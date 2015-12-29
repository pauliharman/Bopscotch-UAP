using System;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Motion;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselSkeleton : DisposableSkeleton, ICarouselDialogItem
    {
        private float _distanceFader;
        private Color _tint;

        public string SelectionValue { get; private set; }
        public float AngleOffsetAtZeroRotation { private get; set; }
        public Vector2 CarouselCenter { private get; set; }
        public Vector2 CarouselRadii { private get; set; }
        public Range DepthRange { private get; set; }
        public Range ScaleRange { private get; set; }
        public new Color Tint { protected get { return _tint; } set { _tint = value; base.Tint = Color.Lerp(Color.Black, _tint, 0.5f + _distanceFader); } }

        public CarouselSkeleton(string selectionValue)
            : base()
        {
            SelectionValue = selectionValue;

            _distanceFader = 0.0f;
            _tint = Color.White;
        }

        public void PositionRelativeToDialog(Vector2 carouselPosition, float carouselRotation)
        {
            Vector2 relativePosition = CarouselCenter + new Vector2(
                (float)(CarouselRadii.X * Math.Cos(carouselRotation + AngleOffsetAtZeroRotation)),
                (float)(CarouselRadii.Y * Math.Sin(carouselRotation + AngleOffsetAtZeroRotation)));
            WorldPosition = carouselPosition + relativePosition;

            float distanceFromApex = relativePosition.Y - (CarouselCenter.Y - CarouselRadii.Y);

            RenderDepth = DepthRange.Maximum - ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * DepthRange.Interval);
            Scale = ScaleRange.Minimum + ((distanceFromApex / (CarouselRadii.Y * 2.0f)) * ScaleRange.Interval);

            _distanceFader = (distanceFromApex / (CarouselRadii.Y * 2.0f)) * 0.5f;
            base.Tint = Color.Lerp(Color.Black, _tint, 0.5f + _distanceFader);
        }
    }
}
