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
        Growth,
        Farm,
        Fire,
        Removal,
        Slow,
    }

    public Sprite sprite;
    public Color color = Color.white;
    public Type type;
    [Range(-1f, 1f)] public float fertilityDelta;
    public float tickTime;
    public int cost;
    public int income;
    [Range(0f, 1f)] public float spreadChance;
    [Range(0f, 0.01f)] public float difficultyScaling = 0f;
    public bool flammable;
    public SoilTile next;
    public SoilTile start;
    [TextArea] public string description;

    static readonly Vector3Int[] neighbours = {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
    };


    public float CurrentSpreadChance(World world)
    {
        return spreadChance + difficultyScaling * world.MapTime;
    }

    public bool CanReplace(SoilTile other)
    {
        if (other)
        {
            return type switch
            {
                Type.Empty => true,
                Type.Growth => other.type == Type.Empty || other.type == Type.Farm,
                Type.Farm => other.type == Type.Empty,
                Type.Fire => other.flammable,
                Type.Removal => other.type == Type.Growth && other.flammable,
                Type.Slow => other.type == Type.Empty,
                _ => true,
            };
        }
        else
        {
            return false;
        }
    }

    public void OnPlace(Vector3Int pos, World world, ref World.SoilData data)
    {
        data.fertility = Mathf.Clamp(data.fertility + fertilityDelta, 0f, 1f);
        if (type == Type.Empty)
        {
            if (data.fertility >= 0.7f && next != null && next != this)
            {
                world.SetTile(pos, next);
                return;
            }
            else if (data.fertility <= 0.3f && start != null && start != this)
            {
                world.SetTile(pos, start);
                return;
            }
        }
        if (tickTime > 0)
        {
            if (type == Type.Growth || type == Type.Farm)
                data.time = Time.time + tickTime * (1.5f - data.fertility);
            else
                data.time = Time.time + tickTime;
        }
        else
            data.time = float.MaxValue;
        // TODO sound
    }

    public void OnTick(Vector3Int pos, World world, ref World.SoilData data)
    {
        switch (type)
        {
            case Type.Growth:
                foreach (var n in neighbours)
                {
                    var tile = world.GetTile(pos + n);
                    if (CanReplace(tile) && Random.value < CurrentSpreadChance(world))
                    {
                        world.SetTile(pos + n, start);
                    }
                }
                data.time += tickTime * (1.5f - data.fertility);
                break;
            case Type.Fire:
                foreach (var n in neighbours)
                {
                    var tile = world.GetTile(pos + n);
                    if (CanReplace(tile) && Random.value < CurrentSpreadChance(world))
                    {
                        world.SetTile(pos + n, start);
                    }
                }
                world.SetTile(pos, next);
                break;
            case Type.Removal:
            case Type.Slow:
            case Type.Farm:
                if (income > 0)
                {
                    world.Money += income;
                    // TODO beep
                }
                world.SetTile(pos, next);
                break;
        }
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.color = color;
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