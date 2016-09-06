using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour 
{
  public float speed;
  public float jumpSpeed;

  [System.NonSerialized]
  public Vector3 direction = Vector3.zero;
  [System.NonSerialized]
  public bool isJumping = false;
  float verticalVelocity = 0f;

  Vector3 realPosition = Vector3.zero;
  Quaternion realRotation = Quaternion.identity;
  float realAimAngle = 0f;
  Animator anim;
  bool firstUpdate = false;
  CharacterController cc;

  // Use this for initialization
  void Start () 
  {
    CacheComponents();
	}

  void CacheComponents()
  {
    if (anim == null)
    {
      anim = GetComponent<Animator>();
    }
    if (cc == null)
    {
      cc = GetComponent<CharacterController>();
    }
  }

  // FixedUpdate is called once per physics loop
  // Do all MOVEMENT and other physics stuff here
  void FixedUpdate () 
  {
    if (photonView.isMine)
    {
      DoLocalMovement();
    }
    else
    {
      transform.position = Vector3.Lerp(transform.position, realPosition, 0.5f);
      transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.5f);
      anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.5f));
    }
	}

  void DoLocalMovement()
  {
    Vector3 dist = direction * speed * Time.deltaTime;

    if (isJumping) 
    {
      isJumping = false;
      if (cc.isGrounded) verticalVelocity = jumpSpeed;
    }

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

    anim.SetFloat("Speed", direction.magnitude);
    cc.Move(dist);
  }

  public void OnPhotonSerializeView(PhotonStream stm, PhotonMessageInfo info)
  {
    CacheComponents();

    // if our player, we're sending
    if (stm.isWriting)
    {
      stm.SendNext(transform.position);
      stm.SendNext(transform.rotation);
      stm.SendNext(anim.GetBool("Jumping"));
      stm.SendNext(anim.GetFloat("Speed"));
      stm.SendNext(anim.GetFloat("AimAngle"));
    }
    // otherwise, other player so we're receiving
    else 
    {
      realPosition = (Vector3)stm.ReceiveNext();
      realRotation = (Quaternion)stm.ReceiveNext();
      anim.SetBool("Jumping", (bool)stm.ReceiveNext());
      anim.SetFloat("Speed", (float)stm.ReceiveNext());
      realAimAngle = (float)stm.ReceiveNext();

      if (!firstUpdate) 
      {
        transform.position = realPosition;
        transform.rotation = realRotation;
        anim.SetFloat("AimAngle", realAimAngle);
        firstUpdate = true;
      }
    }
  }
}
