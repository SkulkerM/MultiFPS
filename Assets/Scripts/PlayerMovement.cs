using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour 
{
  //
  //  This is only enabled for 'my player'
  //
  Animator anim;
  NetworkCharacter netChar;

	// Use this for initialization
	void Start () 
  {
    anim = GetComponent<Animator>();
    netChar = GetComponent<NetworkCharacter>();
	}
	
	// Update is called once per frame
	void Update () 
  {
    // forward/back & left/right movement is stored in "direction"
    netChar.direction = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, 
                                                         Input.GetAxis("Vertical"));
    if (netChar.direction.magnitude > 1.0f) netChar.direction = netChar.direction.normalized;

    // handle jumping
    netChar.isJumping = (Input.GetButton("Jump"));

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
}
