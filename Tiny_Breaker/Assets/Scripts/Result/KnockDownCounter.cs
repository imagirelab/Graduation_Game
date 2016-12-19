﻿using UnityEngine;
using UnityEngine.UI;
using StaticClass;

public class KnockDownCounter : MonoBehaviour
{
    [SerializeField]
    int PlayerID = 0;

    [SerializeField]
    Sprite[] numbars = new Sprite[10];

    [SerializeField]
    Image ones = null;
    [SerializeField]
    Image tens = null;
    [SerializeField]
    Image hundreds = null;

    void Start()
    {

    }

    void Update()
    {
        int value = RoundDataBase.getInstance().PassesKnockDownCount[PlayerID];

        int hundredsNum = (Mathf.FloorToInt(value) % 1000) / 100;
        int tensNum = (Mathf.FloorToInt(value) % 100) / 10;
        int onesNum = Mathf.FloorToInt(value) % 10;

        if (hundredsNum < 10)
            hundreds.sprite = numbars[hundredsNum];
        if (tensNum < 10)
            tens.sprite = numbars[tensNum];
        if (onesNum < 10)
            ones.sprite = numbars[onesNum];
    }
}