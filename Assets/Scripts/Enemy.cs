using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 5.0f;
    public float acc = 40.0f;

    public int projectiles = 0;
    float shotCooldown = 0;
    float timeBetweenShots = 1;

    public float shieldMagic = 0;
    public float shieldMagicLeft = 0;
    bool shieldActive = false;
    const float TIME_BETWEEN_SHIELD_EVALUATIONS = 1.5f;
    float timeUntillNextShieldEvaluation;

    float scootingTimer = TIME_FOR_SCOOT;
    const float TIME_FOR_SCOOT = 1;

    enum Stance{DECIDING, CHARGING, SHOOTING, SHIELDING, SCOOTING, PROTECTING, GUARDING }

    Stance stance = Stance.DECIDING;

    public Collider2D FrontBumper;
    public Collider2D BackBumper;
    bool isGrounded = false;
    bool gettingUnstuck = false;

    Transform target;

    Rigidbody2D myridg;
    Collider2D myCol;
    GameObject shieldGO;

    static ObjectPool projectileSpawner;
    static ObjectPool dropsSpawner;
    static Transform wizardProjectiles;

    private void Awake()
    {
        if (dropsSpawner == null)
            dropsSpawner = GameObject.Find("DropsSpawner").GetComponent<ObjectPool>();
        if (projectileSpawner == null)
            projectileSpawner = GameObject.Find("EnemyProjectileSpawner").GetComponent<ObjectPool>();
        if (wizardProjectiles == null)
            wizardProjectiles = GameObject.Find("ProjectileSpawner").transform;
        myridg = GetComponent<Rigidbody2D>();
        myCol = GetComponent<Collider2D>();
        shieldGO = transform.Find("Shield").gameObject;
    }

    // Use this for initialization
    void Start () {
        target = GameObject.Find("Wizard").transform;
	}

    private void OnEnable()
    {
        stance = Stance.DECIDING;
    }

    // Update is called once per frame
    void Update() {

        int move = 0; // 1 = right, -1 = left
        switch (stance)
        {
        case Stance.DECIDING:
            if (projectiles != 0)
                stance = Stance.SHOOTING;
            else if (shieldMagicLeft != 0)
                stance = Stance.PROTECTING;
            else
                stance = Stance.CHARGING;
            break;

        case Stance.SHOOTING:
            shotCooldown -= Time.deltaTime;
            if (shotCooldown <= 0)    // try to shoot
            {
                var solution = CanYouHitTheWizard();
                if (solution != null)
                {
                    if (myridg.velocity.magnitude > speed / 3)  // Wait untill you are standing more or less still
                        break;
                    RaycastHit2D rch = Physics2D.Raycast(transform.position, target.position - transform.position, 50, LayerMask.GetMask("Wizard", "Terrain"));
                    if (rch.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
                        Shoot(solution.Item1);
                    else
                        Shoot(solution.Item2);
                    shotCooldown = timeBetweenShots;
                    if (projectiles <= 0)
                        stance = Stance.CHARGING;
                    else
                        stance = Stance.SCOOTING;
                    break;
                } else
                    stance = Stance.SCOOTING;
            }
            break;
        case Stance.GUARDING:
            shotCooldown -= Time.deltaTime;
            if (shotCooldown <= 0)    // try to shoot
            {
                var solution = CanYouHitTheWizard();
                if (solution != null)
                {
                    RaycastHit2D rch = Physics2D.Raycast(transform.position, target.position - transform.position, 50, LayerMask.GetMask("Wizard", "Terrain"));
                    if (rch.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
                        Shoot(solution.Item1);
                    else
                        Shoot(solution.Item2);
                    shotCooldown = timeBetweenShots;
                    if (projectiles <= 0)
                        stance = Stance.CHARGING;
                    break;
                }
            }
            break;
        case Stance.SCOOTING:
            scootingTimer -= Time.deltaTime;
            if (scootingTimer <= 0)
            {
                scootingTimer = TIME_FOR_SCOOT;
                stance = Stance.SHOOTING;
                break;
            }
            goto case Stance.CHARGING;
        case Stance.PROTECTING:
            if (shieldActive)
                shieldMagicLeft -= Time.deltaTime;
            if (shieldMagicLeft <= 0)
            {
                shieldActive = false;
                shieldGO.SetActive(false);
                stance = Stance.CHARGING;
                break;
            }
            timeUntillNextShieldEvaluation -= Time.deltaTime;
            if (timeUntillNextShieldEvaluation <= 0)
            {
                if (AreThereIncommingProjectiles())
                {
                    shieldActive = true;
                    shieldGO.SetActive(true);
                } else
                {
                    shieldActive = false;
                    shieldGO.SetActive(false);
                }
                timeUntillNextShieldEvaluation = TIME_BETWEEN_SHIELD_EVALUATIONS;
            }
            if (shieldActive)
            {
                if (target.position.x < transform.position.x)
                    shieldGO.transform.rotation = Quaternion.Euler(0, 0, 90);
                else
                    shieldGO.transform.rotation = Quaternion.Euler(0, 0, 0);
            } else
                goto case Stance.CHARGING;
            break;
        case Stance.CHARGING:
            if (target.position.x < transform.position.x)
                move = -1;
            else
                move = 1;
            break;
        }

        // Movement
        {
            float h_vel = myridg.velocity.x;
            // failsafe to get unstuck
            if (myridg.velocity == Vector2.zero)
                gettingUnstuck = true;
            if (gettingUnstuck == true)
            {
                move = -move;
                if (Mathf.Abs(h_vel) > speed / 2)
                    gettingUnstuck = false;
            }
            if (move == 1)
            {
                if (h_vel < speed)
                {
                    h_vel += acc * Time.deltaTime;
                    if (h_vel > speed)
                        h_vel = speed;
                }

            } else if (move == -1)
            {
                if (h_vel > -speed)
                {
                    h_vel -= acc * Time.deltaTime;
                    if (h_vel < -speed)
                        h_vel = -speed;
                }
            } else
            {
                if (h_vel > 0)
                {
                    h_vel -= acc / 2 * Time.deltaTime;
                    if (h_vel < 0)
                        h_vel = 0;
                } else if (h_vel < 0)
                {
                    h_vel += acc / 2 * Time.deltaTime;
                    if (h_vel > 0)
                        h_vel = 0;
                }
            }
            myridg.velocity = new Vector2(h_vel, myridg.velocity.y);
            if (h_vel > 0)
                GetComponent<SpriteRenderer>().flipX = true;
            else
                GetComponent<SpriteRenderer>().flipX = false;
        }
        // Jumping
        if ((move == 1 && FrontBumper.IsTouchingLayers())|| (move == -1 && BackBumper.IsTouchingLayers()))
        {
            if (isGrounded)
            {
                var points = new ContactPoint2D[2];
                Vector2 normal;
                if (2 == myridg.GetContacts(points))
                {
                    normal = (points[0].normal + points[1].normal) / 2;
                } else
                {
                    normal = points[0].normal;
                }
                myridg.velocity = new Vector2(myridg.velocity.x, jump / 2) + normal.normalized * (jump / 2);
            }
        }
        // climbing ladders
        if (myCol.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            float v_vel = myridg.velocity.y;
            if (v_vel < 0)
                v_vel += (acc * 2 - Physics2D.gravity.y) * Time.deltaTime;
            else if (v_vel < speed / 2)
            {
                v_vel += (acc * .25f - Physics2D.gravity.y) * Time.deltaTime;
                if (v_vel > speed / 2)
                    v_vel = speed / 2;
            }
            myridg.velocity = new Vector2(myridg.velocity.x, v_vel);
        }
        isGrounded = false;
    }

    private Tuple<float,float> CanYouHitTheWizard()
    {
        // Can you hit the wizard?
        float s = Projectile.BLOB_VELOCITY;
        float ss = s * s;
        float x = target.position.x - transform.position.x;
        float y = target.position.y - transform.position.y;
        float g = 9.81f;

        float root = ss * ss - g * (g * x * x + 2 * y * ss);
        if (root < 0)
            return null;

        float angle1 = Mathf.Atan2(ss - Mathf.Sqrt(root), g * x);   // Low angle
        float angle2 = Mathf.Atan2(ss + Mathf.Sqrt(root), g * x);   // High angle
        return new Tuple<float, float>(angle1, angle2);
    }

    private void Shoot(float angle)
    {
        GameObject go = projectileSpawner.GetNextObject();
        Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
        go.transform.position = transform.position;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rgb.velocity = dir * go.GetComponent<Projectile>().velocity + myridg.velocity;
        projectiles--;
    }

    private bool AreThereIncommingProjectiles()
    {
        for (int i = 0; i < wizardProjectiles.childCount; i++)
        {
            Transform projectile = wizardProjectiles.GetChild(i);
            if (projectile.gameObject.activeSelf && (projectile.position - transform.position).magnitude < 10)
                return true;
        }
        return false;
    }

    public void GetHit()
    {
        GameObject drop = dropsSpawner.GetNextObject();
        drop.transform.position = transform.position;
        gameObject.SetActive(false);
    }

    public void ShieldHit()
    {
        shieldMagicLeft -= 2;
        timeUntillNextShieldEvaluation = TIME_BETWEEN_SHIELD_EVALUATIONS;
    }

    public void SetAsGuard()
    {
        stance = Stance.GUARDING;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
        {
            collision.collider.gameObject.GetComponent<Wizard>().GetHit(40);
            GetHit();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
            collision.collider.gameObject.layer == LayerMask.NameToLayer("Ladder"))
            isGrounded = true;
    }
}
