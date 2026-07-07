using UnityEngine;

/// <summary>
/// Animates a LineRenderer as a meandering wavy trail between two board points.
/// Used for snake paths: a swirling vortex trail, or a swaying jungle vine.
/// The wave tapers to zero at both ends so it connects cleanly to head/tail.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class WavyLink : MonoBehaviour
{
    public Vector3 a, b;
    public Color color = Color.magenta;
    public float amplitude = 0.30f;
    public float waves = 3.5f;
    public float speed = 2.2f;
    public float width = 0.12f;
    public int segments = 40;

    private LineRenderer lr;
    private float phase;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments + 1;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = width;
        lr.numCapVertices = 4;
        lr.useWorldSpace = true;
    }

    void Update()
    {
        if (lr == null) return;
        phase += Time.deltaTime * speed;

        Vector3 dir = b - a;
        float len = dir.magnitude;
        if (len < 0.001f) return;
        Vector3 ndir = dir / len;
        Vector3 perp = new Vector3(-ndir.y, ndir.x, 0f);

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float taper = Mathf.Sin(t * Mathf.PI);                 // 0 at ends, 1 in middle
            float wave = Mathf.Sin(t * waves * Mathf.PI * 2f + phase) * amplitude * taper;
            Vector3 pos = Vector3.Lerp(a, b, t) + perp * wave;
            pos.z = -0.5f;
            lr.SetPosition(i, pos);
        }
    }
}

/// <summary>Spins a transform (used to make vortex badges swirl).</summary>
public class Spinner : MonoBehaviour
{
    public float speed = 90f;
    void Update() => transform.Rotate(0f, 0f, speed * Time.deltaTime);
}

/// <summary>Bobs a transform up and down (used for the floating diamond).</summary>
public class Bobber : MonoBehaviour
{
    public float amp = 0.12f, speed = 3f;
    Vector3 basePos;
    bool have;
    void OnEnable() { basePos = transform.position; have = true; }
    void Update()
    {
        if (!have) { basePos = transform.position; have = true; }
        var p = basePos;
        p.y += Mathf.Sin(Time.time * speed) * amp;
        transform.position = p;
    }
}
