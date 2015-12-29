//using System;
//using System.Xml.Linq;

//using Microsoft.Xna.Framework;

//using Leda.Core;
//using Leda.Core.Motion;
//using Leda.Core.Serialization;
//using Leda.Core.Game_Objects.Behaviours;

//namespace Bopscotch.Gameplay.Objects.Characters.Player
//{
//    public class PlayerMotionEngine : IMotionEngine, ISerializable
//    {
//        private float _speed;
//        private float _gravity;
//        private float _verticalVelocity;
//        private int _horizontalMovementDirection;
//        private int _verticalMovementDirection;
//        private Vector2 _delta;
//        private bool _jumpTriggered;
//        private bool _actionDisabled;
//        private Range _speedLimits;
//        private int _speedLimitDuration;

//        public string ID { get; set; }
//        public bool PlayerHasHitGround { private get; set; }
//        public bool VerticalMovementCanSmash { get { return Math.Abs(_verticalVelocity) > Smash_Required_Vertical_Velocity; } }
//        public bool PlayerIsDead { private get; set; }
//        public bool HorizontalMovementBlocked { get; set; }
//        public bool LevelCleared { private get; set; }

//        public Display.Speedometer Speedometer { private get; set; }

//        public Input.InputProcessorBase InputProcessor { private get; set;}

//        public bool MovingLeft
//        {
//            get { return _horizontalMovementDirection < 0; }
//            set { if (value) { _horizontalMovementDirection = -1; } else { _horizontalMovementDirection = 1; } }
//        }

//        public Vector2 Delta { get { return _delta; } }
//        public bool VerticalDirectionChanged { get { return (Math.Sign(_delta.Y) != _verticalMovementDirection); } }

//        public PlayerMotionEngine()
//        {
//            ID = "player-motion-engine";

//            _speed = Minimum_Movement_Speed;
//            _gravity = Definitions.Normal_Gravity_Value;
//            _jumpTriggered = false;
//            _actionDisabled = false;

//            _speedLimits = new Range(Minimum_Movement_Speed, Starting_Maximum_Movement_Speed);
//            _speedLimitDuration = 0;

//            _horizontalMovementDirection = 0;
//            _verticalMovementDirection = 0;
//            _delta = Vector2.Zero;

//            InputProcessor = null;

//            PlayerIsDead = false;
//            HorizontalMovementBlocked = false;
//            LevelCleared = false;
//        }

//        public void Update(int millisecondsSinceLastUpdate)
//        {
//            _verticalMovementDirection = Math.Sign(_delta.Y);

//            SetHorizontalMovementSpeed();
//            SetHorizontalMovementDelta(millisecondsSinceLastUpdate);

//            CheckForAndStartJumpOrSlam();
//            SetVerticalMovementDelta(millisecondsSinceLastUpdate);

//            UpdateSpeedometer();

//            if (_speedLimitDuration > 0) { CheckForAndHandleSpeedLimitChanges(millisecondsSinceLastUpdate); }
//        }

//        private void SetHorizontalMovementSpeed()
//        {
//            if ((NormalMovementIsAllowed) && (!InputProcessor.ActionDoesNotAffectPlayer))
//            {
//                if (((MovingLeft) && (InputProcessor.MoveLeft)) || (!MovingLeft && (InputProcessor.MoveRight)))
//                {
//                    _speed = Math.Min(_speed + Speed_Change_Rate, _speedLimits.Maximum);
//                }
//                else if (((MovingLeft) && (InputProcessor.MoveRight)) || (!MovingLeft && (InputProcessor.MoveLeft)))
//                {
//                    _speed = Math.Max(_speed - Speed_Change_Rate, _speedLimits.Minimum);
//                }
//            }
//        }

//        private bool NormalMovementIsAllowed
//        {
//            get { return ((!PlayerIsDead) && (!HorizontalMovementBlocked) && (!LevelCleared)); }
//        }

//        private void SetHorizontalMovementDelta(int millisecondsSinceLastUpdate)
//        {
//            if (!NormalMovementIsAllowed) { _delta.X = 0.0f; }
//            else { _delta.X = _speed * millisecondsSinceLastUpdate * _horizontalMovementDirection; }
//        }

//        private void CheckForAndStartJumpOrSlam()
//        {
//            if ((!InputProcessor.ActionDoesNotAffectPlayer) && (InputProcessor.ActionTriggered) && (!_actionDisabled) && (NormalMovementIsAllowed))
//            {
//                _gravity = Slam_Gravity_Value;
//                if (_verticalVelocity >= Bounce_Vertical_Velocity) { _jumpTriggered = true; }
//            }
//        }

//        private void SetVerticalMovementDelta(int millisecondsSinceLastUpdate)
//        {
//            _delta.Y = 0.0f;

//            if (!PlayerIsDead) { HandleNormalVerticalMovement(millisecondsSinceLastUpdate); }
//        }

//        private void HandleNormalVerticalMovement(int millisecondsSinceLastUpdate)
//        {
//            if ((PlayerHasHitGround) && (_verticalVelocity >= 0.0f))
//            {
//                _verticalVelocity = Bounce_Vertical_Velocity;
//                _gravity = Definitions.Normal_Gravity_Value;

//                if (_jumpTriggered) { _verticalVelocity = Jump_Vertical_Velocity; }

//                _jumpTriggered = false;
//            }

//            _delta.Y = 0.0f;
//            while (millisecondsSinceLastUpdate-- > 0)
//            {
//                _delta.Y += _verticalVelocity;
//                if (_verticalVelocity <= -Definitions.Zero_Vertical_Velocity) { _verticalVelocity *= _gravity; }
//                else if (_verticalVelocity >= Definitions.Zero_Vertical_Velocity) { _verticalVelocity /= _gravity; _actionDisabled = false; }
//                else { _verticalVelocity = Definitions.Zero_Vertical_Velocity; }
//            }

//            _delta.Y = Math.Min(_delta.Y, Definitions.Terminal_Velocity);
//            PlayerHasHitGround = false;
//        }

//        private void UpdateSpeedometer()
//        {
//            if (!HorizontalMovementBlocked)
//            {
//                Speedometer.SpeedFraction = (_speed - Minimum_Movement_Speed) / (Absolute_Maximum_Movement_Speed - Minimum_Movement_Speed);
//            }
//        }

//        private void CheckForAndHandleSpeedLimitChanges(int millisecondsSinceLastUpdate)
//        {
//            if (_speedLimitDuration > 0)
//            {
//                _speedLimitDuration -= millisecondsSinceLastUpdate;
//                if (_speedLimitDuration < 1)
//                {
//                    _speedLimits.Minimum = Minimum_Movement_Speed;
//                    _speedLimits.Maximum = Starting_Maximum_Movement_Speed;
//                }
//            }
//        }

//        public void SetForSpringLaunch()
//        {
//            _actionDisabled = true;
//            _jumpTriggered = false;
//            _gravity = Definitions.Normal_Gravity_Value;
//            _verticalVelocity = Spring_Launch_Vertical_Velocity;
//        }

//        public XElement Serialize()
//        {
//            Serializer serializer = new Serializer(this);
//            serializer.AddDataItem("speed", _speed);
//            serializer.AddDataItem("gravity", _gravity);
//            serializer.AddDataItem("vertical-velocity", _verticalVelocity);
//            serializer.AddDataItem("horizontal-movement-direction", _horizontalMovementDirection);
//            serializer.AddDataItem("vertical-movement-direction", _verticalMovementDirection);
//            serializer.AddDataItem("delta", _delta);
//            serializer.AddDataItem("jump-triggered", _jumpTriggered);
//            serializer.AddDataItem("action-disabled", _actionDisabled);
//            serializer.AddDataItem("has-hit-ground", PlayerHasHitGround);
//            serializer.AddDataItem("is-dead", PlayerIsDead);
//            serializer.AddDataItem("horizontal-movement-blocked", HorizontalMovementBlocked);
//            serializer.AddDataItem("level-cleared", LevelCleared);
//            serializer.AddDataItem("moving-left", MovingLeft);
//            serializer.AddDataItem("speed-limits", _speedLimits);
//            serializer.AddDataItem("speed-limit-duration", _speedLimitDuration);

//            return serializer.SerializedData;
//        }

//        public void Deserialize(XElement serializedData)
//        {
//            Serializer serializer = new Serializer(serializedData);

//            _speed = serializer.GetDataItem<float>("speed");
//            _gravity = serializer.GetDataItem<float>("gravity");
//            _verticalVelocity = serializer.GetDataItem<float>("vertical-velocity");
//            _horizontalMovementDirection = serializer.GetDataItem<int>("horizontal-movement-direction");
//            _verticalMovementDirection = serializer.GetDataItem<int>("vertical-movement-direction");
//            _delta = serializer.GetDataItem<Vector2>("delta");
//            _jumpTriggered = serializer.GetDataItem<bool>("jump-triggered");
//            _actionDisabled = serializer.GetDataItem<bool>("action-disabled");

//            PlayerHasHitGround = serializer.GetDataItem<bool>("has-hit-ground");
//            PlayerIsDead = serializer.GetDataItem<bool>("is-dead");
//            HorizontalMovementBlocked = serializer.GetDataItem<bool>("horizontal-movement-blocked");
//            LevelCleared = serializer.GetDataItem<bool>("level-cleared");
//            MovingLeft = serializer.GetDataItem<bool>("moving-left");

//            _speedLimits = serializer.GetDataItem<Range>("speed-limits");
//            _speedLimitDuration = serializer.GetDataItem<int>("speed-limit-duration");
//        }

//        public void SetForStartSequence(bool startsMovingLeft)
//        {
//            _speedLimits = new Range(Minimum_Movement_Speed, Starting_Maximum_Movement_Speed);
//            _speed = Minimum_Movement_Speed;
//            MovingLeft = startsMovingLeft;

//            HorizontalMovementBlocked = true;
//            PlayerIsDead = false;
//        }

//        private const float Speed_Change_Rate = 0.01f;

//        private const float Slam_Gravity_Value = 0.95f;
//        private const float Smash_Required_Vertical_Velocity = 0.65f;

//        private const float Bounce_Vertical_Velocity = -0.35f;
//        private const float Jump_Vertical_Velocity = -1.2f;
//        private const float Spring_Launch_Vertical_Velocity = -2.5f;

//        private const float Minimum_Movement_Speed = 0.5f;
//        private const float Starting_Maximum_Movement_Speed = 0.8f;
//        private const float Absolute_Maximum_Movement_Speed = 0.9f;
//    }
//}
