using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{   
    public GameObject chessPiece;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(chessPiece, new Vector3(0, 0, -1), Quaternion.identity);
    }
}
