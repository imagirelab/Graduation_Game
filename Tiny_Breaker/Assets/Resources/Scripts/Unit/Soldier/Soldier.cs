﻿// 敵のステータスなどを管理するクラス

using UnityEngine;
using StaticClass;

public class Soldier : Unit
{
    [SerializeField]
    float deadTime = 1.0f;
    float deadcount = 0.0f;

    public int powerUPCount = 0;

    void Start()
    {
        // 作られたときにリストに追加する
        SolgierDataBase.getInstance().AddList(this.gameObject, transform.gameObject.tag);
        
        deadcount = 0.0f;

        //死亡フラグ
        IsDead = false;

        //巡回ルート
        if (gameObject.transform.parent == null)
            loiteringPointObj = new Transform[] { transform };

        //ステータスの決定
        SetStatus();
    }

    void Update()
    {
        //死亡処理
        if (status.CurrentHP <= 0)
            Dead();

        if (IsDead)
            Dying();
        else
        {
            if (targetObject != null)
            {
                //発見
                IsFind = false;
                if (Vector3.Distance(transform.position, targetObject.transform.position) < 40.0f)  //40.0f = Collider.Radius * LocalScale
                {
                    IsFind = true;
                    state = State.Find;
                }

                //攻撃
                IsAttack = false;
                if (Vector3.Distance(transform.position, targetObject.transform.position) < ATKRange * transform.localScale.x + 1.0f)  //攻撃範囲
                {
                    IsAttack = true;
                    gameObject.GetComponent<UnitAttack>().enabled = true;
                    state = State.Attack;
                }

                //徘徊
                if (!IsFind && !IsAttack)
                {
                    gameObject.GetComponent<UnitAttack>().enabled = false;
                    state = State.Search;
                }
            }
            else
            {
                IsFind = false;
                IsAttack = false;
                gameObject.GetComponent<UnitAttack>().enabled = false;
                state = State.Wait;
            }
        }
    }
    
    //死にかけている時の処理
    void Dying()
    {
        deadcount += Time.deltaTime;

        if (deadcount > deadTime)
            Destroy(gameObject);
    }

    //死んだときの処理
    void Dead()
    {
        if (!IsDead)
        {
            IsDead = true;

            //リストから外すタイミングを死んだ条件の中に入れる
            SolgierDataBase.getInstance().RemoveList(this.gameObject);

            //いらない子供から消していく
            if (transform.IsChildOf(transform))
                foreach (Transform child in transform)
                {
                    if (child.name == "Models")
                    {
                        foreach (Component comp in child.GetComponents<Component>())
                            if (comp != child.GetComponent<Transform>())
                                Destroy(comp);

                        foreach (Transform grandson in child)
                            if (grandson.name == "Model")
                            {
                                foreach (Component comp in grandson.GetComponents<Component>())
                                    if (comp != grandson.GetComponent<Transform>())
                                        Destroy(comp);

                                foreach (GameObject e in GetAllChildren.GetAll(grandson.gameObject))
                                    if (e.GetComponent<Collider>())
                                    {
                                        e.GetComponent<Collider>().enabled = true;
                                        e.AddComponent<Rigidbody>();
                                    }
                            }
                    }
                    else
                        Destroy(child.gameObject);
                }

            foreach (Component comp in this.GetComponents<Component>())
                if (comp != GetComponent<Transform>() && comp != GetComponent<Soldier>())
                    Destroy(comp);
        }
    }

    //ステータスの設定
    void SetStatus()
    {
        //基礎ステータスの代入
        status.SetStatus();

        //今のステータスを算出する
        for (int i = 0; i < powerUPCount; i++)
            status.CurrentHP += (int)(status.GetHP * 0.5f);
        for (int i = 0; i < powerUPCount; i++)
            status.CurrentATK += (int)(status.GetATK * 0.5f);
        for (int i = 0; i < powerUPCount; i++)
            status.CurrentSPEED += status.GetSPEED * 0.15f;
        for (int i = 0; i < powerUPCount; i++)
            status.CurrentAtackTime -= status.GetAtackTime * 0.05f;

        status.MaxHP = status.CurrentHP;
    }

    //破壊されたときにリストから外す
    void OnDisable()
    {
        if (!IsDead)
            SolgierDataBase.getInstance().RemoveList(this.gameObject);
    }
}
