using System.Collections;
using UnityEngine;

public class ArachnidProceduralAnimationSolver : MonoBehaviour
{
    [Tooltip("Array of Leg IK Targets for the rig")]
    [SerializeField] private Transform[] legTargets;
    [Tooltip("Minimum distance needed before the system decides a step is needed")]
    [Range(0.01f, 0.5f)] [SerializeField] private float stepLength = 0.15f;
    [Tooltip("Maxmimum height of steps taken, used in calculation of a sin curve")]
    [Range(0.01f, 0.5f)] [SerializeField] private float stepHeight = 0.15f;
    [Tooltip("Smoothness of movement for calculations - lower the value the faster it is")]
    [Range(0.01f, 10f)] [SerializeField] private float speedInverse = 8f;
    [Tooltip("If the system should adjust body orientation whilst walking")]
    [SerializeField] private bool adjustOrientation = true;

    private float maxRange = 3f; // maximum raycast range

    private int legCount; // quick reference for amount of legs
    private Vector3[] defaultLegSpacing; // original positions of the legs
    private Vector3[] priorLegSpacing; // position of legs on last update
    private bool legMoving = false; // value to hold if leg is moving or not

    // 'root' is representative of the body
    private Vector3 priorRootNormal; // orientation of body on last update
    private Vector3 priorRootPos; // position of body on last update

    private Vector3 vel; // relative velocity
    private Vector3 lastVel; // relative velocity on last update
    private float velMultiplier = 15f; // velocity multiplier


    void Start()
    {
        // gather transform details
        priorRootNormal = transform.up;
        priorRootPos = transform.position;

        // gather details on all of the legs
        legCount = legTargets.Length;
        defaultLegSpacing = new Vector3[legCount];
        priorLegSpacing = new Vector3[legCount];
        for (int i = 0; i < legCount; i++)
        {
            // loop through to get all leg details
            defaultLegSpacing[i] = legTargets[i].localPosition;
            priorLegSpacing[i] = legTargets[i].position;
        }

        //scale multiplier
        stepLength *= transform.parent.lossyScale.x;
        stepHeight *= transform.parent.lossyScale.x;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OrientationToggle();
        }
    }

    // fixed update for raycasts
    void FixedUpdate()
    {
        #region Velocity Calculations
        vel = transform.position - priorRootPos; // velocity calculated as a direction vector
        vel = (vel + speedInverse * lastVel) / (speedInverse + 1f); // used to indicate which direction feet should place
        
        if (vel.magnitude < 0.000025f) vel = lastVel;
        else lastVel = vel;

        vel *= velMultiplier;
        #endregion

        #region Decide which leg to move
        Vector3[] newPositions = new Vector3[legCount]; // vector to hold new calculated positions
        float largestDistance = stepLength; // largest distance or (at default) regular step length

        int moveIndex = -1; // value to indicate which foot in index should move
        for (int i = 0; i < legCount; i++) // loop through all legs
        {
            newPositions[i] = transform.TransformPoint(defaultLegSpacing[i]); // gets intended position of legs

            // length of the distance calculated to be moved, accounting for the velocity and direction of the movement 
            float moveDistance = Vector3.ProjectOnPlane(newPositions[i] + vel - priorLegSpacing[i], transform.up).magnitude;
            if (moveDistance > largestDistance) // if calculated distance value is larger than current largest distance
            {
                largestDistance = moveDistance; // set new largest value
                moveIndex = i; // current index is now priority to move
            }
        }
        for (int i = 0; i < legCount; i++) // loop through again
        {
            if (i != moveIndex) // if not priority to move
            {
                legTargets[i].position = priorLegSpacing[i]; // remain at current position
            }
        }
        #endregion

        #region Calculate and begin to move chosen leg
        if (moveIndex != -1 && !legMoving) // if leg is ready to be moved and one is not already being moved
        {
            legMoving = true;

            int i = moveIndex; // to shorten line length
            float clampVMag = Mathf.Clamp(vel.magnitude, 0.0f, 1.5f); // clamp velocity magnitude for
            // new target point, using position of target and velocity direction vectors to place along current heading
            Vector3 targetPoint = newPositions[i] + clampVMag * (newPositions[i] - legTargets[i].position) + vel;

            CheckForGround(ref targetPoint, i); // function returns hit point if ground is beneath

            StartCoroutine(PerformStep(moveIndex, targetPoint)); // coroutine to move leg
        }
        #endregion

        #region Body Position and Orientation Adjustment
        priorRootPos = transform.position; // updates old body reference

        if (legCount > 3 && adjustOrientation) 
        {
            // calculates directional vectors using the legs positions
            Vector3 v1 = legTargets[0].position - legTargets[1].position;
            Vector3 v2 = legTargets[2].position - legTargets[3].position;
            Vector3 n = Vector3.Cross(v1, v2).normalized; // cross function returns the normal of all 4 direction vectors

            Vector3 orientation = Vector3.Lerp(priorRootNormal, n, 1f / (speedInverse + 1)); // lerp from current pos to new one

            // update values
            transform.up = priorRootNormal = orientation;

            transform.rotation = Quaternion.LookRotation(transform.parent.forward, orientation);
        }
        #endregion
    }

    /// <summary>
    /// Alters passed in value to be the point of hit if ground is below
    /// </summary>
    /// <param name="point">Point at which raycast is sent from</param>
    /// <returns></returns>
    private void CheckForGround(ref Vector3 point, int index)
    {
        // raycast from slightly above the legs position
        Ray ray = new Ray(point + maxRange * legTargets[index].up, -legTargets[index].up);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange * 2)) // ray length double value of MaxRange
        {
            point = hit.point;
        }
    }

    /// <summary>
    /// Performs step function across multiple frames
    /// </summary>
    /// <param name="i"> leg index value </param>
    /// <param name="target"> target position to step to</param>
    /// <returns></returns>
    IEnumerator PerformStep(int i, Vector3 target)
    {
        for (int j = 1; j <= speedInverse; ++j) // breaks across more frames the higher speed is
        {
            // lerp from prior position to target position using calculated value
            legTargets[i].position = Vector3.Lerp(priorLegSpacing[i], target, j / (speedInverse + 1f));

            // lifts position of leg around for an arc using sin curve calculations
            legTargets[i].position += transform.up * Mathf.Sin(j / (speedInverse + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        // once updates are done
        legTargets[i].position = target; // hard sets target
        priorLegSpacing[i] = legTargets[i].position; // updates old position reference
        legMoving = false; // de flags legMove bool

    }

    public void OrientationToggle()
    {
        adjustOrientation = !adjustOrientation;
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < legCount; i++)
        {
            // gizmo to show foot position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.02f);

            // gizmo to show foot safe space
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegSpacing[i]), stepLength);
        }
    }
}
