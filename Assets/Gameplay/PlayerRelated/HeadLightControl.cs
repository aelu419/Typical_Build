using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLightControl : MonoBehaviour
{

    public bool light_;

    //intensity, fall off intensity, inner radius, outer radius

    public Vector4 light_min_max_off;
    public Vector4 light_min_max_on;

    [HideInInspector] public Vector4 current_setting;

    private UnityEngine.Experimental.Rendering.Universal.Light2D head_light;
    private float speed = 5;

    private Vector4 right_direction;
    private Vector4 left_direction;

    public float lerp_state;

    // Start is called before the first frame update
    void Start()
    {
        light_ = false;
        head_light = GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();

        right_direction = new Vector4(
            transform.localPosition.x,
            transform.localPosition.y,
            transform.localPosition.z,
            transform.rotation.eulerAngles.z
            ) ;

        left_direction = right_direction * -1;
        //vertical position of light remains the same during flipping
        left_direction.y = right_direction.y;
    }

    // Update is called once per frame
    void Update()
    {
        float perturbation = Mathf.Lerp(0.25f, 0.175f, lerp_state);
        current_setting = new Vector4(
            Mathf.Lerp(light_min_max_off.x, light_min_max_on.x, lerp_state),
            Mathf.Lerp(light_min_max_off.y, light_min_max_on.y, lerp_state),
            Mathf.Lerp(light_min_max_off.z, light_min_max_on.z, lerp_state),
            Mathf.Lerp(light_min_max_off.w, light_min_max_on.w, lerp_state)
            );

        head_light.intensity = current_setting.x * (1 + perturbation * Mathf.PerlinNoise(0, Time.time));
        head_light.pointLightInnerAngle = current_setting.y * (1 + perturbation * Mathf.PerlinNoise(0, speed * Time.time));
        head_light.pointLightInnerRadius = current_setting.z * (1 + perturbation * Mathf.PerlinNoise(5, speed * Time.time));
        head_light.pointLightOuterRadius = current_setting.w * (1 + perturbation * Mathf.PerlinNoise(7, speed * Time.time));
    }
}
