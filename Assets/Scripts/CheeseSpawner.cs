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

        while (spawned < cheeseToSpawn && safety > 0)
        {
            int rand = Random.Range(0, spawnPoints.Length);

            if (spawnPoints[rand].childCount == 0)
            {
                Instantiate(
                    cheesePrefab,
                    spawnPoints[rand].position,
                    Quaternion.identity,
                    spawnPoints[rand]);

                spawned++;
            }

            safety--;
        }
    }
}