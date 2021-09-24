using System;
using UnityEngine;

struct Inputs {
    public Vector2 axis;
    public Vector2 tempAxis;
    public uint framesPassed;
    public byte jump;
}

public class CharController : MonoBehaviour {
    public float moveForce;
    public float jumpForce;
    public float gravMult;

    private Inputs curInputs;
    private Rigidbody RB3D;
    private int grounds = 0;
    private float sqrt2 = 0;

    // Start is called before the first frame update
    void Start() {
        RB3D = gameObject.GetComponent<Rigidbody>();
        sqrt2 = 1f / Mathf.Sqrt(2);         //sqrt is a fairly intensive operation, storing it in memory to avoid using opertaion every fixed update
    }

    // Update is called once per frame
    void Update() {
        if (Cursor.visible)
            return;

        //Store input from each update to be considered for fixed updates, dont do needless addition of 0 if unneeded
        curInputs.tempAxis.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (curInputs.tempAxis.x != 0f) curInputs.axis.x += curInputs.tempAxis.x;
        if (curInputs.tempAxis.y != 0f) curInputs.axis.y += curInputs.tempAxis.y;
        ++curInputs.framesPassed;
        if (Input.GetButtonDown("Jump") && curInputs.jump == 0 && grounds > 0) curInputs.jump = 5;
    }

    private void FixedUpdate() {
        //if both inputs were pressed then normalize inputs
        if (curInputs.axis.x != 0f && curInputs.axis.y != 0f)
            curInputs.axis.Set(curInputs.axis.x * sqrt2, curInputs.axis.y * sqrt2);

        if (curInputs.axis.x != 0f) {
            RB3D.AddForce(transform.right * moveForce * curInputs.axis.x / curInputs.framesPassed * Time.fixedDeltaTime);
            curInputs.axis.x = 0f;
        }

        if (curInputs.axis.y != 0f) {
            RB3D.AddForce(transform.forward * moveForce * curInputs.axis.y / curInputs.framesPassed * Time.fixedDeltaTime);
            curInputs.axis.y = 0f;
        }

        //jump on maxed cooldown
        if (curInputs.jump == 5)
            RB3D.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        
        //reduce jump cooldown if grounded
        if (curInputs.jump > 0 && grounds > 0)
            --curInputs.jump;

        //Add downwards force if ungrounded (RB3D has drag)
        if (grounds <= 0)
            RB3D.AddForce(Physics.gravity * gravMult, ForceMode.Acceleration);

        curInputs.framesPassed = 0;
    }

    //TODO: add/remove col references to dynamic array for checking each active collision if their hit normal is at a threshhold for grounded when needed (function called by jumping)
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ground"))
            ++grounds;
    }
    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("Ground"))
            --grounds;
    }
}
