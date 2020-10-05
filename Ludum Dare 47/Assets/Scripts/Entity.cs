using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Ring currentLevel;

    public float freezeTime;
    private float freezeTimer;
    private bool freeze;
    private bool pressFreeze;

    private bool win;
    public GameObject winText;

    public ResetTransform resetTransform;

    public GameObject timer;

    private bool onPlatform;

    private Rigidbody2D rb;

    public Animator animator;
    public Animator freezeAnimator;

    private void Awake()
    {
        win = false;
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
        if (!win)
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

            float move = Input.GetAxis("Horizontal");

            angle = Mathf.Atan2(transform.position.y, transform.position.x);
            radius = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y);

            acceleration = gravity;

            if (Input.GetKey(KeyCode.Space) && isGrounded)
            {
                animator.SetBool("IsJumping", true);
                acceleration -= jump;
            }

            velocity += acceleration * Time.deltaTime;

            if (move != 0.0f)
            {
                GetComponent<SpriteRenderer>().flipX = (move < 0.0f);
            }

            if (onPlatform && !Input.GetKey(KeyCode.Space))
            {
                acceleration = 0.0f;
                velocity = 0.0f;
            }

            if (Mathf.Abs(velocity) > maxSpeed)
            {
                velocity = Mathf.Sign(velocity) * maxSpeed;
            }

            angleVelocity = move * (moveSpeed/currentLevel.GetComponent<Ring>().radius) * Time.deltaTime;

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
        else
        {
            if (freezeTimer < 3.0f)
            {
                rb.MovePosition(new Vector2(0.0f, 0.0f));

                angleVelocity = 50 * moveSpeed * Time.deltaTime;
                angle += angleVelocity * Time.deltaTime;
                rotation = angle * Mathf.Rad2Deg + 90.0f;
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation);
                freezeTimer += Time.deltaTime;
            }
            else
            {
                GetComponent<SpriteRenderer>().enabled = false;
            }
          

            winText.GetComponent<Text>().enabled = true;
        }

    }

    private void Update()
    {
        freezeAnimator.SetBool("Freeze", false);
        freezeAnimator.SetBool("Unfreeze", false);

        if (!win)
        {
            Collider2D hit = Physics2D.OverlapCircle(scan.position, scanRadius, whatIsGround);

            if (freezeTimer < freezeTime)
            {
                freezeTimer += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                FindObjectOfType<AudioManager>().Play("Freeze");
                freezeTimer = 0.0f;
                freeze = true;
                pressFreeze = true;
                freezeAnimator.ResetTrigger("Unfreeze");
                freezeAnimator.SetTrigger("Freeze");
            }

            if (hit && freezeTimer > freezeTime && pressFreeze)
            {
                if (hit.gameObject.CompareTag("Freezable") && !hit.gameObject.GetComponent<FreezeBlocks>().GetIsFrozen())
                {
                    hit.gameObject.GetComponent<FreezeBlocks>().SetIsFrozen(true);
                }
                pressFreeze = false;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                FindObjectOfType<AudioManager>().Play("Unfreeze");
                freeze = false;
                freezeAnimator.ResetTrigger("Freeze");
                freezeAnimator.SetTrigger("Unfreeze");
                if (hit)
                {
                    if (hit.gameObject.CompareTag("Freezable") && hit.gameObject.GetComponent<FreezeBlocks>().GetIsFrozen())
                    {
                        hit.gameObject.GetComponent<FreezeBlocks>().SetIsFrozen(false);
                    }
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!Input.GetKey(KeyCode.Space) && isGrounded)
        {
            onPlatform = true;
            platform = collision.collider.gameObject;
        }
        else if (Input.GetKey(KeyCode.Space) || isGrounded)
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
        if (collision.gameObject.CompareTag("Ring"))
        {
            FindObjectOfType<AudioManager>().Play("Ding");

            // New Ring
            collision.gameObject.GetComponent<EdgeCollider2D>().enabled = true;
            collision.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.05490196f, 0.09803922f, 0.8784314f, 1.0f);
            BoxCollider2D[] colliders = collision.gameObject.GetComponentsInChildren<BoxCollider2D>();

            foreach (BoxCollider2D bc in colliders)
            {
                bc.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.05490196f, 0.5950408f, 0.8784314f, 1.0f);
                bc.enabled = true;
            }

            resetTransform.position.y += 5.0f;

            currentLevel.ResetRing(timer.GetComponent<LoopClock>());

            // Old Ring
            currentLevel.gameObject.GetComponent<EdgeCollider2D>().enabled = false;
            currentLevel.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.2910f, 0.3149f, 0.5660f, 1.0f);
            BoxCollider2D[] oldColliders = currentLevel.gameObject.GetComponentsInChildren<BoxCollider2D>();

            foreach (BoxCollider2D bc in oldColliders)
            {
                bc.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.2910f, 0.3149f, 0.5660f, 1.0f);
                bc.enabled = false;
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                var brain = (camera == null) ? null : camera.GetComponent<CinemachineBrain>();
                var vcam = (brain == null) ? null : brain.ActiveVirtualCamera as CinemachineVirtualCamera;
                if (vcam != null)
                {
                    camera.GetComponent<CamMovement>().SetDesiredOrthographicSize(3.5f * collision.gameObject.GetComponent<Ring>().radius);
                    if (collision.gameObject.GetComponent<Ring>().radius == 1.0f)
                    {
                        vcam.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
                        camera.GetComponent<CamMovement>().SetDesiredOrthographicSize(7.0f);
                        vcam.Follow = null;
                    }
                }
            }

            collision.gameObject.GetComponent<CircleCollider2D>().enabled = false;

            collision.gameObject.GetComponent<Ring>().player = transform;

            timer.GetComponent<LoopClock>().level = collision.gameObject.GetComponent<Ring>();

            currentLevel = collision.gameObject.GetComponent<Ring>();

            collision.gameObject.GetComponent<Ring>().ResetRing(timer.GetComponent<LoopClock>());

        }
        else if (collision.gameObject.CompareTag("Portal"))
        {
            freezeTimer = 0.0f;
            FindObjectOfType<AudioManager>().Play("Victory");
            Camera camera = Camera.main;
            if (camera != null)
            {
                var brain = (camera == null) ? null : camera.GetComponent<CinemachineBrain>();
                var vcam = (brain == null) ? null : brain.ActiveVirtualCamera as CinemachineVirtualCamera;
                if (vcam != null)
                {
                    vcam.Follow = null;
                }
            }

            win = true;
        }
    }

    public float GetAngle()
    {
        return angle;
    }

    public void SetAngle(float a)
    {
        angle = a;
    }
}
