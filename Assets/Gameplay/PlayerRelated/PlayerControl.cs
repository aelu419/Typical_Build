using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl Instance
    {
        get { return _instance; }
    }

    private static PlayerControl _instance;

    public PlayerSFXLibrary sfx_lib;

    public float charSize; //the height of the main character, in world units
    [HideInInspector] public Rect collider_bounds;

    //player state machine related 
    public float climb_threshold; //the threshold for height difference, above it triggers climbing
    [HideInInspector] public bool in_climb;
    //private float climb_extent; //the initial height difference when initiating a climb

    private bool light_toggle;

    //movement related
    public float climb_speed, accel, x_vel_max;
    public Vector3 destination;
    [HideInInspector] public Vector3 destination_override;
    [HideInInspector] public Vector3 relation_to_destination; //negative or positive; 
                                                       //sign change means the player has either 
                                                       //arrived or rushed pass the destination
    [HideInInspector] public bool new_order;
    [HideInInspector] public bool direction; //true when facing right
    private Vector2 velocity_temp;
    private bool velocity_override;
    //private Vector3 relation_temp;
    //private ContactPoint2D[] cp;

    //private ReadingManager rManager;
    //private SpriteRenderer renderer_; //the sprite renderer assigned to the main character
    private Rigidbody2D rigid;

    private Animator animator;

    private BoxCollider2D box;

    [HideInInspector]
    public HeadLightControl head_light_controller;

    [HideInInspector]
    public List<string> neighbours;
    public ContactPoint2D[] contacts;
    //[HideInInspector] public List<GameObject> word_blocks_in_contact;
    //[HideInInspector] public string word_blocks_in_contact_str;

    //private float stuck_time = 0.0f; //to deal with really weird situations

    public float light_progress; //0 is shut off, 1 is up
    //private SpriteRenderer torso;

    private event System.Action OnFirstFrame;

    void Awake()
    {
        destination = Vector3.zero;
        destination_override = Vector3.zero;

        OnFirstFrame = null;

        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        //register events
        EventManager.Instance.OnProgression += OnProgression;
        EventManager.Instance.OnRegression += OnRegression;

        //connect to rest of the game
        rigid = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
        animator.SetBool("in_climb", false);
        //torso = transform.GetChild(1).GetComponent<SpriteRenderer>();
        head_light_controller = transform.GetChild(0).GetComponent<HeadLightControl>();

        //cControler = CameraController.Instance;

        box = GetComponent<BoxCollider2D>();

        //set character state
        in_climb = false;
        light_toggle = false;
        new_order = false;
        contacts = new ContactPoint2D[50];

        //set coordinate related fields
        transform.localScale = new Vector3(charSize, charSize, charSize);
        UpdateRelativePosition();
        /*relation_temp = new Vector3(
            relation_to_destination.x,
            relation_to_destination.y,
            relation_to_destination.z
            );*/
        
        collider_bounds = new Rect(
            box.bounds.min,
            box.bounds.size
            );

    }

    public void SpawnAtRoot(Vector2 spawn_root)
    {
        OnFirstFrame += () =>
        {
            //Debug.LogError(spawn_root.x + ", " + spawn_root.y);
            rigid.position = new Vector2(
               spawn_root.x,
               spawn_root.y + charSize / 2f + 1f
               );

            destination = new Vector3(
                rigid.position.x, 
                rigid.position.y, 
                0
                );

            light_progress = 0;
        };
    }

    public void UpdateDestination(float new_x)
    {
        destination_override = new Vector3(new_x, 0, 0);
        new_order = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (OnFirstFrame != null)
        {
            OnFirstFrame();
            OnFirstFrame = null;
        }

        //basic variables for the rest of the method
        collider_bounds = new Rect(
            box.bounds.min,
            box.bounds.size
            );


        //control the motion of the player:

        /* ---deprecated---
        //freeze the character if it is not inside camera range
        if (rigid.position.x < cControler.CAM.xMin
            || rigid.position.x > cControler.CAM.xMax)
        {
            //Debug.Log("outside camera scope");
            rigid.velocity = Vector2.zero;
            return;
        }
        */

        rigid.gravityScale = 1.0f;
        //change of destination by external scripts
        if (new_order)
        {
            destination = new Vector3(
                destination_override.x,
                destination.y,
                destination.z);
            destination_override = Vector3.zero;
            new_order = false;
        }

        UpdateRelativePosition();

        if (velocity_override)
        {
            rigid.velocity = new Vector2(velocity_temp.x, rigid.velocity.y);
        }

        if (!in_climb)
        {
            if (!Approximately(relation_to_destination.x, 0))
            {
                float x_vel = rigid.velocity.x;

                //stopping distance under constant acceleration
                float stopping_distance_x = rigid.velocity.x * rigid.velocity.x / 2 / accel;


                //decide first if should decelerate or accelerate
                bool should_decel =
                    //the player is going in the right direction
                    Mathf.Sign(rigid.velocity.x) != Mathf.Sign(relation_to_destination.x)
                    //and the destination is within stopping distance
                    && Mathf.Abs(relation_to_destination.x) <= stopping_distance_x;

                if (should_decel)
                {

                    //plain decel
                    //Debug.Log("decelerating");
                    float original_sign = Mathf.Sign(x_vel);
                    x_vel -= Mathf.Sign(relation_to_destination.x) * -1 * accel * Time.deltaTime;
                    //prevent over-decelerating
                    if (original_sign != Mathf.Sign(x_vel))
                    {
                        x_vel = 0;
                    }
                }
                else
                {
                    //accelerate accordingly
                    float dvdt = Mathf.Sign(relation_to_destination.x) * -1 * accel * Time.deltaTime;

                    x_vel += dvdt;

                    //clamp to maximum velocity
                    x_vel = Mathf.Min(Mathf.Abs(x_vel), x_vel_max) * Mathf.Sign(x_vel);
                }

                rigid.velocity = new Vector2(x_vel, rigid.velocity.y);

                float yMax = rigid.position.y - charSize / 2f;
                int n_contacts = rigid.GetContacts(contacts);
                neighbours = new List<string>();

                for(int i = 0; i < n_contacts; i++)
                {
                    if (!contacts[i].collider.gameObject.CompareTag("Word Block")) break;
                    neighbours.Add(contacts[i].collider.gameObject.name);
                    //WordBlockBehavior block_content = word_blocks_in_contact[i].GetComponent<WordBlockBehavior>();
                    float block_top = contacts[i].collider.bounds.max.y;
                    //word_blocks_in_contact[i].GetComponent<BoxCollider2D>().bounds.max.y;
                    
                    //Debug.Log(contacts[i] + " with " + contacts[i].collider.gameObject.name);
                    float hdiff = block_top - yMax;
                    if (hdiff > 0.0f || Mathf.Approximately(Mathf.Abs(contacts[i].normal.x), 1))
                    {
                        //teleport for small height gaps
                        if (hdiff < climb_threshold)
                        {
                            //the collision itself stops the player character
                            //but since climbing animation should not occur, the stoppage
                            //should only happen in the y direction
                            //this means the x velocity must be overriden
                            rigid.position = new Vector3(
                                rigid.position.x,
                                block_top + charSize / 2f + 0.1f,
                                transform.position.z
                            );

                            velocity_override = true;
                        }
                        //climb for large height gaps
                        else
                        {
                            //Debug.Log("\t" + "climbing");
                            destination.y = block_top + charSize / 2f + 0.1f;
                            //yMax = Mathf.Max(block_top + charSize / 2f + 0.1f, yMax);
                            in_climb = true;
                            Debug.Log("Start climb to: " + destination.y);
                            break;
                        }
                    }
                }

                if (in_climb)
                {
                    //destination.y = yMax;
                    //climb_extent = yMax - rigid.position.y;
                    animator.SetBool("in_climb", true);
                }
                else
                {
                    destination.y = rigid.position.y;
                }
            }
            else
            {
                rigid.position = new Vector2(
                    destination.x,
                    rigid.position.y);
                relation_to_destination.x = 0;
                rigid.velocity = new Vector2(0, rigid.velocity.y);
            }
        }
        else
        {
            //while climbing:
            rigid.gravityScale = 0.0f;
            //actual climbing
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("Climb"))
            {
                //Debug.Log("climbing...");
                if (relation_to_destination.y <= 0)
                {
                    rigid.velocity = new Vector2(0, climb_speed);
                }
                else
                {
                    in_climb = false;
                    animator.SetBool("in_climb", false);
                    rigid.velocity = Vector2.zero;
                    Debug.Log("stop climbing");
                    //climb_extent = 0;
                }
            }
            else
            {
                rigid.velocity = Vector2.zero;
            }
        }

        //glitch jump when stuck
        /*
        if (accelerating && 
            (relation_temp.x == relation_to_destination.x 
            || Approximately(hor_spd_temp, 0)))
        {
            stuck_time += Time.deltaTime;
            if(stuck_time > 0.5f)
            {
                in_climb = true;
                destination.y = rigid.position.y + 0.1f;
                Debug.Log("glitch jumped");
                //TODO: do a special glitch jump animation :)
                //animator.SetBool("glitch_jump", true);

                //rigid.MovePosition(new Vector2(
                    //rigid.position.x,
                    //rigid.position.y + 0.1f));
            }
        }
        else
        {
            stuck_time = 0.0f;
        }*/

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("light on");
            light_toggle = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //Debug.Log("light off");
            light_toggle = false;
        }

        animator.SetFloat("speed", Mathf.Abs(rigid.velocity.x));
        if (animator.GetBool("light_toggle") != light_toggle)
        {
            //light toggle status changed
            //play oneshot squeaking sound for helmet raising/lowering
            if (light_toggle)
            {
                MusicManager.Instance.PlayOneShot(sfx_lib.helm_open, rigid.position);
            }
            else
            {
                MusicManager.Instance.PlayOneShot(sfx_lib.helm_close, rigid.position);
            }
            
        }
        animator.SetBool("light_toggle", light_toggle);

        head_light_controller.light_ = light_toggle;
        head_light_controller.lerp_state = light_progress;

        transform.rotation = Quaternion.Euler(0, direction ? 0 : 180f, 0);

        velocity_temp = rigid.velocity;
    }


    //update the stored relative position of the player to the cursor
    private void UpdateRelativePosition() {
        relation_to_destination = (Vector3)rigid.position - destination;
        relation_to_destination.x = Approximately(relation_to_destination.x, 0) ? 0 : relation_to_destination.x;
        relation_to_destination.y = Approximately(relation_to_destination.y, 0) ? 0 : relation_to_destination.y;
        relation_to_destination.z = Approximately(relation_to_destination.z, 0) ? 0 : relation_to_destination.z;
    }

    private bool Approximately(float a, float b)
    {
        //return Mathf.Approximately(a, b);
        return Mathf.Abs(a - b) <= 0.005;
    }

    public void OnReachNPC()
    {
        MusicManager.Instance.PlayOneShot(sfx_lib.npc_encounter, rigid.position);
    }

    public void OnTalkToNPC()
    {
        MusicManager.Instance.PlayOneShot(sfx_lib.npc_talk, rigid.position);
    }

    //handle collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.LogError("IMPLEMENT COLLISION ANIMATION, collision speed: " + collision.relativeVelocity.y);
        if (collision.relativeVelocity.y > 2.5f)
        {
            //Debug.Log(collision.relativeVelocity.y);
            MusicManager.Instance.PlayOneShot(sfx_lib.collision, rigid.position);
            StartCoroutine(CameraController.Instance.Shake(collision.relativeVelocity.y - 2.5f, 0.25f));
            ParticleSystem sprinkle = GetComponent<ParticleSystem>();
            sprinkle.Emit(2
                + Mathf.CeilToInt(
                    Mathf.Max(0, Mathf.Log(collision.relativeVelocity.y, 2)))
                    );
            //.Play();
        }

        if (collision.gameObject.CompareTag("Word Block"))
        {
            //Debug.Log(collision.gameObject.GetComponent<WordBlockBehavior>().content.content);
            //word_blocks_in_contact.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Word Block"))
        {
            //word_blocks_in_contact.RemoveAll(
                //(GameObject go) => go.Equals(collision.gameObject)
            //);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Cover Object"))
        {
            //Debug.Log("coming into contact with cover object");
            NPCBehaviour n = other.GetComponent<NPCBehaviour>();
            if (n != null)
            {
                n.Engage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Cover Object"))
        {
            //Debug.Log("exiting contact with cover object");
            NPCBehaviour n = other.GetComponent<NPCBehaviour>();
            if (n != null)
            {
                n.Disengage();
            }
        }
    }

    public void OverrideCollisionType(GameObject go)
    {
        if (go.CompareTag("Word Block"))
        {
            //word_blocks_in_contact.Add(go);
        }
    }

    private void OnProgression()
    {
        direction = true;
    }
    private void OnRegression()
    {
        direction = false;
    }
}
