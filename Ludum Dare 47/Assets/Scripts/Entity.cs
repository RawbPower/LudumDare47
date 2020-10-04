using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
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

    private bool freeze;

    private float angleAcceleration = 0.0f;
    private float angleVelocity = 0.0f;
    private float angleRelVel;
    private GameObject platform;

    private Collider2D ground;
    private bool isGrounded;
    public Transform[] feetPos;
    public Transform scan;
    public float scanRadius;
    public float checkRadius;
    public LayerMask whatIsGround;
    private Ring ring;

    public float freezeTime;
    private float freezeTimer;

    public ResetTransform resetTransform;

    public GameObject timer;

    private bool onPlatform;

    private Rigidbody2D rb;

    private void Awake()
    {
        resetTransform = new ResetTransform(transform);
        rb = GetComponent<Rigidbody2D>();
        onPlatform = false;
        freeze = false;
        freezeTimer = freezeTime;
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

        float move = Input.GetAxis("Horizontal");
    
        angle = Mathf.Atan2(transform.position.y, transform.position.x);
        radius = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y);

        acceleration = gravity;

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            Debug.Log("Jump");
            acceleration -= jump;
        }

        velocity += acceleration * Time.deltaTime;

        GetComponent<SpriteRenderer>().flipX = (angleVelocity < 0.0f);

        if (onPlatform)
        {
            acceleration = 0.0f;
            velocity = 0.0f;
        }

        if (Mathf.Abs(velocity) > maxSpeed)
        {
            velocity = Mathf.Sign(velocity)*maxSpeed;
        }

        angleVelocity = move * moveSpeed * Time.deltaTime;

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

        radius += velocity * Time.deltaTime;
        angle += angleVelocity * Time.deltaTime;

        rotation = angle * Mathf.Rad2Deg + 90.0f;


        rb.MovePosition(new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)));
        //rb.MoveRotation(rotation);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation);

    }

    private void Update()
    {
       Collider2D hit = Physics2D.OverlapCircle(scan.position, scanRadius, whatIsGround);

        if (Input.GetKeyDown(KeyCode.X))
        {
            freezeTimer = 0.0f;
            if (hit)
            {
                if (hit.gameObject.CompareTag("Freezable"))
                {
                    hit.gameObject.GetComponent<FreezeBlocks>().SetIsFrozen(true);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (freezeTimer < freezeTime)
        { 
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(scan.position, scanRadius);
            freezeTimer += Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            onPlatform = true;
            platform = collision.collider.gameObject;
        }
        else
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        collision.gameObject.GetComponent<EdgeCollider2D>().enabled = true;
        collision.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.05490196f, 0.09803922f, 0.8784314f, 1.0f);
        BoxCollider2D[] colliders = collision.gameObject.GetComponentsInChildren<BoxCollider2D>();

        foreach (BoxCollider2D bc in colliders)
        {
            bc.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.05490196f, 0.5950408f, 0.8784314f, 1.0f);
            bc.enabled = true;
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            var brain = (camera == null) ? null : camera.GetComponent<CinemachineBrain>();
            var vcam = (brain == null) ? null : brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            if (vcam != null)
                camera.GetComponent<CamMovement>().SetDesiredOrthographicSize(vcam.m_Lens.OrthographicSize / 2.0f);
        }

        resetTransform.position.y += 5.0f;

        collision.gameObject.GetComponent<CircleCollider2D>().enabled = false;

        collision.gameObject.GetComponent<Ring>().player = transform;

        timer.GetComponent<LoopClock>().level = collision.gameObject.GetComponent<Ring>();

        collision.gameObject.GetComponent<Ring>().ResetRing();
    }
}
