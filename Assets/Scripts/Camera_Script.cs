using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Script : MonoBehaviour {

    public Transform player;
    Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        Vector2 mouse2 = camera.ScreenToWorldPoint(Input.mousePosition) - player.position;
        Vector3 mouse = (mouse2.sqrMagnitude > 9 ? mouse2.normalized * 3 : mouse2) / 3;
        transform.position = player.position + mouse + new Vector3(0, 0, -10);
	}
}
