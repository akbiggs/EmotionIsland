#region Using Statements

using System;
using Microsoft.Xna.Framework;

#endregion

#region Instructions / Notes
// 2D Camera System by Ian Campbell, 2013
// David Amador's "2D Camera With Zoom and Rotation" was used as a (very helpful) base.
// http://www.david-amador.com/2009/10/xna-camera-2d-with-zoom-and-rotation/


// HOW TO USE: Pass this Camera's transformation matrix to
// spriteBatch.Begin(). This will offset everything that
// gets drawn by the transformation matrix. It's that easy!
//
// Using it will look something like this:
//
// spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp,
//     DepthStencilState.None, RasterizerState.CullNone, null, **Game1.Camera.GetTransformation()**);   <-- The matrix!
//
// Then just do your regular spriteBatch.Draw()ing and watch the magic happen.


// OTHER INSTRUCTIONS: The camera has a few special properties:
// Shake: The camera can be shaken for dramatic flair. The shake direction
//      is always random, but you can specify how intense the shaking is
//      and how quickly it subsides.
// Override: The camera's position and zoom can be overridden. The
//      camera will smoothly transition between the normal position/zoom
//      to the overridden position/zoom. The override method must be called
//      as long as you want the camera to focus on the overridden parameters.
//      One way to do this would be to have volumes in the world that
//      override the camera with specific position/zoom parameters as long
//      as the player is inside them.
// View Area: A rectangle that defines what the camera can see in world
//      space. This can be used to limit the drawing of objects to only
//      what the camera can see. The view area can be padded horizontally
//      and vertically to prevent things like particles from disappearing
//      before they are completely off screen.
// Rotation: The camera can be rotated around its center-point. Be aware
//      that the View Area is an axis-aligned bounding box and does not
//      rotate with the camera. This can cause all kinds of graphical
//      ugliness if you aren't careful. Padding the View Area creates a
//      margin that you can rotate the camera in without it turning ugly.
#endregion

namespace EmotionIsland
{
    public class Camera2D
    {
        /// <summary> A camera for a 2D game. </summary>
        public Camera2D(float backBufferWidth, float backBufferHeight)
        {
            this.backBufferWidth = backBufferWidth;
            this.backBufferHeight = backBufferHeight;
        }

        #region Camera Fields and Properties
        float backBufferWidth = 256;
        float backBufferHeight = 256;

        // Camera position:
        Vector2 finalPosition = Vector2.Zero; 
        public Vector2 Center { get { return this.finalPosition; } }
        public Vector2 TopLeft { get { return this.Center - new Vector2(this.ViewArea.Width, this.ViewArea.Height); } }
        // Camera rotation:
        private float rotation = 0; public float Rotation { get { return this.rotation; } set { this.rotation = value; } }
        // Camera zoom:
        const float MIN_ZOOM    = 0.50f;
        const float MAX_ZOOM    = 1.50f;
        float targetZoom = 1.00f; float finalZoom = 1.00f;
        public float Zoom
        {
            get { return this.finalZoom; }
            set
            {
                this.targetZoom = value;
                if (this.targetZoom > MAX_ZOOM) this.targetZoom = MAX_ZOOM;
                if (this.targetZoom < MIN_ZOOM) this.targetZoom = MIN_ZOOM;
            }
        }
        #endregion

        #region Utility Methods
        /// <summary> Gets the transformation matrix of the camera. Pass this to a SpriteBatch to offset what is drawn by it. </summary>
        public Matrix GetTransformation()
        {
            //return Matrix.Identity;

            return Matrix.CreateTranslation(new Vector3((int)-this.Center.X, (int)-this.Center.Y, 0))
                * Matrix.CreateScale(new Vector3(this.Zoom, this.Zoom, 0))
                * Matrix.CreateRotationZ(this.Rotation)
                * Matrix.CreateTranslation(new Vector3((this.backBufferWidth / 2.0f), (this.backBufferHeight / 2.0f), 0))
                * Matrix.CreateTranslation(new Vector3((int)this.shakeDirection.X, (int)this.shakeDirection.Y, 0))
                ;
        }

        /// <summary> Instantly focuses the camera on the specified position, at the specified
        /// zoom. Pass null for either parameter to use their current values instead.</summary>
        public void Snap(Vector2? snapPosition, float? snapZoom)
        {
            if (snapPosition != null) this.finalPosition = (Vector2)snapPosition;
            if (snapZoom != null) this.finalZoom = (float)snapZoom;
        }

        /// <summary> Reset the camera, clearing rotation, scale, zoom, shake, etc. </summary>
        public void Reset()
        {
            this.Rotation = 0;
            this.shakeIntensity = 0;
        }

        #region Override
        // Position override fields:
        Vector2 overridePosition = Vector2.Zero;

        /// <summary> Overrides the camera's position, causing it to center on the specified position
        /// instead. Smoothly transitions between the target position and the override position. This
        /// method must be called every frame for as long as you want the override to last. </summary>
        public void OverridePosition(Vector2 overridePosition)
        {
            this.overridePosition = overridePosition;
        }

        // Zoom override fields:
        float overrideZoom = 0.00f;

        /// <summary> Overrides the camera's zoom, causing it to zoom to the specified magnification
        /// instead. Smoothly transitions between the target zoom and the override zoom. This
        /// method must be called every frame for as long as you want the override to last.</summary>
        public void OverrideZoom(float overrideZoom)
        {
            if (overrideZoom <= MIN_ZOOM) overrideZoom = MIN_ZOOM;
            if (overrideZoom >= MAX_ZOOM) overrideZoom = MAX_ZOOM;

            this.overrideZoom = overrideZoom;
        }
        #endregion

        #region Camera Shake
        // Camera shake fields:
        Vector2 shakeDirection = Vector2.Zero; Random randomNumber = new Random();
        float shakeIntensity = 0.0f; float shakeDegredation = 0.95f;

        /// <summary> Shakes the camera. </summary>
        /// <param name="shakeIntensity">The intensity of the shaking.</param>
        /// <param name="shakeDegredation">How quickly the shaking degrades. Larger numbers produce
        /// longer shakes. Cannot go below 0.50, or above 0.999. 0.95f is a good all-around value.</param>
        public void Shake(float shakeIntensity, float shakeDegredation)
        {
            // If the camera is currently shaking with an intensity greater than what is being supplied,
            // the shake being supplied will be swallowed by the current, more intense shake.
            if (Math.Abs(shakeIntensity) > Math.Abs(this.shakeIntensity))
            {
                this.shakeIntensity = shakeIntensity;

                if (shakeDegredation <= 0.500f) shakeDegredation = 0.500f;
                if (shakeDegredation >= 0.999f) shakeDegredation = 0.999f;
                this.shakeDegredation = shakeDegredation;
            }
        }

        /// <summary> Updates the shaking of the camera. </summary>
        private void updateShake()
        {
            // If the shake intensity has fallen below 0.01f, the camera is considered to no longer be shaking
            if (Math.Abs(this.shakeIntensity) <= 0.01f) { this.shakeIntensity = 0; return; }

            // Get a new random shake direction
            this.shakeDirection.X = (float)(this.randomNumber.NextDouble() - 0.5);
            this.shakeDirection.Y = (float)(this.randomNumber.NextDouble() - 0.5);

            // Shake the camera
            this.shakeDirection *= this.shakeIntensity * this.Zoom;
            // Degrade the shaking
            this.shakeIntensity *= this.shakeDegredation;
        }
        #endregion

        #region View Area
        // View area fields:
        const int VIEW_AREA_PADDING_HOR = 128;
        const int VIEW_AREA_PADDING_VER = 128;
        Rectangle viewArea = new Rectangle(); public Rectangle ViewArea { get { return this.viewArea; } }

        /// <summary> Updates the View Area. Thew View Area is a Rectangle that describes the area that the
        /// camera can see, which is useful for optimizations like drawing only objects that are viewable.</summary>
        private void updateViewArea()
        {
            this.viewArea.X = (int)(this.Center.X - ((this.backBufferWidth / 2 + VIEW_AREA_PADDING_HOR) / this.finalZoom));
            this.viewArea.Y = (int)(this.Center.Y - ((this.backBufferHeight / 2 + VIEW_AREA_PADDING_VER) / this.finalZoom));
            this.viewArea.Width = (int)((this.backBufferWidth + VIEW_AREA_PADDING_HOR * 2) / this.finalZoom);
            this.viewArea.Height = (int)((this.backBufferHeight + VIEW_AREA_PADDING_VER * 2) / this.finalZoom);
        }
        #endregion
        #endregion

        #region Update
        /// <summary> Updates the camera. </summary>
        /// <param name="targetPosition">Position for the camera to follow.</param>
        public void Update(Vector2 targetPosition)
        {
            // Override position
            if (this.overridePosition != Vector2.Zero) targetPosition = this.overridePosition;
            this.finalPosition = Vector2.Lerp(this.finalPosition, targetPosition, 0.125f);

            // Override zoom
            if (this.overrideZoom != 0.00f) this.finalZoom = MathHelper.Lerp(this.finalZoom, this.overrideZoom, 0.125f);
            else this.finalZoom = MathHelper.Lerp(this.finalZoom, this.targetZoom, 0.125f);

            // Limit camera positon
            if (this.finalPosition.X - (this.backBufferWidth / 2) / this.finalZoom - 32 < 0) this.finalPosition.X = (this.backBufferWidth / 2) / this.finalZoom + 32;
            else if (this.finalPosition.X + (this.backBufferWidth / 2) / this.finalZoom + 32 > 500 * 32)
                this.finalPosition.X = (500 * 32) - ((this.backBufferWidth / 2) / this.finalZoom) - 32;
            if (this.finalPosition.Y - (this.backBufferHeight / 2) / this.finalZoom - 32 < 0) this.finalPosition.Y = (this.backBufferHeight / 2) / this.finalZoom + 32;
            else if (this.finalPosition.Y + (this.backBufferHeight / 2) / this.finalZoom + 32 > 500 * 32)
                this.finalPosition.Y = (500 * 32) - (this.backBufferHeight / 2) / this.finalZoom - 32;

            // Update camera shake
            this.updateShake();

            // Update view area
            this.updateViewArea();

            // Reset override parameters
            this.overridePosition = Vector2.Zero;
            this.overrideZoom = 0.00f;
        }
        #endregion
    }
}