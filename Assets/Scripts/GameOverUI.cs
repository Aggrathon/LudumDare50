using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI recordText;
    public bool victory;
    public World world;

    private void OnEnable()
    {
        // TODO fanfare
        Time.timeScale = 0f;
        if (victory)
        {
            float oldTime = PlayerPrefs.GetFloat("victory", float.MaxValue);
            float newTime = world.MapTime;
            if (newTime < oldTime)
            {
                PlayerPrefs.SetFloat("victory", newTime);
                recordText.text = $"It took {Mathf.RoundToInt(newTime)} seconds to pay off your debt! (New record!)";
            }
            else
            {
                recordText.text = $"It took {Mathf.RoundToInt(newTime)} seconds to pay off your debt! (Personal best: {Mathf.RoundToInt(oldTime)})";
            }
        }
        else
        {
            float oldTime = PlayerPrefs.GetFloat("defeat", float.MaxValue);
            float newTime = world.MapTime;
            if (newTime < oldTime)
            {
                PlayerPrefs.SetFloat("defeat", newTime);
                recordText.text = $"Your dream life lasted {Mathf.RoundToInt(newTime)} seconds! (New record!)";
            }
            else
            {
                recordText.text = $"Your dream life lasted {Mathf.RoundToInt(newTime)} seconds! (Personal best: {Mathf.RoundToInt(oldTime)})";
            }

        }
    }
}
