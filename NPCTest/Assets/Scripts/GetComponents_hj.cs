using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetComponents_hj : MonoBehaviour
{

    public float attackrange = 3;
    public float pursuitrange = 100;


    public GameObject[] enemys;
    public GameObject[] friends;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        enemys = GameObject.FindGameObjectsWithTag("enemyparty");
        friends = GameObject.FindGameObjectsWithTag("friendlyparty");
        //for(enemys)
    }
}
