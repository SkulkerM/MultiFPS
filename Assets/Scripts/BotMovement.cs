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
  float aggroRange = 10000000f;

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
      }
      else 
      {
        netChar.direction = Vector3.zero;
      }

      // by default, look the direction we're moving
      Vector3 lookDirection;

      // if we have an enemy in range...
      TeamMember closest = null;
      float dist = 0f;
      TeamMember myTeam = GetComponent<TeamMember>();
      RaycastHit hitInfo;
      foreach (TeamMember tm in GameObject.FindObjectsOfType<TeamMember>())
      {
        if (tm != myTeam && (tm.teamID == 0 || tm.teamID != myTeam.teamID))
        {
          float d = Vector3.Distance(tm.transform.position, transform.position);
          if (d < aggroRange)
          {
            // we need to sheet a ray out of the bot's eyes to the player... so, we need to elevate the position up to
            // eye level and shoot from there
            Vector3 pos = transform.position; pos.y += 1f;
            // the direction is independent of height.. we are shooting from our origin to his so that
            // once we push it up on Y, we wind up making mid-body to mid-body
            Vector3 dir = tm.transform.position - transform.position;
            Ray ray = new Ray(pos, dir);
            if (Physics.Raycast(ray, out hitInfo) && 
                hitInfo.transform.GetComponent<TeamMember>() == tm) 
            {
              if (closest == null || d < dist)
              {
                closest = tm; dist = d;
              }
            }
          }
        }
      }

      if (closest != null)
      {
        lookDirection = closest.transform.position - transform.position;
      }
      else 
      {
        lookDirection = netChar.direction; 
      }

      Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
      lookRotation.eulerAngles = new Vector3(0, lookRotation.eulerAngles.y, 0);
      transform.rotation = lookRotation; 
    }
  }
}
