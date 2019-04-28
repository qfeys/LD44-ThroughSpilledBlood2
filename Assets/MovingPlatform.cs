using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public Vector2 position1;
    public Vector2 position2;

    public float speed = 1;

    public Vector2 velocity;

    Rigidbody2D myRidg;

    bool direction;

    // Start is called before the first frame update
    void Start()
    {
        myRidg = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 position1, Vector2 position2, float speed = 1)
    {
        this.position1 = position1; this.position2 = position2; this.speed = speed;
        direction = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (((direction ? position1 : position2) - (Vector2)transform.position).magnitude < 0.1f)
            direction = !direction;
        Vector2 dir = ((direction ? position1 : position2) - (Vector2)transform.position).normalized;
        velocity = dir * speed;
        myRidg.velocity = velocity;
    }
}
