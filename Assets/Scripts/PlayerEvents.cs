using System;
using UnityEngine;

public class PlayerEvents : MonoBehaviour {
    public static PlayerEvents instance;

    private void Awake() {
        instance = this;
    }

    public event Action onPlayerJump;
    public void e_PlayerJump() {
        if (onPlayerJump != null) onPlayerJump();
    }

    public event Action onPlayerLand;
    public void e_PlayerLand() {
        if (onPlayerLand != null) onPlayerLand();
    }
}