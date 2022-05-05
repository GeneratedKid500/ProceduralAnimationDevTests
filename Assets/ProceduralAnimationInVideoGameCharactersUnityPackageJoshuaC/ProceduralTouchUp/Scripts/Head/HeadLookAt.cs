using UnityEngine;

/// Tilts the head of a Unity Humanoid Avatar towards areas specified through the prefab collider
public class HeadLookAt : MonoBehaviour
{
    Animator anim;

    [SerializeField] float headMovementSpeed = 0.3f;

    [Range(0.0f, 1.0f)] [SerializeField] float targetLookAtWeight = 1;
    private float currentLookAtWeight = 0;

    [Header("Values")]
    [SerializeField] bool ikActive = true;
    [SerializeField] bool nearLookObj = true;
    [SerializeField] Transform lookObj;
    private string objTag;

    private void Start()
    {
        anim = GetComponent<Animator>();

        headMovementSpeed *= Time.deltaTime;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (ikActive && lookObj != null && nearLookObj)
        {
            // uses ray cast and the objects tag to check if a direct sight line on the object can be established
            // also just checks to see if the target weight is over 0 to prevent any unneccesary ray casting
            Vector3 dirToObj = lookObj.position - transform.parent.position;
            if (targetLookAtWeight > 0 && Physics.Raycast(transform.parent.position, dirToObj, out RaycastHit hit, Mathf.Infinity) 
            && hit.transform.tag == objTag)
            {
                // increase and decrease current weight relative to target
                if (currentLookAtWeight < targetLookAtWeight)
                {
                    currentLookAtWeight += headMovementSpeed;
                }
                else if (currentLookAtWeight > targetLookAtWeight)
                {
                    currentLookAtWeight -= headMovementSpeed;
                }

                // draw green line to help visualise 'can see' in inspector
                Debug.DrawRay(transform.parent.position, dirToObj, Color.green);
            }
            // if target weight is not bigger than 0 OR if a direct sight line cant be established
            else
            {
                // decrease current weight anyways to be sure
                if (currentLookAtWeight > 0)
                {
                    currentLookAtWeight -= headMovementSpeed;
                }
                // if current weight is 0 disable near an object
                else if (currentLookAtWeight <= 0 && targetLookAtWeight == 0)
                {
                    nearLookObj = false;
                }

                // draw red line to help visualise 'cant see' in inspector
                Debug.DrawRay(transform.parent.position, dirToObj, Color.red);
            }

            anim.SetLookAtPosition(lookObj.position);
        }
        else
        {
            // still decreases weight
            if (currentLookAtWeight > 0)
            {
                currentLookAtWeight -= headMovementSpeed;
                // keep object as to not be jittery when transitioning back
                // check first to ensure is not null (would cause error)
                if (lookObj != null)
                {
                    anim.SetLookAtPosition(lookObj.position);
                }
            }
        }
        anim.SetLookAtWeight(currentLookAtWeight);

    }

    // values passed in by headLookTarget script
    public void SetNewLookAt(Transform obj, float strength, string passedTag)
    {
        nearLookObj = true;
        lookObj = obj;
        targetLookAtWeight = Mathf.Clamp01(strength);
        objTag = passedTag;
    }

    public void EnableHeadIK() => ikActive = true;
    public void DisableHeadIK() => ikActive = false;
}
