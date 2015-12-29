using System.Xml.Linq;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Serialization;

namespace Bopscotch.Tests
{
    public class TestSkeleton : StorableSkeleton, IAnimated
    {
        private SkeletalAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public TestSkeleton()
            : base()
        {
            _animationEngine = new SkeletalAnimationEngine(this);
        }

        public void Update()
        {
            Bones["b2"].RelativeRotation += 0.02f;
            Bones["b3"].RelativeRotation += 0.04f;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("animation-engine", _animationEngine);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(_animationEngine);

            base.Deserialize(serializer);

            _animationEngine = serializer.GetDataItem<SkeletalAnimationEngine>("animation-engine");

            return serializer;
        }
    }
}
