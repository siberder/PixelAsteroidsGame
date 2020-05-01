using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : EnemyBase
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] skins;
    public float minMoveSpeed = 5f;
    public float maxMoveSpeed = 10f;
    public Asteroid smallerAsteroidPrefab;
    public int smallerAsteroidsCount = 2;

    int skinIdx;

    private void Start()
    {
        //float force = Random.Range(minMoveSpeed, maxMoveSpeed);
        //entityRigidbody.AddForce(Random.insideUnitSphere.normalized * force);
        EntityRigidbody.AddTorque(Random.Range(90f, 360f) * Time.fixedDeltaTime);
    }

    public void SetForceTowardPoint(Vector3 point)
    {
        float force = Random.Range(minMoveSpeed, maxMoveSpeed);
        EntityRigidbody.velocity = (point - transform.position).normalized * force * Time.fixedDeltaTime;
    }

    public void SetSkin(int idx)
    {
        spriteRenderer.sprite = skins[idx];
        skinIdx = idx;
    }

    public void SetRandomSkin()
    {
        SetSkin(Random.Range(0, skins.Length));
    }

    protected override void DestroyEntityImplementation()
    {
        if (smallerAsteroidPrefab)
        {
            for (int i = 0; i < smallerAsteroidsCount; i++)
            {
                var asteroid = Instantiate(smallerAsteroidPrefab, transform.position, transform.rotation);
                asteroid.transform.position += Random.insideUnitSphere.normalized * ((CircleCollider2D)EntityCollider).radius;
                asteroid.SetSkin(skinIdx);
                asteroid.EntityRigidbody.AddForce(EntityRigidbody.velocity, ForceMode2D.Impulse);
                asteroid.EntityRigidbody.AddForce(asteroid.transform.position - transform.position, ForceMode2D.Impulse);

                LevelController.Instance.AddSpawnedEntity(asteroid);
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity entity = collision.collider.GetComponent<Entity>();
        if (entity)
        {
            entity.DestroyEntity();
        }

        DestroyEntity();
    }
}
