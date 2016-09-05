using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {
  public float selfDestructTime = 1;

	// Update is called once per frame
	void Update () {
    selfDestructTime -= Time.deltaTime;
    if (selfDestructTime <= 0)
    {
      PhotonView pv = GetComponent<PhotonView>();
      if (pv == null || pv.instantiationId == 0)
      {
        Destroy(gameObject);
      }
      else 
      {
        PhotonNetwork.Destroy(gameObject);
      }
    }
  }
}
