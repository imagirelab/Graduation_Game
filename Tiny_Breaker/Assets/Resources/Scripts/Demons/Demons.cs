﻿// 悪魔単体の処理をするクラス

using UnityEngine;
using System.Collections;

public class Demons : MonoBehaviour
{
    //プレイヤーの仮ステータス
    [SerializeField, TooltipAttribute("悪魔のステータス")]
    public DemonsData status;
    [SerializeField, TooltipAttribute("悪魔の成長値ポイント")]
    public DemonsGrowPointData growPoint;

    public GameObject demonSpirit;

    private Vector3 moveDirection;      //移動する方向の角度
    private float time;                 //時間

    // 接触しているゲームオブジェクト
    private GameObject hitCollisionObject;

    // Use this for initialization
    void Start ()
    {
        moveDirection = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //死亡処理
        if (status.CurrentHP <= 0)
            Dead();
    }

    // 移動命令の処理
    public void MoveOrder(Vector3 targetPosition)
    {
        if (this.gameObject)
        {
            //目的地と0.1mより離れている場合の処理
            if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                //角度計算
                moveDirection = (targetPosition - transform.position).normalized;
                //目的地への方向を見る
                transform.LookAt(transform.position + new Vector3(targetPosition.x, 0, targetPosition.z));
                //移動方向へ速度をSPEED分の与える
                this.GetComponent<Rigidbody>().velocity = moveDirection * status.CurrentSPEED;
            }
            //目的地に0.1mより近い距離になった場合の処理
            else
            {
                //速度を０に
                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }

    // 建造物に向かい攻撃する命令の処理
    public void BuildingOrder(GameObject target)
    {
        //ターゲットが何も無ければ処理しない
        if (target == null)
        {
            //速度を０に
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            return;
        }

        //目的地と離れている場合の処理
        if (hitCollisionObject != target)
        {
            //角度計算
            moveDirection = (target.transform.position - transform.position).normalized;
            //目的地への方向を見る
            transform.LookAt(transform.position + new Vector3(target.transform.position.x, 0, target.transform.position.z));
            //移動方向へ速度をSPEED分の与える
            this.GetComponent<Rigidbody>().velocity = moveDirection * status.CurrentSPEED;
        }
        //目的地に触れている場合の処理
        else
        {
            //速度を０に
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;

            //アタックタイムを満たしたら
            if (time > status.CurrentAtackTime)
            {
                time = 0;

                // お城クラスを持っていたら処理
                if(target.GetComponent<Castle>() != null)
                    target.GetComponent<Castle>().HPpro -= status.CurrentATK;

                //家クラスを持っていたら処理
                if (target.GetComponent<House>() != null)
                    target.GetComponent<House>().HPpro -= status.CurrentATK;
            }

            //1フレームあたりの時間を取得
            time += Time.deltaTime;

        }
    }

    // 敵に攻撃する命令の処理
    public void EnemyOrder(GameObject target)
    {
        //ターゲットが何も無ければ処理しない
        if (target == null)
        {
            //速度を０に
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            return;
        }

        //目的地と離れている場合の処理
        if (hitCollisionObject != target)
        {
            //角度計算
            moveDirection = (target.transform.position - transform.position).normalized;
            //目的地への方向を見る
            transform.LookAt(transform.position + new Vector3(target.transform.position.x, 0, target.transform.position.z));
            //移動方向へ速度をSPEED分の与える
            this.GetComponent<Rigidbody>().velocity = moveDirection * status.CurrentSPEED;
        }
        //目的地に触れている場合の処理
        else
        {
            //速度を０に
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            
            //アタックタイムを満たしたら
            if (time > status.CurrentAtackTime)
            {
                time = 0;

                // 敵クラスを持っていたら処理
                if (target.GetComponent<Soldier>() != null)
                    target.GetComponent<Soldier>().HPpro -= status.CurrentATK;
            }

            //1フレームあたりの時間を取得
            time += Time.deltaTime;

        }
    }

    //待機命令の処理(いらないかもしれないが)
    public void WaitOrder()
    {
        //今はとりあえず動きを止める
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    //オブジェクトが衝突したときの処理
    void OnCollisionEnter(Collision collision)
    {
        //接触したゲームオブジェクトを保存
        hitCollisionObject = collision.gameObject;
    }

    //オブジェクトが触れている間
    void OnCollisionStay(Collision collision)
    {
        //接触しているゲームオブジェクトを保存
        hitCollisionObject = collision.gameObject;
    }

    //オブジェクトが離れた時
    void OnCollisionExit(Collision collision)
    {
        //離れたら戻す
        hitCollisionObject = null;
    }

    //死んだときの処理
    void Dead()
    {
        //スピリットの生成
        GameObject spirit = (GameObject)Instantiate(demonSpirit, this.gameObject.transform.position, this.gameObject.transform.rotation);
        spirit.GetComponent<DemonsSpirits>().GrowPoint = this.gameObject.GetComponent<Demons>().growPoint;
        Destroy(gameObject);
    }
}
