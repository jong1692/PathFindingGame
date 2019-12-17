using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitController
{
    [SerializeField]
    private AudioSource moveAudio;

    private bool isSuperMode;
    private float superModeTimer;
    private float superModeDelay = 3.0f;

    public bool IsSuperMode
    {
        get { return isSuperMode; }
    }

    void Start()
    {
        initialize();
    }

    protected override void initialize()
    {
        destination = transform.position;

        isSuperMode = false;
        superModeTimer = 0;

        base.initialize();
    }

        void Update()
    {
        if (isSuperMode)
        {
            superModeTimer += Time.deltaTime;

            if (superModeTimer > superModeDelay) endSuperMode();
        }

        Vector3 lastDestination = destination;

        setDestination();

        if (destination != lastDestination)
            move();

        if (moveAudio != null && transform.position == gm.pos2center(destination))
            moveAudio.Stop();
    }

    public override void reset()
    {
        base.reset();

        isSuperMode = false;
        superModeTimer = 0;

        if (moveAudio != null && !moveAudio.isPlaying)
            moveAudio.Play();
    }

    protected override void setDestination()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if ((hit.transform.tag == "Plane" && hit.normal != Vector3.up)
                    || hit.transform.tag == "Wall") return;

                destination = hit.point;
            }
        }
    }

    protected override void move()
    {
        base.move();

        if (moveAudio != null && !moveAudio.isPlaying && !gameManager.Pause && Time.timeScale != 0)
            moveAudio.Play();
    }

    private void beginSuperMode()
    {
        isSuperMode = true;

        GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.red;
    }

    private void endSuperMode()
    {
        isSuperMode = false;

        superModeTimer = 0;

        GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.white;

        gameManager.Combo = 0;
    }

    
    protected override void death()
    {
        base.death();

        moveAudio.Stop();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sphere")
        {
            other.gameObject.SetActive(false);

            gameManager.increaseScore();
        }
        else if (other.tag == "Star")
        {
            other.gameObject.SetActive(false);

            beginSuperMode();
        }
        else if (other.tag == "Enemy" && !isSuperMode)
        {
            death();

            gameManager.decreaseLife();
        }
    }
}
