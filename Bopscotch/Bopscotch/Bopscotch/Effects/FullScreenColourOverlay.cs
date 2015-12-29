using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Effects
{
    public class FullScreenColourOverlay : IGameObject, ISimpleRenderable
    {
        public bool Visible { get { return true; } set { } }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public Color Tint { private get; set; }
        public float TintFraction { private get; set; }

        public FullScreenColourOverlay()
        {
            Tint = Color.White;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures["pixel"], new Rectangle(0, 0, Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height),
                null, Color.Lerp(Color.Transparent, Tint, TintFraction));
        }

        private const int Render_Layer = 0;
    }
}
