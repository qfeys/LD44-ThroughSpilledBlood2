using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    public float lifetime = 2;
    public float damage = 10;
    float lifeLeft;
    bool hasHitAnything;

    public const float BLOB_VELOCITY = 10;

    private void OnEnable()
    {
        lifeLeft = lifetime;
        hasHitAnything = false;
    }

    // Update is called once per frame
    void Update () {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft < 0 && hasHitAnything)
            EndOfLife();
	}

    public float velocity { get { return BLOB_VELOCITY; } }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        hasHitAnything = true;
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            collision.collider.GetComponent<Enemy>().GetHit();
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wizard"))
        {
            collision.collider.GetComponent<Wizard>().GetHit(damage);
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            collision.collider.GetComponentInParent<Wizard>().ShieldHit(damage);
            EndOfLife();
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("EnemyShield"))
        {
            try
            {
                collision.collider.GetComponentInParent<Enemy>().ShieldHit();
            }catch(NullReferenceException e)
            {
                Debug.LogError("That null eference error: projectile: " + gameObject + ", shield:" + collision.collider.gameObject);
                Enemy parent = collision.collider.gameObject.transform.parent.GetComponent<Enemy>();
                Debug.LogError("Enemy: " + parent + " Shield level: " + parent.shieldMagicLeft);
            }
            EndOfLife();
        }
    }

    private void EndOfLife()
    {
        gameObject.SetActive(false);
    }
}
