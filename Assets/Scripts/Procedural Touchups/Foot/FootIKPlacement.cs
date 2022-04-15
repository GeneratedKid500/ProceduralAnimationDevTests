using UnityEngine;

public class FootIKPlacement : MonoBehaviour
{
    // animator
    private Animator anim;
    // animator values
    private readonly int leftFootWeightIK = Animator.StringToHash("LeftFootWeightIK");
    private readonly int rightFootWeightIK = Animator.StringToHash("RightFootWeightIK");
    // animator value storage
    private float leftFootWeight;
    private float rightFootWeight;

    [SerializeField] LayerMask groundLayerMask;
    [Header("Values")]
    [SerializeField] private bool ikActive = true;
    [Range(0, 1f)] [SerializeField] private float distanceToGround = 0.1f;
    [Range(0, 2f)][SerializeField] private float maxReachDistance = 1.2f;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!anim || !ikActive) return;

        try
        {
            leftFootWeight = anim.GetFloat(leftFootWeightIK);
            rightFootWeight = anim.GetFloat(rightFootWeightIK);
        }
        catch
        {
            leftFootWeight = 1;
            rightFootWeight = 1;
        }


        RaycastHit hit;

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootWeight);

        Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, distanceToGround + maxReachDistance, groundLayerMask))
        {
            Vector3 footPos = hit.point;
            footPos.y += distanceToGround;
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
            Vector3 rot = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(rot, hit.normal));
        }

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootWeight);

        ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, distanceToGround + maxReachDistance, groundLayerMask))
        {
            Vector3 footPos = hit.point;
            footPos.y += distanceToGround;
            anim.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
            Vector3 rot = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(rot, hit.normal));
        }
    }

    public void EnableFeetIK() => ikActive = true;
    public void DisableFeetIK() => ikActive = false;
}
