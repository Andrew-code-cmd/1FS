using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldStreamer : MonoBehaviour
{
    public int loadRadius = 1; // количество чанков вокруг игрока
    public Transform player;

    private Vector2Int currentChunk;
    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();

    void Start()
    {
        UpdateChunks();
    }

    void Update()
    {
        Vector2Int newChunk = GetPlayerChunk();
        if (newChunk != currentChunk)
        {
            currentChunk = newChunk;
            UpdateChunks();
        }
    }

    Vector2Int GetPlayerChunk()
    {
        int chunkX = Mathf.FloorToInt(player.position.x / 1000f); // размер чанка 1 км
        int chunkY = Mathf.FloorToInt(player.position.z / 1000f);
        return new Vector2Int(chunkX, chunkY);
    }

    void UpdateChunks()
    {
        HashSet<Vector2Int> newLoaded = new HashSet<Vector2Int>();

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int chunkCoord = currentChunk + new Vector2Int(x, y);
                newLoaded.Add(chunkCoord);

                if (!loadedChunks.Contains(chunkCoord))
                    StartCoroutine(LoadChunk(chunkCoord));
            }
        }

        // выгружаем ненужные чанки
        foreach (var chunk in loadedChunks)
        {
            if (!newLoaded.Contains(chunk))
                StartCoroutine(UnloadChunk(chunk));
        }

        loadedChunks = newLoaded;
    }

    IEnumerator LoadChunk(Vector2Int coord)
    {
        string sceneName = $"Terrain_{coord.x}_{coord.y}";
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return asyncLoad;
    }

    IEnumerator UnloadChunk(Vector2Int coord)
    {
        string sceneName = $"Terrain_{coord.x}_{coord.y}";
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        yield return asyncUnload;
    }
}
