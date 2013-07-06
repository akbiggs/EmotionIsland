using System;
using Microsoft.Xna.Framework;

namespace EmotionIsland.Helpers
{
    public static class ColorHelper
    {
        #region Constants

        /* String representations */
        private const string RED = "Red";
        private const string BLUE = "Blue";
        private const string YELLOW = "Yellow";
        private const string ORANGE = "Orange";
        private const string PURPLE = "Purple";
        private const string GREEN = "Green";
        private const string BLACK = "Black";
        private const string GRAY = "Gray";
        
        #endregion

        #region Members

        private static Color Red = Color.Red;
        private static Color Blue = Color.Blue;
        private static Color Yellow = Color.Yellow;
        private static Color Purple = Color.Purple;
        private static Color Orange = Color.DarkOrange;
        private static Color Green = Color.Green;
        private static Color Black = Color.Black;
        private static Color Gray = Color.Gray;
        #endregion

        public static Color PushTowards(this Color color, Color otherColor, byte push)
        {
            Color result = new Color();
            
            if (color.A < otherColor.A)
                result.A = (byte) Math.Min(color.A + push, otherColor.A);
            else
                result.A = (byte) Math.Max(color.A - push, otherColor.A);
            
            if (color.R < otherColor.R)
                result.R = (byte) Math.Min(color.R + push, otherColor.R);
            else
                result.R = (byte) Math.Max(color.R - push, otherColor.R);
            
            if (color.G < otherColor.G)
                result.G = (byte) Math.Min(color.G + push, otherColor.G);
            else
                result.G = (byte) Math.Max(color.G - push, otherColor.G);
            
            if (color.B < otherColor.B)
                result.B = (byte) Math.Min(color.B + push, otherColor.B);
            else
                result.B = (byte) Math.Max(color.B - push, otherColor.B);

            return result;
        }
    
        public static Color? FromString(String name)
        {
            switch (name)
            {
                case RED:
                    return Red;
                case BLUE:
                    return Blue;
                case YELLOW:
                    return Yellow;
                case ORANGE:
                    return Orange;
                case PURPLE:
                    return Purple;
                case GREEN:
                    return Green;
                case BLACK:
                    return Black;
                case GRAY:
                    return Gray;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns whether or not the given color contains the other color, i.e.
        /// is made from it.
        /// </summary>
        /// <param name="color">The color that might contain the other color.</param>
        /// <param name="containedColor">The color that might be contained within the other color.</param>
        /// <returns>True if the color is contained, false otherwise.</returns>
        public static bool Contains(this Color color, Color containedColor)
        {
            // black is contained in every color...also there's this gray color thing
            if (color == Gray || containedColor == Black)
                return true;

            if (color == containedColor) 
                return true;

            if (color == Purple && (containedColor == Blue || containedColor == Red))
                return true;
            if (color == Orange && (containedColor == Red || containedColor == Yellow))
                return true;
            if (color == Green && (containedColor == Blue || containedColor == Yellow))
                return true;

            return false;
        }

        public static Color Combine(this Color color, Color otherColor)
        {
            
            // primary combinations
            if ((color == Red && otherColor == Blue) || (color == Blue && otherColor == Red))
                return Purple;
            if ((color == Blue && otherColor == Yellow) || (color == Yellow && otherColor == Blue))
                return Green;
            if ((color == Red && otherColor == Yellow) || (color == Yellow && otherColor == Red))
                return Orange;

            // redundant color combinations
            if (color.Contains(otherColor))
                return color;
            if (otherColor.Contains(color))
                return otherColor;

#if DEBUG
            Console.WriteLine(String.Format("Possibly unsupported color combination: {0} and {1}", color.ToString(), otherColor.ToString()));
#endif
            return Black;
        }
    }
}
