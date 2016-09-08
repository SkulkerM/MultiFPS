using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
  public float hitPoints = 100f;
  float currentHitPoints;

	// Use this for initialization
	void Start () 
  {
    currentHitPoints = hitPoints;
	}

  [PunRPC]
  public void TakeDamage(float amt)
  {
//    Debug.Log(gameObject.name + " took damage: " + amt + "; hp: " + currentHitPoints);
    currentHitPoints -= amt;
    if (currentHitPoints <= 0) Die();
  }

  //void OnGUI()
  //{
  //  if (GetComponent<PhotonView>().isMine && gameObject.CompareTag("Player"))
  //  {
  //    if (GUI.Button(new Rect(Screen.width - 200, 50, 100, 40), "Suicide"))
  //    {
  //      Debug.Log("Suicide");
  //      Die();
  //    }
  //  }
  //}

  void Die()
  {
    if (GetComponent<PhotonView>().isMine)
    {
      // if this is my player, initiate respawn
      if (gameObject.CompareTag("Player"))
      {
        var nm = GameObject.FindObjectOfType<NetworkManager>();
        nm.standbyCamera.SetActive(true);
        nm.respawnTimer = 3;
      }
      else if (gameObject.CompareTag("Bot"))
      {
        Debug.Log("No bot respawn yet!");
      }

      PhotonNetwork.Destroy(gameObject);
    }
  }
}
