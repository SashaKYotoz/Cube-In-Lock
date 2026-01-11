using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject chunk;
    [SerializeField] private int chunkCount = 20;
    [SerializeField] private float spawnRadius = 50f;

    private GameObject chunkGroup;
    private Vector3 originPoint = new Vector3(0, -5, 0);

    void Awake()
    {
        chunkGroup = new GameObject("chunkGroup");

        for (int i = 0; i < chunkCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0;
            Vector3 spawnPos = originPoint + randomOffset;
            float randomYaw = Random.Range(0, 4) * 90f;
            Quaternion spawnRot = Quaternion.Euler(0, randomYaw, 0);
            GameObject tmpChunk = Instantiate(chunk, spawnPos, spawnRot);
            tmpChunk.transform.SetParent(chunkGroup.transform);
        }
    }
}