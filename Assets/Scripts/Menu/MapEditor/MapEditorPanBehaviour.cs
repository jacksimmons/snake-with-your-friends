using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MapEditorPanBehaviour : MonoBehaviour
{
    private const float speed = 0.01f;

    // Update is called once per frame
    private void Update()
    {
        float x = 0;
        float y = 0;

        if (Input.GetKey(KeyCode.A))
            x -= 1;
        if (Input.GetKey(KeyCode.D))
            x += 1;
        if (Input.GetKey(KeyCode.W))
            y += 1;
        if (Input.GetKey(KeyCode.S))
            y -= 1;

        Camera.main.transform.position += 
            new Vector3(x, y, 0f) * Camera.main.orthographicSize * speed;
    }
}
