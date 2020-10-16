using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//możliwe stany karty w grze GetRidOfTheCards
public enum CBstate
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}
//klasa przechowująca informacje o kartach charakterystyczne dla gry GetRidOfTheCards
public class CardGR : Card
{
    public static float MOVE_DURATIONN = 0.5f;
    public static string MOVE_EASING = Easing.InOut; //typ modyfikacji ruchu karty
    public static float CARD_HEIGHT = 3.5f;
    public static float CARD_WIDTH = 2f;

    [Header("Definiowanie dynamiczne CardMakao")]
    public CBstate state = CBstate.drawpile;
    //informacje do ruchu i obrotu karty
    public List<Vector3> bezierPts; //punkty na podstawie których odbywa się ruch kart
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;
    public int eventualSortOrder;
    public string eventualSortLayer;
    public bool humanCard = false;

    //gdy karta zakończy ruch, zasygnalizuje ten fakt poprzez użycie metody SendMessage()
    public GameObject reportFinishTo = null;

    [System.NonSerialized]
    public Player callbackPlayer = null;

    //metoda wywołująca interpolowany ruch karty do nowego położenia oraz obrót
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition);
        bezierPts.Add(ePos);

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(eRot);

        if (timeStart == 0)
        {
            timeStart = Time.time;
        }

        timeDuration = MOVE_DURATIONN;

        state = CBstate.to;
    }
    //metoda MoveTo nie wymaga przeciążenia parametrem związanym z obrotem
    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            //wszystkie stany karty z przedrostkiem 'to' są obsługiwane przez tę samą funkcję 
            case CBstate.toHand:
            case CBstate.toTarget:
            case CBstate.toDrawpile:
            case CBstate.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);

                // jeśli u <0, karta nie porusza się ( pozostaje w aktualnym połozeniu)
                if (u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                }
                //jeśli u >= 1, ruch karty zakończył się
                else if (u >= 1)
                {
                    uC = 1;
                    //przypisz karcie odpowiedni stan
                    if (state == CBstate.toHand) state = CBstate.hand;
                    if (state == CBstate.toTarget) state = CBstate.target;
                    if (state == CBstate.toDrawpile) state = CBstate.drawpile;
                    if (state == CBstate.to) state = CBstate.idle;
                    //przypisz karcie końcowe położenie
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierPts.Count - 1];

                    timeStart = 0;

                    if(reportFinishTo != null)
                    {
                        //przesłanie informacji o zakończeniu ruchu
                        reportFinishTo.SendMessage("CBCallback", this);
                        reportFinishTo = null;
                    }
                    else if (callbackPlayer != null)
                    {
                        //wywołaj zmianę tury, jeśli istnieje gracz który może wywołać funkcję zwrotną
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                    else
                    {

                    }
                }
                //jesli ruch nadal trwa 
                else
                {
                    //przemieść kartę z położenia początkowego do położenia docelowego oraz wykonaj obrót
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;

                    if (u > 0.5f)
                    {
                        SpriteRenderer sRend = spriteRenderers[0];
                        if (sRend.sortingOrder != eventualSortOrder)
                        {
                            SetSortOrder(eventualSortOrder);
                        }
                        if (sRend.sortingLayerName != eventualSortLayer)
                        {
                            SetSortingLayerName(eventualSortLayer);
                        }
                    }
                }
                break;
        }
    }
    public override void OnMouseUpAsButton()
    {
        if (this.humanCard || this.state == CBstate.drawpile)
        {
            GRoTC.S.CardClicked(this);
            base.OnMouseUpAsButton();
        }
    }
}
