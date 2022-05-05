using UnityEngine;

/// Bundled with a collider to enable head IK and rotation on a Unity Humanoid Avatar.
[RequireComponent(typeof(BoxCollider))]
public class HeadLookTarget : MonoBehaviour
{
    [Range(0.0f, 1.0f)][SerializeField] private float drawStrength = 1;
    private Transform parentTransform;
    private string parentTag;

    private bool isIKManager = true;

    private void Start()
    {
        parentTransform = transform.parent.transform;
        parentTag = transform.parent.tag;

        try
        {
            GameObject.FindGameObjectWithTag("Player").transform.GetComponent<PlayerIKManager>().GetHeadLookAt();
        }
        catch
        {
            isIKManager = false;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!col.tag.Equals("Player")) return;

        if (isIKManager)
        {
            col.transform.GetComponent<PlayerIKManager>().GetHeadLookAt().SetNewLookAt(parentTransform, drawStrength, parentTag);
        }
        else
        {
            col.transform.GetComponentInChildren<HeadLookAt>().SetNewLookAt(parentTransform, drawStrength, parentTag);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (!col.tag.Equals("Player")) return;

        if (isIKManager)
        {
            col.transform.GetComponent<PlayerIKManager>().GetHeadLookAt().SetNewLookAt(parentTransform, 0, parentTag);
        }
        else
        {
            col.transform.GetComponentInChildren<HeadLookAt>().SetNewLookAt(parentTransform, 0, parentTag);
        }
    }
}
