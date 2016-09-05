using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

public class LegacyMouseLook : MonoBehaviour 
{
  [SerializeField] private MouseLook m_MouseLook;
  private Camera m_Camera;

  // Use this for initialization
  void Start () 
  {
    m_Camera = Camera.main;
    m_MouseLook.Init(transform, m_Camera.transform);
	}
	
  private void FixedUpdate()
  {
    m_MouseLook.UpdateCursorLock();
  }
	
  private void Update()
  {
    m_MouseLook.LookRotation(transform, m_Camera.transform);
  }
}
