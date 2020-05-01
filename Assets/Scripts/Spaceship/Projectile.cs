using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    [Header("Settings")]
    public float moveSpeed = 10f;
    public float lifetime = 3f;

    public float StartSpeed { get; private set; }
    public override bool Dead
    {
        get => base.Dead; 
        set
        {
            base.Dead = value;
            EntityCollider.enabled = !value;
        }
    }

    Vector2 NextPoint => transform.position + (transform.up * (moveSpeed + StartSpeed) * Time.fixedDeltaTime);


    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (Dead)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if(!Dead)
        {
            transform.position = NextPoint;
        }
    }

    protected override void DestroyEntityImplementation()
    {
        Dead = true;
    }

    public void Init(Entity source, float startSpeed)
    {
        SourceUser = source;
        StartSpeed = startSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Dead)
        {
            OnHit(collision.gameObject, collision.transform.position);
        }
    }

    void OnHit(GameObject gameObject, Vector2 point)
    {
        Entity entity = gameObject.GetComponent<Entity>();

        entity.DestroyEntity(SourceUserIsPlayer);
        DestroyEntity();
    }
}
