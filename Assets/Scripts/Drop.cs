using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Drop : MonoBehaviour
{

    float value;
    float topValue;

    SpriteRenderer sprite;

    const float DETERIORATION_GRADE = 3;
    static Wizard wizard;

    private void Awake()
    {
        if (wizard == null)
            wizard = GameObject.Find("Wizard").GetComponent<Wizard>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        value -= DETERIORATION_GRADE * Time.deltaTime;
        if (value <= 0)
            gameObject.SetActive(false);
        sprite.color = Color.HSVToRGB(1, 1, value / topValue);
    }

    private void OnEnable()
    {
        topValue = 20;
        value = 20;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.layer == 8)
        {
            wizard.AddHealth(value);
            gameObject.SetActive(false);
        }
    }

}
