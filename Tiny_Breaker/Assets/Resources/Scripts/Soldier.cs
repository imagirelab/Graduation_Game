﻿// 敵のステータスなどを管理するクラス

using UnityEngine;
using StaticClass;

public class Soldier : MonoBehaviour {

    //敵のステータス
    [SerializeField, TooltipAttribute("体力")]
    private int HP = 300;

    //このクラス内で使う変数
    private TextMesh HP_UI;           //HPのUI
    private ParticleSystem deadParticle;    //死亡時のパーティクル
    private bool deadFlag = false;      //死亡判定

    //外から見れる変数
    public int HPpro { get { return HP; } set { HP = value; } }

    // Use this for initialization
    void Start () {

        // 作られたときにリストに追加して
        SolgierDataBase.getInstance().AddList(this.gameObject);

        HP_UI = transform.FindChild("HP").gameObject.GetComponent<TextMesh>();
        deadParticle = this.transform.FindChild("deadParticle").GetComponent<ParticleSystem>();
    }

    //破壊されたときにリストから外す
    void OnDisable()
    {
        SolgierDataBase.getInstance().RemoveList(this.gameObject);
    }

    // Update is called once per frame
    void Update ()
    {        
        if (HP <= 0)
        {
            if (!deadFlag)
            {
                Destroy(HP_UI);
                deadParticle.Play();
                deadFlag = true;
            }

            if (deadParticle.isStopped)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            HP_UI.text = "HP: " + HP.ToString();
        }
        
    }
}
