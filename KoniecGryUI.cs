using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//klasa odpowiada za wywołanie interfejsu końca gry
public class KoniecGryUI : MonoBehaviour
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
        if(GRoTC.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        // wykonywane tylko gdy gra się skończyła
        if (GRoTC.CURRENT_PLAYER == null) return;
        if (GRoTC.CURRENT_PLAYER.type == PlayerType.human)
        {
            txt.text = "Wygrywasz";
        }
        else
        {
            txt.text = "Przegrywasz";
        }
    }
}
