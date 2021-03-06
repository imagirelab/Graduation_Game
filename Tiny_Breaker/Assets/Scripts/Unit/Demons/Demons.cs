﻿using UnityEngine;
using StaticClass;
using System.Collections;
using UnityEngine.UI;

public class Demons : Unit
{
    [SerializeField, TooltipAttribute("種類")]
    Enum.Demon_TYPE DemonType = Enum.Demon_TYPE.POPO;
    
    //[SerializeField, TooltipAttribute("召喚中に消すステータスのUI")]
    //GameObject statusUI;

    [SerializeField, TooltipAttribute("下ルートを通るときの速度")]
    float UnderSpeed = 20.0f;

    SpawnMove spawn;

    IEnumerator ien;

    void Start()
    {
        //パワーアップ条件
        if (level >= powerupLevel)
            switch (DemonType)
            {
                case Enum.Demon_TYPE.POPO:
                    refrecAttack = true;        //反射攻撃
                    break;
                case Enum.Demon_TYPE.PUPU:
                    roundAttack = true;         //範囲攻撃
                    break;
                case Enum.Demon_TYPE.PIPI:
                    penetrateAttack = true;     //貫通攻撃
                    status.CurrentAtackTime /= penetAttackTimeRate;    //攻撃速度上昇
                    break;
            }

        Initialize();
    }

    //初期化
    void Initialize()
    {
        //if (statusUI == null)
        //    statusUI = new GameObject();

        seach = GetComponent<UnitSeach>();
        attack = GetComponent<UnitAttack>();
        move = GetComponent<UnitMove>();
        spawn = GetComponent<SpawnMove>();
        sCollider = GetComponent<SphereCollider>();

        //出現目標値を入れる
        spawn.SetTargetVec(SpawnTargetPosition - transform.position);

        //死亡フラグ
        IsDead = false;

        //攻撃に関する設定
        attack.AtkTime = status.CurrentAtackTime;
        ////下ルート時の加速
        if (rootNum == Enum.Direction_TYPE.Bottom)
            status.CurrentSPEED = UnderSpeed;
        //巡回速度
        loiteringSPEED = status.CurrentSPEED;

        ////設定がなされていなかった時の仮置き
        if (loiteringPointObj == null)
            loiteringPointObj = new Transform[] { goalObject.transform };
        if (goalObject == null)
            goalObject = GameObject.Find("DummyTarget");

        //一番近くの敵を狙う
        SetNearTargetObject();

        ien = DemonLife();
        StartCoroutine(ien);
        StartCoroutine(NearTarget());
    }

    IEnumerator DemonLife()
    {
        //出撃
        spawn.enabled = true;

        ////ステータスUIを消す
        //statusUI.SetActive(false);

        //無敵起動
        StartCoroutine(Invincible());

        //出撃完了まで待つ
        while (!spawn.End)
            yield return null;

        //出撃スクリプト停止
        spawn.enabled = false;
        ////ステータスUIを表示
        //statusUI.SetActive(true);

        //コライダーオン
        sCollider.enabled = true;

        //移動開始
        move.enabled = true;

        //索敵開始
        seach.enabled = false;

        float colliderScalingDiameter = sCollider.radius * transform.localScale.x;

        //生きている時の処理
        while (status.CurrentHP > 0)
        {
            //ダメージを受けたかの確認
            DamageCheck(status.CurrentHP);

            state = Enum.State.Search;

            //もしターゲットをロストしていた場合だけ急きょ次に狙うものを索敵する
            if (targetObject == null)
                SetNearTargetObject();
            
            //無駄な処理を省くための条件
            if (targetDistance - targetColliderRadius < ATKRange + colliderScalingDiameter)
            {
                if (targetDistance - targetColliderRadius > colliderScalingDiameter)
                {
                    RaycastHit hit;
                    Vector3 vec = targetObject.transform.position - transform.position;
                    Ray ray = new Ray(transform.position, vec);
                    ray.origin += new Vector3(0.0f, 1.5f, 0.0f);    //視線の高さ分上げている形
                    int layerMask = ~(1 << transform.gameObject.layer | 1 << 18 | 1 << 21);  //レイキャストが省くレイヤーのビット演算
                    if (Physics.SphereCast(ray, 1.5f, out hit, ATKRange + colliderScalingDiameter, layerMask))
                    {
                        if (hit.collider.gameObject == targetObject)
                        {
                            state = Enum.State.Attack;
                            seach.enabled = false;
                            attack.enabled = true;

                            //SE再生フラグ
                            switch(DemonType)
                            {
                                case Enum.Demon_TYPE.PUPU:
                                    if(level >= powerupLevel)
                                    {
                                        BUBUAttackSE.BUBUattackSEFlag = true;
                                    }
                                    else
                                    {
                                        PUPUAttackSE.PUPUattackSEFlag = true;
                                    }
                                    break;
                                case Enum.Demon_TYPE.POPO:
                                    if (level >= powerupLevel)
                                    {
                                        BOBOAttackSE.BOBOattackSEFlag = true;
                                    }
                                    else
                                    {
                                        POPOAttackSE.POPOattackSEFlag = true;
                                    }
                                    break;
                                case Enum.Demon_TYPE.PIPI:
                                    if (level >= powerupLevel)
                                    {
                                        BIBIAttackSE.BIBIattackSEFlag = true;
                                    }
                                    else
                                    {
                                        PIPIAttackSE.PIPIattackSEFlag = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                //重なっている時
                else
                {
                    state = Enum.State.Attack;
                    seach.enabled = false;
                    attack.enabled = true;
                }
            }
            else if (targetDistance - targetColliderRadius <= seach.findRange + colliderScalingDiameter &&
                    targetDistance - targetColliderRadius >= ATKRange + colliderScalingDiameter)
            {
                seach.enabled = true;
                attack.enabled = false;
            }
            else
            {
                attack.enabled = false;
            }

            //発見状態の条件
            if (seach.enabled)
            {
                if (seach.IsLose)
                    seach.enabled = false;
                if (seach.IsFind)
                    state = Enum.State.Find;
            }

            yield return null;
        }

        //死亡処理
        state = Enum.State.Dead;
        yield return StartCoroutine(Dead());

        Destroy(gameObject);
    }

    IEnumerator Dead()
    {
        IsDead = true;
        DemonDeadSE.deadSEFlag = true;
        
        //リストから外す
        DemonDataBase.getInstance().RemoveList(this.gameObject);

        //死んだ直後に成長値とコストを回収してみる
        if (transform.parent != null)
        {
            PlayerCost playerCost = transform.parent.gameObject.GetComponent<PlayerCost>();
            Player player = transform.parent.gameObject.GetComponent<Player>();

            player.AddCostList(playerCost.GetReturnCost);
            player.AddSpiritList(DemonType);
        }
        
        SoundManager.deadSEFlag = true;

        //いらない子供から消していく
        if (transform.IsChildOf(transform))
            foreach (Transform child in transform)
            {
                if (child.name != "Model")
                    Destroy(child.gameObject);
            }

        //自分のコンポーネントの削除
        foreach (Component comp in this.GetComponents<Component>())
            if (comp != GetComponent<Transform>() && comp != GetComponent<Demons>() && comp != GetComponent<Rigidbody>())
                Destroy(comp);

        //死亡エフェクト
        if (this.gameObject.tag == "Player1")
        {
            GameObject instace = (GameObject)Instantiate(reddeadEffect, reddeadEffect.transform.position, reddeadEffect.transform.rotation);
            instace.transform.SetParent(this.transform, false);   //親を出した悪魔に設定
        }
        else if (this.gameObject.tag == "Player2")
        {
            GameObject instace = (GameObject)Instantiate(bluedeadEffect, bluedeadEffect.transform.position, bluedeadEffect.transform.rotation);
            instace.transform.SetParent(this.transform, false);   //親を出した悪魔に設定
        }

        yield return null;

        //死んでいる時の処理
        StartCoroutine(DeadScale());

        while (deadcount <= deadTime)
        {
            deadcount += Time.deltaTime;

            GetComponent<Rigidbody>().velocity = transform.forward * -1 * deadMoveSpeed;
            
            yield return null;
        }
    }

    //だんだん小さくなって死んでいく
    IEnumerator DeadScale()
    {
        yield return new WaitForSeconds(2.0f);
        
        while (true)
        {
            transform.localScale *= 0.9f;
            yield return null;
        }
    }

    //無敵の処理（無敵になるタイミングで呼び出す）
    IEnumerator Invincible()
    {
        // リストから外す
        DemonDataBase.getInstance().RemoveList(this.gameObject);

        yield return new WaitForSeconds(invincibleTime);

        // リストに追加する
        DemonDataBase.getInstance().AddList(this.gameObject);

        yield return null;
    }

    void OnEnable()
    {
        if (ien != null)
        {
            // リストに追加する
            DemonDataBase.getInstance().AddList(this.gameObject);

            StartCoroutine(ien);
        }
    }

    //破壊されたときにリストから外す
    void OnDisable()
    {
        if (ien != null)
            StopCoroutine(ien);
        DemonDataBase.getInstance().RemoveList(this.gameObject);
    }
}