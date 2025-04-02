using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets;

public class PlayerAIController : MonoBehaviour
{
    PlayerController playerController;
    public Transform target;
    public int letterCnt = 0;
    readonly float maxDistance = 8;
    public bool isJump = false;
    public float moveX = 0f;
    public float moveY = 0f;
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!playerController.isAI)
            return;
            
        if(target == null)
        {
            SetTarget();
        }
        CalcMovement();
        if(target != null && target.tag == "Letter" && Distance() < 1.2f)
        {
            target.GetComponent<PitLetterManager>().Despawn();
            letterCnt++;
        }
    }
    
    float Distance()
    {
        Transform tmp = target;
        if(target.tag == "Letter")
            tmp = target.GetChild(0);
        return (tmp.position - transform.position).magnitude;
    }
    
    void CalcMovement()
    {
        Ray ray = new Ray(transform.position + new Vector3(0, 0.1f, 0), transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            if(hitObject.tag == "Env")
            {
                isJump = true;
            }
            else
            {
                isJump = false;
            }
        }
        else
        {
            isJump = false;
        }
        if(target == null)
            return;
        Vector3 directionToTarget = (target.tag == "Letter" ? target.GetChild(0).position : target.position) - transform.position;
        directionToTarget.y = 0;
        directionToTarget.Normalize();
        float angle = Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), directionToTarget);
        // moveY = Mathf.InverseLerp(0f, 180f, angle) * 2 - 1;
        moveY = angle / 180f;
        moveX = Mathf.Max(0, 1 - moveY * 2);
    }
    
    void SetTarget()
    {
        if(letterCnt < 3)
        {
            GameObject[] letters = GameObject.FindGameObjectsWithTag("Letter");
            if(letters.Length > 0)
                target = letters[Random.Range(0, letters.Length)].transform;
        }
        else
        {
            target = PlayerSpawnManager.Singleton.homes[playerController.positionID.Value].transform;
        }
    }
}
