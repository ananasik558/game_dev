using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshSurface))]
public class LevelGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 20;
    [SerializeField] private int mapHeight = 20;
    [SerializeField] private int roomCount = 6;

    [Header("Sizes")]
    [SerializeField] private float tileSize = 10f;
    [SerializeField] private float wallHeight = 4f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject ceilingPrefab;
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Actors")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject sellPlatformPrefab;
    [SerializeField] private GameObject dronePrefab;

    private int[,] map;
    private NavMeshSurface navMeshSurface; 

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface == null)
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            Debug.LogWarning("NavMeshSurface добавлен автоматически!");
        }

        GenerateLevel();
    }

    void GenerateLevel()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        map = new int[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                map[x, y] = 0;

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        for (int i = 0; i < roomCount; i++)
        {
            CreateRoom(roomCenters);
        }

        for (int i = 0; i < roomCenters.Count - 1; i++)
        {
            ConnectRooms(roomCenters[i], roomCenters[i + 1]);
        }

        DrawFloorsAndCeilings();
        PlaceWalls();
        SpawnItems(roomCenters);

        BuildNavMesh();

        SpawnActors(roomCenters);
    }

    void CreateRoom(List<Vector2Int> centers)
    {
        int w = Random.Range(3, 6);
        int h = Random.Range(3, 6);
        int x = Random.Range(1, mapWidth - w - 1);
        int y = Random.Range(1, mapHeight - h - 1);

        for (int i = x; i < x + w; i++)
            for (int j = y; j < y + h; j++)
                map[i, j] = 1;

        centers.Add(new Vector2Int(x + w / 2, y + h / 2));
    }

    void ConnectRooms(Vector2Int a, Vector2Int b)
    {
        int x = a.x, y = a.y;
        while (x != b.x) { map[x, y] = 1; x += (b.x > x) ? 1 : -1; }
        while (y != b.y) { map[x, y] = 1; y += (b.y > y) ? 1 : -1; }
    }

    void DrawFloorsAndCeilings()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1)
                {
                    Vector3 pos = GridToWorld(new Vector2Int(x, y));
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                    Instantiate(ceilingPrefab, pos + Vector3.up * wallHeight, Quaternion.identity, transform);
                }
            }
        }
    }

    void PlaceWalls()
    {
        float halfSize = tileSize / 2f;
        float wallY = wallHeight / 2f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] != 1) continue; 

                Vector3 center = GridToWorld(new Vector2Int(x, y));

                CheckAndPlaceWall(x - 1, y, center + Vector3.left * halfSize, wallY, Quaternion.Euler(0, 90, 0));   
                CheckAndPlaceWall(x + 1, y, center + Vector3.right * halfSize, wallY, Quaternion.Euler(0, 90, 0)); 
                CheckAndPlaceWall(x, y - 1, center + Vector3.back * halfSize, wallY, Quaternion.identity);         
                CheckAndPlaceWall(x, y + 1, center + Vector3.forward * halfSize, wallY, Quaternion.identity);     
            }
        }
    }

    void CheckAndPlaceWall(int checkX, int checkY, Vector3 pos, float yHeight, Quaternion rot)
    {
        bool isBorder = checkX < 0 || checkX >= mapWidth || checkY < 0 || checkY >= mapHeight;
        bool isEmpty = !isBorder && map[checkX, checkY] == 0;

        if (isBorder || isEmpty)
        {
            Instantiate(wallPrefab, pos + Vector3.up * yHeight, rot, transform);
        }
    }

    void BuildNavMesh()
    {
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh построен!");
        }
        else
        {
            Debug.LogError("Не найден NavMeshSurface!");
        }
    }

    void SpawnActors(List<Vector2Int> centers)
    {
        if (centers.Count == 0) return;

        Vector2Int spawnGrid = centers[0];
        Vector3 spawnPos = GridToWorld(spawnGrid);

        if (sellPlatformPrefab != null)
        {
            Instantiate(sellPlatformPrefab, spawnPos + Vector3.up * 0.4f, Quaternion.identity);
        }

        if (playerTransform != null)
        {
            playerTransform.position = spawnPos + Vector3.up * 1.5f;
            playerTransform.rotation = Quaternion.identity;
        }

        if (dronePrefab != null)
        {
            Vector3 dronePos = spawnPos + Vector3.forward * 3f + Vector3.up * 2f;
            GameObject droneObj = Instantiate(dronePrefab, dronePos, Quaternion.identity);

            var droneScript = droneObj.GetComponent<DroneController>();
            if (droneScript != null && playerTransform != null)
            {
                droneScript.SetTarget(playerTransform);
            }
        }
    }

    void SpawnItems(List<Vector2Int> centers)
    {
        foreach (var center in centers)
        {
            int count = Random.Range(1, 3);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = GridToWorld(center) + new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
                int idx = Random.Range(0, itemPrefabs.Length);
                if (itemPrefabs[idx] != null)
                {
                    Instantiate(itemPrefabs[idx], pos, Quaternion.identity);
                }
            }
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.x - mapWidth / 2f) * tileSize,
            0,
            (gridPos.y - mapHeight / 2f) * tileSize
        );
    }
}