using Assets;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Linq;

public class SimpleCharacterControl : MonoBehaviour, IInitializable
{
    public void Initialize(GameObject character)
    {
        m_animator = character.GetComponent<Animator>();
        m_rigidBody = character.GetComponent<Rigidbody>();
        m_networkAnimator = character.GetComponent<OwnerNetworkAnimator>();
    }

    private enum ControlMode
    {
        /// <summary>
        /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
        /// </summary>
        Tank,
        /// <summary>
        /// Character freely moves in the chosen direction from the perspective of the camera
        /// </summary>
        Direct
    }

    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_turnSpeed = 200;
    [SerializeField] private float m_jumpForce = 4;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private OwnerNetworkAnimator m_networkAnimator;

    [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;

    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 1.5f;
    private float m_powerupAdd = 2f;
    private readonly float m_backwardsWalkScale = 1.2f;
    private readonly float m_backwardRunScale = 0.66f;

    public bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;
    private bool m_jumpInput = false;

    public bool m_isGrounded;

    public List<Collider> m_collisions = new List<Collider>();
    
    PlayerAIController aIController;
    PlayerController playerController;

    private void Awake()
    {
        aIController = GetComponent<PlayerAIController>();
        playerController = GetComponent<PlayerController>();
        if (!m_animator) { m_animator = gameObject.GetComponent<Animator>(); }
        if (!m_rigidBody) { m_rigidBody = gameObject.GetComponent<Rigidbody>(); }
        if (!m_networkAnimator) { m_networkAnimator = gameObject.GetComponent<OwnerNetworkAnimator>(); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        }
        else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count(collision => collision != null) == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count(collision => collision != null) == 0) { m_isGrounded = false; }
    }


    private void Update()
    {
        if(NetworkManager.Singleton.IsApproved && !GetComponent<NetworkObject>().IsOwner)
            return;
            
        if(!m_jumpInput && Input.GetKey(KeyCode.Space))
        {
            m_jumpInput = true;
        }
    }

    private void FixedUpdate()
    {
        if(NetworkManager.Singleton.IsApproved && !GetComponent<NetworkObject>().IsOwner)
            return;
            
        if(InGameHudManager.Singleton.gameState != GameState.Playing)
            return;
            
        m_animator.SetBool("Grounded", m_isGrounded);
        if(!playerController.isAI && InGameHudManager.Singleton.isPowerUpUsing && !InGameHudManager.Singleton.isFreeze)
        {
            if(InGameHudManager.Singleton.powerUpType == PowerUpType.JumpUp)
                m_powerupAdd = 2f;
            if(InGameHudManager.Singleton.powerUpType == PowerUpType.SpeedUp)
                m_moveSpeed = 5f;
        }
        else
        {
            m_powerupAdd = 1.5f;
            m_moveSpeed = 3f;
        }
        
        if(InGameHudManager.Singleton.isFreeze)
        {
            m_moveSpeed = 1f;
        }
        
        switch (m_controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        m_wasGrounded = m_isGrounded;
        m_jumpInput = false;
    }

    private void TankUpdate()
    {
        float v;
        float h;
        bool walk;
        if(GetComponent<PlayerController>().isAI)
        {
            v = GetComponent<PlayerAIController>().moveX;
            h = GetComponent<PlayerAIController>().moveY;
            walk = false;
        }
        else
        {
            v = Input.GetAxis("Vertical");
            h = Input.GetAxis("Horizontal");
            walk = Input.GetKey(KeyCode.LeftShift);
        }


        if (v < 0)
        {
            if (walk) { v *= m_backwardsWalkScale; }
            else { v *= m_backwardRunScale; }
        }
        else if (walk)
        {
            v *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
        transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

        m_animator.SetFloat("MoveSpeed", m_currentV);

        JumpingAndLanding();
    }

    private void DirectUpdate()
    {
        float v;
        float h;
        if(GetComponent<PlayerController>().isAI)
        {
            v = GetComponent<PlayerAIController>().moveX;
            h = GetComponent<PlayerAIController>().moveY;
        }
        else
        {
            v = Input.GetAxis("Vertical");
            h = Input.GetAxis("Horizontal");
        }

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && (aIController.isJump && playerController.isAI || !playerController.isAI && m_jumpInput))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce * m_powerupAdd, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            //m_animator.SetTrigger("Land");
            m_networkAnimator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            //m_animator.SetTrigger("Jump");
            m_networkAnimator.SetTrigger("Jump");
        }
    }
}
