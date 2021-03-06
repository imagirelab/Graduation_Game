﻿//アクティブの間攻撃を繰り返す

using UnityEngine;
using System.Collections;
using StaticClass;
using System.Collections.Generic;

public class UnitAttack : MonoBehaviour
{
    #region フィールド

    [SerializeField]
    GameObject effect = null;

    [SerializeField]
    Enum.Color_Type adType = Enum.Color_Type.White;
    [SerializeField]
    float admag = 1.5f;
    [SerializeField]
    Enum.Color_Type unadType = Enum.Color_Type.White;
    [SerializeField]
    float unadmag = 0.5f;

    //攻撃間隔
    [SerializeField]
    float atkTime = 1.0f;
    public float AtkTime { set { atkTime = value; } }

    //攻撃を実際に計算するときの攻撃時間全体を見てどのあたりで計算を行うかの割合
    //攻撃間隔　２秒　atkDamageDelayRate　0.4fの場合
    //0.8秒の時に計算を行う
    [SerializeField, Range(0, 1.0f), TooltipAttribute("ダメージ計算を行うタイミングの割合")]
    float atkDamageDelayRate = 0.0f;

    [SerializeField, TooltipAttribute("構え時間")]
    float setDelayTime = 0.0f;

    //攻撃実行対象
    GameObject target = null;
    public GameObject Target { get { return target; } }

    [SerializeField, TooltipAttribute("範囲攻撃するときの範囲")]
    float roundRenge = 1.5f;

    [SerializeField, TooltipAttribute("貫通攻撃が届く射程")]
    float penetRenge = 20.0f;
    [SerializeField, TooltipAttribute("貫通攻撃の横幅")]
    float penetWight = 1.5f;

    Unit unit;
    Coroutine cor;

    public GameObject hitEffect;
    public GameObject criticalHitEffect;

    #endregion

    IEnumerator Attack()
    {
        unit = gameObject.GetComponent<Unit>();

        yield return new WaitForSeconds(setDelayTime);

        while (true)
        {
            if (effect != null)
            {
                GameObject instance = (GameObject)Instantiate(effect,
                                        gameObject.transform.position,
                                        gameObject.transform.rotation);
                instance.transform.parent = gameObject.transform;   //親
            }

            //対象物が同じタグだったら仲間だから攻撃しない
            if (target != null)
                if (target.tag == transform.gameObject.tag)
                    target = null;

            //攻撃中心対象がいなくなったら再登録
            if (target == null)
                target = unit.targetObject;

            transform.LookAt(target.transform.position);

            yield return new WaitForSeconds(atkTime * atkDamageDelayRate);

            List<GameObject> targetList = new List<GameObject>();
            //まずは中心攻撃対象だけを登録
            targetList.Add(target);

            //範囲攻撃
            if (unit.RoundAttack)
            {
                //範囲攻撃に使うスフィア
                int layerMask = ~(1 << transform.gameObject.layer | 1 << 18 | 1 << 21);  //レイキャストが省くレイヤーのビット演算
                if (target == null)
                    target = unit.targetObject;
                Collider[] allhit = Physics.OverlapSphere(target.transform.position, roundRenge, layerMask);
                //中心攻撃対象の周りを登録
                foreach (Collider e in allhit)
                    if (e.gameObject != target)
                        targetList.Add(e.gameObject);
            }
            //貫通攻撃
            if (unit.PenetrateAttack)
            {
                //貫通攻撃に使うスフィア
                int layerMask = ~(1 << transform.gameObject.layer | 1 << 18 | 1 << 21);  //レイキャストが省くレイヤーのビット演算
                if (target == null)
                    target = unit.targetObject;
                Vector3 vec = target.transform.position - transform.position;
                Ray ray = new Ray(transform.position, vec);
                RaycastHit[] hitall = Physics.SphereCastAll(ray, penetWight, penetRenge, layerMask);
                //中心攻撃対象の周りを登録
                foreach (RaycastHit e in hitall)
                    if (e.collider.gameObject != target)
                        targetList.Add(e.collider.gameObject);
            }

            //種類によって攻撃処理
            foreach (GameObject e in targetList)
            {
                //攻撃対象がいることを確認してから攻撃
                if (e != null)
                {
                    if (e.GetComponent<Unit>())
                        AttackUnit(e);
                    else if (e.GetComponent<House>())
                        AttackHouse(e);
                    else if (e.GetComponent<DefenseBase>())
                        AttackDefenseBase(e);
                }
            }

            yield return null;
            yield return new WaitForSeconds(atkTime - (atkTime * atkDamageDelayRate));
        }
    }

    //悪魔と兵士について
    void AttackUnit(GameObject _target)
    {
        Unit unitComp = _target.GetComponent<Unit>();

        //倍率
        float mag = 1.0f;

        //倍率計算
        if (unitComp.type == adType)
        {
            mag = admag;

            //クリティカルヒットエフェクト
            if (criticalHitEffect != null && _target != null)
            {
                Instantiate(criticalHitEffect,
                            _target.transform.position + criticalHitEffect.transform.position,
                            criticalHitEffect.transform.rotation);
            }
        }
        if (unitComp.type == unadType)
            mag = unadmag;

        unitComp.AnyDamage(Mathf.RoundToInt(unit.status.CurrentATK * mag), unit);
        
        //ヒットエフェクト
        if (hitEffect != null && _target != null)
        {
            if (_target.GetComponent<SphereCollider>())
            {
                float targetColliderRadius = _target.GetComponent<SphereCollider>().radius * _target.transform.localScale.x;
                Vector3 vec = (gameObject.transform.position - _target.transform.position).normalized;
                vec *= targetColliderRadius;

                Instantiate(hitEffect,
                            _target.transform.position + vec + hitEffect.transform.position,
                            _target.transform.rotation);
            }
        }

        //親にプレイヤーコストを持っている(プレイヤー)場合のコスト処理
        if (unitComp.status.CurrentHP <= 0)
        {
            //倒した数をカウントする
            if (unitComp.state != Enum.State.Dead)
            {
                unitComp.state = Enum.State.Dead;

                RoundDataBase.getInstance().AddPassesKnockDownCount(gameObject.tag);

                if (_target.GetComponent<Soldier>() && unit.transform.parent.gameObject.GetComponent<PlayerCost>())
                {
                    PlayerCost playerCost = unit.transform.parent.gameObject.GetComponent<PlayerCost>();
                    Player player = unit.transform.parent.gameObject.GetComponent<Player>();

                    //parent.transform.root は　悪魔のRootつまりプレイヤー
                    player.AddCostList(playerCost.GetSoldierCost);
                }
            }
        }
    }

    //建物への攻撃はこっち
    void AttackHouse(GameObject _target)
    {
        House houseComp = _target.GetComponent<House>();

        if (!houseComp.IsDead)
        {
            switch (gameObject.tag)
            {
                case "Player1":
                    houseComp.HPpro += unit.status.CurrentATK;
                    break;
                case "Player2":
                    houseComp.HPpro -= unit.status.CurrentATK;
                    break;
                default:
                    break;
            }

            //ヒットエフェクト
            if (hitEffect != null && _target != null)
            {
                if (_target.GetComponent<BoxCollider>())
                {
                    float targetColliderRadius = _target.GetComponent<BoxCollider>().size.magnitude * 0.5f;
                    Vector3 vec = (gameObject.transform.position - _target.transform.position).normalized;
                    vec *= targetColliderRadius;

                    Instantiate(hitEffect,
                                _target.transform.position + vec + hitEffect.transform.position,
                                _target.transform.rotation);
                }
            }
        }

        //親が小屋クラスを持っている(プレイヤー)場合のコスト処理
        if ((gameObject.tag == "Player2" && houseComp.HPpro <= -houseComp.GetHP) ||
            (gameObject.tag == "Player1" && houseComp.HPpro >= houseComp.GetHP))
        {
            //死んだフラグを立てる
            houseComp.IsDead = true;
            //リストからはずす
            BuildingDataBase.getInstance().RemoveList(_target);

            //スポナーがあるときPlayerIDを登録
            if (_target.GetComponent<Spawner>() != null)
            {
                //事前にタグを保存しておく
                houseComp.OldTag = _target.tag;

                //倒した奴のタグにする
                _target.tag = gameObject.tag;
                //子供も一緒に
                foreach (Transform child in _target.transform)
                    child.gameObject.tag = gameObject.tag;

                switch (gameObject.tag)
                {
                    case "Player1":
                        _target.GetComponent<Spawner>().CurrentPlayerID = 1;
                        _target.GetComponent<Spawner>().CurrentTargetID = 2;
                        break;
                    case "Player2":
                        _target.GetComponent<Spawner>().CurrentPlayerID = 2;
                        _target.GetComponent<Spawner>().CurrentTargetID = 1;
                        break;
                    default:
                        _target.GetComponent<Spawner>().CurrentPlayerID = 0;
                        _target.GetComponent<Spawner>().CurrentTargetID = 0;
                        break;
                }
            }

            //コストの計算
            GameObject rootObject = unit.transform.parent.gameObject;
            if (rootObject.GetComponent<PlayerCost>())
            {
                Player player = rootObject.GetComponent<Player>();
                PlayerCost playerCost = rootObject.GetComponent<PlayerCost>();

                player.AddCostList(playerCost.GetHouseCost);
            }
        }
    }

    //城への攻撃はこっち
    void AttackDefenseBase(GameObject _target)
    {
        if (_target.GetComponent<DefenseBase>())
        {
            _target.GetComponent<DefenseBase>().HPpro -= unit.status.CurrentATK;
            
            //ヒットエフェクト
            if (hitEffect != null && _target != null)
            {
                float targetColliderRadius = _target.GetComponent<SphereCollider>().radius * _target.transform.localScale.x;
                Vector3 vec = (gameObject.transform.position - _target.transform.position).normalized;
                vec *= targetColliderRadius;

                Instantiate(hitEffect,
                            _target.transform.position + vec + hitEffect.transform.position,
                            _target.transform.rotation);
            }
        }
    }

    void OnEnable()
    {
        cor = StartCoroutine(Attack());
    }

    void OnDisable()
    {
        //攻撃実行対象を戻す
        target = null;

        StopCoroutine(cor);
    }
}