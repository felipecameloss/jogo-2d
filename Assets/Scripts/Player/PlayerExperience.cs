using System;
using UnityEngine;

// XP e nível. Cada nível ganho vira um "level up pendente" que o UpgradeManager consome.
public class PlayerExperience : MonoBehaviour
{
    public float firstLevelXP = 5f;
    public float xpIncreasePerLevel = 8f;
    public float xpMultiplier = 1f;

    public int Level { get; private set; } = 1;
    public float CurrentXP { get; private set; }
    public int PendingLevelUps { get; private set; }
    public float XPToNext => firstLevelXP + (Level - 1) * xpIncreasePerLevel;

    public event Action OnLevelUp;

    public void AddXP(float amount)
    {
        CurrentXP += amount * xpMultiplier;
        while (CurrentXP >= XPToNext)
        {
            CurrentXP -= XPToNext;
            Level++;
            PendingLevelUps++;
            OnLevelUp?.Invoke();
        }
    }

    public void ConsumePendingLevelUp()
    {
        if (PendingLevelUps > 0) PendingLevelUps--;
    }
}
