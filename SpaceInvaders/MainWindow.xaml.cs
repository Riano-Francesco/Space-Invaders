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
        LoadHighscoreList();
    }
    
    private const int ShipSquareSize = 40;
    private const int gameStartSpeed = 400;
    private const int gameSpeedThreshold = 40;

    private Random rnd = new Random();
    private Ship ship = new Ship();
    private List<Ship> enemys = new List<Ship>();
    
    private int currentScore = 0;
    const int MaxHighscoreListEntryCount = 5;
    
    private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();

    private Bullet bullet = new Bullet(); 
    
    private SolidColorBrush enemyBulletBrush = Brushes.Red;
    private SolidColorBrush myBulletBrush = Brushes.Green;
    
    public enum ShipDirection
    {
        Left,
        Right
    };
    
    private ShipDirection shipDirection = ShipDirection.Right;
    private ShipDirection enemyShipDirection = ShipDirection.Right;

    
    private void Window_ContentRendered(Object sender, EventArgs e)
    {
        DrawGameArea();
        //StartNewGame();
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
                Fill = nextIsOdd ? Brushes.Black : Brushes.Black
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

        // if (DoCollisionCheck())
        // {
            if (nextX > - 1 && nextX < GameArea.ActualWidth )
            {
                ship.Position = new Point(nextX, nextY);
            }
        // }

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

    public void DrawEnemy()
    {
        for (int i = 0; i < 8; i++)
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
                GameArea.Children.Add(enemys[i].UiElement);
                Canvas.SetTop(enemys[i].UiElement, enemys[i].Position.Y);
                Canvas.SetLeft(enemys[i].UiElement, enemys[i].Position.X);
            }
            else
            {
                Canvas.SetTop(enemys[i].UiElement, enemys[i].Position.Y);
                Canvas.SetLeft(enemys[i].UiElement, enemys[i].Position.X);
            }
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

            // if (DoCollisionCheck())
            // {
            if (enemys[7].Position.X != 760.0 )
            {
                if (nextX > -1 && nextX < GameArea.ActualWidth)
                {
                    enemy.Position = new Point(nextX, nextY);
                }
            }
            else
            {
                enemy.Position = new Point(nextX - 40, nextY);
                enemyShipDirection = ShipDirection.Left;
            }
            // }
        }
        DrawEnemy();
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
        
        enemys.Clear();
        
        currentScore = 0;
        
        shipDirection = ShipDirection.Right;
        enemyShipDirection = ShipDirection.Right;
        
        ship.Position = new Point(ShipSquareSize * 9, ShipSquareSize * 19);
        
        for (int i = 0; i < 8; i++)
        {
            enemys.Add(new Ship() { Position = new Point(ShipSquareSize * (i + 1), ShipSquareSize * 5) });
        }
        
        gameTickTimer.Interval = TimeSpan.FromMilliseconds(gameStartSpeed);
        
        DrawShip();
        DrawEnemy();
        
        UpdateGameStatus();
        
        gameTickTimer.IsEnabled = true;
    }
    
    private bool DoCollisionCheck()
    {
        // if ((ship.Position.X == Canvas.GetLeft(bullet.UiElement)) && (ship.Position.Y == Canvas.GetTop(bullet.UiElement)))
        // {
        //     getHit();
        //     return false;
        // }

        if ((ship.Position.Y < 20) || (ship.Position.Y >= GameArea.ActualHeight - 20) ||
            (ship.Position.X < 20) || (ship.Position.X >= GameArea.ActualWidth - 20))
        {
            return true;
        }
        
            if ((ship.Position.X == bullet.Position.X) &&
                (ship.Position.Y == bullet.Position.Y))
            {
                EndGame();
            }
        return false;
    }
    
    private void getHit()
    {
        currentScore++;
        GameArea.Children.Remove(bullet.UiElement);
        DrawEnemyBullet();
        UpdateGameStatus();
    }
    
    private Point GetNextEnemyBulletPosition()
    {
        int maxX = (int)((GameArea.ActualWidth - 20) / ShipSquareSize);
        int maxY = (int)((GameArea.ActualHeight - 20) / ShipSquareSize);
        int bulletX = rnd.Next(1, maxX) * ShipSquareSize;
        int bulletY = 2 * ShipSquareSize;
        
        return new Point(bulletX, bulletY);
    }

    private void DrawEnemyBullet()
    {
        Point bulletPosition = GetNextEnemyBulletPosition();
        
        bullet.UiElement = new Ellipse()
        {
            Width = ShipSquareSize,
            Height = ShipSquareSize,
            Fill = enemyBulletBrush
        };
        
        GameArea.Children.Add(bullet.UiElement);
        Canvas.SetTop(bullet.UiElement, bulletPosition.Y);
        Canvas.SetLeft(bullet.UiElement, bulletPosition.X);
    }
    
    // private void DrawMyBullet()
    // {
    //     Point bulletPosition = GetNextBulletPosition();
    //     
    //     bullet.UiElement = new Ellipse()
    //     {
    //         Width = ShipSquareSize,
    //         Height = ShipSquareSize,
    //         Fill = myBulletBrush
    //     };
    //     
    //     GameArea.Children.Add(snakeFood);
    //     Canvas.SetTop(snakeFood, foodPosition.Y);
    //     Canvas.SetLeft(snakeFood, foodPosition.X);
    // }
    
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
        MessageBox.Show("Ooops, you died! \n\n To start a new game, just press the Spacebar!", "SpaceInvaders");
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
