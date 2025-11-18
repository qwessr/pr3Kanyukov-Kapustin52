using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace pr3
{
    internal class Snake
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int localPort = 5001;
        public static int MaxSpeed = 15;

        static void Main(string[] args)
        {
            try
            {
                // СБРАСЫВАЕМ ВСЕ КОЛЛЕКЦИИ (НОВЫЙ СТАРТ)
                remoteIPAddress.Clear();
                viewModelGames.Clear();
                Leaders.Clear();

                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();
                Thread tTime = new Thread(Timer);
                tTime.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение:" + ex.ToString() + "\n" + ex.Message);
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress.ToList())
            {
                var game = viewModelGames.Find(x => x.IdSnake == User.IdSnake);
                if (game != null)
                {
                    UdpClient sender = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(User.IPAddress), int.Parse(User.Port));
                    try
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(game));
                        sender.Send(bytes, bytes.Length, endPoint);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                    }
                    finally
                    {
                        sender.Close();
                    }
                }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint RemoteIpEndPoint = null;
            try
            {
                Console.WriteLine("Команды сервера:");
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData.ToString());

                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        // При старте новой игры удаляем старую змейку, если есть такая же
                        var existingUser = remoteIPAddress.FirstOrDefault(u => u.IPAddress == viewModelUserSettings.IPAddress && u.Port == viewModelUserSettings.Port);
                        if (existingUser != null)
                        {
                            remoteIPAddress.Remove(existingUser);
                            var vg = viewModelGames.FirstOrDefault(x => x.IdSnake == existingUser.IdSnake);
                            if (vg != null) viewModelGames.Remove(vg);
                        }
                        viewModelUserSettings.IdSnake = AddSnake();
                        remoteIPAddress.Add(viewModelUserSettings);
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                    }
                    else
                    {
                        string[] dataMessage = returnData.Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        var user = remoteIPAddress.Find(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);
                        if (user != null && user.IdSnake >= 0 && user.IdSnake < viewModelGames.Count)
                        {
                            var snake = viewModelGames[user.IdSnake].SnakesPlayers;
                            // Чтобы не было смены направления "в себя"
                            if (dataMessage[0] == "Up" && snake.direction != Snakes.Direction.Down)
                                snake.direction = Snakes.Direction.Up;
                            else if (dataMessage[0] == "Down" && snake.direction != Snakes.Direction.Up)
                                snake.direction = Snakes.Direction.Down;
                            else if (dataMessage[0] == "Left" && snake.direction != Snakes.Direction.Right)
                                snake.direction = Snakes.Direction.Left;
                            else if (dataMessage[0] == "Right" && snake.direction != Snakes.Direction.Left)
                                snake.direction = Snakes.Direction.Right;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames
            {
                SnakesPlayers = new Snakes
                {
                    Points = new List<Snakes.Point> {
                        new Snakes.Point { X = 50, Y = 100 },
                        new Snakes.Point { X = 40, Y = 100 },
                        new Snakes.Point { X = 30, Y = 100 }
                    },
                    direction = Snakes.Direction.Right // Cтартовое направление
                },
                Points = new Snakes.Point(new Random().Next(60, 700), new Random().Next(60, 380))
            };
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.Count - 1;
        }

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);

                var deadSnakes = viewModelGames.Where(x => x.SnakesPlayers.GameOver).ToList();
                foreach (var dead in deadSnakes)
                {
                    var user = remoteIPAddress.FirstOrDefault(x => x.IdSnake == dead.IdSnake);
                    if (user != null)
                    {
                        remoteIPAddress.Remove(user);
                    }
                    viewModelGames.Remove(dead);
                }

                foreach (ViewModelUserSettings User in remoteIPAddress.ToList())
                {
                    var game = viewModelGames.FirstOrDefault(x => x.IdSnake == User.IdSnake);
                    if (game == null) continue;

                    Snakes Snake = game.SnakesPlayers;
                    if (Snake.GameOver) continue;

                    // Сохраняем старые координаты головы для появления яблока не в том же месте
                    int prevX = Snake.Points[0].X;
                    int prevY = Snake.Points[0].Y;

                    // Движение: хвост и голова
                    for (int i = Snake.Points.Count - 1; i > 0; i--)
                    {
                        Snake.Points[i].X = Snake.Points[i - 1].X;
                        Snake.Points[i].Y = Snake.Points[i - 1].Y;
                    }

                    int Speed = 10;
                    if (Snake.direction == Snakes.Direction.Right)
                        Snake.Points[0].X += Speed;
                    else if (Snake.direction == Snakes.Direction.Down)
                        Snake.Points[0].Y += Speed;
                    else if (Snake.direction == Snakes.Direction.Up)
                        Snake.Points[0].Y -= Speed;
                    else if (Snake.direction == Snakes.Direction.Left)
                        Snake.Points[0].X -= Speed;

                    // Проверка границ
                    if (Snake.Points[0].X < 0 || Snake.Points[0].X > 793 || Snake.Points[0].Y < 0 || Snake.Points[0].Y > 420)
                    {
                        Snake.GameOver = true;
                        LoadLeaders();
                        Leaders.Add(new Leaders { Name = User.Name, Points = Snake.Points.Count - 3 });
                        SaveLeaders();
                        continue;
                    }

                    // Проверка столкновения с собой (начиная с 4 сегмента)
                    for (int i = 4; i < Snake.Points.Count; i++)
                    {
                        if (Snake.Points[0].X == Snake.Points[i].X && Snake.Points[0].Y == Snake.Points[i].Y)
                        {
                            Snake.GameOver = true;
                            LoadLeaders();
                            Leaders.Add(new Leaders { Name = User.Name, Points = Snake.Points.Count - 3 });
                            SaveLeaders();
                            break;
                        }
                    }

                    if (Snake.GameOver) continue;

                    // ПРОВЕРКА поедания яблока — добавляем новый сегмент ТОЛЬКО если еще не ели яблоко "на этом шаге"
                    bool eaten = false;
                    if (Math.Abs(Snake.Points[0].X - game.Points.X) < 10 && Math.Abs(Snake.Points[0].Y - game.Points.Y) < 10)
                    {
                        eaten = true;
                        int newX, newY;
                        do
                        {
                            newX = new Random().Next(60, 700);
                            newY = new Random().Next(60, 380);
                        } while ((newX == Snake.Points[0].X && newY == Snake.Points[0].Y));

                        // Меняем только координаты - объект яблока один и тот же! 
                        game.Points.X = newX;
                        game.Points.Y = newY;
                    }

                    // ДОБАВЛЯЕМ новый хвост только один раз за укус и только если съели
                    if (eaten)
                    {
                        Snake.Points.Add(new Snakes.Point
                        {
                            X = Snake.Points[Snake.Points.Count - 1].X,
                            Y = Snake.Points[Snake.Points.Count - 1].Y
                        });

                        LoadLeaders();
                        Leaders.Add(new Leaders { Name = User.Name, Points = Snake.Points.Count - 3 });
                        Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                        game.Top = Leaders.FindIndex(x => x.Points == Snake.Points.Count - 3 && x.Name == User.Name) + 1;
                    }
                }

                Send();
            }
        }

        public static void SaveLeaders()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Leaders);
                File.WriteAllText("./leaders.txt", json);
            }
            catch { }
        }

        public static void LoadLeaders()
        {
            try
            {
                if (File.Exists("./leaders.txt"))
                {
                    string json = File.ReadAllText("./leaders.txt");
                    Leaders = string.IsNullOrEmpty(json) ? new List<Leaders>() : JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
                else
                    Leaders = new List<Leaders>();
            }
            catch
            {
                Leaders = new List<Leaders>();
            }
        }
    }
}
