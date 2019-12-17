using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : UnitController
{
    private enum CHASING_PATTERN
    {
        SHORTEST, PREDICTIVE, OPPOSITIONAL, WACKY
    }

    [SerializeField]
    private CHASING_PATTERN chasingPattern;

    private PlayerController player;
    private EnemyController oppositTarget;

    private float respawnTiemer = 0;
    private float respawnDelay = 5.0f;

    private readonly float tileOffset = 24.5f;

    private bool inRespawning = false;

    void Start()
    {
        initialize();
    }

    protected override void initialize()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();

        if (chasingPattern == CHASING_PATTERN.OPPOSITIONAL)
            oppositTarget = GameObject.Find("Ghost_Violet").GetComponent<EnemyController>();

        base.initialize();

        destination = transform.position;
    }

    void Update()
    {
        if (inRespawning)
        {
            respawnTiemer += Time.deltaTime;
            if (respawnTiemer > respawnDelay)
            {
                respawnTiemer = 0;
                inRespawning = false;
            }
            return;
        }

        Vector3 lastDestination = destination;
        setDestination();

        if (destination != lastDestination)
            move();
    }

    protected override void setDestination()
    {
        if (player.IsSuperMode)
        {
            destination = originPos;
            return;
        }

        switch (chasingPattern)
        {
            case CHASING_PATTERN.SHORTEST:
                destination = player.transform.position;
                break;

            case CHASING_PATTERN.PREDICTIVE:
                destination = player.Destination;
                break;

            case CHASING_PATTERN.OPPOSITIONAL:
                Vector3 vec = oppositTarget.Destination;
                vec.x = tileOffset * 2 - vec.x;

                destination = vec;
                break;

            case CHASING_PATTERN.WACKY:
                if (gm.pos2center(destination) == transform.position ||
                    gm.pos2center(destination) == originPos)
                {
                    int randCell;
                    do
                    {
                        randCell = Random.Range(0, gm.MaxTiles);
                    }
                    while (gm.World[randCell] == GridManager.TileType.Wall);

                    destination = gm.cell2Pos(randCell);
                }
                break;
        }
    }

    public override void reset()
    {
        base.reset();

        inRespawning = false;
        respawnTiemer = 0;
    }

    protected override void death()
    {
        base.death();

        gameManager.increaseScore(50, true);

        inRespawning = true;

        transform.position = originPos;

        destination = transform.position;
        move();
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && player.IsSuperMode && !inRespawning)
        {
            death();
        }
    }
}
