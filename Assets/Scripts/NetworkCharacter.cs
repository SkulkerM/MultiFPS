using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {
  Vector3 realPosition = Vector3.zero;
  Quaternion realRotation = Quaternion.identity;
  float realAimAngle = 0f;
  Animator anim;
  bool firstUpdate = false;

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
  }	

	// Update is called once per frame
	void Update () 
  {
    if (photonView.isMine)
    {
      // Do nothing - the character motor/input/etc. is moving us
    }
    else
    {
      transform.position = Vector3.Lerp(transform.position, realPosition, 0.5f);
      transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.5f);
      anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.5f));
    }
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
