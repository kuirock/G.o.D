using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCharge : MonoBehaviour
{

    public int magicPower{ get;set;} = 0;

    //���@�`���[�W�̃C�x���g
    public void Event(int num)
    {
        magicPower = num;
    }
}
