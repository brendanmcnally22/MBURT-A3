using UnityEngine;

public class PlayerMouseController : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v).normalized;

        transform.position += move * moveSpeed * Time.deltaTime;

        if (move != Vector3.zero)
            transform.forward = move;
    }
}