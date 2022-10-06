using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {
            // If there is a grid, then spawn at its center
            var spawnPos = new Vector3(0f, 0f, 0f);

            var grid = FindObjectOfType<Grid>();
            if (grid != null)
                spawnPos = grid.transform.position;

            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
