using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBomb : MonoBehaviour {

    public int clusterMunitions = 6;
    public const float BOMB_VELOCITY = 12;

    static ObjectPool projectilePool;

    public GameObject audio;

    private void Awake()
    {
        if (projectilePool == null)
            projectilePool = GameObject.Find("ProjectileSpawner").GetComponent<ObjectPool>();
    }

    public float velocity { get { return BOMB_VELOCITY; } }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
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
        rgb.velocity = dir * go.GetComponent<Projectile>().velocity;
    }
}
