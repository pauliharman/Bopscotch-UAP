using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Asset_Management;

using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class CarouselAreaImage : CarouselFlatImage
    {
        private bool _locked;
        private float _lockRenderDepth;

        public override float RenderDepth { set { base.RenderDepth = value; _lockRenderDepth = value - Lock_Render_Depth_Offset; } }

        public CarouselAreaImage(string selectionName, string textureReference, bool locked)
            : base(selectionName, textureReference)
        {
            _locked = locked;
            if (locked) { Tint = Color.Gray; }
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (_locked)
            {
                spriteBatch.Draw(TextureManager.Textures[Lock_Texture], _position, null, Color.Lerp(Color.Black, Color.White, DistanceFadeModifier), 
                    0.0f, new Vector2(TextureManager.Textures[Lock_Texture].Width, TextureManager.Textures[Lock_Texture].Height) / 2.0f, 
                    base.Scale / Definitions.Background_Texture_Thumbnail_Scale, SpriteEffects.None, _lockRenderDepth);
            }
        }

        private const string Lock_Texture = "icon-locked";
        private const float Lock_Render_Depth_Offset = 0.00001f;
    }
}
