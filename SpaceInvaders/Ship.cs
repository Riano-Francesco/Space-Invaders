using System.Windows;
using System.Windows.Media;

namespace SpaceInvaders;

public class Ship
{
    public UIElement UiElement { get; set; }
    public Point Position { get; set; }
    
    public bool IsHit { get; set; }
}

public class SpaceHighscore
{
    public string PlayerName { get; set; }

    public int Score { get; set; }
}
