
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsSkeleton : DisposableSkeleton, IAnimated
    {
        private Vector2 _baseWorldPosition;

        private SkeletalAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public float ParentDialogY { set { _baseWorldPosition.Y = value + Offset_From_Dialog_Top; } }
        
        public ResultsSkeleton(float offsetFromCenter)
            : base()
        {
            _baseWorldPosition = new Vector2(Definitions.Back_Buffer_Center.X + offsetFromCenter, 0.0f);

            _animationEngine = new SkeletalAnimationEngine(this);

            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;
            Visible = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            WorldPosition = _baseWorldPosition;

            base.Draw(spriteBatch);
        }

        private const float Offset_From_Dialog_Top = 400.0f;
        private const int Render_Layer = 4;
        private const float Render_Depth = 0.05f;
    }
}
