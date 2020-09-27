using System.Collections;
using UnityEngine;
using VRM;

[ClassHeader("face control", iconName = "icon")]
public class FaceControl : mMonoBehaviour
{
    [EditorToolbar("Mouth Controll", order = 1)]
    [SerializeField]
    public VRMBlendShapeProxy BlendShapes;
    private void Reset()
    {
        BlendShapes = GetComponent<VRMBlendShapeProxy>();
    }

    [SerializeField]
    float interlavo = 5f;

    [SerializeField]
    float closingTime = 0.06f;

    [SerializeField]
    float m_openingSeconds = 0.03f;

    [SerializeField]
    float m_closeSeconds = 0.1f;

    [EditorToolbar("Eyes controll", order = 2)]
    [SerializeField]
    float testVariable = 0.1f;

    protected Coroutine m_coroutine;

    //static readonly string BLINK_NAME = BlendShapePreset.Blink.ToString();

    float m_nextRequest;
    bool m_request;
    public bool Request
    {
        get { return m_request; }
        set
        {
            if (Time.time < m_nextRequest)
            {
                return;
            }
            m_request = value;
            m_nextRequest = Time.time + 1.0f;
        }
    }

    public void interval(float value)
    { 
        interlavo = value;
    }

    protected IEnumerator BlinkRoutine()
    {
        while (true)
        {
            var waitTime = Time.time + Random.value * interlavo;
            while (waitTime > Time.time)
            {
                if (Request)
                {
                    m_request = false;
                    break;
                }
                yield return null;
            }

            // close
            var value = 0.0f;
            var closeSpeed = 1.0f / m_closeSeconds;
            while (true)
            {
                value += Time.deltaTime * closeSpeed;
                if (value >= 1.0f)
                {
                    break;
                }

                BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), value);
                yield return null;
            }
            BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), 1.0f);

            // wait...
            yield return new WaitForSeconds(closingTime);

            // open
            value = 1.0f;
            var openSpeed = 1.0f / m_openingSeconds;
            while (true)
            {
                value -= Time.deltaTime * openSpeed;
                if (value < 0)
                {
                    break;
                }

                BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), value);
                yield return null;
            }
            BlendShapes.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), 0);
        }
    }

    private void Awake()
    {
        if (BlendShapes == null) BlendShapes = GetComponent<VRM.VRMBlendShapeProxy>();
    }

    private void OnEnable()
    {
        m_coroutine = StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }
    }

}
