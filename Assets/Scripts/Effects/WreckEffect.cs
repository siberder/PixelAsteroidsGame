using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckEffect : Effect
{
    public float additionalLifetime = 3f;
    public float wreckForce = 10f;
    public List<SpriteRenderer> parts = new List<SpriteRenderer>();

    Rigidbody2D[] rigidBodies;

    protected override float Lifetime => GameManager.Instance.playerRespawnDelay + additionalLifetime;

    private void Awake()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        StartCoroutine(Wreck_Routine());
    }

    public void AddForceToAll(Vector2 force, ForceMode2D forceMode2D = ForceMode2D.Impulse)
    {
        foreach (var rbody in rigidBodies)
        {
            rbody.AddForce(force, forceMode2D);
        }
    }

    IEnumerator Wreck_Routine()
    {
        foreach (var rbody in rigidBodies)
        {
            var force = rbody.transform.localPosition * wreckForce;
            rbody.AddForce(force, ForceMode2D.Impulse);
            rbody.AddTorque((Random.value - 1f) * 90f);
        }

        for (float _animTime = 0; _animTime < Lifetime; _animTime += Time.deltaTime)
        {
            var t = (_animTime / Lifetime);
            foreach (var part in parts)
            {
                part.SetAlpha(1f - t);
            }

            yield return null;
        }
    }
}
