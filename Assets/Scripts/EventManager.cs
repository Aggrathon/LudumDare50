using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventManager : MonoBehaviour
{
    public float minTime = 10f;
    public float maxTime = 20f;
    public World world;
    public TextMeshProUGUI message;
    public float messageTime = 5f;

    [Header("Meteor")]
    public SoilTile fireTile;
    [Range(0, 10)] public int meteors = 3;

    [Header("Fertility")]
    [Range(0f, 1f)] public float fertilityIncrease = 0.2f;
    [Range(0f, 1f)] public float fertilityDecrease = 0.2f;
    public SoilTile emptyTile;

    [Header("Spores")]
    public SoilTile weedTile;
    [Range(0, 10)] public int spores = 5;

    [Header("Lotto")]
    public int lottoAmount = 100;


    float time;
    float messTime;

    void Start()
    {
        time = Time.time + maxTime;
        messTime = float.MaxValue;
        message.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Time.time > time)
        {
            switch (Random.Range(0, 5))
            {
                case 0:
                    bool hit = false;
                    for (int i = 0; i < meteors; i++)
                    {
                        int x = Random.Range(0, world.width);
                        int y = Random.Range(0, world.height);
                        var pos = world.GetPos(x, y);
                        var tile = world.GetTile(pos);
                        if (fireTile.CanReplace(tile))
                        {
                            world.SetTile(x, y, pos, fireTile);
                            AudioManager.PlaySound(AudioManager.Sounds.Hit, world.tilemap.CellToWorld(pos));
                            AudioManager.PlaySound(AudioManager.Sounds.Hit, world.tilemap.CellToWorld(pos));
                            hit = true;
                        }
                        if (hit)
                            Message("Meteor strike!", "Look out for wild fires!");
                        else
                            return;
                    }
                    break;
                case 1:
                    Message("Drought!", "Soil fertility reduced everywhere!");
                    for (int i = 1; i < world.width - 1; i++)
                    {
                        for (int j = 1; j < world.height - 1; j++)
                        {
                            var data = world.GetSoil(i, j);
                            if (data.fertility >= 0.3)
                            {
                                data.fertility -= fertilityDecrease;
                                world.SetSoil(i, j, data);
                                var pos = world.GetPos(i, j);
                                if (world.GetTile(pos).type == SoilTile.Type.Empty)
                                {
                                    world.SetTile(i, j, pos, emptyTile);
                                }
                            }
                        }
                    }
                    AudioManager.PlaySound(AudioManager.Sounds.Ping);
                    break;
                case 2:
                    Message("Nice weather!", "Soil fertility increased everywhere!");
                    for (int i = 1; i < world.width - 1; i++)
                    {
                        for (int j = 1; j < world.height - 1; j++)
                        {
                            var data = world.GetSoil(i, j);
                            if (data.fertility <= 0.7)
                            {
                                data.fertility += fertilityIncrease;
                                world.SetSoil(i, j, data);
                                var pos = world.GetPos(i, j);
                                if (world.GetTile(pos).type == SoilTile.Type.Empty)
                                {
                                    world.SetTile(i, j, pos, emptyTile);
                                }
                            }
                        }
                    }
                    AudioManager.PlaySound(AudioManager.Sounds.Ping);
                    break;
                case 3:
                    hit = false;
                    for (int i = 0; i < spores; i++)
                    {
                        int x = Random.Range(0, world.width);
                        int y = Random.Range(0, world.height);
                        var pos = world.GetPos(x, y);
                        var tile = world.GetTile(pos);
                        if (weedTile.CanReplace(tile))
                        {
                            world.SetTile(x, y, pos, weedTile);
                            AudioManager.PlaySound(AudioManager.Sounds.Whoosh, world.tilemap.CellToWorld(pos));
                            hit = true;
                        }
                        if (hit)
                            Message("Strong winds!", "Spores might get blow in from the wilderness!");
                        else
                            return;
                    }
                    break;
                case 4:
                    Message("Lotto jackpot!", "The money has been transferred to your account!");
                    AudioManager.PlaySound(AudioManager.Sounds.Money);
                    world.Money += lottoAmount;
                    break;
            }
            time += Random.Range(minTime, maxTime);
        }
        if (Time.time > messTime)
        {
            message.gameObject.SetActive(false);
            messTime = float.MaxValue;
        }
    }

    void Message(string title, string desc)
    {
        messTime = Time.time + messageTime;
        message.text = $"<b>{title}</b>\n{desc}";
        message.gameObject.SetActive(true);
    }
}
