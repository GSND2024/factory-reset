using System;
using UnityEngine;

public static class TurnSystem
{
    public static event Action OnTurnAdvanced;

    public static void AdvanceTurn()
    {
        OnTurnAdvanced?.Invoke();
    }
}
