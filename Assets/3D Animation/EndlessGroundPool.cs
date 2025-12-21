using System.Collections.Generic;
using UnityEngine;

public class EndlessGroundPool : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform tilePrefab;     // Ground prefab transform

    [Header("Pool Settings")]
    public int poolSize = 6;
    public float tileLength = 10f;   // Plane length in Z (usually 10 if scale.z = 1)
    public float spawnZOffset = 0f;  // start offset in front of player
    public float recycleBehindDistance = 15f; // how far behind player before recycling

    private readonly Queue<Transform> tiles = new Queue<Transform>();
    private float nextSpawnZ;

    void Start()
    {
        // If using Plane, length is 10 * scale.z
        if (tileLength <= 0.01f && tilePrefab != null)
            tileLength = 10f * tilePrefab.localScale.z;

        nextSpawnZ = player.position.z + spawnZOffset;

        // Spawn initial tiles in a row
        for (int i = 0; i < poolSize; i++)
        {
            Transform t = Instantiate(tilePrefab, transform);
            t.position = new Vector3(0f, tilePrefab.position.y, nextSpawnZ);
            nextSpawnZ += tileLength;
            tiles.Enqueue(t);
        }
    }

    void Update()
    {
        if (tiles.Count == 0) return;

        // Check the oldest tile (front of queue). If it's far behind player, move it to the end.
        Transform oldest = tiles.Peek();

        float tileEndZ = oldest.position.z + (tileLength * 0.5f); // approx end of tile
        if (player.position.z - tileEndZ > recycleBehindDistance)
        {
            Transform t = tiles.Dequeue();

            // move tile forward to nextSpawnZ
            t.position = new Vector3(t.position.x, t.position.y, nextSpawnZ);
            nextSpawnZ += tileLength;

            tiles.Enqueue(t);
        }
    }
}