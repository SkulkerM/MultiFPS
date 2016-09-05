using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

  public GameObject standbyCamera;
  private SpawnSpot[] spawnSpots;
  public bool offlineMode = false;
  public float respawnTimer = 0;
  bool connecting = false;
  List<string> chatMessages;
  int maxChatMessages = 5;
  bool hasPickedTeam = false;
  int teamID;

  public void AddChatMessage(string msg)
  {
    GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, msg);
  }

  [PunRPC]
  void AddChatMessage_RPC(string msg)
  {
    while (chatMessages.Count >= maxChatMessages) chatMessages.RemoveAt(0);
    chatMessages.Add(msg);
  }

  void Start () 
  {
    spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
    PhotonNetwork.player.name = PlayerPrefs.GetString("UserName", "The Dude");
    chatMessages = new List<string>();
  }

  void Update()
  {
    if (respawnTimer > 0)
    {
      respawnTimer -= Time.deltaTime;
      if (respawnTimer <= 0) SpawnMyPlayer(teamID);
    }
  }

  void Connect()
  {
    PhotonNetwork.ConnectUsingSettings("MultiFPS v0.1");
  }

  void OnDestroy()
  {
    PlayerPrefs.SetString("UserName", PhotonNetwork.player.name);
  }

  void OnGUI()
  {
    GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    if (!PhotonNetwork.connected && !connecting) 
    {
      GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.BeginVertical();
      GUILayout.FlexibleSpace();

      GUILayout.BeginHorizontal();
      GUILayout.Label("UserName: ");
      PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name);
      GUILayout.EndHorizontal();

      if (GUILayout.Button("Single Player"))
      {
        connecting = true;
        PhotonNetwork.offlineMode = true;
        OnJoinedLobby();
      }

      if (GUILayout.Button("Multi Player")) 
      {
        connecting = true;
        Connect();
      }
      GUILayout.EndVertical();
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.EndArea();
    }
    else if (PhotonNetwork.connected && !connecting)
    {
      if (hasPickedTeam) 
      { 
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        foreach (string msg in chatMessages)
          GUILayout.Label(msg);
        GUILayout.EndVertical();
        GUILayout.EndArea();
      }
      // player has not selected team
      else 
      {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Red Team"))
        {
          SpawnMyPlayer(1);

        }
        else if (GUILayout.Button("Green Team"))
        {
          SpawnMyPlayer(2);
        }
        else if (GUILayout.Button("Random"))
        {
          SpawnMyPlayer(Random.Range(1, 3));
        }
        else if (GUILayout.Button("Renegade")) 
        {
          SpawnMyPlayer(0);
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
      }
    }
  }

  void OnJoinedLobby() 
  {
    PhotonNetwork.JoinRandomRoom();
  }

  void OnPhotonRandomJoinFailed()
  {
    PhotonNetwork.CreateRoom(null);
  }

  void OnJoinedRoom()
  {
    connecting = false;
  }

  void SpawnMyPlayer(int _teamID)
  {
    teamID = _teamID; hasPickedTeam = true;

    AddChatMessage("Spawning player: " + PhotonNetwork.player.name);
    //SpawnSpot spot = spawnSpots[Random.Range(0, spawnSpots.Length)];
SpawnSpot spot = spawnSpots[0];
    GameObject go = PhotonNetwork.Instantiate("PlayerController", spot.transform.position, spot.transform.rotation, 0);
    standbyCamera.SetActive(false);
    go.transform.FindChild("FirstPersonCharacter").gameObject.SetActive(true);
    go.GetComponent<PlayerMovement>().enabled = true;
    go.GetComponent<PlayerShooting>().enabled = true;
    go.GetComponent<CharacterController>().enabled = true;
    go.GetComponent<LegacyMouseLook>().enabled = true;
    go.GetComponent<TeamMember>().teamID = teamID;
    // set the color
    Color clr;
    switch (teamID) 
    {
      case 1:
        clr = new Color(1f, 0.5f, 0.5f); break;
      case 2:
        clr = new Color(0.5f, 1f, 0.5f); break;
      case 0:
      default:
        clr = Color.white; break;
      }
    var meshes = go.transform.GetComponentsInChildren<SkinnedMeshRenderer>(false);
    foreach (var mesh in meshes) 
    {
      if (mesh.material.HasProperty("_Color"))
        mesh.material.color = clr;
    }
  }
}
