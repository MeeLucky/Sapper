using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;

namespace Saper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int h = 10;
        static int w = 10;
        static int bomb = h * w / 10;
        static int itemCount = h*w - bomb;
        static int[,] field = new int[h,w];
        static StackPanel gameField;
        static TextBlock bombLeft;
        static TextBlock timeDisplay;
        static int btnSize = 20;
        static int btnFontSize = Convert.ToInt32(btnSize / 1.4);
        static bool game = true;
        static bool firstClick = false;
        static DateTime start;
        static DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            gameField = GameField;
            bombLeft = BombLeft;
            timeDisplay = TimeDisplay;

            Restart.Click += Restart_Click;
            bombLeft.Text = bomb.ToString();

            this.Height = h* btnSize + 39 + UpMenu.Height;
            this.Width = w* btnSize + 16;
            
            createField();
            DisplayField();
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        static void Victory_Check ()
        {
            if (itemCount <= 0)
            {
                timer.Stop();
                game = false;
                MessageBox.Show("You win!");

            }
        }

        static void Restart_Click(object v, RoutedEventArgs e)
        {
            timer.Stop();
            timeDisplay.Text = "00:00";
            firstClick = false;
            gameField.Children.Clear();
            itemCount = h * w - bomb;
            createField();
            DisplayField();
            game = true;
        }

        static void timeUp(object v, EventArgs e)
        {
            if(game)
            {
                TimeSpan span;
                span = DateTime.Now - start;
                string timeStr = span.ToString(@"mm\:ss");
                timeDisplay.Text = timeStr;
            }
        }

        static void Try_Click (object v, RoutedEventArgs e)
        {
            if (!game)
                return;

            if (!firstClick)
            {
                start = DateTime.Now;

                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(timeUp);
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();

                firstClick = true;
            }

            Button b = (Button)v;
            if (Convert.ToString(b.Content) != "")
                return;

            int[] state = GetState(b);
            //state = x, y, isBomb?
            if (state[2] == -1)
            {
                //death
                b.FontSize = btnSize;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.Background = new SolidColorBrush(Colors.Red);
                b.Content = "*";
                MessageBox.Show("you lose...");
                game = false;
                return;
            }

            if(state[2] > 0)
            {
                //show it
                OpenItem(b, state[2]);
                Victory_Check();
                return;
            }

            if (state[2] == 0)
            {
                OpenArea(state[0], state[1]);
                Victory_Check();
                return;
            }
        }

        static void MakeFlag_Click(object v, RoutedEventArgs e)
        {
            if (!game)
                return;

            Button b = (Button)v;
            int[] state = GetState(b);

            if (Convert.ToString(b.Content) == "")
            {
                b.Foreground = new SolidColorBrush(Colors.Red);
                b.Content = "F";
                bombLeft.Text = (Convert.ToInt32(bombLeft.Text) - 1).ToString();
            }
            else if (Convert.ToString(b.Content) == "F")
            {
                b.Content = "";
                bombLeft.Text = (Convert.ToInt32(bombLeft.Text) + 1).ToString();
            }
        }

        static void OpenArea(int pv, int ph)
        {
            StackPanel row = (StackPanel)gameField.Children[pv];
            Button p = (Button)row.Children[ph];

            if (field[pv, ph] == 0 && Convert.ToString(p.Content) == "")
                OpenEmptyItem(pv, ph);
            else
            {
                if(field[pv, ph] > 0 && Convert.ToString(p.Content) == "")
                    OpenItem(p, GetState(p)[2]);
                return;
            }

            bool up = false;
            bool right = false;
            bool down = false;
            bool left = false;

            if (pv + 1 < h)
            {
                OpenArea(pv + 1, ph);
                down = true;
            }
            if (ph + 1 < w)
            {
                OpenArea(pv, ph + 1);
                right = true;
            }
            if (pv - 1 >= 0)
            {
                OpenArea(pv - 1, ph);
                up = true;
            }
            if (ph - 1 >= 0)
            {
                OpenArea(pv, ph - 1);
                left = true;
            }

            if(up && left)
            {
                row = (StackPanel)gameField.Children[pv - 1];
                p = (Button)row.Children[ph - 1];
                if (GetState(p)[2] == 0)
                    OpenArea(pv - 1, ph - 1);
                else
                    OpenItem(p, GetState(p)[2] );
            }

            if (up && right)
            {
                row = (StackPanel)gameField.Children[pv - 1];
                p = (Button)row.Children[ph + 1];
                if (GetState(p)[2] == 0)
                    OpenArea(pv - 1, ph + 1);
                else
                    OpenItem(p, GetState(p)[2]);
            }

            if (down && left)
            {
                row = (StackPanel)gameField.Children[pv + 1];
                p = (Button)row.Children[ph - 1];
                if (GetState(p)[2] == 0)
                    OpenArea(pv + 1, ph - 1);
                else
                    OpenItem(p, GetState(p)[2]);
            }

            if (down && right)
            {
                row = (StackPanel)gameField.Children[pv + 1];
                p = (Button)row.Children[ph + 1];
                if (GetState(p)[2] == 0)
                    OpenArea(pv+1, ph+1);
                else
                    OpenItem(p, GetState(p)[2]);
            }

        }

        static int [] GetState(Button b)
        {
            string Tag = b.Tag.ToString();
            string[] TagSplitted = Tag.Split(',');
            int[] state = new int[] { Convert.ToInt32(TagSplitted[0]), Convert.ToInt32(TagSplitted[1]), Convert.ToInt32(TagSplitted[2]) };
            //state = x, y, isBomb?
            return state;
        }

        static void OpenItem(Button b, int state2)
        {
            if (Convert.ToString(b.Content) == "")
                itemCount--;
            b.Content = state2;
            b.Background = new SolidColorBrush(Colors.White);
            switch (state2)
            {
                case 0:
                    b.Background = new SolidColorBrush(Colors.White);
                    b.Content = " ";
                    break;
                case 1:
                    b.Foreground = new SolidColorBrush(Colors.Blue);
                    break;
                case 2:
                    b.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case 3:
                    b.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case 4:
                    b.Foreground = new SolidColorBrush(Colors.DarkBlue);
                    break;
                case 5:
                    b.Foreground = new SolidColorBrush(Colors.DarkRed);
                    break;
                case 6:
                    b.Foreground = new SolidColorBrush(Colors.SkyBlue);
                    break;
                default:
                    b.Foreground = new SolidColorBrush(Colors.Black);
                    break;
            }
        }

        static void OpenEmptyItem(int pv, int ph)
        {
            itemCount--;
            StackPanel row = (StackPanel)gameField.Children[pv];
            Button p = (Button)row.Children[ph];
            p.Background = new SolidColorBrush(Colors.White);
            p.Content = " ";
        }

        static void DisplayField ()
        {
            for(int i = 0; i < h; i++)
            {
                StackPanel row = new StackPanel();
                row.HorizontalAlignment = HorizontalAlignment.Left;
                row.Orientation = Orientation.Horizontal;
                for(int j = 0; j < w; j++)
                {
                    Button btn = new Button();
                    btn.Width = btnSize;
                    btn.Height = btnSize;
                    btn.FontSize = btnFontSize;
                    btn.FontWeight = FontWeights.ExtraBold;
                    btn.Click += Try_Click;
                    btn.MouseRightButtonDown += MakeFlag_Click;
                    btn.Tag = $"{i},{j},{field[i, j]}";
                    row.Children.Add(btn);
                }
                gameField.Children.Add(row);
            }
        }

        static void createField()
        {
            Random rnd = new Random();
            for (int i = 0; i < bomb; i++)
            {
                int vert = rnd.Next(0, h);
                int hor = rnd.Next(0, w);
                field[vert, hor] = -1;
            }

            //провекра ВНУТРЕННИХ ячеек
            for (int i = 1; i < h - 1; i++)
            {
                for (int j = 1; j < w - 1; j++)
                {
                    int bombCount = 0;

                    if (field[i, j] == -1)
                        continue;

                    if (field[i, j - 1] == -1)//лево
                        bombCount++;
                    if (field[i - 1, j - 1] == -1)//лево верх
                        bombCount++;
                    if (field[i - 1, j] == -1)//верх
                        bombCount++;
                    if (field[i - 1, j + 1] == -1)//право верх
                        bombCount++;
                    if (field[i, j + 1] == -1)//право
                        bombCount++;
                    if (field[i + 1, j + 1] == -1)//право низ
                        bombCount++;
                    if (field[i + 1, j] == -1)//низ
                        bombCount++;
                    if (field[i + 1, j - 1] == -1)//лево низ
                        bombCount++;

                    field[i, j] = bombCount;
                }
            }

            //Отдельная проверка для краёв и углов
            //праывй край
            for (int i = 1; i < h - 1; i++)
            {
                int bombCount = 0;

                if (field[i, (w - 1)] == -1)
                    continue;

                if (field[i, (w - 1) - 1] == -1)//лево
                    bombCount++;
                if (field[i - 1, (w - 1) - 1] == -1)//лево верх
                    bombCount++;
                if (field[i - 1, (w - 1)] == -1)//верх
                    bombCount++;
                if (field[i + 1, (w - 1)] == -1)//низ
                    bombCount++;
                if (field[i + 1, (w - 1) - 1] == -1)//лево низ
                    bombCount++;

                field[i, (w - 1)] = bombCount;
            }

            //левый край
            for (int i = 1; i < h - 1; i++)
            {
                int bombCount = 0;

                if (field[i, 0] == -1)
                    continue;

                if (field[i - 1, 0] == -1)//верх
                    bombCount++;
                if (field[i - 1, 0 + 1] == -1)//право верх
                    bombCount++;
                if (field[i, 0 + 1] == -1)//право
                    bombCount++;
                if (field[i + 1, 0 + 1] == -1)//право низ
                    bombCount++;
                if (field[i + 1, 0] == -1)//низ
                    bombCount++;

                field[i, 0] = bombCount;
            }

            //верхний край
            for (int i = 1; i < w - 1; i++)
            {
                int bombCount = 0;

                if (field[0, i] == -1)
                    continue;

                if (field[0, i - 1] == -1)//лево
                    bombCount++;
                if (field[0, i + 1] == -1)//право
                    bombCount++;
                if (field[0 + 1, i + 1] == -1)//право низ
                    bombCount++;
                if (field[0 + 1, i] == -1)//низ
                    bombCount++;
                if (field[0 + 1, i - 1] == -1)//лево низ
                    bombCount++;

                field[0, i] = bombCount;
            }

            //нижний край
            for (int i = 1; i < h - 1; i++)
            {
                int bombCount = 0;

                if (field[(h - 1), i] == -1)
                    continue;

                if (field[(h - 1), i - 1] == -1)//лево
                    bombCount++;
                if (field[(h - 1) - 1, i - 1] == -1)//лево верх
                    bombCount++;
                if (field[(h - 1) - 1, i] == -1)//верх
                    bombCount++;
                if (field[(h - 1) - 1, i + 1] == -1)//право верх
                    bombCount++;
                if (field[(h - 1), i + 1] == -1)//право
                    bombCount++;

                field[(h - 1), i] = bombCount;
            }

            //углы
            //левй верхний
            {

                if (field[0, 0] != -1)
                {
                    int bombCount = 0;

                    if (field[0, 0 + 1] == -1)//право
                        bombCount++;
                    if (field[0 + 1, 0 + 1] == -1)//право низ
                        bombCount++;
                    if (field[0 + 1, 0] == -1)//низ
                        bombCount++;

                    field[0, 0] = bombCount;
                }

            }
            //правый верхний
            if (field[0, w - 1] != -1)
            {
                int bombCount = 0;

                if (field[0, (w - 1) - 1] == -1)//лево
                    bombCount++;
                if (field[0 + 1, w - 1] == -1)//низ
                    bombCount++;
                if (field[0 + 1, w - 1 - 1] == -1)//лево низ
                    bombCount++;

                field[0, w - 1] = bombCount;
            }
            //левый нижний
            if (field[h - 1, 0] != -1)
            {
                int bombCount = 0;

                if (field[(h - 1) - 1, 0] == -1)//верх
                    bombCount++;
                if (field[(h - 1) - 1, 0 + 1] == -1)//право верх
                    bombCount++;
                if (field[(h - 1), 0 + 1] == -1)//право
                    bombCount++;

                field[h - 1, 0] = bombCount;
            }
            //правый нижний
            if (field[h - 1, w - 1] != -1)
            {
                int bombCount = 0;

                if (field[(h - 1), (w - 1) - 1] == -1)//лево
                    bombCount++;
                if (field[(h - 1) - 1, (w - 1) - 1] == -1)//лево верх
                    bombCount++;
                if (field[(h - 1) - 1, (w - 1)] == -1)//верх
                    bombCount++;

                field[h - 1, w - 1] = bombCount;
            }
        }
    }
}
