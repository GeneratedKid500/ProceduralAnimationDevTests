using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BugMoveController : MonoBehaviour
{
    private Rigidbody rb;
    [Range(0.01f, 25f)] [SerializeField] private float moveSpeed = 10f;
    [Range(0.01f, 3f)][SerializeField] private float rotateSpeed = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // move back and forth
        Vector3 move = transform.forward * ((Input.GetKey(KeyCode.LeftShift) ? moveSpeed * 1.5f : moveSpeed) * (Time.deltaTime * 25) * Input.GetAxis("Vertical"));
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // rotate left and right
        transform.Rotate(transform.up * rotateSpeed * (Time.deltaTime * 25) * Input.GetAxis("Horizontal"));
    }
}
