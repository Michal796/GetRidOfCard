using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//TYP GRACZA
public enum PlayerType
{
    human,
    ai
}
//KLASA REPREZENTUJĄCA KAŻDEGO GRACZA W GRZE 
[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardGR> hand; //karty w ręce gracza

    //dodanie karty
        public CardGR AddCard(CardGR eCB)
        {
            if (hand == null) hand = new List<CardGR>();

            hand.Add(eCB);
            if (type == PlayerType.human)
        {
            CardGR[] cards = hand.ToArray();

            //wywołanie zapytania językowego LINQ w celu posortowania kart względem pola "rank"
            //karty w ręce gracza są posortowane 
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            hand = new List<CardGR>(cards);
        }

        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSlotDef.layerName;
            FanHand();
            return (eCB);
        }
    public CardGR RemoveCard(CardGR cb)
    {
        if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return (cb);
    }
    // funkcja rozkładająca karty w ręce gracza, aby utworzyły łuk
    public void FanHand()
    {
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += GRoTC.S.handFanDegrees * (hand.Count - 1) / 2;
        }
        //lokalne zmienne pomocnicze
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        // pierwsza karta obracana jest o kąt startRot
        for (int i=0; i<hand.Count; i++)
        {
            //wartość obrotu wokól osi Z
            rot = startRot - GRoTC.S.handFanDegrees * i;
            //obrót danej karty
            rotQ = Quaternion.Euler(0, 0, rot);
            pos = Vector3.up * CardGR.CARD_HEIGHT / 2f;
            //obrócenie wartości położenia karty
            pos = rotQ * pos;

            pos += handSlotDef.pos; //dodanie domyślnego położenia zestawu kart
            pos.z = -0.5f * i;//przesunięcie kart względem osi Z, w celu uniknięcia kolizji komponentów BoxCollider kart

            if (GRoTC.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            //przesuń kartę do ustalonego położenia i obróć ją 
            hand[i].MoveTo(pos, rotQ);
            hand[i].state = CBstate.toHand;
            //tylko karty gracza są odkryte
            hand[i].FaceUp = (type == PlayerType.human);
            //każda kolejna karta ma większą wartość SortingOrder aby była widoczna
            hand[i].eventualSortOrder = i * 4;
        }
    }

    //sztuczna inteligencja gracza ai
    public void TakeTurn()
    {
        if (type == PlayerType.human) return;

        GRoTC.S.phase = TurnPhase.waiting;
        CardGR cb;
        //wyszukanie możliwych do zagrania kart gracza ai
        List<CardGR> validCards = new List<CardGR>();
        foreach (CardGR tCB in hand)
        {
            if (GRoTC.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }
        //jeśli komputer nie może wykonać ruchu, pobiera kartę ze stosu kart do pobrania
        if (validCards.Count == 0)
        {
            cb = AddCard(GRoTC.S.Draw());
            cb.callbackPlayer = this;
            return;
        }
        //komputer zagra losową kartę 
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        GRoTC.S.MoveToTarget(cb);
        //informacja o zakończeniu ruchu karty
        cb.callbackPlayer = this;
    }
    //zmień turę, gdy karta prześlę informację o zakończeniu ruchu
    public void CBCallback(CardGR tCB)
    {
        //Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        GRoTC.S.PassTurn();
    }
        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
