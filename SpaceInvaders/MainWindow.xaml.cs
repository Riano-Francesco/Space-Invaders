using System.Text;
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
        InitializeComponent();
        gameTickTimer.Tick += GameTickTimer_Tick;
        bulletTickTimer.Tick += BulletTickTimer_Tick;

        LoadHighscoreList();
    }
    
    private const int ShipSquareSize = 40;
    private const int gameStartSpeed = 400;
    private const int bulletStartSpeed = 100;
    private const int gameSpeedThreshold = 40;
    private int alienSpeed = 2;

    private int enemysAlive = 8;

    private Random rnd = new Random();
    private Ship ship = new Ship();
    private List<Ship> enemys = new List<Ship>();
    
    private int currentScore = 0;
    const int MaxHighscoreListEntryCount = 5;
    
    private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
    private System.Windows.Threading.DispatcherTimer bulletTickTimer = new System.Windows.Threading.DispatcherTimer();


    private Bullet bullet = new Bullet();
    private Bullet myBullet = new Bullet();
    
    private SolidColorBrush enemyBulletBrush = Brushes.Red;
    private SolidColorBrush myBulletBrush = Brushes.Green;
    
    public enum ShipDirection
    {
        Left,
        Right
    };
    
    private ShipDirection shipDirection = ShipDirection.Right;
    private ShipDirection enemyShipDirection = ShipDirection.Left;


    private void Window_ContentRendered(Object sender, EventArgs e)
    {
        DrawGameArea();
        for (int i = 0; i < 3; i++)
        {
            Image temp = new Image
            {
                Height = 25,
                Width = 25,
                Source = (new BitmapImage(new Uri(
                    "C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\player.png", UriKind.Absolute)))
            };
            life.Children.Add(temp);
        }
    }

    private void DrawGameArea()
    {
        bool doneDrawingBackground = false;
        int nextX = 0;
        int nextY = 0;
        int rowCounter = 0;
        bool nextIsOdd = false;

        while (doneDrawingBackground == false)
        {
            Rectangle rect = new Rectangle
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = nextIsOdd ? Brushes.Transparent : Brushes.Transparent
            };
            
            GameArea.Children.Add(rect);
            Canvas.SetTop(rect, nextY);
            Canvas.SetLeft(rect, nextX);

            nextIsOdd = !nextIsOdd;
            nextX += ShipSquareSize;
            if (nextX >= GameArea.ActualWidth)
            {
                nextX = 0;
                nextY += ShipSquareSize;
                rowCounter++;
                nextIsOdd = (rowCounter % 2 != 0);
            }

            if (nextY >= GameArea.ActualHeight)
            {
                doneDrawingBackground = true;
            }
        }
    }

    private void GameTickTimer_Tick(Object sender, EventArgs e)
    { 
        MoveEnemy();
    }
    private void BulletTickTimer_Tick(Object sender, EventArgs e)
    { 
        MoveEnemyBullet();
        if (myBullet.UiElement != null)
        { 
            MoveMyBullet();
        }
    }
    
    private void MoveShip()
    {
        double nextX = ship.Position.X;
        double nextY = ship.Position.Y;
        
        switch (shipDirection)
        {
            case ShipDirection.Left:
                nextX -= ShipSquareSize;
                break;
            case ShipDirection.Right:
                nextX += ShipSquareSize;
                break;
        }

            if (nextX > - 1 && nextX < GameArea.ActualWidth )
            {
                ship.Position = new Point(nextX, nextY);
            }

        DrawShip();
    }
    
    private void DrawShip()
    {
        if (ship.UiElement == null)
        {
            ship.UiElement = new Rectangle()
            {
                Width = ShipSquareSize,
                Height = ShipSquareSize,
                Fill = (new ImageBrush(new BitmapImage(new Uri(
                    "C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\player.png", UriKind.Absolute))))
            };
            GameArea.Children.Add(ship.UiElement);
            Canvas.SetTop(ship.UiElement, ship.Position.Y);
            Canvas.SetLeft(ship.UiElement, ship.Position.X);
        }
        else
        {
            Canvas.SetTop(ship.UiElement, ship.Position.Y);
            Canvas.SetLeft(ship.UiElement, ship.Position.X);
        }
    }
    
    public void MoveEnemy()
    {
        foreach (Ship enemy in enemys)
        {
            double nextX = enemy.Position.X;
            double nextY = enemy.Position.Y;

            switch (enemyShipDirection)
            {
                case ShipDirection.Left:
                    nextX -= ShipSquareSize;
                    break;
                case ShipDirection.Right:
                    nextX += ShipSquareSize;
                    break;
            }

            if (EnemyCollisionCheck())
            {
                break;
            }

            int temp = 0;
            
            for (int j = 0; j < 8; j++)
            {
                if (enemys[j].IsHit == false)
                {
                    temp = j;
                    break;
                }
            }

            int temp2 = 0;
            for (int k = 7; k > 0; k--)
            {
                if (enemys[k].IsHit == false)
                {
                    temp2 = k;
                    break;
                }
            }
            if (enemys[temp2].Position.X == 760.0 && enemyShipDirection == ShipDirection.Right)
            {
                enemyShipDirection = ShipDirection.Left;
                enemy.Position = new Point(nextX - 80, nextY);
            }
            else if (enemys[temp].Position.X == 0 && enemyShipDirection == ShipDirection.Left)
            {
                enemyShipDirection = ShipDirection.Right;
                
                for (int i = 0; i < enemys.Count; i++)
                {
                    enemys[i].Position = new Point(nextX -(40 * temp) - 40 + (i * 40), nextY + 40);
                }
                break;
            }
            else
            {
                if (nextX > - 1 && nextX < GameArea.ActualWidth)
                {
                    enemy.Position = new Point(nextX, nextY);
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
                        $"C:\\Users\\csl\\RiderProjects\\Space\\SpaceInvaders\\images\\Invader{i+1}.gif",
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
                GameArea.Children.Remove(enemy.UiElement);
            }
        }

        if (bullet.UiElement != null)
        {
            GameArea.Children.Remove(bullet.UiElement);
            bullet.UiElement = null;
        }
        
        enemys.Clear();
        
        currentScore = 0;
        
        shipDirection = ShipDirection.Right;
        enemyShipDirection = ShipDirection.Right;
        
        ship.Position = new Point(ShipSquareSize * 9, ShipSquareSize * 18);
        ship.Leben = 3;

        for (int i = 1; i <= ship.Leben; i++)
        {
        life.Children[i].Visibility = Visibility.Visible;
        }
        
        for (int i = 0; i < 8; i++)
        {
            enemys.Add(new Ship() { Position = new Point(ShipSquareSize * (i + 1), ShipSquareSize * 2) });
        }
        
        gameTickTimer.Interval = TimeSpan.FromMilliseconds(gameStartSpeed);
        bulletTickTimer.Interval = TimeSpan.FromMilliseconds(bulletStartSpeed);

        
        DrawShip();
        DrawEnemy();
        DrawEnemyBullet();
        
        UpdateGameStatus();
        
        gameTickTimer.IsEnabled = true;
        bulletTickTimer.IsEnabled = true;
    }

    private bool MyCollisionCheck()
    {
        int count = 0;
        bool hit = false;
        
        foreach (Ship enemy in enemys)
        {
                count++;
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
            GameArea.Children.Remove(enemys[count - 1].UiElement);
            return true;
        }

        return false;
    }
    
    private bool EnemyCollisionCheck()
    {

        if (enemys[0].Position.Y == 680)
        {
            EndGame();
            return true;
        }

        if ((ship.Position.X == bullet.Position.X) &&
            (ship.Position.Y == bullet.Position.Y + 40))
        {
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
        int maxY = (int)((GameArea.ActualHeight - 40) / ShipSquareSize);
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
        {
            life.Children[ship.Leben].Visibility = Visibility.Hidden;
            ship.Leben -= 1;
            
            bullet.Position = GetNextEnemyBulletPosition();

            if (ship.Leben == 0)
            {
                EndGame();
            }
        }
        else
        {
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
            myBullet.UiElement = new Ellipse()
            {
                Width = ShipSquareSize / 10,
                Height = ShipSquareSize,
                Fill = myBulletBrush
            };
            GameArea.Children.Add(myBullet.UiElement);
            myBullet.Position = new Point(ship.Position.X, ship.Position.Y);
            
            Canvas.SetTop(myBullet.UiElement, ship.Position.Y - 40);
            Canvas.SetLeft(myBullet.UiElement, ship.Position.X + 18);
        }
        else
        {
            Canvas.SetTop(myBullet.UiElement, myBullet.Position.Y);
            Canvas.SetLeft(myBullet.UiElement, myBullet.Position.X + 18);
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
