using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Helper class for calculating and setting sprite sheet UV coordinates
    /// </summary>
    public static class SpriteSheetHelper
    {
        // All frames are 86x86 pixels
        private const float FrameSize = 86f;

        /// <summary>
        /// Gets the frame count for a given animation state
        /// NOTE: Update these values based on your actual sprite sheets!
        /// Open each PNG file and count the number of frames.
        /// </summary>
        public static int GetFrameCount(AnimationState state)
        {
            return state switch
            {
                AnimationState.Idle => 6,
                AnimationState.Walking => 12,
                AnimationState.Running => 12,
                AnimationState.Crouch => 3,
                AnimationState.Jump => 10,
                AnimationState.Punch1 => 7,
                AnimationState.Punch2 => 11,
                AnimationState.DefenseAttack => 4,
                AnimationState.FireKick => 11,
                AnimationState.ExplosiveStrike => 9,
                _ => 6
            };
        }

        /// <summary>
        /// Gets the texture filename for a given animation state
        /// </summary>
        public static string GetTextureFileName(AnimationState state)
        {
            return state switch
            {
                AnimationState.Idle => "Idle.png",
                AnimationState.Walking => "Walking.png",
                AnimationState.Running => "Running.png",
                AnimationState.Crouch => "Crouch.png",
                AnimationState.Jump => "Jumping.png",
                AnimationState.Punch1 => "Energy_Wave.png",
                AnimationState.Punch2 => "Power_Strike.png",
                AnimationState.DefenseAttack => "Defense_attack.png",
                AnimationState.FireKick => "Fire_Kick.png",
                AnimationState.ExplosiveStrike => "Explosive_Strike.png",
                _ => "Idle.png"
            };
        }

        /// <summary>
        /// Sets the UV coordinates for the current sprite frame from a horizontal strip
        /// </summary>
        public static void SetSpriteFrame(int shader, int frame, AnimationState state, float sheetWidth)
        {
            // Get how many frames this animation has
            int frameCount = GetFrameCount(state);

            // Clamp frame index to valid range
            frame = frame % frameCount;

            // Use equal divisions of the texture (prevents size jitter)
            float w = 1f / frameCount;
            float x = frame * w;

            // Full height since each texture is a single row
            float y = 0f;
            float h = 1f;

            // Upload to shader uniforms
            GL.UseProgram(shader);
            int offsetLoc = GL.GetUniformLocation(shader, "uOffset");
            int sizeLoc = GL.GetUniformLocation(shader, "uSize");

            GL.Uniform2(offsetLoc, x, y);
            GL.Uniform2(sizeLoc, w, h);
        }
    }
}