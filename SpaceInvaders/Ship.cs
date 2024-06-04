using System.Windows;
using System.Windows.Media;

namespace SpaceInvaders;

public class Ship // Initialisierung des Objekts Ship
{
    public UIElement UiElement { get; set; } // Objekt vom Typ UIElement
    public Point Position { get; set; } // Objekt vom Typ Point
    
    public bool IsHit { get; set; } // Objekt vom Typ bool

    public int Leben { get; set; } // Objekt vom Typ int
}

public class SpaceHighscore // Initialisierung des Objekts SpaceHighscore
{
    public string PlayerName { get; set; } // Objekt vom Typ string

    public int Score { get; set; } // Objekt vom Typ int
}
