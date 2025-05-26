using TheAdventure.Models;

namespace TheAdventure.Models;

public class HealingItem : TemporaryGameObject
{
    public const int HealAmount = 25;  // Same as bomb damage
    
    public HealingItem(SpriteSheet spriteSheet, (int X, int Y) position) 
        : base(spriteSheet, 10.0, position)  // Healing items stay for 10 seconds
    {
        spriteSheet.ActivateAnimation("Spawn");
    }
}