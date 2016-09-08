using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
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
  TeamMember myTarget = null;
  float targettingCoolDown = 5f;
  float targetAngleCriteria = 10f;
  float targetInaccuracy = 2f;

  // Use this for initialization
  void Start()
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

  void DoDestination()
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
    }
  }

  void DoDirection()
  {
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
  }

  void DoTargetting()
  {
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
          // the direction is independent of height.. we are shooting from our origin to his so that
          // once we push it up on Y, we wind up making mid-body to mid-body
          Vector3 dir = tm.transform.position - transform.position;
          Ray ray = new Ray(transform.position + new Vector3(0f, 1.5f, 0f), dir);
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
    myTarget = closest;
  }

  void DoRotation()
  {
    Vector3 lookDirection;

    if (myTarget != null)
    {
      lookDirection = myTarget.transform.position - transform.position;
    }
    else
    {
      lookDirection = netChar.direction;
    }

    Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
    lookRotation.eulerAngles = new Vector3(0, lookRotation.eulerAngles.y, 0);
    transform.rotation = lookRotation;

    if (myTarget != null)
    {
      Vector3 localLookDir = transform.InverseTransformDirection(lookDirection);
      float targetAimAngle = Mathf.Atan2(localLookDir.y, localLookDir.z) * Mathf.Rad2Deg;
      netChar.aimAngle = targetAimAngle;
    }
    else netChar.aimAngle = 0f;
  }

  void DoFire()
  {
    if (myTarget == null) return;

    Vector3 targetPos = myTarget.transform.position;
    targetPos.y = transform.position.y;
    if (Vector3.Angle(transform.forward, targetPos - transform.position) < targetAngleCriteria)
    {
      Debug.Log("aa: " + -netChar.aimAngle);
      Quaternion aa = Quaternion.Euler(-netChar.aimAngle + Random.Range(-targetInaccuracy, targetInaccuracy), 0, 0);
      Debug.Log("ai: " + aa.eulerAngles);
      Vector3 fireDir = aa * Vector3.forward;
      //Debug.Log("ac " + fireDir);
      //Vector3 inacc = new Vector3(Random.Range(-targetInaccuracy, targetInaccuracy),
      //                            Random.Range(-targetInaccuracy, targetInaccuracy), 0);
      //fireDir = Quaternion.Euler(inacc) * fireDir;
      //Debug.Log("ia " + fireDir);
      fireDir = transform.TransformDirection(fireDir);
      Debug.Log("ws " + fireDir);

      netChar.FireWeapon(transform.position + new Vector3(0f, 1.5f, 0f), fireDir);
    }
  }

  // Update is called once per frame
  void Update()
  {
    DoDestination();
    DoDirection();
    targettingCoolDown -= Time.deltaTime;
    if (targettingCoolDown <= 0) 
    {
      DoTargetting();
      targettingCoolDown = 0.5f;
    }
    DoRotation();
    DoFire();
  }
}
