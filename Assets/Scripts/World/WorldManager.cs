using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GroundTile[] tilePrefabs;
    public GameObject[] obstaclePrefabs;
    public GameObject eggPrefab, goldEggPrefab;

    [Header("Settings")]
    public int tileCount = 6;
    public float tileLength = 24f;
    public float laneDistance = 2f;
    public int emptyStartTiles = 2;
    [Range(0f, 1f)] public float obstacleChance = 0.7f;
    [Range(0f, 1f)] public float doubleObstacleChance = 0.25f;
    [Range(0f, 1f)] public float goldChance = 0.12f;

    readonly Queue<GroundTile> tiles = new Queue<GroundTile>();
    float nextZ;
    int created;

    void Start()
    {
        // Fallback: try to find the player by tag if it wasn't assigned in the Inspector
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null)
        {
            Debug.LogError("WorldManager on '" + name + "' has no player assigned!", this);
            enabled = false;   // stops Update from spamming errors
            return;
        }

        if (tilePrefabs == null || tilePrefabs.Length == 0)
        {
            Debug.LogError("WorldManager on '" + name + "' has no tile prefabs assigned!", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < tileCount; i++)
        {
            GroundTile t = Instantiate(tilePrefabs[i % tilePrefabs.Length], transform);
            Place(t);
        }
    }

    void Update()
    {
        if (player == null || tiles.Count == 0) return;

        GroundTile first = tiles.Peek();
        // recycle a tile once the player is well past its end
        if (player.position.z > first.transform.position.z + tileLength + 10f)
        {
            tiles.Dequeue();
            Place(first);
        }
    }

    void Place(GroundTile t)
    {
        t.transform.position = new Vector3(0f, 0f, nextZ);
        t.ClearItems();
        if (created >= emptyStartTiles) Populate(t);
        created++;
        nextZ += tileLength;
        tiles.Enqueue(t);
    }

    void Populate(GroundTile t)
    {
        float z = t.transform.position.z;
        int lane = Random.Range(-1, 2);
        int eggLane = lane;

        if (obstaclePrefabs != null && obstaclePrefabs.Length > 0 && Random.value < obstacleChance)
        {
            float oz = z + Random.Range(5f, tileLength - 5f);
            Spawn(RandomObstacle(), lane, oz, t.items);

            if (Random.value < doubleObstacleChance)
            {
                int lane2 = OtherLane(lane);
                Spawn(RandomObstacle(), lane2, oz, t.items);
                eggLane = -(lane + lane2);   // the free lane
            }
            else
            {
                eggLane = OtherLane(lane);
            }
        }

        SpawnEggs(t, eggLane, z);
    }

    void SpawnEggs(GroundTile t, int lane, float z)
    {
        GameObject prefab = Random.value < goldChance ? goldEggPrefab : eggPrefab;
        if (prefab == null) return;

        float startZ = z + Random.Range(3f, 8f);
        for (int i = 0; i < 5; i++)
            Spawn(prefab, lane, startZ + i * 2f, t.items);
    }

    GameObject RandomObstacle()
    {
        return obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
    }

    int OtherLane(int a)
    {
        int l = Random.Range(-1, 2);
        while (l == a) l = Random.Range(-1, 2);
        return l;
    }

    void Spawn(GameObject prefab, int lane, float z, Transform parent)
    {
        Vector3 pos = new Vector3(lane * laneDistance, prefab.transform.position.y, z);
        Instantiate(prefab, pos, prefab.transform.rotation, parent);
    }
}