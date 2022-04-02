using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UserInterface : MonoBehaviour
{
    public List<SoilTile> tiles;
    public Transform buttonHolder;
    public Transform infoPanel;
    public World world;
    public Camera mainCamera;
    public int incomeMult = 1000;

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
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var child = buttonHolder.GetChild(i);
            var image = child.GetComponent<Image>();
            image.sprite = tile.sprite;
            image.color = tile.color;
            var button = child.GetComponent<Button>();
            button.onClick.AddListener(() => OnSelectTile(tile));
            var text = child.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
            text.text = tile.name;
            var tooltip = child.GetChild(1);
            text = tooltip.GetComponentInChildren<TextMeshProUGUI>();
            text.text = tile.description;
            tooltip.gameObject.SetActive(false);
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
            infoTitle.text = tile.name;
            if (world.TryGetSoil(cellPos, out World.SoilData soil))
            {
                infoDesc.text = $"Fertility: {Mathf.RoundToInt(soil.fertility * 100),3}%{(tile.flammable ? "\nFlammable" : "")}{(tile.spreadChance > 0 ? "\nSpreads" : "")}{(tile.income > 0 ? $"\nIncome: {tile.income * incomeMult,5}" : "")}";
            }
            else
            {
                infoDesc.text = "Inaccessible";
            }
            lastCell = cellPos;
        }
        // TODO handle orders
    }

    void OnSelectTile(SoilTile tile)
    {
        if (orderTile == tile)
        {
            orderTile = null;
            // TODO Order Overlay
        }
        else
        {
            orderTile = tile;
        }
        Debug.Log(tile);
    }

    void OnTileChanged(Vector3Int pos, SoilTile tile)
    {
        if (pos == lastCell)
        {
            lastCell.z += 100;
        }
        if (orderTile)
        {
            // TODO: update overlay
        }
        // TODO check money
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void Mute()
    {
        //TODO: Mute audio
    }
}
