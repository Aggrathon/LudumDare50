using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SoilTile : TileBase
{

    public enum Type
    {
        Empty,
        Wild,
        Growth,
        Farm,
        Fire,
        Smolder,
        Pesticide,
        Logging,
    }

    public Sprite sprite;// { get; set; }
    public Color color = Color.white;// { get; set; }
    public Type type;
    [Range(-1f, 1f)] public float fertilityDelta;
    public float tickTime;
    public float income;
    [Range(0f, 1f)] public float spreadChance;
    public bool burnable;
    public SoilTile next;
    public SoilTile start;



    static readonly Vector3Int[] neighbours = {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
    };

    public void OnPlace(World world, ref World.SoilData data)
    {
        data.fertility = Mathf.Clamp(data.fertility + fertilityDelta, 0f, 1f);
        if (tickTime > 0)
            data.time = Time.time + tickTime;
        else
            data.time = float.MaxValue;
    }

    public void OnTick(Vector3Int pos, World world, ref World.SoilData data)
    {
        switch (type)
        {
            case Type.Farm:
            case Type.Empty:
                foreach (var n in neighbours)
                {
                    var tile = world.GetTile(pos + n);
                    if (tile.spreadChance > 0 && Random.value < tile.spreadChance * (1.0f + data.fertility) * 0.5f)
                    {
                        world.SetTile(pos, tile.start);
                        break;
                    }
                }
                data.time += tickTime;
                break;
            case Type.Fire:
                foreach (var n in neighbours)
                {
                    var tile = world.GetTile(pos + n);
                    if (tile.burnable && Random.value < spreadChance)
                    {
                        world.SetTile(pos + n, start);
                    }
                }
                world.SetTile(pos, next);
                break;
            case Type.Pesticide:
                world.SetTile(pos, next);
                break;
            case Type.Logging:
                world.SetTile(pos, next);
                break;
        }
    }
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = this.sprite;
        tileData.color = this.color;
        tileData.transform.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        tileData.gameObject = null;
        tileData.flags = TileFlags.LockColor;
        tileData.colliderType = Tile.ColliderType.None;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SoilTile))]
public class SoilTileEditor : UnityEditor.Editor
{
    [MenuItem("Assets/Create/SoilTile")]
    public static void CreateRoadTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Soil Tile", "New Soil Tile", "asset", "Save Soil Tile", "Assets/Tiles");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SoilTile>(), path);
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        SoilTile example = (SoilTile)target;
        if (example == null || example.sprite == null)
            return null;

        var preview = AssetPreview.GetAssetPreview(example.sprite);
        Color[] pixels = preview.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i] * example.color;
        }
        preview = new Texture2D(width, height);
        preview.SetPixels(pixels);
        preview.Apply();
        return preview;
    }
}
#endif