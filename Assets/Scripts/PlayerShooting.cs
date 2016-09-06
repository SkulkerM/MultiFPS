using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

  float coolDown = 0;
  FXManager fxManager;
  WeaponData weaponData = null;

    void Start()
  {
    fxManager = GameObject.FindObjectOfType<FXManager>();
  }

	// Update is called once per frame
	void Update () 
  {
    coolDown -= Time.deltaTime;

	  if (Input.GetButton("Fire1"))
    {
      Fire();
    }
	}

  void Fire()
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

    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

    RaycastHit hitInfo;

    if (FindClosestHitInfo(ray, out hitInfo))
    {
      Debug.Log("We hit: " + hitInfo.collider.name);
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
