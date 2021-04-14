using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LArm : MonoBehaviour
{

    public Transform player, claw;
    public SpriteRenderer sprite_;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 c = claw.localPosition;
        Vector3 m = c / 2.0f;
        //Vector3 r = p - c;

        sprite_.size = new Vector2(c.magnitude, sprite_.size.y);
        transform.localPosition = m;

        if (c.magnitude > 0.1)
        {
            float angle = Mathf.Atan2(c.y, c.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(
                new Vector3(
                    transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y,
                    angle
                    ));
        }
    }
}
