using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI debtText;
    public GameObject debtWarning;
    public Button payButton;
    public World world;
    public UserInterface ui;
    public int payTreshold = 20;
    [Range(1f, 5f)] public float warningTreshold = 1.3f;
    [Range(1f, 5f)] public float debtThreshold = 2.0f;

    public GameObject gameOverDebt;
    public GameObject gameOverMoney;
    public GameObject gameOverVictory;

    int oldMoney = -1;
    int oldDebt = -1;
    int wthres;
    int dthres;

    void Start()
    {
        wthres = Mathf.RoundToInt(warningTreshold * world.Debt);
        dthres = Mathf.RoundToInt(debtThreshold * world.Debt);
        OnMoneyChanged(world.Money, world.Debt);
        world.onMoneychange.AddListener(OnMoneyChanged);
        payButton.onClick.AddListener(PayDebt);
    }

    void OnMoneyChanged(int money, int debt)
    {
        if (money != oldMoney)
        {
            oldMoney = money;
            moneyText.text = (money * ui.moneyMult).ToString();
            payButton.interactable = money > payTreshold;
        }
        if (money + world.Income < ui.MinCost)
        {
            gameOverMoney.SetActive(true);
        }
        if (debt != oldDebt)
        {
            oldDebt = debt;
            debtText.text = (debt * ui.moneyMult).ToString();
            debtWarning.SetActive(debt > wthres);
            if (debt > dthres)
            {
                gameOverDebt.SetActive(true);
            }
            if (debt <= 0)
            {
                gameOverVictory.SetActive(true);
            }
        }
    }

    void PayDebt()
    {
        int pay = world.Money - payTreshold;
        if (pay > 0)
        {
            world.Money -= pay;
            world.Debt -= pay;
        }
    }
}
