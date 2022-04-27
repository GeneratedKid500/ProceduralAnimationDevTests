using UnityEngine;

public class ReEnableMovement : StateMachineBehaviour
{
    ThirdPersonControl tpc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (tpc == null)
        {
            tpc = animator.transform.root.GetComponent<ThirdPersonControl>();
        }

        tpc.DisableCharacter();
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (tpc == null)
        {
            tpc = animator.transform.root.GetComponent<ThirdPersonControl>();
        }

        tpc.EnableCharacter();
    }
}
