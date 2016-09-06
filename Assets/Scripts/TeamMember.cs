using UnityEngine;
using System.Collections;

public class TeamMember : MonoBehaviour 
{
  private int _teamID = 0;

  public int teamID
  {
    get { return _teamID; }
  }

  [PunRPC]
  void SetTeamID(int tid)
  {
    Debug.Log("Setting team ID: " + tid);
    // remember the team ID
    _teamID = tid;
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
    var meshes = transform.GetComponentsInChildren<SkinnedMeshRenderer>(false);
    foreach (var mesh in meshes)
    {
      if (mesh.material.HasProperty("_Color"))
        mesh.material.color = clr;
    }
  }
}
