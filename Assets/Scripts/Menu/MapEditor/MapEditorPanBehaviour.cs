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
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Camera.main.transform.position += 
            new Vector3(x, y, 0f) * Camera.main.orthographicSize * speed;
    }
}
