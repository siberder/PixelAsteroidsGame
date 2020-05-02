using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpaceship : EnemyBase
{
    [Header("Settings")]
    public float acceleration = 100f;
    public float turnSpeed = 90f;
    public float weaponCooldown = 0.25f;

    [Header("References")]
    public GameObject visuals;
    public Transform projectileEmitSource;
    public Projectile projectilePrefab;

    Vector3 randomPoint;
    float weaponCooldownLeft;

    public override bool Dead
    {
        get => base.Dead;

        set
        {
            base.Dead = value;
            visuals.SetActive(!base.Dead);
            EntityCollider.enabled = !base.Dead;
        }
    }

    void GetNewRandomPoint()
    {
        randomPoint = GameManager.Instance.GetRandomPointInPlayArea();
    }

    public void Init()
    {
        GetNewRandomPoint();
    }

    private void FixedUpdate()
    {
        var player = GameManager.Instance.Player;
        bool isInPlayArea = GameManager.Instance.IsPointInPlayArea(transform.position);

        if(!Dead)
        {
            EntityRigidbody.AddForce((randomPoint - transform.position).normalized * acceleration * Time.fixedDeltaTime);

            if(Vector3.Distance(transform.position, randomPoint) < 1f)
            {
                GetNewRandomPoint();
            }

            if (!player.Dead)
            {
                var rotationToPlayer = Quaternion.LookRotation(Vector3.forward, player.transform.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToPlayer, turnSpeed * Time.fixedDeltaTime);
                
                if (isInPlayArea && Quaternion.Angle(transform.rotation, rotationToPlayer) < 5f)
                {
                    HandleFiring();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(randomPoint, 0.05f);
    }

    void HandleFiring()
    {
        if (weaponCooldownLeft <= 0f)
        {
            var projectile = Instantiate(projectilePrefab, projectileEmitSource.position,
                                Quaternion.LookRotation(projectileEmitSource.forward, projectileEmitSource.up));
            projectile.Init(this, EntityRigidbody.velocity.magnitude);

            weaponCooldownLeft = weaponCooldown;
        }
        else
        {
            weaponCooldownLeft -= Time.deltaTime;
        }
    }

    protected override Effect CreateDestroyEffect()
    {
        var effect = base.CreateDestroyEffect();
        if (effect is WreckEffect wreckEffect)
        {
            wreckEffect.AddForceToAll(EntityRigidbody.velocity);
        }

        return effect;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity entity = collision.collider.GetComponent<Entity>();
        if (entity is SpaceshipController)
        {
            entity.DestroyEntity();
        }

        DestroyEntity();
    }


    protected override void DestroyEntityImplementation()
    {
        Destroy(gameObject);
    }
}
