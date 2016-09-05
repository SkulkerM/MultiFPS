using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour 
{
  public GameObject SniperBulletFXPrefab;

  [PunRPC]
  void SniperBulletFX(Vector3 startPos, Vector3 endPos)
  {
    GameObject sniperFx = (GameObject)Instantiate(SniperBulletFXPrefab, startPos, 
                                                  Quaternion.LookRotation(endPos - startPos));
    LineRenderer lr = sniperFx.transform.Find("LineFX").GetComponent<LineRenderer>();
    lr.SetPosition(0, startPos);
    lr.SetPosition(1, endPos);
  }
}
