using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockType;

public class Block : MonoBehaviour
{
    [SerializeField]
    private BLOCK_TYPE blockType;

    public BLOCK_TYPE BlockType
    {
        get { return blockType; }
    }

}