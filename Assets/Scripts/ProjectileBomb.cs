using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBomb : MonoBehaviour {

    public int clusterMunitions = 10;
    public const float BOMB_VELOCITY = 12;

    static ObjectPool projectilePool;

    float timer;
    Rigidbody2D myRidg;

    public GameObject audio;

    private void Awake()
    {
        if (projectilePool == null)
            projectilePool = GameObject.Find("ProjectileSpawner").GetComponent<ObjectPool>();
        myRidg = GetComponent<Rigidbody2D>();
    }

    public float velocity { get { return BOMB_VELOCITY; } }

    public void OnEnable()
    {
        timer = .3f;
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        RaycastHit2D rch = Physics2D.Raycast(transform.position, myRidg.velocity, 1.5f, LayerMask.GetMask("Enemies", "Terrain", "MovingTerrain"));
        if (timer <=0 && rch.collider != null)
        { Explode(); Debug.Log("Prox detonation"); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
        Debug.Log("Collison detonation");
    }

    private void Explode()
    {
        var go = Instantiate(audio, transform.position, Quaternion.identity);
        Destroy(go, 0.5f);
        gameObject.SetActive(false);
        for (int i = 0; i < clusterMunitions; i++)
        {
            FireProjectile(360 / clusterMunitions * i);
        }
    }

    void FireProjectile(float angle)
    {
        GameObject go = projectilePool.GetNextObject();
        Rigidbody2D rgb = go.GetComponent<Rigidbody2D>();
        go.transform.position = transform.position;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        rgb.velocity = dir * go.GetComponent<Projectile>().velocity + myRidg.velocity;
    }
}
