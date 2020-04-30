using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : Entity
{
    public int maxHealth = 1;
    public int scoreReward = 10;

    int health;

    protected override void Awake()
    {
        base.Awake();
        health = maxHealth;
    }

    public virtual void ApplyDamage(int amount, bool playerIsSource)
    {
        health -= amount;
        if(health <= 0)
        {
            DestroyEntity();

            if(playerIsSource)
            {
                LevelController.Instance.RewardPlayer(scoreReward);
            }
        }
    }
}
