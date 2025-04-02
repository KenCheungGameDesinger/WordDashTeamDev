using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HillObject : MonoBehaviour
{
    public AnimationCurve curve;
    public float downY;
    public float upY;
    public float speed = .5f;
    
    void Start()
    {
        speed = Random.Range(.3f, 1.3f);
    }
    
    void Update()
    {
        if(NetworkManager.Singleton.IsApproved && !NetworkManager.Singleton.IsHost)
            return;
        float time = Time.time * speed;
        float value = curve.Evaluate(time);
        transform.position = new Vector3(transform.position.x, Mathf.Lerp(downY, upY, value), transform.position.z);
    }
}
