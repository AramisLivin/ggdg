using System.Collections;
using System.Collections.Generic;
using GoogleARCore.Examples.ObjectManipulation;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public PawnManipulator PawnManipulator;

    public GameObject Pawn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void BarrelAsPawn()
    {
        PawnManipulator.PawnPrefab = Resources.Load("Barrell/BarrelPref") as GameObject;
    }
    
    public void TowerAsPawn()
    {
        PawnManipulator.PawnPrefab = Resources.Load("ArcherTower/ArcherTower") as GameObject;
    }
    
    public void PawnAsPawn()
    {
        PawnManipulator.PawnPrefab = Pawn;
    }
}
