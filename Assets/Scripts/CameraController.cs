using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;


public class CameraController : MonoBehaviour
{

    [SerializeField]
    GameObject[] playerGO;                              // プレイヤーのゲームオブジェクト
    [SerializeField]
    GameObject target;                                  // ターゲット(ボス)
    [SerializeField]
    float targetNearDist = 17f, targetFarDist = 100f;   // ターゲットとの距離(far以上ならば計算しない)
    [SerializeField]
    float camAdditionNearY = 1f, camAdditionFarY = -2f; // カメラのY加算値(カメラが地面ギリギリだと見づらいので補正する)

    readonly float camSize = 29f;                       // ターゲットとの距離が最大の時のカメラサイズ

    Camera cam;

    //TextMeshProUGUI debug;
    Player[] player;                                    // プレイヤー


    void Start()
    {
        cam = GetComponent<Camera>();

        player = new Player[playerGO.Length];
        for(int i = 0; i < playerGO.Length; i++)
        {
            player[i] = playerGO[i].GetComponent<Player>();
            //debug = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        CamUpdate();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Shake(0.5f, 0.2f);
        }
    }

    // カメラコントローラー
    void CamUpdate()
    {
        // 遠いいプレイヤーを求める
        GameObject plGO = playerGO[GetFarPlayer()];

        // ターゲットとプレイヤーの中間を求める
        float center = (target.transform.position.x - plGO.transform.position.x) / 2f + plGO.transform.position.x;

        // カメラ座標を求めた中間座標にする
        cam.transform.position = new Vector3(center, cam.transform.position.y, cam.transform.position.z);

        // ターゲットとプレイヤーの距離を求める
        //float dist = Vector2.Distance(target.transform.position, plGO.transform.position); // こっちだとY 座標も計算するので大ジャンプで計算がおかしくなる
        float dist = Mathf.Abs(target.transform.position.x - plGO.transform.position.x);    // 単純に x 座標だけで求める

        // ターゲットとの距離が100以上なら計算しない
        if (dist > targetFarDist) { return; }

        // ターゲットとの距離が17以下ならカメラのサイズを計算
        if (dist > targetNearDist)
        {
            // 17 の時 size = 7、100の時 size = 29になる式
            cam.orthographicSize = camSize * (dist / targetFarDist);
            //17以下のとき y = 0、100のとき y = 24になる式
            float rate = ((dist - targetNearDist) / (targetFarDist - targetNearDist));
            // カメラが下すぎる時の補正値
            float y = camAdditionNearY + rate * (camAdditionFarY - camAdditionNearY);
            // カメラ位置
            cam.transform.position = new Vector3(cam.transform.position.x, y, cam.transform.position.z);
        }
    }

    // 遠いプレイヤーを得る
    int GetFarPlayer()
    {
        int   ret  = 0;
        float dist = 0;

        for(int i = 0; i < playerGO.Length; i++)
        {
            // ターゲットとプレイヤーの中間を求める
            float center = (target.transform.position.x - playerGO[i].transform.position.x) / 2f + playerGO[i].transform.position.x;

            // カメラ座標を求めた中間座標にする
            cam.transform.position = new Vector3(center, cam.transform.position.y, cam.transform.position.z);

            // ターゲットとプレイヤーの距離を求める
            //float dist = Vector2.Distance(target.transform.position, player[i].transform.position);   // こっちだとY 座標も計算するので大ジャンプで計算がおかしくなる
            float a = Mathf.Abs(target.transform.position.x - playerGO[i].transform.position.x);          // 単純に x 座標だけで求める
            if(dist < a)
            {
                dist = a;
                ret = i;
            }
        }
        return ret;
    }

    // 振動開始
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    // 振動は非同期で行う
    IEnumerator DoShake(float duration, float magnitude)
    {
        var pos = transform.localPosition;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            var x = pos.x + Random.Range(-1f, 1f) * magnitude;
            var y = pos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, pos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }
        transform.localPosition = pos;
    }

}
