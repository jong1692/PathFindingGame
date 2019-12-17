using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    private WayPoint destination;
    private bool canTeleport = true;
    private PlayerController player;

    public WayPoint Destination
    {
        set { destination = value; }
    }

    public bool CanTeleport
    {
        set { canTeleport = value; }
    }

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void teleport(GameObject target)
    {
        if (canTeleport)
        {
            destination.canTeleport = false;
            target.transform.position = destination.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            teleport(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            canTeleport = true;
        }
    }
}
