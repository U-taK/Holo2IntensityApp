using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TCPIPテスト
///親object
///送受信オブジェクトは用途ごとに判別がつくようにkeywordを設定
public class TransferParent
{
    public string keyword;

    public string Comment;

    public int testNum;
    public TransferParent(string key, string comment, int num)
    {
        keyword = key;
        Comment = comment;
        testNum = num;
    }
    public TransferParent()
    {
    }
}