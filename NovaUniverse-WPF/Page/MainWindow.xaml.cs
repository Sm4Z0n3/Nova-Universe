using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public class Civilization
    {
        public Point Position { get; set; }
        public string Name { get; set; }
        public Ellipse Circle { get; set; }
        public int Alertness { get; set; } // 警惕性
        public int AttacksReceived { get; set; } // 收到攻击的次数
        public const int MaxAttacksAllowed = 3; // 最大攻击次数限制

        public Civilization(Point position, string name)
        {
            Position = position;
            Name = name;
            Alertness = 50; // 初始警惕性为 50
            AttacksReceived = 0;
        }
    }

    public partial class MainWindow : Window
    {
        #region 头文件声明
        ///文明数量
        public static int NumberS = 50;
        ///画板宽度|高度
        public static int[] CTSWH = {932,525};
        ///宇宙名
        public static string NAME = "新的宇宙";
        ///每个文明间最小距离
        public static int DISTANCE = 7;
        ///广播传播速度（光速）
        public static double LIGHTSPEED = 40;
        ///最远广播距离
        public static double RECEIVEDDISTANCE = 200;
        ///X个像素一光年(PX/LightYear)
        public static double PX_LIGHTYEARS = 0.5;
        #endregion
        string title = $"Nova Universe | {NAME} | Number[{NumberS}] | Universe[{CTSWH[0]},{CTSWH[1]}] | Scale[1:{PX_LIGHTYEARS}] | LightSpeed[{LIGHTSPEED}]";
        public Log log = new Log();
        public MainWindow()
        {
            InitializeComponent();
            Main__.Height = CTSWH[1];
            Main__.Width = CTSWH[0];
            this.Height = CTSWH[1] + 50;
            this.Width = CTSWH[0] + 10;
            Title = title;
            log.printf(title, Brushes.Gold);
        }

        #region 初始化
        private System.Windows.Threading.DispatcherTimer broadcastTimer;
        List<Civilization> civilizationList = new List<Civilization>();
        List<Point> civilizationPositions = new List<Point>();
        ///绘制出指定数量的文明
        private void DrawCivilizations(int numberOfCivilizations)
        {
            Random random = new Random();
            int i = 0;
            
            while (numberOfCivilizations != i)
            {
                Ellipse civilization = new Ellipse();
                civilization.Width = 9;
                civilization.Height = 9;
                civilization.Fill = Brushes.White;

                int x, y;

                bool validPosition = false;

                do
                {
                    x = random.Next(0, (int)Main__.Width);
                    y = random.Next(0, (int)Main__.Height);

                    validPosition = true;
                    foreach (Point pos in civilizationPositions)
                    {
                        double distance = Math.Sqrt(Math.Pow(pos.X - x, DISTANCE) + Math.Pow(pos.Y - y, DISTANCE));
                        if (distance < DISTANCE)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                } while (!validPosition);

                civilizationPositions.Add(new Point(x, y));

                string selectedCivilizationName = NameList.List.Split('\n')[i].Replace("\n","");
                civilizationList.Add(new Civilization(new Point(x, y), selectedCivilizationName));
                civilization.MouseEnter += (sender, e) =>
                {
                    var control = sender as FrameworkElement;
                    if (control != null)
                        Title = title + $" | {selectedCivilizationName}";
                };
                civilization.MouseLeave += (sender, e) => { Title = title; };

                Canvas.SetLeft(civilization, x);
                Canvas.SetTop(civilization, y);

                Main__.Children.Add(civilization);

                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{i} | [{x},{y}] | {NameList.List.Split('\n')[i]}.Replace(\"\\n\",\"\")");
                    log.printf($"{i} | [{x},{y}] | {NameList.List.Split('\n')[i].Replace("\n", "")}", Brushes.Yellow );
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                }
                i++;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"生成完成 共{numberOfCivilizations}个文明");
            log.printf($"生成完成 共{numberOfCivilizations}个文明",Brushes.Green);
            Console.ForegroundColor = ConsoleColor.White;
            //初始化广播
            _=SendBroadcast(civilizationList);
            StartBroadcastTimer();
        }
        private async Task SendBroadcast(List<Civilization> civilizations)
        {
            Random random = new Random();
            int senderIndex = random.Next(0, civilizations.Count);
            Civilization senderCivilization = civilizations[senderIndex];
            string senderName = senderCivilization.Name;

            Console.WriteLine(senderName + "发送了广播");
            log.printf(senderName + "发送了广播", Brushes.White);
            senderCivilization.Alertness -= 10;

            if (senderCivilization.Alertness < 0)
                senderCivilization.Alertness = 0;
            if (senderCivilization.AttacksReceived > 0)
                senderCivilization.AttacksReceived--;
            double angleIncrement = LIGHTSPEED * Math.PI / 180; // 计算每秒角度增量，以确保速度单位正确

            for (double radius = 1; radius <= 100; radius += 1)
            {
                for (double angle = 0; angle < Math.PI * 2; angle += angleIncrement)
                {
                    double x = senderCivilization.Position.X + radius * Math.Cos(angle);
                    double y = senderCivilization.Position.Y + radius * Math.Sin(angle);

                    Ellipse wave = CreateWaveEllipse(x, y);
                    Main__.Children.Add(wave);

                    await Task.Delay(TimeSpan.FromSeconds(1 / LIGHTSPEED)); // 保持与每秒角度增量对应的时间间隔
                }
            }

            for (int i = 0; i < civilizations.Count; i++)
            {
                if (i == senderIndex) continue;

                Civilization receiverCivilization = civilizations[i];
                Point receiverPosition = receiverCivilization.Position;
                string receiverName = receiverCivilization.Name;

                double distance = Math.Sqrt(Math.Pow(receiverPosition.X - senderCivilization.Position.X, 2) + Math.Pow(receiverPosition.Y - senderCivilization.Position.Y, 2)) * PX_LIGHTYEARS;
                double travelDistance = 0;

                bool receivedBroadcast = false;

                // 如果文明已经接收到广播则继续循环
                while (travelDistance < RECEIVEDDISTANCE && !receivedBroadcast)
                {
                    double deltaX = (receiverPosition.X - senderCivilization.Position.X) / distance * travelDistance;
                    double deltaY = (receiverPosition.Y - senderCivilization.Position.Y) / distance * travelDistance;
                    double currentX = senderCivilization.Position.X + deltaX;
                    double currentY = senderCivilization.Position.Y + deltaY;

                    double distanceToReceiver = Math.Sqrt(Math.Pow(currentX - receiverPosition.X, 2) + Math.Pow(currentY - receiverPosition.Y, 2));
                    receiverCivilization.Alertness -= (int)(receiverCivilization.Alertness * 0.1); // 警惕性减少10%
                    double distanceInLightYears = distance / PX_LIGHTYEARS;

                    if (receiverCivilization.Alertness >= 100)
                    {
                        // 如果警惕性达到最大值且还没达到最大攻击次数，则进行攻击
                        // 添加攻击行为的逻辑...

                        Console.ForegroundColor = ConsoleColor.Red;
                        log.printf($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了攻击", Brushes.Red);

                        Line attackLine = CreateAttackLine(senderCivilization.Position.X, senderCivilization.Position.Y, receiverCivilization.Position.X, receiverCivilization.Position.Y);
                        Main__.Children.Add(attackLine);

                        double attackLineLength = Math.Sqrt(Math.Pow(receiverCivilization.Position.X - senderCivilization.Position.X, 2) + Math.Pow(receiverCivilization.Position.Y - senderCivilization.Position.Y, 2));
                        TimeSpan duration = TimeSpan.FromSeconds(attackLineLength / LIGHTSPEED);
                        await Task.Delay(duration);
                        Main__.Children.Remove(attackLine);

                        Civilization targetCivilization = civilizationList.FirstOrDefault(c => c.Name == receiverName);
                        if (targetCivilization != null)
                        {
                            log.printf($"{targetCivilization.Name}已灭绝 [{targetCivilization.Position}]", Brushes.Red);
                            targetCivilization.Circle.Fill = Brushes.Black;
                            Main__.Children.Remove(targetCivilization.Circle);
                            civilizationList.Remove(targetCivilization);
                        }

                        receiverCivilization.AttacksReceived++; // 增加被攻击次数
                    }

                    else
                    {
                        if (distanceToReceiver <= 10)
                        {
                            //Console.WriteLine($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播");
                            //log.printf($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播", Brushes.Green);
                            int decision = random.Next(1, 101);

                            if (decision <= 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了攻击");
                                log.printf($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了攻击", Brushes.Red);

                                Line attackLine = CreateAttackLine(senderCivilization.Position.X, senderCivilization.Position.Y, receiverCivilization.Position.X, receiverCivilization.Position.Y);
                                Main__.Children.Add(attackLine);

                                double attackLineLength = Math.Sqrt(Math.Pow(receiverCivilization.Position.X - senderCivilization.Position.X, 2) + Math.Pow(receiverCivilization.Position.Y - senderCivilization.Position.Y, 2));
                                TimeSpan duration = TimeSpan.FromSeconds(attackLineLength / LIGHTSPEED);
                                await Task.Delay(duration);
                                Main__.Children.Remove(attackLine);

                                Civilization targetCivilization = civilizationList.FirstOrDefault(c => c.Name == receiverName);
                                if (targetCivilization != null)
                                {
                                    log.printf($"{targetCivilization.Name}已灭绝 [{targetCivilization.Position}]", Brushes.Red);
                                    targetCivilization.Circle.Fill = Brushes.Black;
                                    Main__.Children.Remove(targetCivilization.Circle);
                                    civilizationList.Remove(targetCivilization);
                                }
                            }
                            else if (decision <= 10)
                            {
                                // 联系行为
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了联系");
                                log.printf($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了联系", Brushes.Yellow);

                                // 发送扇形广播
                                Ellipse senderCircle = civilizationList.FirstOrDefault(c => c.Name == senderName)?.Circle;
                                if (senderCircle != null)
                                {
                                    Point senderPosition = new Point(Canvas.GetLeft(senderCircle), Canvas.GetTop(senderCircle));
                                    DrawFanBroadcast(senderPosition);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了沉默");
                                log.printf($"{receiverName} 收到了来自 {distanceInLightYears} 光年外 {senderName} 的广播 它选择了沉默", Brushes.White);
                            }


                            Console.ForegroundColor = ConsoleColor.White;
                            receivedBroadcast = true; // 标记为已接收到广播
                        }
                    }
                    

                    await Task.Delay(TimeSpan.FromSeconds(1 / LIGHTSPEED));
                    travelDistance += LIGHTSPEED;
                }
            }

            SendBroadcast(civilizationList);

        }
        private Ellipse CreateWaveEllipse(double x, double y)
        {
            Ellipse wave = new Ellipse
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2,
                Width = 2,
                Height = 2,
                Fill = Brushes.Green
            };

            Canvas.SetLeft(wave, x);
            Canvas.SetTop(wave, y);

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1), 
                RepeatBehavior = new RepeatBehavior(1)
            };

            wave.BeginAnimation(Ellipse.OpacityProperty, fadeAnimation);

            return wave;
        }
        private Line CreateAttackLine(double startX, double startY, double endX, double endY)
        {
            Line attackLine = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Opacity = 1.0
            };

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(6),
                RepeatBehavior = new RepeatBehavior(1)
            };

            attackLine.BeginAnimation(Line.OpacityProperty, fadeAnimation);

            return attackLine;
        }
        private void DrawFanBroadcast(Point center)
        {
            int numLines = 36; // 定义扇形的线条数量
            double angleIncrement = 360.0 / numLines;

            for (double angle = 0; angle < 360; angle += angleIncrement)
            {
                // 计算扇形线条的末端坐标
                double radians = angle * Math.PI / 180;
                double endX = center.X + 20 * Math.Cos(radians); // 20是扇形的长度，可以根据需要调整
                double endY = center.Y + 20 * Math.Sin(radians);

                // 创建扇形线条
                Line fanLine = new Line
                {
                    X1 = center.X,
                    Y1 = center.Y,
                    X2 = endX,
                    Y2 = endY,
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 2
                };

                // 添加扇形线条到画布上
                Main__.Children.Add(fanLine);
            }
        }
        private void StartBroadcastTimer()
        {
            broadcastTimer = new System.Windows.Threading.DispatcherTimer();
            broadcastTimer.Tick += new EventHandler(BroadcastTimer_Tick);
            broadcastTimer.Interval = TimeSpan.FromSeconds(20); // 设置初始时间间隔
            broadcastTimer.Start();
        }
        private async void BroadcastTimer_Tick(object sender, EventArgs e)
        {
            await SendBroadcast(civilizationList);
            Random random = new Random();
            broadcastTimer.Interval = TimeSpan.FromSeconds(random.Next(20, 121));
        }
        #endregion

        #region 事件
        ///等待画板加载完毕
        private void Main___Loaded(object sender, RoutedEventArgs e)
        {
            DrawCivilizations(NumberS);
            log.Visibility = Visibility.Visible;
        }
        private void New_BTN_Click(object sender, RoutedEventArgs e)
        {
            Creat creat = new Creat();
            this.Main__.UpdateLayout();
            broadcastTimer.Stop();
            creat.Visibility = Visibility.Visible;
            Visibility = Visibility.Collapsed;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            log.Visibility = Visibility.Collapsed;
        }
        private void Main___MouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double minScale = 0.6; // 最小比例
            const double maxScale = 6.0; // 最大比例
            const double scaleChange = 0.1; // 每次滚轮变化的比例

            // 检查是否按下 Ctrl 键
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Point mousePos = e.GetPosition(Main__);
                double deltaX = mousePos.X / Main__.ActualWidth;
                double deltaY = mousePos.Y / Main__.ActualHeight;

                double scaleX = scaleTransform.ScaleX;
                double scaleY = scaleTransform.ScaleY;

                if (e.Delta > 0)
                {
                    // 放大
                    if (scaleX < maxScale && scaleY < maxScale)
                    {
                        scaleX = Math.Min(scaleX + scaleChange, maxScale);
                        scaleY = Math.Min(scaleY + scaleChange, maxScale);
                    }
                }
                else
                {
                    // 缩小
                    if (scaleX > minScale && scaleY > minScale)
                    {
                        scaleX = Math.Max(scaleX - scaleChange, minScale);
                        scaleY = Math.Max(scaleY - scaleChange, minScale);
                    }
                }

                scaleTransform.CenterX = deltaX;
                scaleTransform.CenterY = deltaY;
                scaleTransform.ScaleX = scaleX;
                scaleTransform.ScaleY = scaleY;
            }
        }
        private void Open_BTN_Click(object sender, RoutedEventArgs e)
        {
            log.Visibility = Visibility.Visible;
        }
        private async void TestAttack_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int attackerIndex = random.Next(0, civilizationList.Count);
            int targetIndex = random.Next(0, civilizationList.Count);
            while (attackerIndex == targetIndex)
            {
                targetIndex = random.Next(0, civilizationList.Count);
            }

            Civilization attacker = civilizationList[attackerIndex];
            Civilization target = civilizationList[targetIndex];

            double distance = Math.Sqrt(Math.Pow(attacker.Position.X - target.Position.X, 2) + Math.Pow(attacker.Position.Y - target.Position.Y, 2));
            if (distance <= RECEIVEDDISTANCE * 2)
            {
                Console.WriteLine($"{attacker.Name} 发起了攻击，目标是 {target.Name}");
                log.printf($"{attacker.Name} 发起了攻击，目标是 {target.Name}", Brushes.Red);
                log.printf($"{target.Name} 被摧毁！", Brushes.Red);
                civilizationList.Remove(target);

                // 创建连接线
                Line attackLine = CreateAttackLine(attacker.Position.X, attacker.Position.Y, target.Position.X, target.Position.Y);
                Main__.Children.Add(attackLine);

                // 等待一段时间
                await Task.Delay(TimeSpan.FromSeconds(1));

                // 移除连接线
                Main__.Children.Remove(attackLine);
            }
            else
            {
                log.printf("攻击未能命中目标文明。", Brushes.Red);
            }
        }


        #endregion


    }
}
