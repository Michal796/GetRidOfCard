using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//etapy tur gry
public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}
// klasa odpowiada za logikę gry oraz przemieszczanie poszczególnych kart
public class GRoTC : MonoBehaviour
{
    static public GRoTC S; //singleton
    static public Player CURRENT_PLAYER; //gracz aktualnie wykonujący ruch

    [Header("definiowanie ręczne")]
    public TextAsset deckXML; //zewnętrzne pliki do odczytania rozmiezczenia elementów graficznych kart oraz rozmieszczenie slotu poszczególnych kart
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero; // położenie rodzica wszystkich kart
    public float handFanDegrees = 10f; 
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;

    [Header("Definiowanie dynamiczne")]
    public Deck deck;
    public List<CardGR> drawPile; //karty na stosie kart do pobrania
    public List<CardGR> discardPile; // karty na stosie kart odrzuconych
    public List<Player> players; //lista graczy
    public CardGR targetCard; //karta-cel
    public TurnPhase phase = TurnPhase.idle;

    private LayoutGR layout;
    private Transform layoutAnchor; //rodzic wszystkich kart

    void Awake()
    {
        S = this;
    }
    // Start is called before the first frame update
    void Start()
        //rozdanie kart 
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);
        layout = GetComponent<LayoutGR>();
        layout.ReadLayout(layoutXML.text);
        drawPile = UpgrageCardsList(deck.cards);

        LayoutGame();
    }

    //zamiana obiektów Card na CardGR
    List<CardGR> UpgrageCardsList(List<Card> lCD)
    {
        List<CardGR> lCB = new List<CardGR>();
        foreach(Card tCD in lCD)
        {
            lCB.Add(tCD as CardGR);
        }
        return (lCB);
    }
    //umieszczenie kart na stosie kart do pobrania
    public void ArrangeDrawPile()
    {
        CardGR tCB;
        for (int i = 0; i<drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;
            tCB.FaceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB.state = CBstate.drawpile;
        }
    }
    //wyciągnięcie kolejnej karty ze stosu kart do pobierania
    public CardGR Draw()
    {
        CardGR cd = drawPile[0];
        //jeśli karty na stosie do pobierania się skończyły, przetasuj karty na stosie kart odrzuconych i przemieść je na stos kart do pobierania
        if (drawPile.Count == 0)
        {
            int ndx;
            while (discardPile.Count > 0)
            {
                ndx = Random.Range(0, discardPile.Count);
                drawPile.Add(discardPile[ndx]);
                discardPile.RemoveAt(ndx);
            }
            //przemieszczenie kart na stos do pobierania
            ArrangeDrawPile();
            float t = Time.time;
            foreach (CardGR tCB in drawPile)
            {
                tCB.transform.localPosition = layout.discardPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.02f;
                tCB.state = CBstate.toDrawpile;
                tCB.eventualSortLayer = "0";
            }
        }
        drawPile.RemoveAt(0);
        return (cd);
    }
    //rozpocznij rozdanie
    void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        ArrangeDrawPile();

        //każdemu graczowi przyporządkuj slot na karty
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = tSD;
            players.Add(pl);
            pl.playerNum = tSD.player;
        }
        players[0].type = PlayerType.human;
        CardGR tCB;

        //rozdanie 7 kart każdemu graczowi
        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tCB = Draw();
                //przesunięcie czasu timeStart każdej karty sprawia, że są one pobierane jedna po drugiej
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
                //karty gracza human
                if (players[(j+1)%4].type == PlayerType.human)
                {
                    tCB.humanCard = true;
                }
                //rozdanie pojedynczej karty każdemu graczowi po kolei
                players[(j + 1) % 4].AddCard(tCB);
            }
        }
        //wyznaczenie celu
        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }
    public void DrawFirstTarget()
    {
        CardGR tCB = MoveToTarget(Draw());
        tCB.reportFinishTo = this.gameObject;
    }
    //funkcja wykonywana po otrzymaniu informacji o rozdaniu ostatniej karty
    public void CBCallback(CardGR cb)
    {
        StartGame();
    }
    public void StartGame()
    {
        PassTurn(1);
    }
    public void PassTurn(int num = -1)
    {
        //jesli nie przekazano parametru, wybierz gracza
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            //najpierw sprawdz czy nastąpił koniec gry
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            if (CheckGameOver())
            {
                return;
            }
        }
        //przekaż turę
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;
        CURRENT_PLAYER.TakeTurn();

    }

    //sprawdzenie czy nastąpił koniec gry
    public bool CheckGameOver()
    {
        //jeśli stos kart do pobierania jest pusty, należy ponownie go wypełnić
        if(drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardGR cb in discardPile)
            {
                cards.Add(cb);
            }
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgrageCardsList(cards);
            ArrangeDrawPile();
        }
        //jeśli aktualny gracz pozbył się wszystkich kart, wygrał
        if(CURRENT_PLAYER.hand.Count == 0)
        {
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);
            return (true);
        }
        return (false);
    }
    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__Makao_Scene_0");
    }

    //sprawdzenie, czy możliwe jest wykonanie ruchu
    public bool ValidPlay(CardGR cb)
    {
        if (cb.rank == targetCard.rank) return (true);

        if (cb.suit == targetCard.suit)
        {
            return (true);
        }
        return (false);
    }
    //przemieszczenie wybranej karty na cel
    public CardGR MoveToTarget(CardGR tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBstate.toTarget;
        tCB.FaceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if(targetCard != null)
        {
            MoveToDiscard(targetCard);
        }
        targetCard = tCB;
        return (tCB);
    }
    //przemieszczenie wybranej karty na stos kart odrzuconych
    public CardGR MoveToDiscard(CardGR tCB)
    {
        tCB.state = CBstate.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return (tCB);
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void CardClicked (CardGR tCB)
    {
        //nie można kliknąć karty, gdy turę wykonuje komputer
        if (CURRENT_PLAYER.type != PlayerType.human) return;
        if (phase == TurnPhase.waiting) return;

        //w zależności jaka karta została kliknięta
        switch (tCB.state)
        {
            case CBstate.drawpile:
                CardGR cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                cb.humanCard = true;
                phase = TurnPhase.waiting;
                break;
            case CBstate.hand:
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    phase = TurnPhase.waiting;

                }
                else
                {
                }
                break;
        }
    }
}
