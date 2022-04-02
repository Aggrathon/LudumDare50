using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public Tilemap tilemap;
    public int width = 21;
    public int height = 21;

    public SoilTile emptyTile;
    public SoilTile wildTile;

    float time;
    public float MapTime
    {
        get
        {
            return Time.time - time;
        }
    }

    public struct SoilData
    {
        public SoilData(float minTime, float rndTime)
        {
            fertility = 0.5f;
            time = minTime + Random.value * rndTime;
        }
        public float fertility;
        public float time;
    }

    SoilData[,] map;

    void Start()
    {
        if (width % 2 != 1 || height % 2 != 1)
        {
            Debug.LogWarning("The width and/or height is even!");
        }
        tilemap.SetTile(new Vector3Int(-width, -height), wildTile);
        tilemap.SetTile(new Vector3Int(width, height), wildTile);
        tilemap.BoxFill(Vector3Int.zero, wildTile, -width, -height, width, height);
        tilemap.BoxFill(Vector3Int.zero, emptyTile, -width / 2, -height / 2, width / 2, height / 2);
        map = new SoilData[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = new SoilData(0.1f, emptyTile.tickTime);
            }
        }
        SetTile(0, 0, wildTile);
        SetTile(0, height - 1, wildTile);
        SetTile(width - 1, 0, wildTile);
        SetTile(width - 1, height - 1, wildTile);
        time = Time.time;
    }

    public (int, int) GetIndex(Vector3Int pos)
    {
        return (pos.x + width / 2, pos.y + height / 2);
    }
    public Vector3Int GetPos(int x, int y)
    {
        return new Vector3Int(x - width / 2, y - height / 2);
    }

    public void SetTile(int x, int y, Vector3Int pos, SoilTile tile)
    {
        tilemap.SetTile(pos, tile);
        tile.OnPlace(pos, this, ref map[x, y]);
    }
    public void SetTile(int x, int y, SoilTile tile)
    {
        SetTile(x, y, GetPos(x, y), tile);
    }
    public void SetTile(Vector3Int pos, SoilTile tile)
    {
        (int i, int j) = GetIndex(pos);
        SetTile(i, j, pos, tile);
    }

    public SoilTile GetTile(Vector3Int pos)
    {
        return tilemap.GetTile<SoilTile>(pos);
    }

    public SoilData GetSoil(int x, int y)
    {
        return map[x, y];
    }

    void Update()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j].time < Time.time)
                {
                    var pos = GetPos(i, j);
                    tilemap.GetTile<SoilTile>(pos).OnTick(pos, this, ref map[i, j]);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 1));
    }
}
