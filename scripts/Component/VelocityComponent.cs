using System;
using Godot;

namespace Deathville.Component
{
    public class VelocityComponent : Node
    {
        private const float GRAVITY = 800f;

        [Export]
        private bool _obeyTimeScale = true;
        [Export]
        private float _maxSpeed = 100f;
        [Export]
        private float _acceleration = 1500f;
        [Export]
        private float _deceleration = 1500f;

        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
        }

        public float YVelocity
        {
            set
            {
                _velocity.y = value;
            }
        }

        public float MaxSpeed
        {
            get
            {
                return _maxSpeed;
            }
        }

        public float SpeedPadding;

        private Vector2 _velocity;
        private KinematicBody2D _owner;

        private float _realAcceleration;
        private float _realDeceleration;

        public override void _Ready()
        {
            _owner = Owner as KinematicBody2D;
            _realAcceleration = _acceleration;
        }

        public override void _Process(float delta)
        {
            _realAcceleration = Mathf.Lerp(_realAcceleration, _acceleration, 1f * delta);
            _realDeceleration = Mathf.Lerp(_realDeceleration, _deceleration, 1f * delta);
        }

        public void Accelerate(Vector2 dir)
        {
            var mod = dir.x < 0f ? -1f : 1f;
            if (dir.x == 0f)
            {
                mod = 0f;
            }
            _velocity.x += _realAcceleration * GetProcessDeltaTime() * mod / GetTimeScale();
        }

        public void Decelerate()
        {
            var abs = Mathf.Abs(_velocity.x);
            var x = Mathf.Clamp(abs - _realDeceleration * GetProcessDeltaTime() / GetTimeScale(), 0f, float.MaxValue);
            _velocity.x = x * Mathf.Sign(_velocity.x);
        }

        public void ApplyForce(Vector2 dir, float force)
        {
            _velocity += dir * force;
        }

        public void ApplyKnockback(Vector2 dir, float knockBackForce)
        {
            var vel = dir * knockBackForce;
            _velocity.x = vel.x;
            _velocity.y += vel.y;
            _realAcceleration = 0f;
            _realDeceleration = 0f;
        }

        public void Jump(float speed)
        {
            _velocity.y = -speed;
        }

        public void ApplyGravity(float accelerator = 1f)
        {
            var deltaGrav = GRAVITY * GetProcessDeltaTime() / GetTimeScale();
            _velocity.y += deltaGrav * accelerator;
        }

        public void MoveWithSnap()
        {
            ClampVelocity();
            Vector2? up = _velocity.y < 0f ? (Vector2?) null : Vector2.Up;
            _velocity = _owner.MoveAndSlideWithSnap(_velocity / GetTimeScale(), Vector2.Down, up) * GetTimeScale();
        }

        public void Move()
        {
            ClampVelocity();
            _velocity = _owner.MoveAndSlide(_velocity / GetTimeScale(), Vector2.Up) * GetTimeScale();
        }

        private float GetTimeScale()
        {
            return _obeyTimeScale ? 1f : Engine.TimeScale;
        }

        private void ClampVelocity()
        {
            var abs = Math.Abs(_velocity.x);
            var x = Mathf.Clamp(abs, 0f, _maxSpeed + SpeedPadding);
            _velocity.x = x * Mathf.Sign(_velocity.x);
        }
    }
}