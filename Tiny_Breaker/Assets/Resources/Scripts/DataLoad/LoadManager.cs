﻿using UnityEngine;
using System.Collections;
using System.IO;
using Loader;

public class LoadManager : MonoBehaviour
{
    [SerializeField]
    bool IsLoad = true;

    [SerializeField]
    GameObject prePOPO;
    [SerializeField]
    GameObject prePUPU;
    [SerializeField]
    GameObject prePIPI;
    [SerializeField]
    GameObject preShield;
    [SerializeField]
    GameObject preAx;
    [SerializeField]
    GameObject preGun;
    
    void Start()
    {
        //ロードしない設定なら飛ばす
        if (!IsLoad)
            return;

        //ゲームオブジェクトの設定しわすれがあった時、
        //メッセージを名前にして空のオブジェクトを作る
        if (prePOPO == null)
            prePOPO = new GameObject(this.ToString() + " prePOPO Null");
        if (prePUPU == null)
            prePUPU = new GameObject(this.ToString() + " prePUPU Null");
        if (prePIPI == null)
            prePIPI = new GameObject(this.ToString() + " prePIPI Null");
        if (preShield == null)
            preShield = new GameObject(this.ToString() + " preShield Null");
        if (preAx == null)
            preAx = new GameObject(this.ToString() + " preAx Null");
        if (preGun == null)
            preGun = new GameObject(this.ToString() + " preGun Null");

        string paramtext = GetCSVString("/Resources/CSVData/ParamData.csv");
        string growtext = GetCSVString("/Resources/CSVData/GrowData.csv");
        string costtext = GetCSVString("/Resources/CSVData/CostData.csv");

        ParamData ParamTable = new ParamData();
        ParamTable.Load(paramtext);

        GrowData GrowTable = new GrowData();
        GrowTable.Load(growtext);

        CostData CostTable = new CostData();
        CostTable.Load(costtext);
        
        foreach (var param in ParamTable.All)
        {
            switch (param.ID)
            {
                case "popo":
                    if (prePOPO != null)
                        SetParm(param, prePOPO);
                    break;
                case "pupu":
                    if (prePUPU != null)
                        SetParm(param, prePUPU);
                    break;
                case "pipi":
                    if (prePIPI != null)
                        SetParm(param, prePIPI);
                    break;
                case "shield":
                    if (preShield != null)
                        SetParm(param, preShield);
                    break;
                case "ax":
                    if (preAx != null)
                        SetParm(param, preAx);
                    break;
                case "gun":
                    if (preGun != null)
                        SetParm(param, preGun);
                    break;
                default:
                    break;
            }
        }

        foreach (var grow in GrowTable.All)
        {
            switch (grow.ID)
            {
                case "popo":
                    if (prePOPO != null)
                        SetGrow(grow, prePOPO);
                    break;
                case "pupu":
                    if (prePUPU != null)
                        SetGrow(grow, prePUPU);
                    break;
                case "pipi":
                    if (prePIPI != null)
                        SetGrow(grow, prePIPI);
                    break;
                default:
                    break;
            }
        }

        SetCost(CostTable);

        Debug.Log("Load END");
    }

    /// <summary>
    ///　CSVファイルの文字列を取得
    /// </summary>
    /// <param name="path">Assetフォルダ以下のCSVファイルの位置を書く</param>
    /// <returns>CSVファイルの文字列</returns>
    string GetCSVString(string path)
    {
        StreamReader sr = new StreamReader(Application.dataPath + path);
        string strStream = sr.ReadToEnd();

        return strStream;
    }

    void SetParm(ParamMaster param, GameObject unit)
    {
        if (unit.GetComponent<Unit>())
            unit.GetComponent<Unit>().status.SetDefault(param.HP, param.ATK, param.SPEED, param.ATKSPEED);
        if(unit.transform.FindChild("AttackRange").gameObject.GetComponent<CapsuleCollider>())
            unit.transform.FindChild("AttackRange").gameObject.GetComponent<CapsuleCollider>().radius = param.ATKRENGE;
    }

    void SetGrow(GrowMaster grow, GameObject unit)
    {
        if (unit.GetComponent<Demons>())
            unit.GetComponent<Demons>().GrowPoint.SetDefault(grow.GHP, grow.GATK, grow.GSPEED, grow.GATKSPEED);
    }

    void SetCost(CostData CostTable)
    {
        foreach (var cost in CostTable.All)
        {
            GameObject player = GameObject.Find("Player");
            if (player.GetComponent<PlayerCost>())
                player.GetComponent<PlayerCost>().SetDefault(cost.MaxCost, cost.StateCost, cost.CostParSecond, cost.DemonCost, cost.SoldierCost, cost.HouseCost);
        }
    }
}
