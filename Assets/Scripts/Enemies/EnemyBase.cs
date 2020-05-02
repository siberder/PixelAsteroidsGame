using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : Entity
{
    public int scoreReward = 10;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void DestroyEntity(bool playerIsSource = false)
    {
        base.DestroyEntity();
        if (playerIsSource)
        {
            GameManager.Instance.RewardPlayer(scoreReward);
        }
    }
}
