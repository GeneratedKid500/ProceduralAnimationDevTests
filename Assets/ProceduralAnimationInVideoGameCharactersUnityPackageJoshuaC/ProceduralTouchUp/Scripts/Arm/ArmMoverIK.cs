using UnityEngine;

/// Moves the arm of a Unity Humanoid Avatar to a desired position detected by raycast 
public class ArmMoverIK : MonoBehaviour
{
    private Animator anim;
    private int crouchLayerID;
    private float animCrouchLayerVal = 0;
    private Quaternion handRotation;
    // used to limit new Vector3 creations
    private Vector3 tempVector = Vector3.zero;

    [SerializeField] private bool leftArm;
    private Transform armPoint;
    [Range(0.0f, 2.0f)] [SerializeField] private float armMovementSpeed = 0.3f;
    [Range(0.0f, 0.2f)] [SerializeField] private float handPlacementOffset = 0.05f;

    [Header("Objects")]
    [Tooltip("Origin point of the raycast cast - indicates where the arm will be placed")]
    [SerializeField] private Transform rayOrigin;

    [SerializeField] private LayerMask wallLayerMask;

    [Header("Values")]
    [SerializeField] private bool ikActive = true;
    [SerializeField] private bool nearWall = true;

    [Tooltip("How strongly the character will reach towards the wall")]
    [Range(0.0f, 1.0f)] [SerializeField] private float targetReachWeight = 1;
    private float currentReachWeight;

    [Tooltip("How far from the wall the character will begin reaching")]
    [Range(0, 1.0f)][SerializeField] private float maxReachDist = 0.85f;
    [Tooltip("How close to the wall the character will stop reaching")]
    [Range(0, 1.0f)][SerializeField] private float minReachDist = 0.35f;

    private void Start()
    {
        // if no animator, do not continue
        anim = GetComponent<Animator>();
        if (anim == null) Destroy(this);

        // creates new gameObjects under the parent of this object as "ArmIKPoints"
        armPoint = new GameObject().GetComponent<Transform>();
        armPoint.transform.parent = transform.parent;
        armPoint.transform.name = ("ArmIKPoint" + (leftArm ? "L" : "R"));

        // sets movement speed relative using deltaTime
        armMovementSpeed *= Time.deltaTime;

        // loops through animation layers to find "Crouch Layer"
        // assigns id to crouchLayerID, otherwise is -1 (== invalid)
        for (int i = 0; i < anim.layerCount; i++)
        {
            if (anim.GetLayerName(i).Equals("Crouch Layer"))
            {
                crouchLayerID = i;
                break;
            }
            else crouchLayerID = -1;
        }
    }

    private void FixedUpdate()
    {
        // checks if crouchLayerID is valid, if so
        // updates the reference to the weight value of the crouching animation layer
        if (crouchLayerID != -1)
        {
            animCrouchLayerVal = anim.GetLayerWeight(crouchLayerID);
        }

        if (leftArm)
        {
            // negative values passed in to inverse calculations
            ArmRayCast(-transform.right, -handPlacementOffset);
        }
        else
        {
            ArmRayCast(transform.right, handPlacementOffset);
        }
    }

    private void ArmRayCast(Vector3 transformDir, float handOffset)
    {
        // if the raycast, projected from the transform referenced in the rayOrigin Transform collides with a wall
        // and isn't closer than the value defined by minReachDistance 
        if (ikActive && Physics.Raycast(rayOrigin.position, transformDir, out RaycastHit hit, maxReachDist, wallLayerMask)
            && Vector3.Distance(rayOrigin.position, hit.point) > minReachDist)
        {
            nearWall = true;

            // calculates the arm point position by using the offset value provided
            // stores values in "tempVector"
            tempVector.x = transform.localPosition.x + handOffset;
            tempVector.y = 0;
            tempVector.z = transform.localPosition.z + handOffset;
            // uses TransformDirection to make results consistent on either side of body
            armPoint.transform.position = hit.point - transform.TransformDirection(tempVector);

            // 0.5 = half way to crouching
            if (animCrouchLayerVal > 0.5f)
            {
                // vector will point forward, relative to the plane collided with
                tempVector = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            }
            else
            {
                // vector will point up, relative to the plane collided with
                tempVector = Vector3.ProjectOnPlane(transform.up, hit.normal);
            }

            // translates to Quaternion and stores in handRotation class variable
            handRotation = Quaternion.LookRotation(tempVector, hit.normal);
        }
        else nearWall = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // passes in appropriate AvatarIKGoal
        if (leftArm)
        {
            ArmIKAnimator(AvatarIKGoal.LeftHand);
        }
        else
        {
            ArmIKAnimator(AvatarIKGoal.RightHand);
        }
    }

    private void ArmIKAnimator(AvatarIKGoal arm)
    {
        if (nearWall && ikActive)
        {
            // increases weight value if below target
            if (targetReachWeight > currentReachWeight)
            {
                currentReachWeight += armMovementSpeed;
            }
            // decreases weight value if above target
            else if (currentReachWeight > targetReachWeight)
            {
                currentReachWeight -= armMovementSpeed;
            }

            // sets positions and rotations
            anim.SetIKPosition(arm, armPoint.position);
            // sets rotation using calculations from raycast
            anim.SetIKRotationWeight(arm, targetReachWeight);
            anim.SetIKRotation(arm, handRotation);
        }
        else // if not near a wall
        {
            // decreases weight value while above 0
            if (currentReachWeight > 0)
            {
                // does so slightly slower to create less jittery movement
                currentReachWeight -= (armMovementSpeed * 0.9f);
                // only sets position if weight is > 0
                anim.SetIKPosition(arm, armPoint.position);
            }
        }
        anim.SetIKPositionWeight(arm, currentReachWeight);
    }

    public void EnableArmIK() => ikActive = true;
    public void DisableArmIK() => ikActive = false;
}
