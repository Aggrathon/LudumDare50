using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class UserInterface : MonoBehaviour
{
    public List<SoilTile> tiles;
    public Transform buttonHolder;
    public Transform infoPanel;
    public World world;
    public Camera mainCamera;
    public EventSystem eventSystem;
    public Overlay orderOverlay;
    public TileBase orderMarker;
    public int moneyMult = 1000;

    public int MinCost { get; protected set; }

    Vector3Int lastCell;
    SoilTile orderTile;
    TextMeshProUGUI infoTitle;
    TextMeshProUGUI infoDesc;



    void Start()
    {
        var prototype = buttonHolder.GetChild(0);
        for (int i = 1; i < tiles.Count; i++)
        {
            Instantiate(prototype, Vector3.zero, Quaternion.identity, buttonHolder);
        }
        MinCost = int.MaxValue;
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var child = buttonHolder.GetChild(i);
            var image = child.GetChild(0).GetChild(0).GetComponent<Image>();
            image.sprite = tile.sprite;
            image.color = tile.color;
            var button = child.GetComponent<Button>();
            button.onClick.AddListener(() => OnSelectTile(tile));
            var text = child.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            text.text = tile.name;
            var tooltip = child.GetChild(2);
            string desc = tile.description;
            if (tile.cost > 0)
                desc += $"\n   Cost: {tile.cost * moneyMult,5}";
            if (tile.income > 0)
                desc += $"\nIncome: {tile.income * moneyMult,5}";
            if (tile.tickTime > 0)
                desc += $"\nTime: {Mathf.RoundToInt(tile.tickTime)}";
            tooltip.GetComponentInChildren<TextMeshProUGUI>().text = desc;
            tooltip.gameObject.SetActive(false);
            if (tile.cost < tile.income)
                MinCost = Mathf.Min(MinCost, tile.cost);
        }
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        lastCell = new Vector3Int(100, 100, 100);
        infoTitle = infoPanel.GetChild(0).GetComponent<TextMeshProUGUI>();
        infoDesc = infoPanel.GetChild(1).GetComponent<TextMeshProUGUI>();
        world.onTileChange.AddListener(OnTileChanged);
    }

    void Update()
    {
        var screenPos = Input.mousePosition;
        var worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        var cellPos = world.tilemap.WorldToCell(worldPos);
        if (cellPos != lastCell)
        {
            var tile = world.GetTile(cellPos);
            if (tile == null)
            {
                return;
            }
            infoTitle.text = tile.name;
            if (world.TryGetSoil(cellPos, out World.SoilData soil))
            {
                infoDesc.text = $"Fertility: {Mathf.RoundToInt(soil.fertility * 100),3}%{(tile.flammable ? "\nFlammable" : "")}{(tile.spreadChance > 0 ? $"\nSpreads: {Mathf.RoundToInt(tile.CurrentSpreadChance(world) * 100)} %" : "")}{(tile.income > 0 ? $"\nIncome: {tile.income * moneyMult,5}" : "")}";
            }
            else
            {
                infoDesc.text = "Inaccessible";
            }
            lastCell = cellPos;
        }
        if (orderTile && !eventSystem.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                var tile = world.GetTile(cellPos);
                if (orderTile.CanReplace(tile))
                {
                    if (world.Money >= orderTile.cost && world.Money + world.Income - orderTile.cost + orderTile.income > MinCost)
                    {
                        world.SetTile(cellPos, orderTile);
                        world.Money -= orderTile.cost;
                    }
                    else
                    {
                        OnSelectTile(orderTile);
                        AudioManager.PlaySound(AudioManager.Sounds.Forbidden, worldPos);
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                OnSelectTile(orderTile);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                OnSelectTile(tiles[i]);
            }
        }
    }

    void OnSelectTile(SoilTile tile)
    {
        if (orderTile == tile)
        {
            orderTile = null;
            orderOverlay.Clear();
            AudioManager.PlaySound(AudioManager.Sounds.Knock);
        }
        else
        {
            if (world.Money >= tile.cost && world.Money + world.Income - tile.cost + tile.income > MinCost)
            {
                orderTile = tile;
                orderOverlay.Filter((tile, data) => orderTile.CanReplace(tile) ? orderMarker : null);
                lastCell.z += 100;
                AudioManager.PlaySound(AudioManager.Sounds.Knock);
            }
            else
            {
                AudioManager.PlaySound(AudioManager.Sounds.Forbidden);
            }
        }
        EmphasizeButton();
    }

    void EmphasizeButton()
    {
        if (orderTile == null)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                buttonHolder.GetChild(i).GetChild(0).localScale = Vector3.one;
            }
        }
        else
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == orderTile)
                    buttonHolder.GetChild(i).GetChild(0).localScale = Vector3.one * 1.2f;
                else
                    buttonHolder.GetChild(i).GetChild(0).localScale = Vector3.one * 0.9f;
            }

        }
    }

    void OnTileChanged(Vector3Int pos, SoilTile tile)
    {
        if (pos == lastCell)
        {
            lastCell.z += 100;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void Mute(bool toggle)
    {
        AudioManager.PlaySound(AudioManager.Sounds.Knock);
        //TODO: Mute music
    }

    public void Pause(bool toggle)
    {
        AudioManager.PlaySound(AudioManager.Sounds.Knock);
        Time.timeScale = toggle ? 0.0f : 1.0f;
    }
}
