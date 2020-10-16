using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Klasa odpowiada za informację o zwycięzcy danej rundy
public class Wynik : MonoBehaviour
{
    private Text txt;

    private void Awake()
    {
        txt = GetComponent<Text>();
        txt.text = "";
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //jeśli gra się nie skończyła, nic nie rób
        if (GRoTC.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        //jesli wygrał gracz human, nic nie wyświetlaj (wyświetli się tylko napis "Wygrałeś")
        Player cp = GRoTC.CURRENT_PLAYER;
        if(cp==null||cp.type == PlayerType.human)
        {
            txt.text = "";
        }
        //jeśli wygrał gracz ai, poinformuj który gracz wygrał
        else
        {
            txt.text = "Wygrał gracz nr " + cp.playerNum;
        }
    }
}
