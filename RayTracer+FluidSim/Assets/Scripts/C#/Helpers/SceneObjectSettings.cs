using UnityEngine;

// All position and rotation settings for scene objects
public class SceneObjectSettings : MonoBehaviour
{
    public int MaterialIndex;
    public int MaxDepthBVH;
    private NewRenderer m;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        m = GameObject.Find("Renderer")?.GetComponent<NewRenderer>();
        if (m == null)
        {
            Debug.LogError("Renderer GameObject or NewRenderer component not found.");
        }
    }

    private void OnValidate()
    {
        if (m != null && m.ProgramStarted) { m.DoUpdateSettings = true;  m.DoReloadData = true; }
    }
    private void LateUpdate()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            m.DoUpdateSettings = true;
            m.DoReloadData = true;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }
}