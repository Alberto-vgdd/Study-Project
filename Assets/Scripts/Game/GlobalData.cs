using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{   
    // Gameplay variables
    public static bool PlayerDeath;
    public static bool GamePaused = false;

    //  GameManager variables
    public static GameManagerScript GameManager;

    // Reference to the GameUJI
    public static GameUIScript GameUIScript;
    
    // Layer Masks
    public static LayerMask EnvironmentLayerMask = LayerMask.GetMask("Environment");
    public static LayerMask EnemiesLayerMask = LayerMask.GetMask("Enemies");
    public static LayerMask InteractableLayerMask = LayerMask.GetMask("Interactables");

    // Tags
    public const string PlayerTag = "Player";

    // Variables to lock enemies
    public static bool IsEnemyLocked;
    public static Transform LockedEnemyTransform;

    // Player Components
    public static Transform PlayerTransform;
    public static Transform PlayerTargetTransform;
    public static PlayerMovement  PlayerMovement;
    public static Animator PlayerAnimator;
    public static Camera PlayerCamera;
    public static Transform PlayerCameraHorizontalPivot;

    // Camera Scripts
    public static FreeCameraMovementScript FreeCameraMovementScript;
    public static FixedCameraMovementScript FixedCameraMovementScript;
    public static CameraShakeScript CameraShakeScript;
    public static CameraEnemyTrackerScript CameraEnemyTrackerScript;

    // Input Manager Script
    public static InputManagerScript InputManagerScript;

    // Call the function in the CameraEnemyTrackerScript
    public static void ChangeLockOn(float input)  {    CameraEnemyTrackerScript.ChangeLockOn(input);  }

    // Call the function in the CameraMovementScript
    public static void CenterCamera(){    FreeCameraMovementScript.CenterCamera();}

}