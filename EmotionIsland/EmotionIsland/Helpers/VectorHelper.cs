using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Trauma.Helpers
{
    public static class VectorHelper
    {
        #region Constants

        private const float EPSILON = 0.001f;

        /* Directions */
        private const String UP = "Up";
        private const String UPPER_LEFT = "UpLeft";
        private const String UPPER_RIGHT = "UpRight";
        private const String LEFT = "Left";
        private const String RIGHT = "Right";
        private const String DOWN = "Down";
        private const String LOWER_LEFT = "DownLeft";
        private const String LOWER_RIGHT = "DownRight";

        /* Balancing */
        private const float DEFAULT_BALANCE = 0.5f;
        #endregion

        /// <summary>
        /// Push against the given vector by the given amount.
        /// </summary>
        /// <param name="vector">The vector to push.</param>
        /// <param name="push">The amount to push it by. Should not be negative.</param>
        /// <returns>The vector pushed back by the given amount, or the zero vector if the push amount
        /// was greater than the vector.</returns>
        public static Vector2 PushBack(this Vector2 vector, Vector2 push)
        {
            // push should be an absolute vector -- there is no negative push.
            Debug.Assert(Vector2.Max(push, Vector2.Zero) == push, "Negative push should not be applied.");

            // if going left, shove right, otherwise shove left.
            float newXComponent = vector.X < 0 ? Math.Min(vector.X + push.X, 0) : Math.Max(vector.X - push.X, 0);
            // similarly, if going up, push down, otherwise push up.
            float newYComponent = vector.Y < 0 ? Math.Min(vector.Y + push.Y, 0) : Math.Max(vector.Y - push.Y, 0);

            return new Vector2(newXComponent, newYComponent);
        }

        public static Vector2 PushTowards(this Vector2 vector, Vector2 towards, Vector2 push)
        {
            Vector2 result = vector;
            
            if (vector.X < towards.X)
                result.X = Math.Min(vector.X + push.X, towards.X);
            else
                result.X = Math.Max(vector.X - push.X, towards.X);

            if (vector.Y < towards.Y)
                result.Y = Math.Min(vector.Y + push.Y, towards.Y);
            else
                result.Y = Math.Max(vector.Y - push.Y, towards.Y);

            return result;
        }

        public static Vector2 ShoveToSide(this Vector2 startPosition, Vector2 boxSize, Vector2 shove)
        {
            // figure out which sides to shove it to 
            Vector2 shovedPosition = startPosition;
            if (shove.X > 0)
                shovedPosition.X = startPosition.X + boxSize.X;
            if (shove.Y > 0)
                shovedPosition.Y = startPosition.Y + boxSize.Y;

            return shovedPosition;
        }

        public static Vector2? FromDirectionString(String direction)
        {
            Vector2 vector;
            switch (direction)
            {
                case UP:
                    return new Vector2(0, -1);
                case LEFT:
                    return new Vector2(-1, 0);
                case DOWN:
                    return new Vector2(0, 1);
                case RIGHT:
                    return new Vector2(1, 0);
                case UPPER_LEFT:
                    vector = new Vector2(-1, -1);
                    vector.Normalize();
                    return vector;
                case UPPER_RIGHT:
                    vector = new Vector2(1, -1);
                    vector.Normalize();
                    return vector;
                case LOWER_LEFT:
                    vector = new Vector2(-1, 1);
                    vector.Normalize();
                    return vector;
                case LOWER_RIGHT:
                    vector = new Vector2(1, 1);
                    vector.Normalize();
                    return vector;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create a vector from the given angle.
        /// </summary>
        /// <param name="vector">The vector to create.</param>
        /// <param name="angle">The angle to create it from.</param>
        public static Vector2 FromAngle(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        public static float ToAngle(this Vector2 vector)
        {
            return (float) Math.Atan2(vector.X, -vector.Y);
        }

        /// <summary>
        /// Balances out the vector to make its x-component not completely dwarfed by its y-component,
        /// or vice-versa.
        /// </summary>
        /// <param name="vector">The vector to balance.</param>
        /// <param name="balanceAmount">How much to balance it by. Valid values are 0 to 1.</param>
        /// <returns>The vector balanced out to have its x-component lerped towards its y-component if the
        /// y-component is larger, otherwise the y-component lerped towards its x-component.</returns>
        public static Vector2 Balance(this Vector2 vector, float balanceAmount=DEFAULT_BALANCE)
        {
            Debug.Assert(0 <= balanceAmount && balanceAmount <= 1, "Can't balance by that amount, expecting a value in range of 0 to 1.");
            Vector2 balancedVector = vector;

            // if they're equal, just return the original vector
            if (Math.Abs(balancedVector.X - balancedVector.Y) < EPSILON)
                return balancedVector;
            
            // push smaller component towards larger component
            if (balancedVector.X < balancedVector.Y)
                return Vector2.Lerp(balancedVector, new Vector2(balancedVector.Y, balancedVector.Y), balanceAmount);
            if (balancedVector.Y < balancedVector.X)
                return Vector2.Lerp(balancedVector, new Vector2(balancedVector.X, balancedVector.X), balanceAmount);

            throw new InvalidOperationException("Reached a part of code that shouldn't be reachable.");
        }

        /// <summary>
        /// Returns the negative of the given vector.
        /// </summary>
        /// <param name="vector">The vector to negate.</param>
        /// <returns>The vector negated.</returns>
        public static Vector2 Negate(this Vector2 vector)
        {
            return -1*vector;
        }

        public static float LargestComponent(this Vector2 vector)
        {
            return Math.Max(vector.X, vector.Y);
        }
    }
}
