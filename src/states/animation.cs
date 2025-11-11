namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Possible animation states for the character
    /// </summary>
    public enum AnimationState
    {
        Idle,           // Default standing animation
        Walking,        // Walking.png
        Running,        // Stop_Running.png (running animation)
        Crouch,         // Crouch.png
        Jump,           // Jumping.png
        Punch1,         // Punch_1.png
        Punch2,         // Punch_2.png
        DefenseAttack,  // Defense_attack.png
        FireKick,       // Fire_Kick.png
        ExplosiveStrike // Explosive_Strike.png
    }

    /// <summary>
    /// Direction the character is facing
    /// </summary>
    public enum FacingDirection
    {
        Right,
        Left
    }
}