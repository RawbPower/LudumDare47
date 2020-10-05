using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    public float gravity;
    public float jump;
    public float maxSpeed;

    public float moveSpeed;
    public float friction;
    public float mass = 1.0f;

    public float offset;

    private float angle;
    private float radius;
    private float rotation;

    private float acceleration = 0.0f;
    private float velocity = 0.0f;

    private float angleAcceleration = 0.0f;
    private float angleVelocity = 0.0f;
    private float angleRelVel;
    private GameObject platform;

    private bool doJump;

    private Collider2D ground;
    private bool isGrounded;
    public Transform[] feetPos;
    public float checkRadius;
    public LayerMask whatIsGround;
    private Ring ring;
    public Ring currentLevel;

    public Animator animator;

    public ResetTransform resetTransform;

    private bool onPlatform;

    private Rigidbody2D rb;

    public Transform target;

    private void Awake()
    {
        doJump = false;
        resetTransform = new ResetTransform(transform);
        rb = GetComponent<Rigidbody2D>();
        onPlatform = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        //rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        radius = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y);
        angle = Mathf.Atan2(transform.position.y, transform.position.x);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ground = null;
        isGrounded = false;
        Collider2D tempGround;
        for (int i = 0; i < feetPos.Length; i++)
        {
            tempGround = Physics2D.OverlapCircle(feetPos[i].position, checkRadius, whatIsGround);
            isGrounded = isGrounded || tempGround;
            if (tempGround)
            {
                ground = tempGround;
            }
        }
        isGrounded = ground;

        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
            ring = ground.gameObject.GetComponent<Ring>();
            if (ring == null)
            {
                ring = ground.gameObject.GetComponent<FreezeBlocks>().parentTransform.gameObject.GetComponent<Ring>();
                if (ring == null)
                {
                    Debug.LogError("Error with rings!");
                }
            }
        }

        float targetAngle = target.GetComponent<Entity>().GetAngle();
        float myAngle = angle;
        
        if (targetAngle < 0.0f)
        {
            targetAngle += 2 * Mathf.PI;
        }

        if (myAngle < 0.0f)
        {
            myAngle += 2 * Mathf.PI;
        }

        float dist1 = myAngle - targetAngle;
        float dist2 = (2*Mathf.PI - myAngle) + targetAngle;

        float move;


        if (Mathf.Abs(dist1) < Mathf.Abs(dist2))
        {
            move = Mathf.Sign(dist1 + Mathf.PI);
        }
        else
        {
            move = -Mathf.Sign(dist2 - 2*Mathf.PI);
        }

        angle = Mathf.Atan2(transform.position.y, transform.position.x);
        radius = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y);

        acceleration = gravity;

        if (doJump && isGrounded)
        {
            animator.SetBool("IsJumping", true);
            acceleration -= jump;
            doJump = false;
        }

        velocity += acceleration * Time.deltaTime;

        if (move != 0.0f)
        {
            GetComponent<SpriteRenderer>().flipX = (move < 0.0f);
        }

        if (onPlatform && !doJump)
        {
            acceleration = 0.0f;
            velocity = 0.0f;
        }

        if (Mathf.Abs(velocity) > maxSpeed)
        {
            velocity = Mathf.Sign(velocity) * maxSpeed;
        }

        angleVelocity = move * (moveSpeed / currentLevel.GetComponent<Ring>().radius) * Time.deltaTime;

        if (onPlatform && angleVelocity == 0.0f)
        {
            if (ring)
            {
                if (!platform.GetComponent<FreezeBlocks>())
                {
                    angleVelocity = (ring.rotation / ring.radius) * Mathf.Deg2Rad + Mathf.Sign(ring.rotation) * offset;
                }
                else if (platform.GetComponent<FreezeBlocks>() && !platform.GetComponent<FreezeBlocks>().GetIsFrozen())
                {
                    angleVelocity = (ring.rotation / ring.radius) * Mathf.Deg2Rad + Mathf.Sign(ring.rotation) * offset;
                }
            }
        }

        animator.SetFloat("Speed", Mathf.Abs(move));

        radius += velocity * Time.deltaTime;
        angle += angleVelocity * Time.deltaTime;

        rotation = angle * Mathf.Rad2Deg + 90.0f;


        rb.MovePosition(new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)));
        //rb.MoveRotation(rotation);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation);

    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!doJump && isGrounded)
        {
            onPlatform = true;
            platform = collision.collider.gameObject;
        }
        else if (doJump || isGrounded)
        {
            onPlatform = false;
        }

        if (collision.collider.gameObject.GetComponent<Ring>())
        {
            ring = collision.collider.gameObject.GetComponent<Ring>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        onPlatform = false;
        ring = null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Freezable"))
        {
            doJump = true;
        }
    }

}
