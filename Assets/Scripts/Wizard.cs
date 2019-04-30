using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : MonoBehaviour {

    public float speed = 5.0f;
    public float jump = 5.0f;
    public float acc = 40.0f;

    public float health = 100;

    List<Ability> abilities;

    Ability abilityLeft;
    Ability abilityRight;
    Ability abilitySpace;
    Ability abilityShift;

    float shiftCooldown;
    bool shieldActive;

    Rigidbody2D myridg;
    GameObject shield;

    // Audio
    public AudioSource shot;

    private void Awake()
    {
        myridg = GetComponent<Rigidbody2D>();
        shield = transform.Find("Shield").gameObject;
    }

    // Use this for initialization
    void Start ()
    {
        ObjectPool projectileSpawner = GameObject.Find("ProjectileSpawner").GetComponent<ObjectPool>();
        ObjectPool bombSpawner = GameObject.Find("BombSpawner").GetComponent<ObjectPool>();
        abilityLeft = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            GameObject go = projectileSpawner.GetNextObject();
            Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
            go.transform.position = pos;
            Vector2 dir = (trgt - pos).normalized;
            rgb.velocity = dir * go.GetComponent<Projectile>().velocity + myridg.velocity;
            health -= 2;
            shot.Play();
        }); abilityRight = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            GameObject go = bombSpawner.GetNextObject();
            Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
            go.transform.position = pos;
            Vector2 dir = (trgt - pos).normalized;
            rgb.velocity = dir * go.GetComponent<ProjectileBomb>().velocity + myridg.velocity;
            health -= 10;
        });
        abilitySpace = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            shieldActive = true;
            health -= Time.deltaTime * 2; // 2 per second
            shield.SetActive(true);
            Vector2 dir = (trgt - pos).normalized;
            shield.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 45);
        });
        abilityShift = new Ability((Vector2 pos, Vector2 trgt) =>
        {
            myridg.velocity = new Vector2(myridg.velocity.x, myridg.velocity.y + jump * 2);
            health -= 10;
            shiftCooldown = 1;
        });
    }
	
	// Update is called once per frame
	void Update ()
    {
        // determine speed of the ground you're standing on
        Vector2 groundSpeed = Vector2.zero;
        if (myridg.IsTouchingLayers(LayerMask.GetMask("MovingTerrain")))
        {
            List<Collider2D> colls = new List<Collider2D>();
            myridg.GetContacts(colls);
            Collider2D movingplatform = colls.Find(c => c.gameObject.layer == LayerMask.NameToLayer("MovingTerrain"));
            if (movingplatform != null) groundSpeed = movingplatform.GetComponent<MovingPlatform>().velocity;
        }

        // Horizontal movement
        bool isGrounded = myridg.IsTouchingLayers(LayerMask.GetMask("Terrain","Ladder","MovingTerrain"));
        float h_vel = myridg.velocity.x;
        if (Input.GetButton("Right"))
        {
            if (h_vel < speed + groundSpeed.x)
            {
                h_vel += (isGrounded ? acc : (acc / 2)) * Time.deltaTime;
                if (h_vel > speed + groundSpeed.x)
                    h_vel = speed;
            }

        }
        else if (Input.GetButton("Left"))
        {
            if (h_vel > -speed + groundSpeed.x)
            {
                h_vel -= (isGrounded ? acc : (acc / 2)) * Time.deltaTime;
                if (h_vel < -speed + groundSpeed.x)
                    h_vel = -speed + groundSpeed.x;
            }
        } else
        {
            if (h_vel > groundSpeed.x)
            {
                h_vel -= acc / 2 * Time.deltaTime;
                if (h_vel < groundSpeed.x)
                    h_vel = groundSpeed.x;
            }
            else if (h_vel < groundSpeed.x)
            {
                h_vel += acc / 2 * Time.deltaTime;
                if (h_vel > groundSpeed.x)
                    h_vel = groundSpeed.x;
            }
        }
        myridg.velocity = new Vector2(h_vel, myridg.velocity.y);

        if (h_vel < groundSpeed.x -.1f)
            GetComponent<SpriteRenderer>().flipX = true;
        else if (h_vel > groundSpeed.x +.1f)
            GetComponent<SpriteRenderer>().flipX = false;

        // Vertical movement
        if (Input.GetButton("Up"))
        {
            if (myridg.IsTouchingLayers(LayerMask.GetMask("Terrain", "MovingTerrain")))
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
            } else if (myridg.IsTouchingLayers(LayerMask.GetMask("Ladder")))
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
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,-Camera.main.transform.position.z));

        if (Input.GetButtonDown("Fire1"))
        {
            abilityLeft.Fire(myridg.position, mousePos);
        }
        if (Input.GetButtonDown("Fire2"))
        {
            abilityRight.Fire(myridg.position, mousePos);
        }
        if (Input.GetButton("Fire3"))
        {
            abilitySpace.Fire(myridg.position, mousePos);
        }
        if (Input.GetButton("Fire4") && shiftCooldown <= 0)
        {
            abilityShift.Fire(myridg.position, mousePos);
        }
        shiftCooldown -= Time.deltaTime;

        if (shieldActive == false)
            shield.SetActive(false);
        shieldActive = false;
    }

    internal void GetHit(float damage)
    {
        health -= damage;
    }

    internal void ShieldHit(float damage)
    {
        health -= damage / 10;
    }

    internal void AddHealth(float health)
    {
        this.health += health;
        if (this.health > 100)
            this.health = 100;
    }
}
