using UnityEngine;

public class CheeseSpawner : MonoBehaviour
{
    public GameObject cheesePrefab;
    public Transform[] spawnPoints;
    public int cheeseToSpawn = 4;

    void Start()
    {
        SpawnCheese();
    }

    public void SpawnCheese()
    {
        int spawned = 0;
        int safety = 100;

        // safety counter is here so i do not get stuck forever if there are not enough free points.
        while (spawned < cheeseToSpawn && safety > 0)
        {
            int rand = Random.Range(0, spawnPoints.Length);

            // if the spawn point has no child, i treat it as empty.
            // not the fanciest thing ever but honestly it works fine for this project.
            if (spawnPoints[rand].childCount == 0)
            {
                Instantiate(
                    cheesePrefab,
                    spawnPoints[rand].position,
                    Quaternion.identity,
                    spawnPoints[rand]
                );

                spawned++;
            }

            safety--;
        }
    }
}