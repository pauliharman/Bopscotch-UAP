//using System;
//using System.Xml.Linq;

//using Microsoft.Xna.Framework;

//using Leda.Core.Game_Objects.Base_Classes;
//using Leda.Core.Game_Objects.Behaviours;
//using Leda.Core.Asset_Management;
//using Leda.Core.Serialization;
//using Leda.Core.Motion;
//using Leda.Core.Shapes;
//using Leda.Core.Timing;
//using Leda.Core;

//using Bopscotch.Gameplay.Objects.Environment.Blocks;
//using Bopscotch.Gameplay.Objects.Environment.Signposts;
//using Bopscotch.Gameplay.Objects.Environment.Flags;

//namespace Bopscotch.Gameplay.Objects.Characters.Player
//{
//    public class Player : Base.Character, ICircularCollidable
//    {
//        public delegate void PlayerEventHandler();

//        private bool _hasLandedOnBlock;
//        private bool _didNotLandSafely;
//        private bool _hasTouchedGoalFlag;
//        private bool _hasAlreadyTouchedGoalFlag;

//        private PlayerMotionEngine _motionEngine;
//        private PlayerEvent _lastEvent;

//        private Timer _sequenceTimer;
//        public TimerController.TickCallbackRegistrationHandler TickCallback { set { value(_sequenceTimer.Tick); } }

//        public override bool Mirror { get { return base.Mirror; } set { base.Mirror = value; if (_motionEngine != null) { _motionEngine.MovingLeft = value; } } }
//        public bool IsDead { get; private set; }
//        public bool IsMovingLeft { get { return _motionEngine.MovingLeft; } }
//        public bool HorizontalMovementBlocked { get { return _motionEngine.HorizontalMovementBlocked; } }
//        public PlayerEventHandler PlayerEventCallback { private get; set; }

//        public Circle CollisionBoundingCircle { get; private set; }

//        public Input.InputProcessorBase InputProcessor { set { _motionEngine.InputProcessor = value; } }
//        public Display.Speedometer Speedometer { set { _motionEngine.Speedometer = value; } }
//        public PlayerEvent LastEvent 
//        { 
//            get { return _lastEvent; }
//            private set { _lastEvent = value; PlayerEventCallback(); } 
//        }

//        public Player()
//            : base()
//        {
//            ID = "player";
//            PlayerEventCallback = null;
//            _lastEvent = PlayerEvent.None;

//            RenderLayer = Render_Layer;
//            RenderDepth = Render_Depth;

//            CollisionBoundingCircle = new Circle(Vector2.Zero, Body_Collision_Radius);
//            Collidable = true;
//            Visible = true;

//            _motionEngine = new PlayerMotionEngine();
//            MotionEngine = _motionEngine;

//            _sequenceTimer = new Timer("player-sequence-timer", TimedSequenceCompletionHandler);

//            _hasTouchedGoalFlag = false;
//            _motionEngine.HorizontalMovementBlocked = true;
//            ResetCollisionFlags();
//            IsDead = false;
//            LifeCycleState = LifeCycleStateValue.Entering;
//        }

//        private void TimedSequenceCompletionHandler()
//        {
//            switch (LifeCycleState)
//            {
//                case LifeCycleStateValue.Entering:
//                    _motionEngine.HorizontalMovementBlocked = false;
//                    LifeCycleState = LifeCycleStateValue.Active;
//                    break;
//                case LifeCycleStateValue.Exiting:
//                    if (Data.Profile.PlayingRaceMode) { LastEvent = PlayerEvent.Resurrected; }
//                    break;
//            }
//        }

//        private void ResetCollisionFlags()
//        {
//            _hasLandedOnBlock = false;
//            _didNotLandSafely = true;

//            _hasAlreadyTouchedGoalFlag = _hasTouchedGoalFlag;
//            _hasTouchedGoalFlag = false;
//        }

//        public override void Update(int millisecondsSinceLastUpdate)
//        {
//            switch (LifeCycleState)
//            {
//                case LifeCycleStateValue.Active: CheckAndHandleBadLandingLastUpdate(); break;
//            }

//            base.Update(millisecondsSinceLastUpdate);

//            UpdateMotionAnimationSequence();
//            CheckAndHandleGoalPassed();
//            ResetCollisionFlags();
//        }

//        private void CheckAndHandleBadLandingLastUpdate()
//        {
//            if ((_hasLandedOnBlock) && (_didNotLandSafely)) { StartDeathSequence(); }
//        }

//        private void StartDeathSequence()
//        {
//            IsDead = true;
//            LifeCycleState = Leda.Core.LifeCycleStateValue.Exiting;
//        }

//        protected override void StartExitSequence()
//        {
//            if (IsDead)
//            {
//                _motionEngine.PlayerIsDead = true;

//                LastEvent = PlayerEvent.Died;
//                Visible = false;
//                Collidable = false;

//                if (Data.Profile.PlayingRaceMode) { _sequenceTimer.NextActionDuration = Race_Death_Sequence_Duration_In_Milliseconds; }
//            }
//            else
//            {
//                _motionEngine.LevelCleared = true;
//            }
//        }

//        private void UpdateMotionAnimationSequence()
//        {
//            if (_motionEngine.VerticalDirectionChanged)
//            {
//                if (_motionEngine.Delta.Y > 0.0f) { AnimationEngine.Sequence = AnimationDataManager.Sequences["player-fall"]; }
//                //else if () {}     // TODO: Switch to end level sequence if level cleared
//                else { AnimationEngine.Sequence = AnimationDataManager.Sequences["player-jump"]; }
//            }
//        }

//        private void CheckAndHandleGoalPassed()
//        {
//            if ((_hasAlreadyTouchedGoalFlag) & (!_hasTouchedGoalFlag))
//            {
//                LastEvent = PlayerEvent.Goal_Passed;
//                if (!Data.Profile.PlayingRaceMode) { StartExitSequence(); }
//            }
//        }

//        public override void HandleCollision(ICollidable collider)
//        {
//            if (collider is Block) { HandleBlockCollision((Block)collider); }
//            if (collider is SignpostBase) { HandleSignpostCollision((SignpostBase)collider); }
//            if (collider is Flag) { HandleFlagCollision((Flag)collider); }
//        }

//        private void HandleBlockCollision(Block collidingBlock)
//        {
//            if (collidingBlock is SpikeBlock) { StartDeathSequence(); }
//            else if (collidingBlock is SmashBlock) { HandleSmashBlockCollision((SmashBlock)collidingBlock); }
//            else { HandleSolidBlockCollision(collidingBlock); }
//        }

//        private void HandleSmashBlockCollision(SmashBlock collidingSmashBlock)
//        {
//            if (_motionEngine.VerticalMovementCanSmash) { collidingSmashBlock.HandleSmash(); }
//            else { HandleSolidBlockCollision(collidingSmashBlock); }
//        }

//        private void HandleSolidBlockCollision(Block collidingBlock)
//        {
//            _hasLandedOnBlock = true;

//            if (collidingBlock.HasBeenLandedOnSquarely(WorldPosition))
//            {
//                _motionEngine.PlayerHasHitGround = true;
//                _didNotLandSafely = false;

//                if (_motionEngine.Delta.Y > 0)
//                {
//                    WorldPosition -= new Vector2(0.0f, WorldPosition.Y - (collidingBlock.TopSurfaceY - Body_Collision_Radius));
//                    if (collidingBlock is SpringBlock) { _motionEngine.SetForSpringLaunch(); }
//                }
//            }
//        }

//        private void HandleSignpostCollision(SignpostBase collider)
//        {
//            if (collider is OneWaySignpost) { HandleOneWaySignpostCollision((OneWaySignpost)collider); }
//        }

//        private void HandleOneWaySignpostCollision(OneWaySignpost collider)
//        {
//            if ((collider.Mirror != _motionEngine.MovingLeft) && (IsHorizontallyCloseEnoughForEffect(collider, Signpost_Effect_Distance)))
//            {
//                Mirror = !_motionEngine.MovingLeft;
//            }
//        }

//        private bool IsHorizontallyCloseEnoughForEffect(ICollidable collider, float maximumDistance)
//        {
//            return (Math.Abs(collider.WorldPosition.X - WorldPosition.X) < maximumDistance);
//        }

//        private void HandleFlagCollision(Flag collider)
//        {
//            if (IsHorizontallyCloseEnoughForEffect(collider, Flag_Effect_Distance))
//            {
//                LastEvent = PlayerEvent.Restart_Point_Touched;
//                if (collider is GoalFlag) { _hasTouchedGoalFlag = true; }
//            }
//        }

//        protected override XElement Serialize(Serializer serializer)
//        {
//            base.Serialize(serializer);

//            serializer.AddDataItem("has-landed-on-block", _hasLandedOnBlock);
//            serializer.AddDataItem("did-not-land-safely", _didNotLandSafely);
//            serializer.AddDataItem("has-touched-goal", _hasTouchedGoalFlag);
//            serializer.AddDataItem("already-touched-goal", _hasAlreadyTouchedGoalFlag);
//            serializer.AddDataItem("sequence-timer", _sequenceTimer);
//            serializer.AddDataItem("motion-engine", _motionEngine);
//            serializer.AddDataItem("last-event", LastEvent);

//            return serializer.SerializedData;
//        }

//        protected override Serializer Deserialize(Serializer serializer)
//        {
//            serializer.KnownSerializedObjects.Add(_sequenceTimer);
//            serializer.KnownSerializedObjects.Add(_motionEngine);

//            base.Deserialize(serializer);

//            _hasLandedOnBlock = serializer.GetDataItem<bool>("has-landed-on-block");
//            _didNotLandSafely = serializer.GetDataItem<bool>("did-not-land-safely");
//            _hasTouchedGoalFlag = serializer.GetDataItem<bool>("has-touched-goal");
//            _hasAlreadyTouchedGoalFlag = serializer.GetDataItem<bool>("already-touched-goal");
//            _sequenceTimer = serializer.GetDataItem<Timer>("sequence-timer");
//            _motionEngine = serializer.GetDataItem<PlayerMotionEngine>("motion-engine");
//            LastEvent = serializer.GetDataItem<PlayerEvent>("last-event");

//            return serializer;
//        }

//        public void ActivateForRace()
//        {
//            LifeCycleState = LifeCycleStateValue.Entering;
//            Activate();

//            _motionEngine.SetForStartSequence(IsMovingLeft);

//            if (IsDead) { _sequenceTimer.NextActionDuration = Race_Resurrect_Sequence_Duration_In_Milliseconds; }
//            else { _sequenceTimer.NextActionDuration = Race_Start_Sequence_Duration_In_Milliseconds; }

//        }

//        private void Activate()
//        {
//            // TODO: Effect

//            Visible = true;
//            IsDead = false;
//            Collidable = true;
//        }

//        public void ActivateForSurvival()
//        {
//            _motionEngine.HorizontalMovementBlocked = false;

//            LifeCycleState = LifeCycleStateValue.Active;
//            Activate();
//        }

//        public void TintBones(XElement customTints)
//        {
//            if (customTints != null)
//            {
//                foreach (XElement tint in customTints.Elements("tint"))
//                {
//                    if (Bones.ContainsKey(tint.Attribute("bone-id").Value))
//                    {
//                        Bones[tint.Attribute("bone-id").Value].Tint =
//                            new Color((int)tint.Attribute("tint-r"), (int)tint.Attribute("tint-g"), (int)tint.Attribute("tint-b"));
//                    }
//                }
//            }
//        }

//        public void TintBones(string skinName)
//        {
//            switch (skinName)
//            {
//                case Skin_Name: Bones["mouth"].Tint = new Color(0, 0, 93); break;
//                case PC_Player_Two_Skin_Name: Bones["mouth"].Tint = new Color(0, 10, 0); break;
//            }
//        }

//        public enum PlayerEvent
//        {
//            None,
//            Goal_Passed,
//            Restart_Point_Touched,
//            Died,
//            Resurrected
//        }

//        public const string Skeleton_Name = "player-skeleton";
//        public const string Skin_Name = "player-dusty-skin";
//        public const string PC_Player_Two_Skin_Name = "player-dustin-skin";

//        private const int Render_Layer = 2;
//        private const float Render_Depth = 0.4f;

//        private const int Block_Collision_Action_Boundary = 120;
//        private const float Body_Collision_Radius = 35.0f;
//        private const float Signpost_Effect_Distance = 40.0f;
//        private const float Flag_Effect_Distance = 35.0f;

//        private const int Survival_Death_Sequence_Duration_In_Milliseconds = 1500;
//        private const int Exit_Sequence_Duration_In_Milliseconds = 3000;

//        private const int Race_Death_Sequence_Duration_In_Milliseconds = 1000;
//        private const int Race_Resurrect_Sequence_Duration_In_Milliseconds = 2000;
//        private const int Race_Start_Sequence_Duration_In_Milliseconds = 3500;
//    }
//}
