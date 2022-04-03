using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public Tilemap tilemap;
    public int width = 21;
    public int height = 21;

    public SoilTile emptyTile;
    public SoilTile wildTile;

    [SerializeField] int startingMoney = 10;
    [SerializeField] int startingDebt = 50000;
    public float interestTime = 5f;
    public float interest = 0.02f;
    float nextInterest;
    public int Money { get { return startingMoney; } set { startingMoney = value; onMoneychange.Invoke(startingMoney, startingDebt); } }
    public int Debt { get { return startingDebt; } set { startingDebt = value; onMoneychange.Invoke(startingMoney, startingDebt); } }

    public int Income { get; protected set; }

    public UnityEvent<Vector3Int, SoilTile> onTileChange;
    public UnityEvent<int, int> onMoneychange;

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
        public SoilData(float rndTime)
        {
            fertility = 0.5f;
            time = Time.time + Random.value * rndTime;
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
        tilemap.ClearAllTiles();
        tilemap.SetTile(new Vector3Int(-width, -height), wildTile);
        tilemap.SetTile(new Vector3Int(width, height), wildTile);
        tilemap.BoxFill(Vector3Int.zero, wildTile, -width, -height, width, height);
        tilemap.BoxFill(Vector3Int.zero, emptyTile, -width / 2, -height / 2, width / 2, height / 2);
        map = new SoilData[width, height];
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                map[i, j] = new SoilData(emptyTile.tickTime);
            }
        }
        for (int i = 0; i < width; i++)
        {
            SetTile(i, 0, wildTile);
            SetTile(i, height - 1, wildTile);
            map[i, 0] = new SoilData(wildTile.tickTime);
            map[i, height - 1] = new SoilData(wildTile.tickTime);
        }
        for (int i = 1; i < height - 1; i++)
        {
            SetTile(0, i, wildTile);
            SetTile(width - 1, i, wildTile);
            map[0, i] = new SoilData(wildTile.tickTime);
            map[width - 1, i] = new SoilData(wildTile.tickTime);
        }
        SetTile(1, 1, wildTile);
        SetTile(1, height - 2, wildTile);
        SetTile(width - 2, 1, wildTile);
        SetTile(width - 2, height - 2, wildTile);
        time = Time.time;
        nextInterest = Time.time + interestTime * 2;
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
        if (tile == null)
        {
            Debug.LogError("Tile should not be null");
        }
        Income = Mathf.Max(1, Income + tile.income - tilemap.GetTile<SoilTile>(pos).income);
        tilemap.SetTile(pos, tile);
        tile.OnPlace(pos, this, ref map[x, y]);
        onTileChange.Invoke(pos, tile);
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
    public bool TryGetSoil(int x, int y, out SoilData soil)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            soil = new SoilData();
            return false;
        }
        soil = map[x, y];
        return true;
    }
    public bool TryGetSoil(Vector3Int pos, out SoilData soil)
    {
        (int i, int j) = GetIndex(pos);
        return TryGetSoil(i, j, out soil);
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
        if (nextInterest < Time.time)
        {
            nextInterest += interestTime;
            Debt += Mathf.RoundToInt(Debt * interest);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width - 2, height - 2, 1));
    }
}
