# GetRidOfCard
Domyślna wartość rozdzielczości dla gry GetRidOfCard wynosi 1024x768.

Do utworzenia talii kart wykorzystano pliki graficzne z publicznie dostępnego produktu Vectorized Playing Cards 1.3, który został stworzony przez Chrisa Aguilara (https://sourceforge.net/projects/vector-cards/). Program odczytuje rozmieszczenie kart na stole oraz rozmieszczenie elementów graficznych na każdej karcie z zewnętrznych plików tekstowych, udostępnionych przez autora książki "Projektowanie gier przy użyciu środowiska Unity i języka C#" J. G. Bonda. Autor udostępnił również klasy Utils i PT_XMLReader jako narzędzia do opracowania projektu.

Założenia gry: W tej grze gracz mierzy się z 3 innymi graczami, których ruchami zarządza skrypt. Gracze wykonują ruchy turowo. Celem gry jest pozbycie się wszystkich kart z ręki jako pierwszy. Zagrać daną kartę z ręki można tylko wtedy, gdy ma ona taką samą rangę lub kolor, jak karta aktualnie będąca celem. Przykładowo, na kartę "3 serce" zagrać można dowolną kartę o kolorze serce, lub kartę z numerem 3 o dowolnym kolorze. Jeśli gracz nie może zagrać żadnej z kart posiadanych w ręce, wyciaga kartę ze stosu kart do pobrania.

Sterowanie: sterowanie w grze odbywa się wyłącznie przy użyciu myszki. Aby wybrać kartę z ręki lub stosu kart do pobrania, należy na nią kliknąć

Skrypty:
- Card - klasa przechowuje informację o każdej karcie w grze, oraz zarządza warstwami obiektów graficznych komponentu SpriteRenderer.
- CardGR - jest to klasa dziedzicząca po klasie Card, przechowująca wszystkie informacje charakterystyczne dla gry GetRidOfCard.
- Deck - odczytuje dane z zewnętrznego pliku, i na ich podstawie tworzy talię kart.
- GRoTC - główna klasa gry, odpowiada za jej logikę oraz przemieszczanie kart w poszczególne miejsca.
- KoniecGryUI - klasa odpowiedzialna za wywołanie interfejsu końca gry, po jej zakończeniu.
- LayoutGR - klasa odczytuje położenie poszczególnych slotów kart z zewnętrznego pliku.
- Player - klasa odpowiada za informacje o każdym graczu, oraz wykonywanie ruchów za komputer.
- TurnLight - klasa odpowiada za rozświetlenie obszaru wokół kart gracza, który aktualnie wykonuje ruch (w celu zwiększenia czytelności gry).
- Wynik - klasa odpowiada za wyświetlenie informacji który gracz wygrał rozgrywkę.
- PT_XMLReader, Utils - klasy udostępnione przez przez autora ww. ksiązki jako narzędzia do wykonania projektu. Klasę PT_XMLReader wykorzystano do odczytania danych o położeniu kart, oraz elementów graficznych każdej karty z zewnętrznego pliku, napisanego w języku xml. Klasę Utils wykorzystano do wykonania ruchu kart, który odbywa się w oparciu o krzywą Beziera.
