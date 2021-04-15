using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteAlways]
public class NPCBehaviour : MonoBehaviour
{

    public Transform bubble, bubble_head, bubble_tail, bubble_text;
    SpriteRenderer sprite;
    public bool engaged;

    public Vector2 margin;
    public float normalized_bubble_x;
    public float bubble_y;

    public float hint_size_decrement; //the size of the hint text at bottom

    public float float_speed, float_magnitude;

    string[] script;
    int index;

    int sprite_rand_index;

    public NPCScriptable default_content;
    NPCScriptable content;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();

        //engaged = true;
        if (Application.isPlaying)
        {
            Disengage();
        }
        else
        {
            Engage();
        }
    }

    bool is_talking;
    const int UPDATES_PER_FRAME = 5;
    public IEnumerator Talk()
    {
        if (!is_talking && content != null && !content.randomize && content.sprites.Length > 1)
        {
            is_talking = true;
            for(int i = 1; i < content.sprites.Length; i++)
            {
                sprite.sprite = content.sprites[i];
                for(int j = 0; j < UPDATES_PER_FRAME; j++)
                {
                    yield return null;
                }
            }
            sprite.sprite = content.sprites[0];
        }
        else
        {
            //jump up and down
            if (content != null && content.sprites != null)
                sprite.sprite = content.sprites[sprite_rand_index];
            System.Func<float, float> parabola = x => -4 * x * x + 4 * x;
            float duration = 0.5f, height = 0.3f;
            float t = 0.0f;
            Vector3 cached = transform.position;
            while (t < duration)
            {
                transform.position = cached + new Vector3(
                    0, parabola(t / duration) * height, 0);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = cached;
        }
        is_talking = false;
    }

    // Update is called once per frame
    void Update()
    {

        bubble.localPosition = new Vector3(
            0, 
            (Mathf.Sin(Time.time * float_speed) + 1 ) / 2 * float_magnitude,
            0);

        if (engaged)
        {
            Vector2 right_up = sprite.bounds.size;
            SpriteRenderer tail_sprite = bubble_tail.GetComponent<SpriteRenderer>();

            bubble_tail.localPosition = (Vector3)(right_up / 2)
                + tail_sprite.bounds.size / 2;
            
            Vector3 sbubble = new Vector3(
                sprite.bounds.size.x * normalized_bubble_x,
                bubble_y,
                0
                );

            RectTransform text = bubble_text as RectTransform;
            text.localPosition = new Vector3(
                0,
                right_up.y / 2 + tail_sprite.bounds.size.y + margin.y + sbubble.y / 2,
                0
                );
            
            text.sizeDelta = sbubble;

            sbubble.x += margin.x * 2;
            sbubble.y += margin.y * 2;

            SpriteRenderer head_sprite = bubble_head.GetComponent<SpriteRenderer>();
            head_sprite.size = sbubble;

            bubble_head.localPosition = new Vector3(
                0,
                right_up.y / 2 + tail_sprite.bounds.size.y + sbubble.y / 2,
                0);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlayerControl.Instance.OnTalkToNPC();
                StartCoroutine(Talk());
                NextLine();
            }
        }   
    }

    //initialize the npc behaviour based on which npc it is associated to
    public void Initialize(string identifier)
    {
        foreach (NPCScriptable n in ScriptableObjectManager.Instance.NPCManager.npcs)
        {
            List<NPCScriptable.NPCSegment> matching = new List<NPCScriptable.NPCSegment>();
            foreach (NPCScriptable.NPCSegment s in n.segments)
            {
                if (identifier.ToLower().Equals((n.name+s.name).ToLower()))
                {
                    //Debug.Log("Fetching npc: " + n.name + s.name);
                    if (!n.randomize)
                    {
                        Materialize(n, s);
                        return;
                    }
                    else
                    {
                        matching.Add(s);
                    }
                }
            }
            if (matching.Count > 0)
            {
                Materialize(n, matching[Mathf.FloorToInt(Random.value * matching.Count)]);
                return;
            }
        }
        Materialize(default_content, default_content.segments[0]);
    }

    private void Materialize(NPCScriptable n, NPCScriptable.NPCSegment s)
    {
        content = n;
        sprite_rand_index = Mathf.FloorToInt(Random.value * n.sprites.Length); 
        GetComponent<SpriteRenderer>().sprite = content.sprites[sprite_rand_index];
        script = s.script.Split('\n');
        index = -1;
        NextLine();
    }

    private void NextLine()
    {
        index++;
        if (index < script.Length)
        {
            float hint_size = bubble_text.GetComponent<TextMeshPro>().fontSize - hint_size_decrement;
            bubble_text.GetComponent<TextMeshPro>().text = script[index] + "\n<size="+hint_size+">[Enter]</size>";
        }
        else
        {
            bubble_text.GetComponent<TextMeshPro>().text = "...";
        }
    }

    public void Engage()
    {
        PlayerControl.Instance.OnReachNPC();
        bubble.gameObject.SetActive(true);
        engaged = true;
        is_talking = false;
        StartCoroutine(Talk());
    }

    public void Disengage()
    {
        //Debug.LogError("Implemenet Disengage");
        bubble.gameObject.SetActive(false);
        engaged = false;
    }
}
