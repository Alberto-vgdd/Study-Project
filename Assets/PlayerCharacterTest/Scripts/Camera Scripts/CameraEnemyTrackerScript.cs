using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTrackerScript : MonoBehaviour 
{
    // Variables to Lock Enemies
    private static List<Transform> m_LockableEnemies;
    private Collider[] m_NearbyEnemyColliders;
    private bool m_EnemyLocked;

    // This Camera, used to switch between targets.
    private Camera m_PlayerCamera;

    //[Header("Player Transform")]
    private Transform m_PlayerTransform;

    [Header("Lock On Parameters")]
    public float m_MaximumLockDistance;
    public float m_FixedStepMultiplier;
    private float m_TimeSinceLastRefresh;

    private bool inputEnabled = true;

    

    void Awake()
    {
        // Reference to GlobalData 
        GlobalData.CameraEnemyTrackerScript = this;
        
        // Clear the enemy array.
        m_LockableEnemies = new List<Transform>();
    }

    void Start()
    {
        m_PlayerCamera = GlobalData.PlayerCamera;
        m_PlayerTransform = GlobalData.PlayerTargetTransform;
    }
	void LateUpdate () 
    {
        if (!inputEnabled)
		{
			return;
		}
        UpdateLockOn();
	}

    void FixedUpdate()
    {
        if (GlobalData.IsEnemyLocked)
        {
            m_TimeSinceLastRefresh += Time.fixedDeltaTime;
            if (m_TimeSinceLastRefresh >= m_FixedStepMultiplier*Time.fixedDeltaTime)
            {
                RefreshNearEnemies();
                m_TimeSinceLastRefresh = 0;
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

        if (GlobalData.IsEnemyLocked && m_LockableEnemies.Count == 0)
        {
            UnlockEnemy();
        }
        else if (GlobalData.IsEnemyLocked && !m_LockableEnemies.Contains(GlobalData.LockedEnemyTransform))
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
        if (m_LockableEnemies.Count != 0)
        {
            float closestDistance = float.MaxValue;
            float enemyDistance;

            foreach (Transform enemy in m_LockableEnemies)
            {
                enemyDistance = Vector3.Distance(m_PlayerTransform.position,enemy.position);
                if (closestDistance > enemyDistance)
                {
                    if (!Physics.Raycast(enemy.position,m_PlayerTransform.position - enemy.position , Vector3.Distance(GlobalData.PlayerTransform.position,enemy.position), (1 << LayerMask.NameToLayer("Environment"))))
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
        m_LockableEnemies = new List<Transform>();
        m_NearbyEnemyColliders =  Physics.OverlapSphere(GlobalData.PlayerTransform.position,m_MaximumLockDistance,GlobalData.EnemiesLayerMask.value);
       
        foreach(Collider enemyCollider in m_NearbyEnemyColliders)
        {
            m_LockableEnemies.Add(enemyCollider.transform);
        }
    }


    // Move to the closest enemy on the screen (given the direction)
    public void ChangeLockOn(float input)
    {
        Transform newLockedEnemy = GlobalData.LockedEnemyTransform;
        float DistanceToPreviousEnemy = 999*Mathf.Sign(input);
        
        if (Mathf.Sign(input) > 0)
        {
            foreach(Transform enemy in m_LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(GlobalData.LockedEnemyTransform.position).x < m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x < DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }
        else if (Mathf.Sign(input) < 0)
        {
            foreach(Transform enemy in m_LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(GlobalData.LockedEnemyTransform.position).x > m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x > DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
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
