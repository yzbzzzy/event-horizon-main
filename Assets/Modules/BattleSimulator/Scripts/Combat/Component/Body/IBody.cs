﻿using System;
using UnityEngine;

namespace Combat.Component.Body
{
    public interface IBody : IDisposable
    {
        IBody Parent { get; }

        Vector2 Position { get; }
        float Rotation { get; }
        float Offset { get; }

        Vector2 Velocity { get; }
        float AngularVelocity { get; }

        float Weight { get; }
        float Scale { get; }

        Vector2 VisualPosition { get; }
        float VisualRotation { get; }

        void Move(Vector2 position);
        void Turn(float rotation);
        void SetSize(float size);
        void ApplyAcceleration(Vector2 acceleration);
        void ApplyAngularAcceleration(float acceleration);
        void ApplyForce(Vector2 position, Vector2 force);
        void SetVelocityLimit(float value);

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);

        void AddChild(Transform child);
    }

    public static class BodyExtensions
    {
        public static Vector2 WorldPosition(this IBody body)
        {
            var position = body.Position + RotationHelpers.Direction(body.Rotation)*body.Offset;

            if (body.Parent == null)
                return position;

            return body.Parent.WorldPosition() + RotationHelpers.Transform(position, body.Parent.WorldRotation())*body.Parent.WorldScale();
        }

        public static Vector2 VisualWorldPosition(this IBody body)
        {
            var position = body.VisualPosition + RotationHelpers.Direction(body.VisualRotation) * body.Offset;

            if (body.Parent == null)
                return position;

            return body.Parent.VisualWorldPosition() + RotationHelpers.Transform(position, body.Parent.VisualWorldRotation()) * body.Parent.WorldScale();
        }

        public static Vector2 ChildPosition(this IBody body, Vector2 position)
        {
            return new Vector2(body.Offset/body.Scale + position.x, position.y);
        }

        public static Vector2 WorldPositionNoOffset(this IBody body)
        {
            var position = body.Position;

            if (body.Parent == null)
                return position;

            return body.Parent.WorldPosition() + RotationHelpers.Transform(position, body.Parent.WorldRotation())*body.Parent.WorldScale();
        }

        public static float WorldRotation(this IBody body)
        {
            if (body.Parent == null)
                return body.Rotation;

            return body.Rotation + body.Parent.WorldRotation();
        }

        public static float VisualWorldRotation(this IBody body)
        {
            if (body.Parent == null)
                return body.VisualRotation;

            return body.VisualRotation + body.Parent.VisualWorldRotation();
        }

        public static Vector2 WorldVelocity(this IBody body)
        {
            if (body.Parent == null)
                return body.Velocity;

            return body.Parent.Velocity + RotationHelpers.Transform(body.Velocity, body.Parent.WorldRotation());
        }

        public static float WorldAngularVelocity(this IBody body)
        {
            if (body.Parent == null)
                return body.AngularVelocity;

            return body.AngularVelocity + body.Parent.WorldAngularVelocity();
        }

        public static float WorldScale(this IBody body)
        {
            if (body.Parent == null)
                return body.Scale;

            return body.Scale * body.Parent.WorldScale();
        }

        public static float TotalWeight(this IBody body)
        {
            if (body.Parent == null)
                return body.Weight;

            return body.Weight + body.Parent.TotalWeight();
        }

        public static IBody Owner(this IBody body)
        {
            var parent = body;
            while (parent.Parent != null)
                parent = parent.Parent;

            return parent;
        }
    }
}
