using System.Collections;
using UnityEngine;

public class ArachnidProceduralAnimationSolver2 : MonoBehaviour
{
    [Tooltip("All Leg IK Targets for the rig")]
    [SerializeField] private Transform[] legTargets;
    [Tooltip("Minimum distance needed before the system decides a step is needed")]
    [Range(0.01f, 0.5f)] [SerializeField] private float stepLength = 0.15f;
    [Tooltip("Maxmimum height of steps taken, used in calculation of sin curve")]
    [Range(0.01f, 0.5f)] [SerializeField] private float stepHeight = 0.15f;
    [Tooltip("Smoothness of movement for calculations")]
    [Range(0.01f, 10f)] [SerializeField] private float speed = 8f;
    [Tooltip("If the system should adjust body orientation whilst walking")]
    [SerializeField] bool adjustOrientation = true;

    private float maxRange = 1f; // maximum raycast range

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

    }

    // fixed update for raycasts
    void FixedUpdate()
    {
        #region Velocity Calculations
        vel = transform.position - priorRootPos; // velocity calculated as a direction vector
        vel = (vel + speed * lastVel) / (speed + 1f); // used to indicate which direction feet should place
        
        if (vel.magnitude < 0.000025f) vel = lastVel;
        else lastVel = vel;
        vel *= velMultiplier;
        #endregion

        #region Decide which leg to move
        Vector3[] newPositions = new Vector3[legCount]; // vector to hold new calculated positions
        int moveIndex = -1; // value to indicate which foot in index should move
        float largestDistance = stepLength; // largest distance or (at default) regular step length

        for (int i = 0; i < legCount; i++) // loop through all legs
        {
            newPositions[i] = transform.TransformPoint(defaultLegSpacing[i]); // gets original position of legs

            // length of the distance calculated to be moved
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

            Vector3[] positionAndNormal = GetHitPointNormal(targetPoint); // function returns hit point and normal of hit

            StartCoroutine(PerformStep(moveIndex, positionAndNormal[0])); // coroutine to move leg
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

            Vector3 up = Vector3.Lerp(priorRootNormal, n, 1f / (speed + 1)); // lerp from current pos to new one

            // update values
            transform.up = up;
            priorRootNormal = up;
        }
        #endregion
    }

    /// <summary>
    /// Returns point of raycast hit and normal of the surface hit
    /// </summary>
    /// <param name="point">Point at which raycast is sent from</param>
    /// <returns></returns>
    private Vector3[] GetHitPointNormal(Vector3 point)
    {
        Vector3[] result = new Vector3[2]; // result vector array

        // raycast from slightly above the legs position
        Ray ray = new Ray(point + maxRange * transform.up, -transform.up);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 2f * maxRange)) // ray length double value of MaxRange
        {
            // return new hit point and normal value of hit point to rotate
            result[0] = hit.point;
            result[1] = hit.normal;
        }
        else
        {
            // return passed in value as should still be moved to keep up with system
            result[0] = point;
            //result[1] = Vector3.zero;
        }
        return result;
    }

    /// <summary>
    /// Performs step function across multiple frames
    /// </summary>
    /// <param name="i"> leg index value </param>
    /// <param name="target"> target position to step to</param>
    /// <returns></returns>
    IEnumerator PerformStep(int i, Vector3 target)
    {
        for (int j = 1; j <= speed; ++j) // breaks across more frames the higher speed is
        {
            // lerp from prior position to target position using calculated value
            legTargets[i].position = Vector3.Lerp(priorLegSpacing[i], target, j / (speed + 1f));

            // lifts position of leg around for an arc using sin curve calculations
            legTargets[i].position += transform.up * Mathf.Sin(j / (speed + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        // once updates are done
        legTargets[i].position = target; // hard sets target
        priorLegSpacing[i] = legTargets[i].position; // updates old position reference
        legMoving = false; // de flags legMove bool

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < legCount; i++)
        {
            // gizmo to show foot position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);

            // gizmo to show foot safe space
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegSpacing[i]), stepLength);
        }
    }
#endif
}
