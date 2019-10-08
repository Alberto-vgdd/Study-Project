using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public GameObject artAsset;
    public float fallSpeed = 2.5f;
    public float fallTime = 2;
    private float fallTimer = -1;
    public float waitTime = 1;
    private float waitTimer = -1f;


    private bool isUsed = false;
    private bool isFalling = false;
    private string playerTag;

    private Rigidbody platformRigidbody;
    private BoxCollider platformCollider;
    private Vector3 originalPosition;

    private int environmentLayerMask;
    private int enemiesLayerMask;

    
    
    void Awake()
    {
        platformRigidbody = GetComponent<Rigidbody>();
        platformCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        playerTag = GlobalData.PlayerTag;

        environmentLayerMask = 1 << (int) Mathf.Log(GlobalData.EnvironmentLayerMask.value,2);
        enemiesLayerMask = 1 << (int) Mathf.Log(GlobalData.EnemiesLayerMask.value,2);

        originalPosition = platformRigidbody.position;

        if (artAsset != null)
            artAsset.transform.SetParent(gameObject.transform);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !isUsed)
        {
            platformRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            waitTimer = 0f;
            isUsed = true;
        }
        else if (collision.gameObject.layer.Equals(environmentLayerMask | enemiesLayerMask))
        {
            Physics.IgnoreCollision(platformCollider, collision.collider, true);
        }
    }

    void FixedUpdate()
    {
        if (isUsed)
        {
            if (isFalling)
            {
                fallTimer += Time.fixedDeltaTime;
                platformRigidbody.MovePosition( platformRigidbody.position + Vector3.down*fallSpeed*Mathf.SmoothStep(0f,1f,fallTimer/fallTime*10)*Time.fixedDeltaTime);

                if (fallTimer > fallTime)
                {
                     platformRigidbody.interpolation = RigidbodyInterpolation.None;
                    platformRigidbody.MovePosition(originalPosition);
                    isUsed = false;
                    isFalling = false;
                    waitTimer = -1f;
                    fallTimer = -1f;
                }
            }
            else
            {
                waitTimer += Time.fixedDeltaTime;

                if (waitTimer > waitTime)
                {
                    isFalling = true;
                    fallTimer = 0f;
                }
            }
        }
    }
}
