using UnityEngine;

public class RagdollOnOff : MonoBehaviour
{
    private ThirdPersonControl tpc;

    #region Toggles
    [Header("Toggles")] // toggles
    // ragdoll state machine 
    [SerializeField] private RagdollState state = RagdollState.animating;
    private enum RagdollState
    {
        animating, // regular state
        ragdolling, // when ragdolling
        blending // gathering values
    }
    private bool ragdolling; // bool to help control state

    // should we wait for button to stand up or auto after time passed
    [SerializeField] private bool pressToStand = false;
    [Range(0.0F, 5.0F)][SerializeField] private float minTimeToStand = 2f;
    private float timeWaited = 0f; // private counter
    #endregion

    #region Values
    [Header("Values")] // values
    [Range(0, 2.0f)][SerializeField] private float ragdollCameraOffset = 0;
    private float defaultCameraOffset;
    #endregion

    #region Components
    [Header("Components")] // components
    [SerializeField] Transform hips;

    [SerializeField] Collider mainCollider;
    private Collider[] limbColliders;

    [SerializeField] Rigidbody mainRigidbody;
    private Rigidbody[] limbRigidbodies;

    [SerializeField] Animator anim;
    #endregion

    #region Value Stores
    Transform[] bodyParts;
    // stored positions / rotations of each limb
    Vector3[] storedPositions;
    Quaternion[] storedRotations;
    // key ragdoll positions
    Vector3 ragdollHipPos, ragdollHeadPos, ragdollFeetPos;

    //How long do we blend when transitioning from ragdolled to animated
    float ragdollToAnimatorBlendTime = 0.5f;
    float animatorToGetUpTransitionTime = 0.05f;

    // store the time when we transitioned from ragdolled to blendToAnim state
    float ragdollEndTime = -100;
    #endregion

    private void Awake()
    {
        // get reference to controller script
        tpc = transform.root.GetComponent<ThirdPersonControl>();
        defaultCameraOffset = tpc.cameraOffset;

        // get main body components
        if (mainCollider == null) mainCollider = transform.root.GetComponent<Collider>();
        if (mainRigidbody == null) mainRigidbody = transform.root.GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponent<Animator>();

        // get rigidbody colliders
        limbColliders = GetComponentsInChildren<Collider>();
        foreach (Collider limb in limbColliders)
        {
            limb.isTrigger = true;
        }
        limbRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in limbRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        bodyParts = hips.GetComponentsInChildren<Transform>();
        storedPositions = new Vector3[bodyParts.Length];
        storedRotations = new Quaternion[bodyParts.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RagdollToggle();
        }

        if (ragdolling)
        {
            // update ragdoll state when animating to begin transition
            if (state == RagdollState.animating)
            {
                RagdollOn();
                state = RagdollState.ragdolling;
            }

            // update camera and main GO position to match the model
            Vector3 bodyPos = hips.position;
            hips.position = transform.position;
            transform.root.position = bodyPos;

            // stand up after a certain amount of time on the ground
            if (!pressToStand && Mathf.Abs(limbRigidbodies[0].velocity.y) < 0.1)
            {
                if (timeWaited < minTimeToStand)
                {
                    timeWaited += Time.deltaTime;
                }
                else
                {
                    RagdollToggle();
                }
            }
        }
    }


    //LateUpdate is called after all other updates - including animator updates
    private void LateUpdate()
    {
        if (!ragdolling)
        {
            if (state == RagdollState.ragdolling)
            {
                RagdollOff();
                ragdollEndTime = Time.time; //store the state change time
                state = RagdollState.blending;

                // store references to the positions of the body parts
                for (int i = 0; i < bodyParts.Length; i++)
                {
                    storedPositions[i] = bodyParts[i].position;
                    storedRotations[i] = bodyParts[i].rotation;
                }

                // save the values of some key positons
                //point between feet
                ragdollFeetPos = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftToes).position + anim.GetBoneTransform(HumanBodyBones.RightToes).position);
                ragdollHeadPos = anim.GetBoneTransform(HumanBodyBones.Head).position; // head
                ragdollHipPos = anim.GetBoneTransform(HumanBodyBones.Hips).position; //hips

                // checks hips directional vector to see what side body is facing
                if (anim.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)
                {
                    anim.Play("GetUpFromBack", 0, 0);
                }
                else
                {
                    anim.Play("GetUpFromFront", 0, 0);
                }
            }
            else if (state == RagdollState.blending)
            {
                // if time passed is smaller than the transition get up time
                if (Time.time <= ragdollEndTime + animatorToGetUpTransitionTime)
                {
                    // calculate the difference between the actual position and the animator desired position
                    Vector3 animRagdollDiff = ragdollHipPos - anim.GetBoneTransform(HumanBodyBones.Hips).position;
                    Vector3 newRootPos = hips.position + animRagdollDiff; // gather a new root position

                    // raycast that returns all of the point that were hit on the way down
                    RaycastHit[] hits = Physics.RaycastAll(newRootPos, Vector3.down);
                    newRootPos.y = 0;
                    
                    // checks new root positioms for higher value to ensure root
                    // is not below ground or is on approrpriate level
                    foreach (RaycastHit hit in hits)
                    {
                        if (!hit.transform.IsChildOf(transform))
                        {
                            newRootPos.y = Mathf.Max(newRootPos.y, hit.transform.position.y);
                        }
                    }
                    // moves hips of model back to root position 
                    // and root to the newly calculated position
                    hips.position = transform.root.position;
                    transform.root.position = newRootPos;

                    // calculates orientation and direction character is facing
                    Vector3 ragdollDir = ragdollHeadPos - ragdollFeetPos;
                    ragdollDir.y = 0;

                    // gathers the new current position of the character as the animator is shifting the characters orientation
                    Vector3 animMeanFeetPos = 0.5f * (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + anim.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    Vector3 animDir = anim.GetBoneTransform(HumanBodyBones.Head).position - animMeanFeetPos;
                    animDir.y = 0;

                    //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                    //hence setting the y components of the vectors to zero. 
                    hips.rotation = transform.root.rotation;
                    transform.root.rotation *= Quaternion.FromToRotation(animDir.normalized, ragdollDir.normalized);
                }
                // calculates the time it will take to get up 
                // will decrease each frame as more time passes
                float ragdollBlendAmount = 1.0f - (Time.time - ragdollEndTime - animatorToGetUpTransitionTime) / ragdollToAnimatorBlendTime;
                ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

                for (int i = 0; i < bodyParts.Length; i++)
                {
                    // do not move root of model
                    if (bodyParts[i] != transform)
                    {
                        if (bodyParts[i] == anim.GetBoneTransform(HumanBodyBones.Hips)) // only interpolate the position of the hips
                        {
                            bodyParts[i].position = Vector3.Lerp(bodyParts[i].position, storedPositions[i], ragdollBlendAmount);
                        }
                        // spherically interpolate the rotation of the body parts versus the stored rotations 
                        // whilst the animator manipulates them
                        bodyParts[i].rotation = Quaternion.Slerp(bodyParts[i].rotation, storedRotations[i], ragdollBlendAmount);
                    }
                }

                // once ragdoll blend amount goes too low
                // blending is no longer necessary
                if (ragdollBlendAmount <= 0)
                {
                    state = RagdollState.animating;
                    return;
                }
            }
        }
    }

    #region Ragdoll Components
    void SetRagdollComponents(bool value, Vector3 velocity)
    {
        for (int i = 0; i < Mathf.Min(limbColliders.Length, limbRigidbodies.Length); i++)
        {
            limbColliders[i].isTrigger = !value;
            limbRigidbodies[i].isKinematic = !value;

            if (value) limbRigidbodies[i].AddForce(velocity * 1.2f, ForceMode.Impulse);
        }
    }

    private void RagdollOn()
    {
        // extract velocity from main rigidbody
        Vector3 vel = mainRigidbody.velocity;
        if (Mathf.Abs(vel.y) < 0.3f) vel.y = 0; 

        // set lower camera offset
        tpc.cameraOffset = ragdollCameraOffset;

        // disable main body components
        mainCollider.enabled = false;
        mainRigidbody.isKinematic = true;
        anim.enabled = false;

        SetRagdollComponents(true, vel);
    }

    private void RagdollOff()
    {
        SetRagdollComponents(false, Vector3.zero);

        // reset camera offset
        tpc.cameraOffset = defaultCameraOffset;

        // enable main body components
        mainCollider.enabled = true;
        mainRigidbody.isKinematic = false;
        anim.enabled = true;
    }

    public void RagdollToggle()
    {
        if (ragdolling)
        {
            // checks minimum y velocity to ensure ragdoll has stopped
            if (Mathf.Abs(limbRigidbodies[0].velocity.y) < 0.1)
            {
                ragdolling = false;
                timeWaited = 0;
            }
        }
        else
        {
            if (tpc.isMovementEnabled())
            {
                tpc.DisableCharacter();
                ragdolling = true;
            }
        }
    }
    #endregion
}
