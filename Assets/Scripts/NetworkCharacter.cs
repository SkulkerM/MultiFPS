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
  [System.NonSerialized]
  public float aimAngle = 0f;

  float verticalVelocity = 0f;

  Vector3 realPosition = Vector3.zero;
  Quaternion realRotation = Quaternion.identity;
  float realAimAngle = 0f;
  Animator anim;
  bool firstUpdate = false;
  CharacterController cc;
  float coolDown = 0;
  FXManager fxManager;
  WeaponData weaponData = null;

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
    if (fxManager == null)
    {
      fxManager = GameObject.FindObjectOfType<FXManager>();
    }
  }

  void Update()
  {
    coolDown -= Time.deltaTime;
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

    anim.SetFloat("AimAngle", aimAngle);
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

  public void FireWeapon(Vector3 origin, Vector3 dir)
  {
    if (coolDown > 0) return;
    if (weaponData == null)
    {
      weaponData = gameObject.GetComponentInChildren<WeaponData>();
      if (weaponData == null)
      {
        Debug.Log("Don't have weapon data?!");
        return;
      }
    }

    Ray ray = new Ray(origin, dir);

    RaycastHit hitInfo;

    if (FindClosestHitInfo(ray, out hitInfo))
    {
//      Debug.Log("We hit: " + hitInfo.collider.name);
      Transform t = hitInfo.transform;
      Health h = t.GetComponent<Health>();
      while (h == null && (t = t.parent)) h = t.GetComponent<Health>();
      if (h != null)
      {
        var tm = t.GetComponent<TeamMember>();
        var myTm = GetComponent<TeamMember>();
        if (tm == null || myTm == null || myTm.teamID == 0 || tm.teamID == 0 || tm.teamID != myTm.teamID)
          h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, weaponData.damage);
        else Debug.Log("His team: " + tm.teamID + " and my team: " + myTm.teamID);
      }
      DoGunFX(hitInfo.point);
    }
    // otherwise, didn't hit anything
    else DoGunFX(Camera.main.transform.position + (Camera.main.transform.forward * 100f));

    coolDown = weaponData.fireRate;
  }

  void DoGunFX(Vector3 hitPoint)
  {
    fxManager.GetComponent<PhotonView>().RPC("SniperBulletFX",
                                             PhotonTargets.All,
                                             weaponData.transform.position,
                                             hitPoint);
  }

  bool FindClosestHitInfo(Ray ray, out RaycastHit closest)
  {
    bool hasHit = false;
    RaycastHit[] hits = Physics.RaycastAll(ray);

    closest = default(RaycastHit);

    foreach (RaycastHit hit in hits)
    {
      if (hit.transform != this.transform && (!hasHit || hit.distance < closest.distance))
      {
        // we have hit something that is:
        // a) not us
        // b) the first thing we hit
        // c) if not b, closer than last hit
        hasHit = true;
        closest = hit;
      }
    }
    return hasHit;
  }

}
