using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.transform.parent.position = spawnPoint.position;
        }
    }
}
