using TheAdventure.Scripting;
using System;
using TheAdventure;

public class RandomHealing : IScript
{
    DateTimeOffset _nextItemTimestamp;

    public void Initialize()
    {
        _nextItemTimestamp = DateTimeOffset.UtcNow.AddSeconds(Random.Shared.Next(5, 10));
    }

    public void Execute(Engine engine)
    {
        if (_nextItemTimestamp < DateTimeOffset.UtcNow)
        {
            _nextItemTimestamp = DateTimeOffset.UtcNow.AddSeconds(Random.Shared.Next(5, 10)); // Less frequent than bombs
            var playerPos = engine.GetPlayerPosition();
            var itemPosX = playerPos.X + Random.Shared.Next(-100, 100); // Wider spawn range than bombs
            var itemPosY = playerPos.Y + Random.Shared.Next(-100, 100);
            engine.AddHealingItem(itemPosX, itemPosY, false);
        }
    }
}