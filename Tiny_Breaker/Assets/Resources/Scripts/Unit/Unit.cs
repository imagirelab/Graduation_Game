﻿using UnityEngine;
using StaticClass;

public class Unit : MonoBehaviour
{
    //タイプ
    public enum Type
    {
        Blue,
        Red,
        Green,
        White
    }
    public Type type = Type.White;

    public enum State
    {
        Search,
        Find,
        Attack,
        Dead,
        Wait
    }
    public State state = State.Search;

    //ステータス
    [SerializeField, TooltipAttribute("ステータス")]
    public Status status;

    [SerializeField]
    public float ATKRange = 10.0f;

    [SerializeField]
    public float loiteringSPEED = 1.0f;

    [HideInInspector]
    public bool IsDead;
    [HideInInspector]
    public bool IsDamage;   //ダメージ確認
    protected int oldHP = 0;    //直前の体力確認用

    [HideInInspector]
    public GameObject goalObject;       //ゴール
    [HideInInspector]
    public GameObject targetObject;       //目標

    public GameObject deadEffect;       //死亡エフェクト

    public GameObject deadSE;           //遊び

    [HideInInspector]
    public string targetTag;       //相手のタグ

    //巡回地点
    protected Transform[] loiteringPointObj;
    public Transform[] LoiteringPointObj { set { loiteringPointObj = value; } }

    int currentRootPoint = 0;
    [HideInInspector]
    public int rootNum = 0;

    [SerializeField]
    protected float deadMoveSpeed = 0.0f;    //死んだときに動くならその値
    [SerializeField]
    protected float deadTime = 1.0f;
    protected float deadcount = 0.0f;

    //出現場所の目的場所
    public bool setSpawnTargetFlag = false;
    Vector3 spawnTargetPosition;
    public Vector3 SpawnTargetPosition
    {
        get { return spawnTargetPosition; }
        set
        {
            spawnTargetPosition = value;
            setSpawnTargetFlag = true;
        }
    }

    //無敵時間
    [SerializeField]
    protected float invincibleTime = 1.3f;
    protected float invincibleCount = 0.0f;
    protected bool invincibleFlag = false;

    public void Move(Vector3 target, float speed)
    {
        //NavMeshAgentで動かす
        if (GetComponent<NavMeshAgent>())
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            //agent.speed = speed * 8.0f;
            //agent.speed = 50.0f;        //全員一律で同じ速さで壁を回避
            agent.speed = speed;
            agent.destination = target;
        }
    }
    
    public void SetNearTargetObject()
    {
        //プレイヤーのTarget
        targetObject = goalObject;

        //悪魔
        GameObject nearestObject = DemonDataBase.getInstance().GetNearestObject(targetTag, this.transform.position, rootNum);
        GameObject nearSol = SolgierDataBase.getInstance().GetNearestObject(transform.gameObject.tag, this.transform.position, rootNum);
        GameObject nearBuild = BuildingDataBase.getInstance().GetNearestObject(this.transform.position);
        
        if (nearestObject != null && targetObject != null)
            if (nearestObject.tag != transform.gameObject.tag)
                if (Vector3.Distance(this.transform.position, nearestObject.transform.position) <
                    Vector3.Distance(this.transform.position, targetObject.transform.position))
                    targetObject = nearestObject;

        if (nearSol != null && targetObject != null)
            if (nearSol.tag != transform.gameObject.tag)
                if (Vector3.Distance(this.transform.position, nearSol.transform.position) <
                    Vector3.Distance(this.transform.position, targetObject.transform.position))
                    targetObject = nearSol;

        if (nearBuild != null && targetObject != null)
            if (nearBuild.tag != transform.gameObject.tag)
                if (Vector3.Distance(this.transform.position, nearBuild.transform.position) <
                    Vector3.Distance(this.transform.position, targetObject.transform.position))
                    targetObject = nearBuild;
    }

    //ダメージを受けたか確認する
    public void DamageCheck(int nowHP)
    {
        if(nowHP < oldHP)
        {
            IsDamage = true;
        }
        else
        {
            IsDamage = false;
        }
        oldHP = nowHP;
    }

    //巡回ルートの座標を所得する
    public Vector3 GetRootPosition()
    {
        Vector3 rootPosition = loiteringPointObj[currentRootPoint].transform.position;
        
        return rootPosition;
    }

    //ポイントを通過するための更新
    public void UpdataRootPoint(float distance)
    {
        if (Vector3.Distance(transform.position, loiteringPointObj[currentRootPoint].transform.position) < distance)
        {
            if (currentRootPoint < loiteringPointObj.Length - 1)
                currentRootPoint++;
        }
    }
}
