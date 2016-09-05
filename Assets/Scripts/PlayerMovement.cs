using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
  //
  //  This is only enabled for 'my player'
  //

  public float speed;
  public float jumpSpeed;

  Vector3 direction = Vector3.zero;
  float verticalVelocity = 0f;
  CharacterController cc;
  Animator anim;

	// Use this for initialization
	void Start () 
  {
    cc = GetComponent<CharacterController>();
    anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
  {
    // forward/back & left/right movement is stored in "direction"
    direction = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    if (direction.magnitude > 1.0f) direction = direction.normalized;
    anim.SetFloat("Speed", direction.magnitude);

    // handle jumping
    if (cc.isGrounded && Input.GetButton("Jump"))
        verticalVelocity = jumpSpeed;
    AdjustAimAngle();
  }

  void AdjustAimAngle()
  {
    float aim;
    Camera cam = GetComponentInChildren<Camera>();
    if (cam.transform.rotation.eulerAngles.x <= 90f)
      aim = -cam.transform.rotation.eulerAngles.x;
    else aim = 360f - cam.transform.rotation.eulerAngles.x;
    //Debug.Log("Aim: " + aim);
    anim.SetFloat("AimAngle", aim);
  }

  // FixedUpdate is called once per physics loop
  // Do all MOVEMENT and other physics stuff here
  void FixedUpdate()
  {
    Vector3 dist = direction * speed * Time.deltaTime;
    
    if (cc.isGrounded && verticalVelocity < 0f) 
    {
      anim.SetBool("Jumping", false);
      verticalVelocity = Physics.gravity.y * Time.deltaTime;
    }
    else 
    {
      if (Mathf.Abs(verticalVelocity) > jumpSpeed * 0.75f) 
        anim.SetBool("Jumping", true);

      verticalVelocity += Physics.gravity.y * Time.deltaTime;
    }

    dist.y = verticalVelocity * Time.deltaTime;
    cc.Move(dist);
  }
}
