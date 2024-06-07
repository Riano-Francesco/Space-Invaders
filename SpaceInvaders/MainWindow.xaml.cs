﻿using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Windows.Themes;

namespace SpaceInvaders;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent(); // Lädt alle Komponenten
        gameTickTimer.Tick += GameTickTimer_Tick; // aufrufen des Ticks für die Spielgeschwindigkeit
        bulletTickTimer.Tick += BulletTickTimer_Tick; // aufrufen des Ticks für die Projektil-geschwindigkeit
        deathAnimationTickTimer.Tick += DeathAnimationTickTimer_Tick; // aufrufen des Ticks für die Todes-Animationsgeschwindigkeit
        LoadHighscoreList(); // aufrufen der Highscoreliste falls eine vorhanden ist
    }
    
    private const int ShipSquareSize = 40; // Variable um die größe der einzelnen Felder zu bestimmen
    private const int gameStartSpeed = 400; // Variable für die Startgeschwindigkeit des Spiels (Bewegung der Feinde)
    private const int bulletStartSpeed = 100; // Variable für Projektilgeschwindigkeit 
    private const int deathAnimationSpeed = 100; // Variable für die Geschwindigkeit der Todesanimation
    private const int gameSpeedThreshold = 40; // Variable für die schnellste Spielgeschwingkeit - nie genutzt

    private int alienSpeed = 2; // Variable für die Gegnergeschwindigkeit über modulo - tick wird nicht benötigt
    private int enemysAlive = 24; // Variable für die Anzahl an lebende Feinden zu begin
    private int currentHit = 0; // Variable für die Anzahl der getroffenen Feinde - zu begin keine

    private int currentFrameMyBullet = 1; // Variable für die Bildlaufgeschwindigkeit des eigenen Projektils
    private int currentFrameEnemyBullet = 1; // Variable für die Bildlaufgeschwindigkeit des feindlichen Projektils
    private int currentDeathFrame = 1; // Variable für die Bildlaufgeschwindigkeit der gegnerischen Todesanimation
    private int currentDeathAnimationFrame = 1; // Variable für die Bildlaufgeschwindigkeit der gegnerischen Todesanimation und die eigene
    private int currentEnemyHitAnimationFrame = 1; // Variable für die Bildlaufgeschwindigkeit der Hitanimation wenn man getroffen wird

    private Random rnd = new Random(); // Variable rnd zum erstellen von Random zahlen
    private Ship ship = new Ship(); // Variable ship vom typ Ship (Objekt)
    private List<Ship> enemys = new List<Ship>(); // Variablenliste vom Typ Ship
    private UIElement deathAnimation; // gegnerische Todesanim. vom Typ UIElement
    private UIElement myDeathAnimation; // eigene Todesanim.
    private UIElement enemyHitAnimation; // Hitanimation wenn Schiff vom gegner getroffen wird
    
    private int currentScore = 0; // variable um aktuellen score auf null zu setzen (neues spiel)
    const int MaxHighscoreListEntryCount = 5; // maximale einträge im Fenster des Highscores
    
    private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer(); // notwendig wenn man mit tick arbeitet
    private System.Windows.Threading.DispatcherTimer bulletTickTimer = new System.Windows.Threading.DispatcherTimer();
    private System.Windows.Threading.DispatcherTimer deathAnimationTickTimer = new System.Windows.Threading.DispatcherTimer();

    private Bullet bullet = new Bullet(); // variable vom Typ Bullet - gegner
    private Bullet myBullet = new Bullet(); // Variable vom Typ Bullet - eigene
    
    private SolidColorBrush enemyBulletBrush = Brushes.Red; // variable vom typ SolidColorBrush - Farbe des gegner projektils
    
    public enum ShipDirection // Liste für die möglichen Bewegungsrichtungen
    {
        Left,
        Right
    };
    
    private ShipDirection shipDirection = ShipDirection.Right; // eigene Bewegungsrichtung
    private ShipDirection enemyShipDirection = ShipDirection.Left; // gegner Bewegungsrichtung


    private void Window_ContentRendered(Object sender, EventArgs e) // Methode ruft Spielfeld auf und fügt Bild für Lebensanzeige hinzu
    {
        DrawGameArea();
        for (int i = 0; i < 3; i++) // 3 Läufe für 3 Leben
        {
            Image temp = new Image // erstellen des Bildes für Lebensanzeige
            {
                Height = 25,
                Width = 25,
                Source = (new BitmapImage(new Uri(
                    "C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\player.png", UriKind.Absolute)))
            };
            life.Children.Add(temp); // hinzufügen des Bildes zum Wrappanel
        }
    }

    private void DrawGameArea() // Methode zum zeichnen des Spielfeldes
    {
        bool doneDrawingBackground = false; // variable zum verlassen der While nachdem Hintergrund fertig gezeichnet wurde
        int nextX = 0; // Position X
        int nextY = 0; // Position Y
        int rowCounter = 0; // Zähler für die Anzahl der Reihen des Spielfeldes
        bool nextIsOdd = false; // ist die nächste zahl eine ungerade zahl 

        while (doneDrawingBackground == false) // kopfgesteuert - bedingung muss erfüllt sein um reinzukommen
        {
            Rectangle rect = new Rectangle // erstellen eines neuen Rechtecks (UI ELement)
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = nextIsOdd ? Brushes.Transparent : Brushes.Transparent // Kurzschreibweise einer if - je nach true oder false wird jeweilige brush gesetzt
            };
            
            GameArea.Children.Add(rect); // hinzufügen des Rechtecks zum Canvas
            Canvas.SetTop(rect, nextY); // positionierung des Rechtecks von oben beginnend
            Canvas.SetLeft(rect, nextX); // positionierung des Rechtecks von links beginnend

            nextIsOdd = !nextIsOdd; // hier wird es negiert - also immer das gegenteil gesetzt
            nextX += ShipSquareSize; // X wird die größe des Rechtecks addiert
            if (nextX >= GameArea.ActualWidth) // wenn nächstes X größer gleich Spielfeldbreite (rechts)
            {
                nextX = 0; 
                nextY += ShipSquareSize; // Y wird die größe des Rechtecks addiert
                rowCounter++; // Reihen werden hochgezählt pro erreichen des maximalen X werts (Spielfeldbreite abhängig)
                nextIsOdd = (rowCounter % 2 != 0); // zuweisung von 0 oder 1 zu nextIsOdd um Schachbrettmuster zu erzeugen
            }

            if (nextY >= GameArea.ActualHeight) // wenn nächstes Y größer gleich Spielfeldhöhe (unten)
            {
                doneDrawingBackground = true; // Schleifen ausstiegsbedingung wird true gesetzt
            }
        }
    }

    private void GameTickTimer_Tick(Object sender, EventArgs e) // Tick Methode Gegnerbewegung
    {
        MoveEnemy();
    }
    private void BulletTickTimer_Tick(Object sender, EventArgs e) // Tick Methode Projektilbewegung
    { 
        MoveEnemyBullet();
        if (myBullet.UiElement != null) // wenn eigenes Projektil vorhanden
        { 
            MoveMyBullet(); // Methode ausführen
        }
        if (enemyHitAnimation != null) // wenn Hit animation ausgeführt wird
        { 
            EnemyHitAnimation(); // Methode um animation fortzusetzen
        }
    }
    private void DeathAnimationTickTimer_Tick(Object sender, EventArgs e) // Tick Methode Todesanimation
    {
        if (deathAnimation != null) // wenn Todesanimation ausgeführt wird
        { 
            EnemyDeathAnimation(new Point()); // wird es mit neuer Position fortgesetzt
        }

        if (myDeathAnimation != null) // wenn eigene Todesanimation läuft
        {
            MyDeathAnimation(); // Methode um Animation fortzusetzen
        }
    }
    
    private void MoveShip()
    {
        double nextX = ship.Position.X; // Variablenzuweisung der Schiffposition
        double nextY = ship.Position.Y;
        
        switch (shipDirection) // Switch für die Berechnung der X Achse bei entsprechender Richtung
        {
            case ShipDirection.Left:
                nextX -= ShipSquareSize; // Linke Richtung wird X subtrahiert mit der einzelnen Feldgröße
                break;
            case ShipDirection.Right:
                nextX += ShipSquareSize; // Rechts wird es auf X addiert
                break;
        }

            if (nextX > - 1 && nextX < GameArea.ActualWidth ) // Bedingung, Falls X Position kleiner 0 oder größer maximale Spielfeld breite ist 
            {
                ship.Position = new Point(nextX, nextY); // dann wird neue Position dem Schiff zugewiesen - kein out of bounds
            }

        DrawShip();
    }
    
    private void DrawShip() 
    {
        if (ship.UiElement == null) // Wenn UI Element nicht existiert (= null)
        {
            ship.UiElement = new Rectangle() // Schaffe ein neues als Rechteck
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = (new ImageBrush(new BitmapImage(new Uri( // Füll es mit einem Bild optisch
                    "C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\player.png", UriKind.Absolute))))
            };
            GameArea.Children.Add(ship.UiElement); // Füge das UI Element dem Canvas hinzu
            Canvas.SetTop(ship.UiElement, ship.Position.Y); // Mit der Position von Oben gesehen
            Canvas.SetLeft(ship.UiElement, ship.Position.X); // Und von Links gesehen
        }
        else
        {
            Canvas.SetTop(ship.UiElement, ship.Position.Y); // aktualisieren der Position im Canvas 
            Canvas.SetLeft(ship.UiElement, ship.Position.X);
        }
    }
    
    public void MoveEnemy()
    {
        foreach (Ship enemy in enemys)
        {
            double nextX = enemy.Position.X; // Variablenzuweisung der Schiffposition
            double nextY = enemy.Position.Y;

            switch (enemyShipDirection)
            {
                case ShipDirection.Left: // Linke Richtung wird X subtrahiert mit der einzelnen Feldgröße
                    nextX -= ShipSquareSize;
                    break;
                case ShipDirection.Right: // Rechts wird es auf X addiert
                    nextX += ShipSquareSize;
                    break;
            }

            if (EnemyCollisionCheck()) // Bedingung um entsprechende Methode aufzurufen - falls Rückgabewert True 
            {
                break;
            }

            int temp = 0;
            
            for (int j = 0; j < 24; j++) // durchgehende Enemy liste
            {
                if (enemys[j].IsHit == false) // wenn enemy nicht getroffen 
                {
                    temp = j; // temp bekommt den ersten! enemy der noch existiert zugewiesen
                    break;
                }
            }

            int temp2 = 0;
            for (int k = 23; k > -1; k--) // durchgehende Enemy liste entgegengesetzt
            {
                if (enemys[k].IsHit == false )
                {
                    temp2 = k; // temp2 bekommt den letzten! enemy der noch existiert zugewiesen
                    break;
                }
            }

            // wenn der erste noch bestehende enemy rechts den Rand berührt mit laufrichtung rechts
            if (enemys[temp2].Position.X == 760.0 && enemyShipDirection == ShipDirection.Right) 
            {
                enemyShipDirection = ShipDirection.Left; // dann wird die richtung aller nach links geändert 
                enemy.Position = new Point(nextX - 80, nextY); // und dementsprechend eine neue position zugewiesen
            }
            // ebenfalls wenn der erste noch bestehende enemy links den Rand berührt mit laufrichtung links
            else if (enemys[temp].Position.X == 0 && enemyShipDirection == ShipDirection.Left)
            {
                if (gameTickTimer.Interval.TotalMilliseconds > 40) // wenn die Spielgeschwindigkeit größer 40 ist
                {
                    // dann wird immer bei Kontakt Rand links 40 subtrahiert und das spiel schneller gemacht
                gameTickTimer.Interval = TimeSpan.FromMilliseconds(gameTickTimer.Interval.TotalMilliseconds - 40);
                }
                
                UpdateGameStatus();
                
                enemyShipDirection = ShipDirection.Right; // wird die richtung aller nach rechts geändert
                
                for (int i = 0; i < enemys.Count; i++) // .Count = zählt wie viele Items in der liste sind
                {
                    if (enemys[i].Position.X > 1)
                    { // für alle Enemys nach dem ersten, die nicht in Spalte 1 (links) sind
                        enemys[i].Position = new Point(enemys[i].Position.X - 40, enemys[i].Position.Y + 40); // neue positionszuweisung Horizontal und Vertikal
                    }
                    else
                    { // nur die erste Spalte
                        enemys[i].Position = new Point(enemys[i].Position.X, enemys[i].Position.Y + 40); // ebenfalls, nur Vertikal 
                    }
                }
                break;
            }
            else
            {
                if (nextX > - 1 && nextX < GameArea.ActualWidth) // Bedingung, Falls X Position kleiner 0 oder größer maximale Spielfeld breite ist 
                {
                    enemy.Position = new Point(nextX, nextY); // dann wird neue Position dem Schiff zugewiesen - kein out of bounds
                }
            }
        }
        DrawEnemy();
    }

    public void DrawEnemy()
    {
        for (int i = 0; i < enemys.Count; i++)
        {
            if (enemys[i].UiElement == null)
            {
                enemys[i].UiElement = new Rectangle()
                {
                    Width = ShipSquareSize,
                    Height = ShipSquareSize,
                    Fill = (new ImageBrush(new BitmapImage(new Uri(
                        $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\Invader{(i % 8) + 1}.gif",
                        UriKind.Absolute))))
                };
                enemys[i].IsHit = false;
                GameArea.Children.Add(enemys[i].UiElement);
                Canvas.SetTop(enemys[i].UiElement, enemys[i].Position.Y);
                Canvas.SetLeft(enemys[i].UiElement, enemys[i].Position.X);
            }
            else
            {
                if (!enemys[i].IsHit)
                {
                Canvas.SetTop(enemys[i].UiElement, enemys[i].Position.Y);
                Canvas.SetLeft(enemys[i].UiElement, enemys[i].Position.X);
                
                }
                
                // hier wird die Deathanimation manipuliert und überprüft, ob sie gespielt werden soll
                if (enemys[i].IsHit && currentDeathFrame < 5)
                {
                    (enemys[i].UiElement as Rectangle).Fill = new ImageBrush(new BitmapImage(new Uri(
                        $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Death{currentDeathFrame++}.png",
                        UriKind.Absolute)));
                }

                if (enemys[i].IsHit && currentDeathFrame >= 5)
                {
                    enemys[i].Leben--;
                }
                
                if (enemys[i].Leben == 0)
                {
                    currentDeathFrame = 1;
                }
            }
        }
    }
    
    private void Window_KeyDown(Object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                    shipDirection = ShipDirection.Left;
                MoveShip();
                break;
            case Key.Right:
                    shipDirection = ShipDirection.Right;
                MoveShip();
                break;
            case Key.Up:
                DrawMyBullet();
                break;
            case Key.Space:
                StartNewGame();
                int background = (rnd.Next() % 7) + 1; // random der bei jedem Spielstart den Hintergrund verändert
                Window.Background = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\backgrounds\\universe{background}.png",
                    UriKind.Absolute)));
                break;
        }
    }
    
    private void StartNewGame()
    {
        bdrWelcomeMessage.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Collapsed;
        bdrEndOfGame.Visibility = Visibility.Collapsed;
        
        foreach (Ship enemy in enemys)
        {
            if (enemy.UiElement != null)
            {
                GameArea.Children.Remove(enemy.UiElement); // Entfernen der Enemy UI'S auf Spielfeld
            }
        }

        if (bullet.UiElement != null)
        {
            GameArea.Children.Remove(bullet.UiElement); // Entfernen der Projektil UI auf Spielfeld
            bullet.UiElement = null;
        }
        
        if (deathAnimation != null)
        {
            GameArea.Children.Remove(deathAnimation); // Entfernen der Deathanimation UI auf Spielfeld
            deathAnimation = null;
        }
        
        if (myDeathAnimation != null)
        {
            GameArea.Children.Remove(myDeathAnimation); // Entfernen meiner Deathanimation UI auf Spielfeld
            myDeathAnimation = null;
        }
        
        enemys.Clear(); // Liste leeren
        
        currentScore = 0;
        
        shipDirection = ShipDirection.Right;
        enemyShipDirection = ShipDirection.Right;
        
        ship.Position = new Point(ShipSquareSize * 9, ShipSquareSize * 18); // Schiff auf Startposition setzen
        ship.Leben = 3;

        for (int i = 1; i <= ship.Leben; i++)
        {
        life.Children[i].Visibility = Visibility.Visible; // Leben wieder einblenden
        }
        
        // Enemys darstellen und der Spalte zuweisen
        for (int i = 0; i < 8; i++)
        {
            enemys.Add(new Ship() { Leben = 1, Position = new Point(ShipSquareSize * (i + 1), ShipSquareSize * 2) });
            enemys.Add(new Ship() { Leben = 1, Position = new Point(ShipSquareSize * (i + 1), ShipSquareSize * 3) });
            enemys.Add(new Ship() { Leben = 1, Position = new Point(ShipSquareSize * (i + 1), ShipSquareSize * 4) });
        }
        
        gameTickTimer.Interval = TimeSpan.FromMilliseconds(gameStartSpeed);
        bulletTickTimer.Interval = TimeSpan.FromMilliseconds(bulletStartSpeed);
        deathAnimationTickTimer.Interval = TimeSpan.FromMilliseconds(deathAnimationSpeed);
        
        DrawShip();
        DrawEnemy();
        DrawEnemyBullet();
        
        UpdateGameStatus();
        
        gameTickTimer.IsEnabled = true;
        bulletTickTimer.IsEnabled = true;
        deathAnimationTickTimer.IsEnabled = true;
        
        // Schiff wird wieder eingeblendet
        GameArea.Children[GameArea.Children.IndexOf(ship.UiElement)].Visibility = Visibility.Visible;
    }

    private bool MyCollisionCheck()
    {
        int count = 0;
        bool hit = false;
        
        foreach (Ship enemy in enemys)
        {
            count++;
            
            // Hitabfrage meines Profektils mit Enemy der noch keinen Hit hat
            if ((enemy.Position.X == myBullet.Position.X) && (enemy.Position.Y == myBullet.Position.Y - 40) && enemy.IsHit == false)
            {
                SetHit();
                enemy.IsHit = true;
                enemysAlive--;
                hit = true;

                break;
            }
        }
        
        if (enemysAlive == 0)
        {
            EndGame();
        }
        
        if (hit)
        {
            EnemyDeathAnimation(enemys[count - 1].Position); // count - 1 = das schiff welches getroffen wurde und subtraktion da count++ einem hinzugezählt wurde den wir noch nicht hatten
            GameArea.Children.Remove(enemys[count - 1].UiElement);
            return true;
        }

        return false;
    }

    private void EnemyDeathAnimation(Point position)
    {
        if (deathAnimation == null)
        {
            deathAnimation = new Ellipse()
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Death{currentDeathAnimationFrame}.png",
                    UriKind.Absolute)))
            };
            GameArea.Children.Add(deathAnimation);
            Canvas.SetTop(deathAnimation, position.Y);
            Canvas.SetLeft(deathAnimation, position.X);
        }
        else
        {
            if (currentDeathAnimationFrame < 5)
            {
                (deathAnimation as Ellipse).Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Death{currentDeathAnimationFrame++}.png",
                    UriKind.Absolute)));
            }
            else
            {
                GameArea.Children.Remove(deathAnimation);
                deathAnimation = null;
                currentDeathAnimationFrame = 1;
            }
        }
    }
    
    private void MyDeathAnimation()
    {
        if (myDeathAnimation == null)
        {
            myDeathAnimation = new Ellipse()
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Death{currentDeathAnimationFrame}.png",
                    UriKind.Absolute)))
            };
            GameArea.Children.Add(myDeathAnimation);
            Canvas.SetTop(myDeathAnimation, ship.Position.Y);
            Canvas.SetLeft(myDeathAnimation, ship.Position.X);
        }
        else
        {
            if (currentDeathAnimationFrame < 5)
            {
                (myDeathAnimation as Ellipse).Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Death{currentDeathAnimationFrame++}.png",
                    UriKind.Absolute)));
            }
            else
            {
                currentDeathAnimationFrame = 1;
            }
        }
    }
    
    private void EnemyHitAnimation()
    {
        if (enemyHitAnimation == null)
        {
            enemyHitAnimation = new Ellipse()
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Hit{currentEnemyHitAnimationFrame}.png",
                    UriKind.Absolute)))
            };
            GameArea.Children.Add(enemyHitAnimation);
            Canvas.SetTop(enemyHitAnimation, ship.Position.Y);
            Canvas.SetLeft(enemyHitAnimation, ship.Position.X);
        }
        else
        {
            if (currentEnemyHitAnimationFrame < 5)
            {
                (enemyHitAnimation as Ellipse).Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Hit{currentEnemyHitAnimationFrame++}.png",
                    UriKind.Absolute)));
            }
            else
            {
                GameArea.Children.Remove(enemyHitAnimation);
                enemyHitAnimation = null;
                currentEnemyHitAnimationFrame = 1;
            }
        }
    }
    
    private bool EnemyCollisionCheck()
    {
        foreach (Ship enemy in enemys)
        {
            // Abfrage, ob Enemy und Schiffposition übereinstimmen
            if (enemy.Position.Y == ship.Position.Y - 40 && enemy.IsHit == false)
            {
                MyDeathAnimation();
                // Schiff ausblenden
                GameArea.Children[GameArea.Children.IndexOf(ship.UiElement)].Visibility = Visibility.Hidden;
                EndGame();
                return true;
            }
        }
            // wenn eigenes Projektil enemy trifft
        if ((ship.Position.X == bullet.Position.X) &&
            (ship.Position.Y == bullet.Position.Y + 40))
        {
            EnemyHitAnimation();
            return true;
        }
        return false;
    }
    
    private void SetHit()
    {
        currentScore++;
        GameArea.Children.Remove(myBullet.UiElement);
        UpdateGameStatus();
    }
    
    private Point GetNextEnemyBulletPosition()
    {
        int maxX = (int)((GameArea.ActualWidth - 40) / ShipSquareSize);
        int bulletX = rnd.Next(1, maxX) * ShipSquareSize;
        int bulletY = 3 * ShipSquareSize;
        
        return new Point(bulletX, bulletY);
    }

    private void DrawEnemyBullet()
    {
        if (bullet.UiElement == null)
        {
            Point bulletPosition = GetNextEnemyBulletPosition();

            bullet.UiElement = new Ellipse()
            {
                Width = ShipSquareSize / 10,
                Height = ShipSquareSize,
                Fill = enemyBulletBrush
            };
            GameArea.Children.Add(bullet.UiElement);
            bullet.Position = new Point(bulletPosition.X, bulletPosition.Y);
            Canvas.SetTop(bullet.UiElement, bulletPosition.Y);
            Canvas.SetLeft(bullet.UiElement, bulletPosition.X + 18);
        }
        else
        {
            Canvas.SetTop(bullet.UiElement, bullet.Position.Y);
            Canvas.SetLeft(bullet.UiElement, bullet.Position.X + 18);
        }
    }

    private void MoveEnemyBullet()
    {
        double nextX = bullet.Position.X;
        double nextY = bullet.Position.Y;

        if (EnemyCollisionCheck())
        { // Children sind Elemente innerhalb dieses Wrappanels
            life.Children[ship.Leben].Visibility = Visibility.Hidden;
            ship.Leben -= 1;
            
            bullet.Position = GetNextEnemyBulletPosition();

            if (ship.Leben == 0)
            {
                MyDeathAnimation();
                GameArea.Children[GameArea.Children.IndexOf(ship.UiElement)].Visibility = Visibility.Hidden;
                GameArea.Children.Remove(enemyHitAnimation);
                EndGame();
            }
        }
        else
        { // wenn kugel spielfeldrand trifft wird sie neu gesetzt beim erneuten abschuss
            if (bullet.Position.Y == GameArea.ActualHeight)
            {
                bullet.Position = GetNextEnemyBulletPosition();
            }
            else
            {
                bullet.Position = new Point(nextX, nextY + 40);
            }
        }
        DrawEnemyBullet();
    }
    
    private void DrawMyBullet()
    {
        if (myBullet.UiElement == null)
        {
            currentFrameMyBullet = 1;
            myBullet.UiElement = new Ellipse()
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Bullet{currentFrameMyBullet}.png",
                    UriKind.Absolute)))
            };
            GameArea.Children.Add(myBullet.UiElement);
            myBullet.Position = new Point(ship.Position.X, ship.Position.Y);
            
            Canvas.SetTop(myBullet.UiElement, ship.Position.Y - 40);
            Canvas.SetLeft(myBullet.UiElement, ship.Position.X);
        }
        else
        {
            Canvas.SetTop(myBullet.UiElement, myBullet.Position.Y);
            Canvas.SetLeft(myBullet.UiElement, myBullet.Position.X);
            if (currentFrameMyBullet < 6)
            {
                (myBullet.UiElement as Ellipse).Fill = new ImageBrush(new BitmapImage(new Uri(
                    $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\Animations\\Bullet{currentFrameMyBullet++}.png",
                    UriKind.Absolute)));
            }
        }
    }

    private void MoveMyBullet()
    {
        double nextX = myBullet.Position.X;
        double nextY = myBullet.Position.Y;

        if (MyCollisionCheck())
        {
            myBullet.UiElement = null;
        }
        else
        {
            if (myBullet.Position.Y == 0)
            {
                GameArea.Children.Remove(myBullet.UiElement);
                myBullet.UiElement = null;
            }
            else
            {
                myBullet.Position = new Point(nextX, nextY - 40);
            }
        }

        if (myBullet.UiElement != null)
        {
        DrawMyBullet();
        }
    }
    
    private void UpdateGameStatus()
    {
        this.tbStatusScore.Text = currentScore.ToString();
        this.tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
    }
    
    private void EndGame()
    {
        bool isNewHighscore = false;
        if (currentScore > 0)
        {
            int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
            if ((currentScore > lowestHighscore) || (this.HighscoreList.Count < MaxHighscoreListEntryCount))
            {
                bdrNewHighscore.Visibility = Visibility.Visible;
                txtPlayerName.Focus();
                isNewHighscore = true;
            }
        }

        if (!isNewHighscore)
        {
            tbFinalScore.Text = currentScore.ToString();
            bdrEndOfGame.Visibility = Visibility.Visible;
        }

        gameTickTimer.IsEnabled = false;
        bulletTickTimer.IsEnabled = false;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
    {
        bdrWelcomeMessage.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Visible;
    }

    private void LoadHighscoreList()
    {
        if (File.Exists("space_highscorelist.xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<SpaceHighscore>));
            using (Stream reader = new FileStream("space_highscorelist.xml", FileMode.Open))
            {
                List<SpaceHighscore> tempList = (List<SpaceHighscore>)serializer.Deserialize(reader);
                this.HighscoreList.Clear();
                foreach (var item in tempList.OrderByDescending(x => x.Score))
                    this.HighscoreList.Add(item);
            }
        }
    }

    private void SaveHighscoreList()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SpaceHighscore>));
        using (Stream writer = new FileStream("space_highscorelist.xml", FileMode.Create))
        {
            serializer.Serialize(writer, this.HighscoreList);
        }
    }

    private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
    {
        int newIndex = 0;
        // Where should the new entry be inserted?
        if ((this.HighscoreList.Count > 0) && (currentScore < this.HighscoreList.Max(x => x.Score)))
        {
            SpaceHighscore justAbove =
                this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
            if (justAbove != null)
                newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
        }

        // Create & insert the new entry
        this.HighscoreList.Insert(newIndex, new SpaceHighscore()
        {
            PlayerName = txtPlayerName.Text,
            Score = currentScore
        });
        // Make sure that the amount of entries does not exceed the maximum
        while (this.HighscoreList.Count > MaxHighscoreListEntryCount)
            this.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);

        SaveHighscoreList();

        bdrNewHighscore.Visibility = Visibility.Collapsed;
        bdrHighscoreList.Visibility = Visibility.Visible;
    }

    public ObservableCollection<SpaceHighscore> HighscoreList { get; set; } =
        new ObservableCollection<SpaceHighscore>();
}