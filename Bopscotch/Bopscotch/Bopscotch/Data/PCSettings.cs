using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

using Bopscotch.Input;

namespace Bopscotch.Data
{
    public sealed class PCSettings : UniversalSettings
    {
        public float DisplaySafeAreaFraction { get; set; }
        public Vector2 DisplaySafeAreaTopLeft { get; set; }

        public PCSettings()
            : base()
        {
            //DisplaySafeAreaTopLeft = new Vector2((Game1.Instance.GraphicsDevice.Viewport.Width - Definitions.Back_Buffer_Width) / 2.0f, 0.0f);
            //DisplaySafeAreaFraction = //1.0f - ((float)Game1.Instance.GraphicsDevice.Viewport.Width / (float)Definitions.Back_Buffer_Width);
            //DisplaySafeAreaTopLeft = -(new Vector2(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height) * DisplaySafeAreaFraction);

            DisplaySafeAreaTopLeft = Vector2.Zero;
            DisplaySafeAreaFraction = 0.1f;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("safe-area-fraction", DisplaySafeAreaFraction);
            serializer.AddDataItem("safe-area-top-left", DisplaySafeAreaTopLeft);

            return serializer.SerializedData;
        }

        protected override void Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            DisplaySafeAreaFraction = serializer.GetDataItem<float>("safe-area-fraction");
            DisplaySafeAreaTopLeft = serializer.GetDataItem<Vector2>("safe-area-top-left");
        }

        private string _identity;
        public string Identity
        {
            get
            {
                if (string.IsNullOrEmpty(_identity))
                {
                    _identity = "TODO: Sort out Identity for PC";
                }
                return _identity;
            }
        }

        public const float Default_Display_Safe_Area_Fraction = 0.0f;//0.15f;
    }
}
