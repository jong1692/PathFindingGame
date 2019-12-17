﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockType;

public class GridManager : MonoBehaviour
{

    public enum TileType
    {
        Plane,
        Wall,
        Waypoint
    }

    TileType[] world = null;
    public TileType[] World
    {
        get { return world; }
    }

    int max_tiles;
    public int MaxTiles
    {
        get { return max_tiles; }
    }

    int n_x;
    int n_z;

    private readonly float tileOffset = 24.5f;

    static Vector3[] corners = { new Vector3(1, -0, 0), new Vector3(0, 0, 1) };

    GameObject player;

    Vector3 locateToCenter(Vector3 pos)
    {
        return pos + new Vector3(0.5f, 0, 0.5f);
    }

    public void SetAsWall(Vector3 pos)
    {
        world[pos2Cell(pos)] = TileType.Wall;
    }

    public void BuildWorld(int n_rows, int n_cols, MapManager.BlockData[] blockDatas)
    {
        max_tiles = n_rows * n_cols;

        n_x = n_rows;
        n_z = n_cols;

        // set up the player's position to the center of the grid.
        //player.transform.position = locateToCenter(new Vector3(n_x / 2, 0, n_z / 2)); // place the player to the center.
        int player_cell = pos2Cell(player.transform.position);

        // construct a game world and assign walls.
        world = new TileType[max_tiles];
        for (int i = 0; i < max_tiles; i++)
        {
            world[i] = TileType.Wall;
        }

        for (int i = 0; i < blockDatas.Length; i++)
        {
            if (blockDatas[i].blockType != BLOCK_TYPE.WATER)
            {
                int idx = pos2Cell(blockDatas[i].position);

                if ((blockDatas[i].blockType == BLOCK_TYPE.WAYPOINT))
                    world[idx] = TileType.Waypoint;
                else if (world[idx] != TileType.Waypoint)
                    world[idx] = TileType.Plane;
            }
        }

        for (int i = 0; i < max_tiles; i++) drawRect(i, world[i] == TileType.Plane ? Color.green : Color.black);
    }

    void Awake()
    {
        player = GameObject.Find("Player");
    }

    public int pos2Cell(Vector3 pos)
    {
        return ((int)pos.z) * n_x + (int)pos.x;
    }

    public Vector3 cell2Pos(int cellno)
    {
        return new Vector3(cellno % n_x, 0, cellno / n_x);
    }

    public Vector3 pos2center(Vector3 pos)
    {

        return locateToCenter(cell2Pos(pos2Cell(pos)));
    }

    void drawRect(int cellno, Color c, float duration = 10000.0f)
    {
        Vector3 correction = new Vector3(0, -0.5f, 0);
        Vector3 lb = cell2Pos(cellno) + correction;


        Debug.DrawLine(lb, lb + corners[0], c, duration);
        Debug.DrawLine(lb, lb + corners[1], c, duration);
        Vector3 rt = lb + corners[0] + corners[1] + correction;
        Debug.DrawLine(rt, rt - corners[0], c, duration);
        Debug.DrawLine(rt, rt - corners[1], c, duration);
    }

    int[] findNeighbors(int cellno, TileType[] world)
    {
        List<int> neighbors = new List<int> { -1, 1, -n_x, n_x };// -n_x - 1, -n_x + 1, n_x - 1, n_x + 1 };
        int cnt = neighbors.Count;

        if (cellno % n_x == 0) neighbors.RemoveAll((no) => { return no == -1 || no == -1 - n_x || no == -1 + n_x; });
        if (cellno % n_x == n_x - 1) neighbors.RemoveAll((no) => { return no == 1 || no == 1 - n_x || no == 1 + n_x; });
        if (cellno / n_x == 0) neighbors.RemoveAll((no) => { return no == -n_x || no == -n_x - 1 || no == -n_x + 1; });
        if (cellno / n_x == n_z - 1) neighbors.RemoveAll((no) => { return no == n_x || no == n_x - 1 || no == n_x + 1; });

        for (int i = 0; i < neighbors.Count;)
        {
            neighbors[i] += cellno;
            if (neighbors[i] < 0 || neighbors[i] >= n_x * n_z || world[neighbors[i]] == TileType.Wall) neighbors.RemoveAt(i);
            else i++; /* increase unless removing */
        }

        Vector3 X = cell2Pos(cellno);
        if (world[pos2Cell(X)] == TileType.Waypoint)
        {
            Vector3 vec = X;
            vec.x = tileOffset * 2 - vec.x - 1;

            neighbors.Add(pos2Cell(vec));
        }

        //??
        for (int i = 0; i < neighbors.Count;)
        {
            Vector3 Xp = cell2Pos(neighbors[i]);
            //if ((X.x - Xp.x) * (X.z - Xp.z) != 0)
            if (world[pos2Cell(Xp)] == TileType.Wall)
            {
                neighbors.RemoveAt(i);
                continue;
            }
            i++;
        }
        return neighbors.ToArray();
    }

    int[] buildPath(int[] parents, int from, int to)
    {
        if (parents == null) return null;

        List<int> path = new List<int>();
        int current = to;
        while (current != from)
        {
            path.Add(current);
            current = parents[current];
        }
        path.Add(from); // to -> ... -> ... -> from

        path.Reverse(); // from -> ... -> ... -> to
        return path.ToArray();
    }

    void drawPath(int[] path)
    {
        if (path == null) return;
        Vector3 correction = new Vector3(0, -0.5f, 0);

        for (int i = 0; i < path.Length - 1; i++)
        {
            Debug.DrawLine(locateToCenter(cell2Pos(path[i])) + correction, locateToCenter(cell2Pos(path[i + 1])) + correction, Color.blue, 5.0f);
        }
    }

    class Node
    {
        public int no;
        public float f; // final cost = global cost + heuristic cost
        public float g; // global cost
    }

    public enum MethodType { BFS, AStar };

    float computeDistance(int cell1, int cell2)
    {
        return Vector3.Distance(cell2Pos(cell1), cell2Pos(cell2));
    }

    public MethodType method = MethodType.BFS;

    int[] findShortestPath(int from, int to, TileType[] world)
    {
        //print("BFS");
        int max_tiles = n_x * n_z;

        if (from < 0 || from >= max_tiles || to < 0 || to >= max_tiles) return null;

        // initialize the parents of all tiles to negative value, implying no tile number associated.
        int[] parents = new int[max_tiles];
        for (int i = 0; i < parents.Length; i++) parents[i] = -1;

        List<int> N = new List<int>() { from };
        int nIterations = 0;
        while (N.Count > 0)
        {
            int current = N[0]; N.RemoveAt(0); // dequeue
            nIterations++;

            int[] neighbors = findNeighbors(current, world);
            foreach (var neighbor in neighbors)
            {
                if (neighbor == to)
                {
                    // found the destination
                    parents[neighbor] = current;
                    return buildPath(parents, from, to); // read parents array and construct the shoretest path by traversal
                }

                if (parents[neighbor] == -1) // neighbor's parent is not set yet.
                {
                    parents[neighbor] = current; // make current tile as neighbor's parent
                    N.Add(neighbor); // enqueue
                }
            }
        }
        return null; // I cannot find any path from source to destination        
    }


    int[] findAstarPath(int from, int to, TileType[] world)
    {
        print("A*");
        int max_tiles = n_x * n_z;

        if (from < 0 || from >= max_tiles || to < 0 || to >= max_tiles) return null;

        // initialize the parents of all tiles to negative value, implying no tile number associated.
        int[] parents = new int[max_tiles];

        for (int i = 0; i < parents.Length; i++) parents[i] = -1;

        List<Node> closed = new List<Node>();
        List<Node> open = new List<Node>() { new Node { no = from, f = 0f, g = 0f } };
        int nIterations = 0;
        while (open.Count > 0)
        {
            var lowScore = open.Min(node => node.f);
            var current = open.First(n => n.f == lowScore);
            open.Remove(current);
            closed.Add(current);

            int[] neighbors = findNeighbors(current.no, world);
            nIterations++;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == to)
                {
                    // found the destination
                    parents[neighbor] = current.no;
                    //print("nIterations: " + nIterations);
                    return buildPath(parents, from, to); // read parents array and construct the shoretest path by traversal
                }

                if (closed.FirstOrDefault(n => n.no == neighbor) != null) continue;

                // computeDistance(current.no, neighbor) : global cost 계산시 인접 노드간 거리를 고려. 사선인 경우, sqrt(2)가 반환
                var g = current.g + computeDistance(current.no, neighbor);
                var h = computeDistance(neighbor, to); // computeDistance(neighbor, to) : heuristic cost 계산
                var nodeInOpen = open.FirstOrDefault(n => n.no == neighbor);
                if (nodeInOpen == null)
                {
                    parents[neighbor] = current.no;

                    open.Insert(0, new Node { no = neighbor, f = g + h, g = g });
                    continue;
                }
                if (g + h < nodeInOpen.f)
                {
                    nodeInOpen.f = g + h;
                    nodeInOpen.g = g;
                    parents[neighbor] = current.no;
                }
            }
        }
        return null; // I cannot find any path from source to destination        
    }

    public IEnumerator Move(GameObject obj, Vector3 destination, float movementSpeed)
    {
        int start = pos2Cell(obj.transform.position);
        int end = pos2Cell(destination);
        int[] path = null;


        if (method == MethodType.BFS)
            path = findShortestPath(start, end, world);
        else if (method == MethodType.AStar)
            path = findAstarPath(start, end, world);
        if (path == null) yield break;

        // path should start from "source" to "destination".

        drawPath(path);
        List<int> remaining = new List<int>(path); // convert int array to List
        remaining.RemoveAt(0); // we don't need the first one, since the first element should be same as that of source.
        while (remaining.Count > 0)
        {
            int to = remaining[0]; remaining.RemoveAt(0);

            Vector3 toLoc = locateToCenter(cell2Pos(to));
            toLoc.y = 0;

            while (obj.transform.position != toLoc)
            {
                if (world[to] == TileType.Waypoint)
                {
                    //&& obj.transform.position == world[to]
                    Vector3 vec = toLoc;
                    vec.x = tileOffset * 2 - vec.x;

                    float distance = (obj.transform.position - vec).magnitude;

                    if (Mathf.Approximately(distance, 0))
                    {
                        //Debug.Log(locateToCenter(cell2Pos(remaining[0])));

                        break;
                    }
                }

                obj.transform.position = Vector3.MoveTowards(obj.transform.position, toLoc, movementSpeed * Time.deltaTime);
                obj.transform.LookAt(toLoc);

                drawRect(pos2Cell(obj.transform.position), Color.red, Time.deltaTime);
                yield return null;
            }
        }
    }
}
