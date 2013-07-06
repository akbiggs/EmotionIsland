using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    /// <summary>
    /// A static class for all helper functions.
    /// </summary>
    public static class MathExtra
    {
        // This is the random generator.
        private static Random random = new Random();

        /// <summary>
        /// Returns a random float between 0.0 and 1.0.
        /// </summary>
        public static float RandomFloat()
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Returns a random number less than i.
        /// <param name="i">The upper bound.</param>
        /// </summary>
        public static int RandomInt(int i)
        {
            return random.Next(i);
        }

        /// <summary>
        /// Returns a random number less than i.
        /// <param name="lower">The lower bound.</param>
        /// <param name="upper">The upper bound.</param>
        /// </summary>
        public static int RandomInt(int lower, int upper)
        {
            return random.Next(lower, upper);
        }

        public static bool RandomBool()
        {
            return MathExtra.RandomInt(0, 2) == 0;
        }

        /// <summary>
        /// Shuffles the given list randomly.
        /// </summary>
        public static void ShuffleList<T>(IList<T> list)
        {
            if (list.Count > 1)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    T tmp = list[i];
                    int randomIndex = random.Next(i + 1);

                    list[i] = list[randomIndex];
                    list[randomIndex] = tmp;
                }
            }
        }

        // ==============================
        // ======= VECTOR METHODS =======
        // ==============================

        public static float FastStep(float value1, float value2, float amount)
        {
            float x = amount-1;
            float func = -(x * x) + 1;
            return func * (value2 - value1) + value1;
        }

        /// <summary>
        /// Adds some length to a given vector.
        /// If the length is negative, and a length is subtracted larger than the current
        /// length of the vector, the vector is set to zero.
        /// </summary>
        /// <param name="vector">The vector to preform on.</param>
        /// <param name="x">The amount of units to subtract from the vector.</param>
        public static void AddToVectorLength(ref Vector2 vector, float x)
        {
            float length = vector.Length();
            if (length <= -x)
            {
                vector = Vector2.Zero;
            }
            else
            {
                vector *= (length + x) / length;
            }
        }

        /// <summary>
        /// Sets the length of a vector.
        /// </summary>
        public static void SetVectorLength(ref Vector2 vector, float x)
        {
            if (vector != Vector2.Zero)
            {
                vector.Normalize();
                vector *= x;
            }
        }

        /// <summary>
        /// Restricts the given vector to a maximum length.
        /// </summary>
        public static void RestrictVectorLength(ref Vector2 vector, float x)
        {
            if (vector.Length() > x)
                SetVectorLength(ref vector, x);
        }

        /// <summary>
        /// Returns if the two parallel vectors face the same direction or not.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool CheckParallelVectorDirection(Vector2 v1, Vector2 v2)
        {
            return (Vector2.Dot(v1, v2) > 0);
        }

        /// <summary>
        /// Restricts a vector based on different components.
        /// </summary>
        public static void RestrictComponentLength(ref Vector2 vector, Vector2 xDirection, float x, float y)
        {
            Vector2 _xPart = MathExtra.GetProjectionVector(vector, xDirection);
            Vector2 _yPart = vector - _xPart;
            RestrictVectorLength(ref _xPart, x);
            RestrictVectorLength(ref _yPart, y);
            vector = _xPart + _yPart;
        }

        /// <summary>
        /// Returns a unit vector corresponding to the given angle.
        /// </summary>
        /// <param name="angle">An angle in radians.</param>
        /// <returns>A unit vector.</returns>
        public static Vector2 GetVectorFromAngle(float angle)
        {
            Vector2 a = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            return a;
        }

        /// <summary>
        /// Returns the angle that the given vector corresponds to.
        /// </summary>
        /// <param name="v">The vector to convert to an angle.</param>
        /// <returns>The angle between this vector's endpoint and the origin.</returns>
        public static float GetAngleFromVector(Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }

        /// <summary>
        /// Returns a random unit vector.
        /// </summary>
        /// <returns>A random unit vector.</returns>
        public static Vector2 GetRandomUnitVector()
        {
            return GetVectorFromAngle(RandomFloat() * MathHelper.Pi * 2);
        }

        /// <summary>
        /// Finds the vector perpendicular to the given vector with the same length.
        /// In particular, the vector rotated 90 degrees clockwise.
        /// </summary>
        /// <param name="v">A vector to use.</param>
        /// <returns>A perpendicular vector.</returns>
        public static Vector2 GetPerpendicularVector(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        /// <summary>
        /// Gets the projection vector of one onto another.
        /// </summary>
        /// <param name="vector">The vector to find the projection of.</param>
        /// <param name="projOnTo">The projection plane.</param>
        /// <returns>A projection of a vector.</returns>
        public static Vector2 GetProjectionVector(Vector2 vector, Vector2 projOnTo)
        {
            return projOnTo * Vector2.Dot(vector, projOnTo) / projOnTo.LengthSquared();
        }


        /// <summary>
        /// Finds the distance between two angles.
        /// </summary>
        public static float DistanceBetweenAngle(float angleA, float angleB)
        {
            angleA = MathHelper.WrapAngle(angleA);
            angleB = MathHelper.WrapAngle(angleB);

            float biggerAngle = angleA >= angleB ? angleA : angleB;
            float smallerAngle = angleA >= angleB ? angleB : angleA;

            float innerAngle = biggerAngle - smallerAngle;
            float outerAngle = (smallerAngle - (-MathHelper.Pi)) + ((MathHelper.Pi - biggerAngle));

            return innerAngle > outerAngle ? outerAngle : innerAngle;

        }

        /// <summary>
        /// Checks if the angle "middle" is between the acute angle between "angleA" and "angleB."
        /// </summary>
        public static bool BetweenAngle(float angleA, float angleB, float middle)
        {
            angleA = MathHelper.WrapAngle(angleA);
            angleB = MathHelper.WrapAngle(angleB);
            middle = MathHelper.WrapAngle(middle);

            float biggerAngle = angleA >= angleB ? angleA : angleB;
            float smallerAngle = angleA >= angleB ? angleB : angleA;

            float innerAngle = biggerAngle - smallerAngle;
            float outerAngle = (smallerAngle - (-MathHelper.Pi)) + ((MathHelper.Pi - biggerAngle));

            if (innerAngle < outerAngle)
                return (biggerAngle > middle && middle > smallerAngle);
            else
                return (middle > biggerAngle || middle < smallerAngle);
        }

        /// <summary>
        /// Determine whether a point P is inside the triangle ABC.
        /// Ported from http://blogs.msdn.com/b/rezanour/archive/2011/08/07/barycentric-coordinates-and-point-in-triangle-tests.aspx
        /// into 2D space.
        /// </summary>
        /// <returns>True if the point is inside, false if it is not.</returns>
        public static bool PointInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            // Prepare our barycentric variables
            Vector3 u = new Vector3(B.X - A.X, B.Y - A.Y, 0);
            Vector3 v = new Vector3(C.X - A.X, C.Y - A.Y, 0);
            Vector3 w = new Vector3(P.X - A.X, P.Y - A.Y, 0);
            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);

            // Test sign of r
            if (Vector3.Dot(vCrossW, vCrossU) < 0)
                return false;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            if (Vector3.Dot(uCrossW, uCrossV) < 0)
                return false;

            // At this point, we know that r and t and both > 0
            float denom = uCrossV.Length();
            float r = vCrossW.Length() / denom;
            float t = uCrossW.Length() / denom;

            return (r <= 1 && t <= 1 && r + t <= 1);
        }

        public static float ApplyFriction(float value, float friction)
        {
            if (Math.Abs(value) <= friction)
                return 0;
            else
                return value - Math.Sign(value) * friction;
        }

        // ==============================
        // ======= FLOAT METHODS ========
        // ==============================

        public static float PushTowards(this float f1, float f2, float amount)
        {
            if (f1 <= f2)
                return Math.Min(f2, f1 + amount);
            else
                return Math.Max(f2, f1 - amount);
        }
    }
}
