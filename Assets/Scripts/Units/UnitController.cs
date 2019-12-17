using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitController : MonoBehaviour
{
    [SerializeField]
    protected float movementSpeed = 3.0f;

    [SerializeField]
    protected AudioSource deathAudio;

    protected GameManager gameManager;

    protected GridManager gm = null;
    protected Coroutine move_coroutine = null;
    protected Vector3 destination;
    protected Vector3 originPos;

    public Vector3 Destination
    {
        get { return destination; }
        set { destination = value; }
    }

    protected virtual void initialize()
    {
        gm = Camera.main.GetComponent<GridManager>() as GridManager;
        originPos = transform.position;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    protected virtual void move()
    {
        if (move_coroutine != null) StopCoroutine(move_coroutine);
        move_coroutine = StartCoroutine(gm.Move(gameObject, destination, movementSpeed));
    }

    protected virtual void death()
    {
        if (deathAudio != null)
            deathAudio.Play();
    }

    public virtual void reset()
    {
        transform.position = originPos;

        destination = transform.position;
        move();
    }

    protected abstract void setDestination();

    protected abstract void OnTriggerEnter(Collider other);
}
