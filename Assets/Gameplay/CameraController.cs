using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity;

public class CameraController : MonoBehaviour
{
    [ExecuteAlways]
    [HideInInspector] public Rect CAM; //the range of the camera as a rectangle, in world units
    public float BUFFER_SIZE;

    //[Range(0, 10)]
    //public float CAM_SPEED, CAM_ACCEL;

    private Camera cam;
    //private ReadingManager rManager;
    GameObject player;

    [Range(0, 5)]
    public float shift_magnitude, shift_duration; // the magnitude of shift, and the speed of the shift transition
    [Range(0, 10)]
    public int key_threshold; // amount of weight until camera shift is triggered
    [HideInInspector]
    public float shift; // the amount of horizontal shift (%) to the camera position, relative to player
    [Range(0, 1)]
    public float key_efficacy; // amount of time until the last key press loses efficacy
    float key_timer; // time elapsed after last key press
    int key_weight; // the weight of the current shift factors (negative = left, 0 = neutral, positive = right)
    bool key_weight_pinned; // whether the camera is in a pinned state
    int current_key_animation_state; // the direction of the current (and previous un-neutralized) camera shift state
    Vector3 shift_unit; // the width of the player
    float t; // current time during the shift

    public AnimationCurve easing;

    [Range(0, 100)]
    public float shake_freq;
    Vector3 shake;
    [Range(0, 0.3f)]
    public float shake_scale;

    float base_size;
    float _zoom;
    public float zoom
    {
        set
        {
            if (value > 0)
            {
                _zoom = value;
            }
        }
        get { return _zoom; }
    }

    private static CameraController _instance;
    public static CameraController Instance
    {
        get { return _instance; }
    }

    private void OnEnable()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //fetch main camera and get settings
        cam = Camera.main;
        player = PlayerControl.Instance.gameObject;

        shift_unit = player.GetComponent<BoxCollider2D>().bounds.size;

        base_size = cam.orthographicSize;
        zoom = 1;

        //StudioListener sl = GetComponent<StudioListener>();
        //sl.attenuationObject = player;

        EventManager.Instance.OnProgression += () => UpdateKeyWeight(1);
        EventManager.Instance.OnRegression += () => UpdateKeyWeight(-1);

        EventManager.Instance.OnBackPortalOpen += (Vector2 v) => SetPinState(true, true);
        EventManager.Instance.OnBackPortalClose += () => SetPinState(false, true);
        //EventManager.Instance.OnFrontPortalEngage += () => SetPinState(true, false);
        //EventManager.Instance.OnFrontPortalDisengage += () => SetPinState(false, false);

        key_timer = 0;
        key_weight = 0;

        shake = Vector3.zero;

        GetComponent<StudioListener>().attenuationObject = gameObject;
    }

    public IEnumerator Shake(float magnitude, float duration)
    {
        if (duration <= 0)
        {
            throw new System.ArgumentException("duration must be >= 0 !");
        }
        float t = 0;
        float anchorX = Random.value * 10;
        float anchorY = Random.value * 10;
        float anchorZ = Random.value * 10;

        shake = Vector2.zero;
        float shake_scale_curr;
        while (t < duration)
        {
            shake_scale_curr = magnitude * shake_scale * (duration - t) / duration;
            shake.x = (Mathf.PerlinNoise(t * shake_freq, anchorX) - 0.5f) * shake_scale_curr;
            shake.y = (Mathf.PerlinNoise(t * shake_freq, anchorY) - 0.5f) * shake_scale_curr;
            shake.z = 30 * (Mathf.PerlinNoise(t * shake_freq, anchorZ) - 0.5f) * shake_scale_curr;
            t += Time.deltaTime;
            yield return null;
        }
        shake = Vector2.zero;
        yield return null;
    }

    //right is true
    void SetPinState(bool pinned, bool direction)
    {
        //repeating the same pin action shouldn't do anything
        if (key_weight_pinned == pinned
            && direction == key_weight > 0)
        {
            return;
        }

        key_weight_pinned = pinned;
        if (pinned)
        {
            t = Mathf.Max(0, t);
        }
        else
        {
            t = shift_duration;
        }

        if (direction)
        {
            key_weight = key_threshold;
            key_timer = key_efficacy;
            current_key_animation_state = 1;
        }
        else
        {
            key_weight = -1 * key_threshold;
            key_timer = key_efficacy;
            current_key_animation_state = 0;
        }
        
    }

    void UpdateKeyWeight(int weight)
    {
        if (key_weight_pinned)
        {
            return;
        }

        if (key_weight == 0)
        {
            //when going off of neutral, just update weight and timer
            key_weight += weight;
            key_timer = key_efficacy;
        }
        else
        {
            int sign = (int)(Mathf.Sign(weight));
            key_weight += sign;
            if (Mathf.Abs(key_weight) > key_threshold)
            {
                key_weight = sign * key_threshold;
            }
            key_timer = key_efficacy;
        }
    }

    event System.Action OnFirstFrame;

    public void SpawnAtRoot(Vector2 root)
    {
        OnFirstFrame += () =>
        {
            transform.position = new Vector3(root.x, root.y);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (OnFirstFrame != null)
        {
            OnFirstFrame();
            OnFirstFrame = null;
            return;
        }
        cam.orthographicSize = base_size * zoom;

        float cam_h = 2f * cam.orthographicSize;
        float cam_w = cam_h * cam.aspect;

        /* update key state */

        //decrement key weight after a period of not pressing a key
        if (!key_weight_pinned)
        {
            key_timer -= Time.deltaTime;

            if (key_timer <= 0 && key_weight != 0)
            {
                //current_key_animation_state = 0;
                int sign = (int)Mathf.Sign(key_weight);
                key_timer = key_efficacy;
                key_weight -= sign;
                //when key weight reaches neutral, set duration to full to prepare for back-transition
                if (key_weight == 0)
                {
                    //t = shift_duration;
                }
            }
        }

        //transitioning back to neutral state
        if (key_weight == 0 && t > 0)
        {
            t = Mathf.Clamp(t - Time.deltaTime, 0, shift_duration);
        }
        //set state as neutral
        if (key_weight == 0 && t <= 0)
        {
            t = 0;
            current_key_animation_state = 0;
        }
        //kick off transition upon reaching key threshold
        if (Mathf.Abs(key_weight) >= key_threshold && t == 0)
        {
            current_key_animation_state = (int)Mathf.Sign(key_weight);
        }
        //transitioning from neutral to shifted state after animation kick off
        if (key_weight != 0 && current_key_animation_state != 0)
        {
            t = Mathf.Clamp(t + Time.deltaTime, 0, shift_duration);
        }

        /* translate key state to shift state, if necessary */
        shift = easing.Evaluate(t / shift_duration);

        Vector3 shift_raw = new Vector3(current_key_animation_state * shift_magnitude * shift * shift_unit.x, 0, 0);
        Vector2 focus = (Vector2)(player.transform.position + shift_raw);

        cam.transform.position = new Vector3(focus.x + shake.x, 
            Mathf.Lerp(cam.transform.position.y, focus.y, PlayerControl.Instance.climb_speed * Time.deltaTime) + shake.y,
            -10);

        cam.transform.rotation = Quaternion.Euler(
            cam.transform.rotation.eulerAngles.x,
            cam.transform.rotation.eulerAngles.y,
            shake.z);
        
        CAM = new Rect(
            cam.transform.position.x - cam_w / 2f,
            cam.transform.position.y - cam_h / 2f,
            cam_w, cam_h);
    }
}
