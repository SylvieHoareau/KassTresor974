using UnityEngine;

public static class EnemyKillCounter
{
    public static int currentSessionKills = 0;

    public static void AddKill()
    {
        currentSessionKills++;
    }

    public static void Reset()
    {
        currentSessionKills = 0;
    }
}