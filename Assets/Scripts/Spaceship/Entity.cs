using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Entity")]
    public Effect destroyEffect;

    public Collider2D EntityCollider { get; protected set; }
    public Rigidbody2D EntityRigidbody { get; protected set; }

    protected Vector3 topRightBoundCorner;
    protected Vector3 botLeftBoundCorner;

    public delegate void DestroyEntityDelegate(Entity source);
    public event DestroyEntityDelegate OnDestroy;

    public Entity SourceUser { get; set; }
    public bool SourceUserIsPlayer => SourceUser is SpaceshipController;

    public virtual bool Dead { get; set; }

    protected virtual void Awake()
    {
        EntityCollider = GetComponent<Collider2D>();
        EntityRigidbody = GetComponent<Rigidbody2D>();

        topRightBoundCorner = LevelController.Instance.TopRightBoundCorner + EntityCollider.bounds.extents;
        botLeftBoundCorner = LevelController.Instance.BotLeftBoundCorner - EntityCollider.bounds.extents;
    }

    protected void WrapWorldPosition()
    {
        Vector3 position = transform.position;

        if (transform.position.x > topRightBoundCorner.x)
        {
            position.x = botLeftBoundCorner.x;
        }
        else if (transform.position.x < botLeftBoundCorner.x)
        {
            position.x = topRightBoundCorner.x;
        }

        if (transform.position.y > topRightBoundCorner.y)
        {
            position.y = botLeftBoundCorner.y;
        }
        else if (transform.position.y < botLeftBoundCorner.y)
        {
            position.y = topRightBoundCorner.y;
        }

        transform.position = position;
    }

    protected virtual Effect CreateDestroyEffect()
    {
        if (destroyEffect)
        {
            var effect = Instantiate(destroyEffect, transform.position, transform.rotation);
            return effect;
        }

        return null;
    }

    public virtual void DestroyEntity(bool playerIsSource = false)
    {
        if(!Dead)
        {
            Dead = true;
            CreateDestroyEffect();
            OnDestroy?.Invoke(this);
            DestroyEntityImplementation();
            LevelController.Instance.RemoveDeadEntity(this);
        }
    }

    protected abstract void DestroyEntityImplementation();
}
