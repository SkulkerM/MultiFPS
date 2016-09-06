using UnityEngine;
using System.Collections;

public class BotMovement : MonoBehaviour
{
  // This script is only for 'my bot' -- in other words, only the local client will 
  // have this enabled.  This means that the MASTER client is probably responsible 
  // for spawning bots.
  // Remote bots will have this script disabled.

  NetworkCharacter netChar;
  static Waypoint[] waypoints;
  Waypoint destination;
  float waypointTargetDist = 1f;

	// Use this for initialization
	void Start () 
  {
    netChar = GetComponent<NetworkCharacter>();
    if (waypoints == null)
      waypoints = GameObject.FindObjectsOfType<Waypoint>();

    destination = GetClosestWaypoint();
	}
	
  Waypoint GetClosestWaypoint()
  {
    Waypoint closest = null;
    float dist = 0f;
    float d = 0f;

    foreach (Waypoint w in waypoints) 
    {
      if ((d = Vector3.Distance(transform.position, w.transform.position)) < dist || closest == null)
      {
        closest = w; dist = d;
      }
    }
    return closest;
  }

	// Update is called once per frame
	void Update () 
  {
    if (destination != null) 
    {
      // check if we've arrived
      if (Vector3.Distance(destination.transform.position, transform.position) <= waypointTargetDist)
      {
        if (destination.edges != null && destination.edges.Length > 0) 
        {
          destination = destination.edges[Random.Range(0, destination.edges.Length)];
        }
        else 
        {
          destination = null;
        }
      }

      if (destination != null)
      {
        netChar.direction = destination.transform.position - transform.position;
        netChar.direction.y = 0;
        netChar.direction.Normalize();
        transform.rotation = Quaternion.LookRotation(netChar.direction);
      }
      else 
      {
        netChar.direction = Vector3.zero;
      }
    }
  }
}
