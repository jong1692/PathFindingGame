using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using BlockType;

public class MapManager : MonoBehaviour
{
    [Serializable]
    public struct BlockData
    {
        public BLOCK_TYPE blockType;
        public Vector3 position;
    }

    private struct MapData
    {
        public BlockData[] blockDatas;
    }

    [Serializable]
    private struct BlockPrefab
    {
        public BLOCK_TYPE blockType;
        public GameObject Prefab;
    }

    [SerializeField]
    private bool isCreatMode;
    [SerializeField]
    private Transform mapRoot;
    [SerializeField]
    private int mapSize = 40;
    [SerializeField]
    private BlockPrefab[] blockPrefabs;
    [SerializeField]
    private Transform map;

    private GridManager gm = null;
    private WayPoint waypoint;

    private int sphereCnt = 0;

    private void Awake()
    {
        gm = Camera.main.GetComponent<GridManager>() as GridManager;
    }

    void Start()
    {
        if (isCreatMode) saveMapData();
        else loadMapData();
    }

    private void saveMapData()
    {
        BlockData[] blockDatas = new BlockData[map.childCount];

        for (int i = 0; i < map.childCount; i++)
        {
            Block block = map.GetChild(i).GetComponent<Block>();

            blockDatas[i].blockType = block.BlockType;
            blockDatas[i].position = block.transform.position;
        }

        MapData mapData = new MapData();
        mapData.blockDatas = blockDatas;

        string toJson = JsonUtility.ToJson(mapData);
        File.WriteAllText(Application.dataPath + "/Data/MapData.json", toJson);

        Debug.Log("saved map data");
    }


    private void loadMapData()
    {
        string jsonData = File.ReadAllText(Application.dataPath + "/Data/MapData.json");

        MapData mapData = new MapData();
        mapData = JsonUtility.FromJson<MapData>(jsonData);

        creatMapFromData(mapData);
    }

    private void creatMapFromData(MapData mapData)
    {
        foreach (BlockData blockData in mapData.blockDatas)
        {
            GameObject prefab = null;
            foreach (BlockPrefab blockPrefab in blockPrefabs)
            {
                if (blockData.blockType == blockPrefab.blockType)
                {
                    if (blockData.blockType == BLOCK_TYPE.SPHERE) sphereCnt++;

                    prefab = blockPrefab.Prefab;
                    break;
                }
            }
            if (prefab == null) continue;

            GameObject block = Instantiate(prefab, blockData.position, Quaternion.identity, mapRoot);

            if (block.GetComponent<WayPoint>() != null)
            {
                if (waypoint == null) waypoint = block.GetComponent<WayPoint>();
                else
                {
                    block.GetComponent<WayPoint>().Destination = waypoint;
                    waypoint.GetComponent<WayPoint>().Destination = block.GetComponent<WayPoint>();
                }
            }
        }

        GameObject.Find("GameManager").GetComponent<GameManager>().SphereCnt = sphereCnt;
        gm.BuildWorld(mapSize, mapSize, mapData.blockDatas);
    }
}
