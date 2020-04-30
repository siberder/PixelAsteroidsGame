using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpaceshipController : Entity
{
    [Header("Settings")]
    public float acceleration = 100f;
    public float turnSpeed = 90f;
    public float weaponCooldown = 0.25f;

    [Header("References")]
    public GameObject visuals;
    public Animator animator;
    public GameObject thrustSprite;
    public Transform projectileEmitSource;
    public Projectile projectilePrefab;    

    float weaponCooldownLeft;
    bool invincible;
    bool dead;

    bool inputFire;
    float inputVertical;
    float inputHorizontal;

    protected override bool Dead
    {
        get => dead;

        set
        {
            dead = value;
            visuals.SetActive(!dead);
            UpdateEntityColliderActivity();
        }
    }

    private bool Anim_Invincible
    { 
        get => animator.GetBool("Invincible");
        set => animator.SetBool("Invincible", value);
    }

    private bool ThrustSpriteVisible
    {
        get => thrustSprite.activeSelf;
        set => thrustSprite.SetActive(value);
    }
    public bool Invincible
    {
        get => invincible;
        set
        {
            invincible = value;
            Anim_Invincible = value;
            UpdateEntityColliderActivity();
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (!Dead)
        {
            HandleInput();
        }

        WrapWorldPosition();
    }

    private void FixedUpdate()
    {
        if(!Dead)
        {
            HandleMoving();
            HandleFiring();
        }
    }

    void HandleInput()
    {
        inputVertical = Mathf.Clamp01(Input.GetAxis("Vertical"));
        inputHorizontal = Input.GetAxis("Horizontal");
        inputFire = Input.GetButton("Fire1") || Input.GetButton("Fire2") || Input.GetButton("Fire3");
    }

    void HandleFiring()
    {
        if (weaponCooldownLeft <= 0f)
        {
            if (inputFire)
            {
                var projectile = Instantiate(projectilePrefab, projectileEmitSource.position,
                                    Quaternion.LookRotation(projectileEmitSource.forward, projectileEmitSource.up));
                projectile.Init(this, EntityRigidbody.velocity.magnitude);

                weaponCooldownLeft = weaponCooldown;
            }
        }
        else
        {
            weaponCooldownLeft -= Time.deltaTime;
        }
    }

    void HandleMoving()
    {       
        ThrustSpriteVisible = inputVertical > float.Epsilon;

        EntityRigidbody.AddForce(inputVertical * transform.up * acceleration * Time.fixedDeltaTime);
        transform.Rotate(0, 0, -inputHorizontal * turnSpeed * Time.fixedDeltaTime);
    }

    void UpdateEntityColliderActivity()
    {
        EntityCollider.enabled = !dead && !invincible;
    }

    protected override void DestroyEntityImplementation()
    {
        LevelController.Instance.OnPlayerDied();
    }

    protected override Effect CreateDestroyEffect()
    {
        var effect = base.CreateDestroyEffect();
        if(effect is WreckEffect wreckEffect)
        {
            wreckEffect.AddForceToAll(EntityRigidbody.velocity);
        }

        return effect;
    }

    IEnumerator MakeInvincible_Routine()
    {
        Invincible = true;
        yield return new WaitForSeconds(LevelController.Instance.playerInvincibilityTime);
        Invincible = false;
    }

    public void Respawn()
    {
        Dead = false;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        EntityRigidbody.velocity = Vector3.zero;

        StartCoroutine(MakeInvincible_Routine());
    }
}
