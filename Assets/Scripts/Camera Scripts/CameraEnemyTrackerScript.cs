using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTrackerScript : MonoBehaviour 
{
    // Variables to Lock Enemies
    private static List<Transform> lockableEnemies;
    private Collider[] nearbyEnemyColliders;
    private bool enemyLocked;

    // Global Data References
    private Camera playerCamera;
    private Transform playerTransform;

    [Header("Lock On Parameters")]
    public float MaximumLockDistance = 10f;
    public float FixedStepMultiplier = 2f;
    private float timeSinceLastRefresh;
    private float refreshTime;
    private bool inputEnabled = true;

    

    void Awake()
    {
        // Set reference to GlobalData 
        GlobalData.CameraEnemyTrackerScript = this;
        
        // Clear the enemy array.
        lockableEnemies = new List<Transform>();
    }

    void Start()
    {
        // Get references from GlobalData 
        playerCamera = GlobalData.PlayerCamera;
        playerTransform = GlobalData.PlayerTargetTransform;

        refreshTime = FixedStepMultiplier * Time.fixedDeltaTime;
    }

    void LateUpdate () 
    {
        if (inputEnabled)
		{
            UpdateLockOn();
        }
    }

    void FixedUpdate()
    {
        if (GlobalData.IsEnemyLocked)
        {
            timeSinceLastRefresh += Time.fixedDeltaTime;
            if (timeSinceLastRefresh >= refreshTime)
            {
                RefreshNearEnemies();
                timeSinceLastRefresh = 0;
            }
            
        } 
    }
        
    void UpdateLockOn()
    {
    
        if (!GlobalData.PlayerDeath && Input.GetButtonDown(InputAxis.LockOn) )
        {
            if (GlobalData.IsEnemyLocked)
            {
                UnlockEnemy();
                
            }
            else
            {
                RefreshNearEnemies();
                LockEnemy();
            }
        }

        if (GlobalData.IsEnemyLocked && lockableEnemies.Count == 0)
        {
            UnlockEnemy();
        }
        else if (GlobalData.IsEnemyLocked && !lockableEnemies.Contains(GlobalData.LockedEnemyTransform))
        {
            UnlockEnemy();
        }
        else if (GlobalData.PlayerDeath)
        {
            UnlockEnemy();
        }

        //else if the enemy is behind a wall for more than 2 secs.
    }

    // This code realeses the enemey locked on.
    void UnlockEnemy()
    {
        GlobalData.LockedEnemyTransform = null;
        GlobalData.IsEnemyLocked = false;
    }

    // Function that locks on the closest enemy available. If there are no enemies available, just center the camera.
    void LockEnemy()
    {
        if (lockableEnemies.Count != 0)
        {
            float closestDistance = float.MaxValue;
            float enemyDistance;

            foreach (Transform enemy in lockableEnemies)
            {
                enemyDistance = Vector3.Distance(playerTransform.position,enemy.position);
                if (closestDistance > enemyDistance)
                {
                    if (!Physics.Raycast(enemy.position,playerTransform.position - enemy.position , Vector3.Distance(GlobalData.PlayerTransform.position,enemy.position), (1 << LayerMask.NameToLayer("Environment"))))
                    {
                        closestDistance = enemyDistance;
                        GlobalData.LockedEnemyTransform = enemy;
                    }
                }
            }

            if (GlobalData.LockedEnemyTransform != null)
            {
                GlobalData.IsEnemyLocked = true;
            }
            else
            {
                GlobalData.CenterCamera();
            }
        }
        else
        {
            GlobalData.CenterCamera();
        }
    }

    // This function updates the lockable enemeies by casting a sphere.
    void RefreshNearEnemies()
    {
        lockableEnemies = new List<Transform>();
        nearbyEnemyColliders =  Physics.OverlapSphere(GlobalData.PlayerTransform.position,MaximumLockDistance,GlobalData.EnemiesLayerMask.value);
       
        foreach(Collider enemyCollider in nearbyEnemyColliders)
        {
            lockableEnemies.Add(enemyCollider.transform);
        }
    }


    // Move to the closest enemy on the screen (given the direction)
    public void ChangeLockOn(float input)
    {
        Transform newLockedEnemy = GlobalData.LockedEnemyTransform;
        float DistanceToPreviousEnemy = 999*Mathf.Sign(input);
        
        if (Mathf.Sign(input) > 0)
        {
            foreach(Transform enemy in lockableEnemies)
            {
                if (playerCamera.WorldToViewportPoint(GlobalData.LockedEnemyTransform.position).x < playerCamera.WorldToViewportPoint(enemy.position).x && playerCamera.WorldToViewportPoint(enemy.position).x < DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = playerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }
        else if (Mathf.Sign(input) < 0)
        {
            foreach(Transform enemy in lockableEnemies)
            {
                if (playerCamera.WorldToViewportPoint(GlobalData.LockedEnemyTransform.position).x > playerCamera.WorldToViewportPoint(enemy.position).x && playerCamera.WorldToViewportPoint(enemy.position).x > DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = playerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }

        GlobalData.LockedEnemyTransform = newLockedEnemy;
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

}
