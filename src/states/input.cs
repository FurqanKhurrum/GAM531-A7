namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Represents the current input state from the player
    /// </summary>
    public struct InputState
    {
        // Basic movement
        public bool Left;
        public bool Right;
        public bool Jump;
        public bool Sprint;

        // Basic actions
        public bool Crouch;           // For Crouch.png
        public bool Defend;           // Defense stance

        // Basic attacks
        public bool Attack;           // For Punch_1.png / Punch_2.png combo

        // Special attacks
        public bool DefenseAttack;    // For Defense_attack.png (counter/parry)
        public bool FireKick;         // For Fire_Kick.png
        public bool ExplosiveStrike;  // For Explosive_Strike.png

        // Edge-triggered presses (this-frame only)
        public bool LeftPressed;
        public bool RightPressed;
        public bool AttackPressed;    // For combo system
        public bool DefenseAttackPressed;
        public bool FireKickPressed;
        public bool ExplosiveStrikePressed;

        // Helper properties
        public bool IsMoving => Left || Right;
        public int MoveDirection => Left ? -1 : (Right ? 1 : 0);
        public bool IsAnyAttack => Attack || DefenseAttack || FireKick || ExplosiveStrike;
        public bool IsAnySpecialAttack => DefenseAttack || FireKick || ExplosiveStrike;
    }
}