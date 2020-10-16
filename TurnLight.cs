using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// KLASA ODPOIWADA ZA ROZŚWIETLENIE OBSZARU PRZY AKTUALNIE WYKONUJĄCYM TURĘ GRACZU, W CELU INFORMACJI GRACZA
// CZYJA AKUTALNIE TRWA TURA 
public class TurnLight : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.back * 3;
        if (GRoTC.CURRENT_PLAYER == null)
        {
            return;
        }
        transform.position += GRoTC.CURRENT_PLAYER.handSlotDef.pos;
    }
}
