﻿using UnityEngine;
using System.Collections.Generic;
using StaticClass;

public class Spawner : MonoBehaviour
{
    //巡回ルート配列
    [SerializeField]
    GameObject[] rootes;    //一つ目は出現位置。あと巡回ルート
    Dictionary<int, List<Transform>> rootPointes = new Dictionary<int, List<Transform>>();

    //プレイヤー達
    [SerializeField]
    Player[] players = new Player[GameRule.getInstance().playerNum];

    int currentPlayerID = 0;
    public int CurrentPlayerID
    {
        get { return currentPlayerID; }
        set { currentPlayerID = value; }
    }

    int currentTargetID = 0;
    public int CurrentTargetID
    {
        get { return currentTargetID; }
        set { currentTargetID = value; }
    }

    //兵士たち
    GameObject[] soldiers;
    
    //同時生成数
    [SerializeField, Range(1, 5)]
    int spawnNum = 1;
    //生成間隔
    [SerializeField]
    float spawnCount = 10.0f;

    float timer = 0.0f;

    //生成する兵士の番号
    int solNum = 0;

    void Start ()
    {
        currentPlayerID = 0;
        currentTargetID = 0;

        timer = 0.0f;

        //設定し忘れたときは今いる場所を設定
        if (rootes == null)
            rootes = new GameObject[] { transform.gameObject };

        //巡回ルートの作成
        for (int i = 0; i < rootes.Length; i++)
        {
            rootPointes.Add(i, new List<Transform>());

            foreach (Transform child in rootes[i].transform)
                rootPointes[i].Add(child);

            //プレイヤーの数以内であれば
            if(i < players.Length)
                rootPointes[i].Add(players[i].SpawnPoint); //最後に最終目的地
        }

        GameObject Ax = (GameObject)Resources.Load("Prefabs/Soldier/SoldierAx");
        Ax.GetComponent<Unit>().status.SetStatus();
        GameObject Gun = (GameObject)Resources.Load("Prefabs/Soldier/SoldierGun");
        Gun.GetComponent<Unit>().status.SetStatus();
        GameObject Shield = (GameObject)Resources.Load("Prefabs/Soldier/SoldierShield");
        Shield.GetComponent<Unit>().status.SetStatus();

        soldiers = new GameObject[] { Ax, Gun, Shield };
    }
	
	void Update ()
    {
        //プレイヤーのどちらかが倒されていたら更新しない
        foreach (Player e in players)
            if (e.SpawnPoint.gameObject == null)
                return;

        timer += Time.deltaTime;

        //生成時間に達しているか確認
        if (timer > spawnCount)
        {
            //時間をリセット
            timer = 0;

            //兵士の生成
            Spawn();

            //兵士番号を進める
            SolCountUP();
        }
    }

    void Spawn()
    {
        //中立
        if (currentPlayerID == 0)
        {
            //生成数までループ
            for (int i = 0; i < spawnNum; i++)
            {
                //プレイヤー達の方向にそれぞれ進ませる
                for(int j = 0; j < rootes.Length; j++)
                {
                    GameObject instance = (GameObject)Instantiate(soldiers[solNum],
                                        rootes[j].transform.position,
                                        soldiers[solNum].transform.rotation);
                    instance.transform.parent = gameObject.transform;   //親
                    instance.GetComponent<Unit>().targetTag = players[j].tag;   //相手のタグを設定
                    instance.GetComponent<Unit>().LoiteringPointObj = rootPointes[j].ToArray();     //巡回ルート
                    instance.GetComponent<Unit>().goalObject = players[j].SpawnPoint.gameObject;    //最終目標
                }
            }
        }

        //だれかのもの
        if (currentPlayerID != 0)
        {
            int id = currentPlayerID - 1;   //配列番号に合わせる 味方になるID
            int targetID = currentTargetID - 1; //相手になるID

            //巡回ルートの再作成
            rootPointes.Clear();
            for (int i = 0; i < rootes.Length; i++)
            {
                rootPointes.Add(i, new List<Transform>());

                foreach (Transform child in rootes[targetID].transform)
                {
                    rootPointes[i].Add(child);
                }

                //プレイヤーの数以内であれば
                if (i < players.Length)
                    rootPointes[i].Add(players[id].target.transform); //最後に最終目的地
            }

            //生成数までループ
            for (int i = 0; i < spawnNum; i++)
            {
                for (int j = 0; j < rootes.Length; j++)
                {
                    GameObject instance = (GameObject)Instantiate(soldiers[solNum],
                                    rootes[j].transform.position,
                                    soldiers[solNum].transform.rotation);
                    instance.transform.parent = gameObject.transform;       //親
                    instance.tag = players[id].transform.gameObject.tag;    //自分のタグを設定
                    instance.GetComponent<Unit>().targetTag = players[id].TergetTag;   //相手のタグを設定
                    instance.GetComponent<Unit>().LoiteringPointObj = rootPointes[id].ToArray();    //徘徊ルート
                    instance.GetComponent<Unit>().goalObject = players[id].target;  //最終目標
                }
            }
        }
    }

    void SolCountUP()
    {
        solNum++;

        if (solNum >= soldiers.Length)
            solNum = 0;
    }
}