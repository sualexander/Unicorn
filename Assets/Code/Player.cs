using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 20;
    public float maxSpeed = 80;
    public float jumpHeight = 10;
    public float friction = 0.5f;

    Rigidbody2D rb;
    float movementInput;

    float jumpTime;
    public float jumpBuffer = 0.25f;

    bool flipped, isFlipping;
    public float flipTime = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x + (movementInput * speed), -maxSpeed, maxSpeed);
        if (Mathf.Abs(velocity.y) < 0.1f) {
            if (jumpTime > 0) {
                velocity.y = flipped ? -jumpHeight : jumpHeight;
            } else if (movementInput == 0) {
                velocity.x *= friction;
            }
        }

        rb.linearVelocity = velocity;
    }

    void Update()
    {
        jumpTime -= Time.deltaTime;
    }

    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>().x;
    }

    void OnJump()
    {
        jumpTime = jumpBuffer;
    }

    void OnFlip()
    {
        if (isFlipping) return;
        flipped = !flipped;
        rb.gravityScale *= -1;
        StartCoroutine(FlipPlayer());

        IEnumerator FlipPlayer()
        {
            isFlipping = true;
            float elapsed = 0;
            Quaternion start = transform.rotation;
            Quaternion target = start * Quaternion.Euler(0, 0, 180);
            while (elapsed < flipTime)
            {
                transform.rotation = Quaternion.Slerp(start, target, elapsed / flipTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = target;
            isFlipping = false;
        }
    }
}
