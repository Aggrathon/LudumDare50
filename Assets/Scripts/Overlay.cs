using UnityEngine;
using UnityEngine.Tilemaps;

public class Overlay : MonoBehaviour
{

    public Tilemap tilemap;
    public World world;

    System.Func<SoilTile, World.SoilData, TileBase> filter;

    private void Start()
    {
        Clear();
        world.onTileChange.AddListener(OnTileChanged);
    }

    public void Clear()
    {
        tilemap.gameObject.SetActive(false);
        tilemap.ClearAllTiles();
    }

    public void Filter(System.Func<SoilTile, World.SoilData, TileBase> filter)
    {
        this.filter = filter;
        for (int i = 0; i < world.width; i++)
        {
            for (int j = 0; j < world.height; j++)
            {
                var pos = world.GetPos(i, j);
                tilemap.SetTile(pos, filter(world.GetTile(pos), world.GetSoil(i, j)));
            }
        }
        tilemap.gameObject.SetActive(true);
    }

    void OnTileChanged(Vector3Int pos, SoilTile tile)
    {
        if (tilemap.gameObject.activeSelf)
        {
            if (world.TryGetSoil(pos, out World.SoilData data))
            {
                tilemap.SetTile(pos, filter(tile, data));
            }
        }
    }
}