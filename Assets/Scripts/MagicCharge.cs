using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCharge : MonoBehaviour
{

    public int magicPower{ get;set;} = 0;

    //魔法チャージのイベント
    public void Event(int num)
    {
        magicPower = num;
    }
}
