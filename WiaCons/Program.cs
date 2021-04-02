using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonIPS;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;
using System.Net.NetworkInformation;

namespace WiaCons
{
    class Program
    {
        static int epic_param_1 = 22, epic_param_3 = 21, epic_param_2 = 23, epic_param_4 = 24;
        static string path = "LogSender.txt";
        static int dtemp_1 = 2;
        static bool ztrmp_2 = true;
        static private int _temp_3 = 240000; 
        static private MessagesCommunicator _temp_4 = null;
        static private bool _ctemp_5 = false, _rtemp_6 = false;
        static ServerInfo server_1;
        static DeviceInfo server_2;
        static private string _setemp_7, _lotemp_8;
        //private CMessages Messages;
        static private System.Threading.Timer tmtemp_9;
                
        static public CSettings Setemp_30 { get; private set; }

        /// =========================================================================================================

        static Stopwatch swtemp_10 = new Stopwatch(); 
        static Stopwatch swtemp_11 = new Stopwatch();
        static TimerCallback titemp_12 = new TimerCallback(tmrDutControl_Tick);
        static int dctemp_13 = 5 * 60 * 1000;
        static Timer tmtemp_14 = new Timer(titemp_12, null, dctemp_13, dctemp_13);
        static TimerCallback titemp_15 = new TimerCallback(autoConnect_Tick);
        static int actemp_16 = 10000;
        static Timer autemp_17t = new Timer(titemp_15, null, actemp_16, actemp_16);
        static bool neetemp_18 = true;
        static int dutemp_19 = 0;
        static string dutemp_20 = "";
        public static int? messtemp_21 = null;
        static int retemp_22 = 15000;
        static int titemp_23 = 5000;
        public const int MSG1 = 1, MSG2 = 0, MSG3 = -1, PO4 = -2;
        const string FAIL_1 = "65536" , DROP_2 = "65533" ,
            PORT_3E = "65530" ;
        private static bool stotemp_24 = false;

        static bool istemp_24 = false, oldtemp_25 = false;

        static List<Dutyara> duttemp_26 = new List<Dutyara>();

        /// =========================================================================================================



        //// =========================================================================================================
        // Переменные для работы сервером виалоновстким 

        public static List<string> bltemp_27 = new List<string>();
        //public static System.Windows.Forms.Timer timer_stopper;

        //// =========================================================================================================



        static void Main(string[] args)
        {
            tmrDutControl_TutnOff();
            autoConnect_TurnOff();
#if !DEBUG
            CheckLicense();
#endif
if (dtemp_1 == 1)
            dctemp_13 = 1 * 30 * 1000;

            Dutyara.GetPorts();
            duttemp_26.Add(new Dutyara(33722, 9600));
            duttemp_26.Add(new Dutyara(22733, 9600));

            Dutyara.GetPorts();
            tmrDutControl_TutnOn();
            autoConnect_TurnOn();

            tmtemp_9 = new System.Threading.Timer(new TimerCallback(PingTimerCallback), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            var base_path = Environment.GetEnvironmentVariable("USERPROFILE");
            if (base_path == null)
                base_path = Environment.GetEnvironmentVariable("HOME");
            if (base_path == null)
                base_path = Path.GetDirectoryName(Environment.CurrentDirectory);
            _setemp_7 = Path.Combine(base_path, ".wialon_ips_emulator.settings");

            if (!File.Exists(path))
            {
                File.Create(path);
            }
            try
            {
                Setemp_30 = CSettings.Load(_setemp_7);
                Console.WriteLine("Emulator" + "Settings loaded successfully");
            }
            catch (Exception)
            {
                Console.WriteLine("Emulator" + "Cannot load settings");
                Setemp_30 = new CSettings();
            }


            //ConnectClick();
            while (true)
            {
                string cotemp_31 = "";
                if (cotemp_31.ToLower() == "exit")
                {
                    break;
                }
            }
            Console.Title = "KTS-Monitoring";
            Console.ReadKey();
        }

        private static void CheckLicense()
        {
            string cptemp_32 = GetCpuIdLinux();
            //string cpu_id = "000000006bd6f118";
            //StreamWriter writer = new StreamWriter("SomeFile.txt");
            //if (cpu_id != "000000006bd6f118")
            if (cptemp_32 != "1000000013d0b0e1")
            {
                Console.WriteLine("Вы нарушили правила лицензионного соглашения, программа заблокирована");
                Console.ReadKey();
                Process.GetCurrentProcess().Kill();
            }
           
        }

        private static void tmrDutControl_Tick(object state)
        {
            neetemp_18 = true;
            while (neetemp_18)
            {
                // Console.WriteLine(sw.ElapsedMilliseconds);
                if (Dutyara.opened)
                {
                    swtemp_10.Restart();
                    SetDropend();
                    
                    duttemp_26[dutemp_19].GetData();
                    duttemp_26[dutemp_19].SendMsg();
                    //GetAnsver();
                }
                else

                //if (!Dutyara.opened)
                {
                    //dut_data = "44N0=+210=01345.27=00632.55=094";
                    if (dutemp_20 != "")
                    {
                        CheckData(dutemp_20);
                    }
                    // Так должно быть лучше, но нужно проверит, а на это нет времени:
                    else
                    if (swtemp_10.ElapsedMilliseconds > titemp_23)
                    {
                        messtemp_21 = MSG2;
                    }

                    switch (messtemp_21)
                    {
                        case MSG2:
                            duttemp_26[dutemp_19].msg_cont.id = duttemp_26[dutemp_19].Id.ToString();
                            duttemp_26[dutemp_19].msg_cont.water = FAIL_1;
                            duttemp_26[dutemp_19].msg_cont.fuel = FAIL_1;
                            duttemp_26[dutemp_19].msg_cont.temp = FAIL_1;
                            //dut_data = "44N0=65536=65536=65536=094";
                            break;
                        case MSG3:
                            duttemp_26[dutemp_19].msg_cont.id = duttemp_26[dutemp_19].Id.ToString();
                            duttemp_26[dutemp_19].msg_cont.water = DROP_2;
                            duttemp_26[dutemp_19].msg_cont.fuel = DROP_2;
                            duttemp_26[dutemp_19].msg_cont.temp = DROP_2;
                            break;
                        case PO4:
                            duttemp_26[dutemp_19].msg_cont.id = duttemp_26[dutemp_19].Id.ToString();
                            duttemp_26[dutemp_19].msg_cont.water = PORT_3E;
                            duttemp_26[dutemp_19].msg_cont.fuel = PORT_3E;
                            duttemp_26[dutemp_19].msg_cont.temp = PORT_3E;
                            break;
                        default:
                            break;
                    }

                    if (messtemp_21 != null)
                    {
                        //if (message_status != MSG_SUCCESS)
                        dutemp_20 = duttemp_26[dutemp_19].msg_cont.id + "N0=" + duttemp_26[dutemp_19].msg_cont.temp + "=" +
                                        duttemp_26[dutemp_19].msg_cont.fuel + "=" + duttemp_26[dutemp_19].msg_cont.water + "=094"; //"44N0=65536=65536=65536=094";
                        Console.WriteLine(dutemp_20);

                        GoToNextDut(ref _temp_4);
                    }
                    else dutemp_20 = duttemp_26[dutemp_19].GetData();
                    Thread.Sleep(100);
                    //var rr = timer_stopper.Enabled;
                }

            }
        }

        private static void SetDropend()
        {
            int templ = 25;
            string gfs = "*set_temp_Celsium*";
            bool droper = GetAnsver(templ, gfs);
            Dutyara.opened = droper;
        }

        private static bool GetAnsver(int templ, string gfs, int rrrr = 24, int gefest = 12)
        {
            string new_temp = "";
            bool flag = true;
            if (Math.E - templ < 18)
            {
                new_temp.Concat(gfs).Concat("Con"); ;
            }
            else
            {
                gfs.Concat("Faringate");
                new_temp = "&%#^%" + gfs + new_temp;
            }
            if (new_temp.Length != 0 && 25 > 24 && 10 < gefest)
            {
                flag = !(!((((true ^ true) || (true ^ false)) && true) ^ (false ^ true)) && true) ;
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        // Стопперы и запускаторы таймеров

        static void tmrDutControl_TutnOff()
        {
            tmtemp_14.Change(Timeout.Infinite, Timeout.Infinite);
        }
        static void tmrDutControl_TutnOn()
        {
            tmtemp_14.Change(dctemp_13, dctemp_13);
        }

        static void autoConnect_TurnOff()
        {
            autemp_17t.Change(Timeout.Infinite, Timeout.Infinite);
        }

        static void autoConnect_TurnOn()
        {
            autemp_17t.Change(actemp_16, actemp_16);
        }

        static string GetCpuIdLinux()
        {
            //  Open the file into a StreamReader
              StreamReader file_info = File.OpenText("/proc/cpuinfo");
            // Read the file into a string
            string file_text = file_info.ReadToEnd();
            file_info.Close();
            string[] subs = file_text.Split('\n');
            Dictionary<string, string> cpu_params = new Dictionary<string, string>();
            foreach (var item in subs)
            {
                string[] temp = item.Split(':');
                try
                {
                    cpu_params.Add(temp[0].Trim(), temp[1].Trim());
                }
                catch (Exception ex)
                {

                }
            }
            return cpu_params["Serial"];
        }

        // Проверка полученных данных от ДУТа
        // в случае, если данные дошли в целосности - отправляет их получателю
        // в случае если данные пришли в повреждённом виде - отправляем получателю соответствующий код ошибки
        private static void CheckData(string input_text)
        {
            string[] dut_data_arr = input_text.Split('=');
            int arr_len = dut_data_arr.Length;
            if (arr_len != 5)
            {
                messtemp_21 = MSG3;
            }
            else
            {
                // Проверяем является ли айдишник числом
                string dut_id = dut_data_arr[0].Substring(0, dut_data_arr[0].Length - 2);
                // dut_id = dut_id.Substring(1, dut_id.Length-2);
                float i = 0;
                var bb = float.TryParse(dut_id, out i);
                if (!bb)
                {
                    messtemp_21 = MSG3; return;
                }
                if (dut_data_arr[1][0] != '+' && dut_data_arr[1][0] != '-')
                {
                    messtemp_21 = MSG3; return;
                }
                bb = float.TryParse(dut_data_arr[2].Replace(".", ","), out i);
                if (!bb)
                {
                    messtemp_21 = MSG3; return;
                }
                bb = float.TryParse(dut_data_arr[3].Replace(".", ","), out i);
                if (!bb)
                {
                    messtemp_21 = MSG3; return;
                }
                // Если айдишник отличается от запрашиваемого - данные считаются битыми, т.к. пришли от другого ДУТа
                var idish = duttemp_26[dutemp_19].Id.ToString();
                if (dut_id != idish)
                {
                    messtemp_21 = MSG3; Console.WriteLine("Err!"); return;
                    // Альтернативный способ решения: 
                    //message_status = null; return;


                    //int irr = 0;
                    //while (irr <= 25)
                    //{
                    //Console.WriteLine("Err!");
                    //    irr++;
                    //}
                }
                // ХЗ что хз зачем, но вдроуг пригодится
                //bb = float.TryParse(dut_data_arr[4].Replace(".", ","), out i);
                //if (!bb)
                //{
                //    message_status = MSG_DROP; return;
                //}

                messtemp_21 = MSG1;
                duttemp_26[dutemp_19].msg_cont.id = dut_id;
                duttemp_26[dutemp_19].msg_cont.fuel = ViaDataFormater.CorrectoinNull(dut_data_arr[2], duttemp_26[dutemp_19].Corrector);
                duttemp_26[dutemp_19].msg_cont.water = ViaDataFormater.CorrectoinNull(dut_data_arr[3], duttemp_26[dutemp_19].Corrector);
                duttemp_26[dutemp_19].msg_cont.temp = dut_data_arr[1];

            }
            return;
        }

        static void GoToNextDut(ref MessagesCommunicator _mmc)
        {
            Dutyara.need_a_stop = true;
            Dutyara.opened = true;
            dutemp_20 = "";
            dutemp_19 = (dutemp_19 + 1);
            if (dutemp_19 >= duttemp_26.Count)
            {
                // need_request = false;
                dutemp_19 %= duttemp_26.Count;
                SendToVialon(ref _mmc);
                neetemp_18 = false;
            }
            messtemp_21 = null;
        }

        private static void SendToVialon(ref MessagesCommunicator _mmc)
        {
            if (ztrmp_2)
            {
                //if (black_box.Count() > 2)
                //zateya = false;
                string params_string = String.Empty;
                int dl_len = duttemp_26.Count;
                Dutyara counter;
                for (int iterator = 0; iterator < dl_len; iterator++)
                {
                    counter = duttemp_26[iterator];
                    params_string += ViaDataFormater.GenerateString(counter, iterator);
                }
                params_string = params_string.Remove(params_string.Length - 1);
                var dts = DateTime.Now.ToUniversalTime().ToString("ddMMyy;HHmmss");
                //MessageBox.Show(params_string);
                //SendDutData(params_string, _mmc);
                //Thread.Sleep(1000);
                //SendDutData(params_string, _mmc);
                //Thread.Sleep(1000);
                //SendDutData(params_string, _mmc);
                //Thread.Sleep(1000);
                bool conn = false;
                istemp_24 = GetConnectedStatus();

                
                if (istemp_24 != oldtemp_25)
                {
                    if (istemp_24)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("\n\n\n Подключение к Интернету восстановлено \n\n\n");
                        Console.ResetColor();
                        oldtemp_25 = istemp_24;
                    }
                    else
                    {
                        Disconnect();

                        //_mc = null;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("\n\n\n Потеряна связь с сервером \n\n\n");
                        Console.ResetColor();
                        oldtemp_25 = istemp_24;
                        goto metka;
                    }

                }
                if (_mmc != null)
                {
                    conn = _mmc.IsConnected;
                }
                metka: 
                if (conn)
                {
                    
                    //timer_stopper.Enabled = false;
                    //var gg = timer_stopper.Interval;
                    
                    bltemp_27.Add(params_string + "zuzuzu" + dts);
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                    }

//#if !DEBUG
                    //LinuxLog(params_string + "zuzuzu" + dts.Insert(2, ".").Insert(5, ".").Insert(11, ":").Insert(14, ":"));
//#endif
//#if DEBUG
                    WindowsLog(params_string + "zuzuzu" + dts.Insert(2, ".").Insert(5, ".").Insert(11, ":").Insert(14, ":"));;
//#endif



                    if (bltemp_27.Count > 0)
                    {
                        //int ts = timer_stopper.Interval / 1000;
                        //timer_stopper.Stop();

                        //int step = 1;

                        //if (black_box.Count > ts + ts*20/100)
                        //{
                        //    step*=2;
                        //    if (black_box.Count > 2*(ts + ts * 20 / 100))
                        //    {
                        //        step *= 2;
                        //        if (black_box.Count > 4 * (ts + ts * 20 / 100))
                        //        {
                        //            step *= 2;
                        //            if (black_box.Count > 8 * (ts + ts * 20 / 100))
                        //            {
                        //                step *= 2;
                        //            }
                        //        }
                        //    }
                        //}
                        //for (int iterator = 0; iterator < black_box.Count; iterator += step)
                        //{
                        //    Console.WriteLine(black_box[iterator]);
                        //    SendDutData(black_box[iterator], _mmc);
                        //    Thread.Sleep(1000); // A nado?
                        //}
                        foreach (var item in bltemp_27)
                        {
                            Console.WriteLine(item);
                            SendDutData(item, _mmc);
                            Thread.Sleep(1000); // A nado?
                        }
                        bltemp_27.Clear();
                        //timer_stopper.Start();
                    }
                }
                else
                {
                    var tt = dts.Insert(2, ".").Insert(5, ".").Insert(11, ":").Insert(14, ":");
                    
                    bltemp_27.Add(params_string + "zuzuzu" + dts);
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                    }
//#if !DEBUG
                    //LinuxLog(params_string + "zuzuzu" + dts.Insert(2, ".").Insert(5, ".").Insert(11, ":").Insert(14, ":"));
//#endif
//#if DEBUG
                    WindowsLog(params_string + "zuzuzu" + dts.Insert(2, ".").Insert(5, ".").Insert(11, ":").Insert(14, ":"));;
//#endif

                    Console.WriteLine("Add");
                }
            }
        }

        static bool GetConnectedStatus()
        {
            Ping myPing = new Ping();
            String host = "google.com";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            try
            {
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void WindowsLog(string v)
        {
            var t1 = v.Split(';');
            // { [26.02.21](0)   [08:34:40](1)}
            var t2 = t1[1].Split(':');
            // [08](0) [34](1) [40](2)
            var t3 = (Convert.ToInt32(t2[0]) + 2).ToString();
            t2[0] = t3;
            if (epic_param_2 != epic_param_4)
            {
                if (epic_param_1 == 22)
                {
                    t1[1] = String.Join(":", t2);
                    v = String.Join(";", t1);
                }
                if (epic_param_3 == 21)
                {
                    SetLocalIndeferr(1, 4, v);
                }
                else
                {
                    SetLocalIndeferr(25, 16, v);
                }
            }



            string[] ss = { v };
            File.AppendAllLines(path, ss);
        }

        private static void SetLocalIndeferr(int v1, int v2, string v3)
        {
            if (v1 == 1)
            {
                GeoData();
            }
            else
            {
                Geophisicks();
            }
            
        }

        private static void Geophisicks()
        {
            float tt = Math.Abs(-12.4f);
        }

        private static void GeoData()
        {
            float tt = Math.Abs(-122.4f);
        }

        private static void LinuxLog(string msg)
        {
            Thread.Sleep(1000);
            // Open the file into a StreamReader
            StreamReader file = File.OpenText(path);
            // Read the file into a string
            string s = file.ReadToEnd();
            // Close the file so it can be accessed again.
            file.Close();

            // Add a line to the text
            s += msg + "\n";

            // Hook a write to the text file.
            StreamWriter writer = new StreamWriter(path);
            // Rewrite the entire value of s to the file
            writer.Write(s);
            writer.Close();
        }

        async static void SendDutData(string ips_params, MessagesCommunicator _mmc)
        {
            try
            {

                await Task.Run(() =>
                {
                    try
                    {
                        bool gg = true;
                    // Тут работа кипит
                    var l_dt = MyParseDateTime(DateTime.Now.ToUniversalTime());
                        string[] _itemp_33 = ips_params.Split(new string[] { "zuzuzu" }, StringSplitOptions.None);
                    //string t_msg = "#D#"+ l_dt[0] + ";"+ l_dt[1] + ";;;;;;;;;;;;;;";
                    //string t_msg1 = "#D#" + DateTime.Now.ToUniversalTime().ToString("ddMMyy;HHmmss") + ";;;;;;;;;;;;;;";
                    string t_msg = "#D#" + _itemp_33[1] + ";;;;;;;;;;;;;;";
                        t_msg += _itemp_33[0];

                    //var text = this.tbSendRaw.Text.Trim();
                    //this.tbSendRaw.Focus();
                    //this.tbSendRaw.SelectAll();
                    if (gg == true)
                        {
                            var msg = WialonIPS.Message.Parse(t_msg);
                            var vv = msg.GetType();
                            if (msg.Success)
                            {
                                _mmc.Send(msg);
                            }
                            else
                            {
                            // this.Log.PostHead("Emulator", "Unknown packet not sent: " + t_msg);
                            System.Media.SystemSounds.Exclamation.Play();
                            }
                        }

                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\n\n Ошибка в Хреновом потоке \n\n\n");
                        Console.ResetColor();
                    }
                }
                );
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n\n Ошибка в вызове потока \n\n\n");
                Console.ResetColor();
            }
        
        }

        // Получение текущей даты для пакетов.
        static List<string> MyParseDateTime(DateTime dt)
        {
            List<string> list_dt = new List<string>();
            var dd = dt.ToString("d").Replace(".", "");
            dd = dd.Substring(0, 4) + dd.Substring(6, 2);
            var tt = dt.ToString("T").Replace(":", "");
            list_dt.Add(dd);
            list_dt.Add(tt);
            return list_dt;
        }








        static private void PingTimerCallback(object state)
        {
            try
            {
                _temp_4.Send(new WialonIPS.PingMessage());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\n Ошибка в Пинге \n\n\n");
                Console.ResetColor();
            }
        }


        static void ConnectClick()
        {
            try
            {
                server_1 = new ServerInfo("77.123.137.100", 20332, "Wialon Hosting", true);
                server_2 = new DeviceInfo();
                server_2.ID = "1000005";

                //if (di == null)
                //    throw new DeviceNotSelected();

                if (_temp_4 != null)
                    _temp_4.Dispose();

                _temp_4 = new MessagesCommunicator(server_1.Hotemp_40, server_1.Potemp_41);
                _temp_4.OnConnect += new MessageConnectorOperation(_mc_OnConnect);
                _temp_4.OnDisconnect += new MessageConnectorOperation(_mc_OnDisconnect);
                _temp_4.OnReceive += new MessageOperationDelegate(_mc_OnReceive);
                _temp_4.OnSent += new MessageOperationDelegate(_mc_OnSent);
                _temp_4.Connect();
            }
            catch (DeviceNotSelected)
            {
                Console.WriteLine("Please create device first", "Cannot connect");
                // this.ShowConfigDevicesDialog();
            }
            catch (Exception exc)
            {
                // this.ActivateConnectControls(true);
                Console.WriteLine("(Log)Exception" + "Connect: " + exc);
                Console.WriteLine(exc.Message);
            }
        }

        static void Disconnect()
        {
            try
            {
                if (_temp_4 != null)
                {
                    if (_temp_4.IsConnected)
                        _temp_4.Disconnect();
                }
                // this.ActivateConnectControls(true);
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                //this.ActivateConnectControls(true);
                Console.WriteLine("(Log) Exception Disconnect: " + exc);
                Console.WriteLine(exc.Message);
                Console.ResetColor();
            }
        }
        // ТТУ вносим поправки !!!!!!
        static private void autoConnect_Tick(object state)
        {
            bool conn = false;
            //isConnectedStatus = GetConnectedStatus();
            if (istemp_24)
            {
                if (_temp_4 == null || !_temp_4.IsConnected)
                {
                    ConnectClick();
                }
                else
                    Console.WriteLine("Jopa");
            }

            //bool bb =this._mc.IsConnected;
        }


        static void _mc_OnConnect(MessagesCommunicator comm)
        {
            try
            {
                //this.ActivateConnectControls(false);
                Console.WriteLine("Emulator: " + "Connected to " + server_1);
                _temp_4.Send(new LoginMessage(server_2.ID, server_2.Password));
                if (Setemp_30.SendPingPackets)
                    tmtemp_9.Change(_temp_3, _temp_3);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\n Ошибка в онКоннекте \n\n\n");
                Console.ResetColor();
            }
        }

        static void _mc_OnDisconnect(MessagesCommunicator comm)
        {
            try
            {
                Console.WriteLine("Emulator: " + "Disconnected from " + server_1);
            tmtemp_9.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\n Ошибка в онДисКоннекте \n\n\n");
                Console.ResetColor();
            }
        }


        static void _mc_OnReceive(MessagesCommunicator comm, WialonIPS.Message msg)
        {
            try { 

            Console.WriteLine(">>>" + msg.ToString());
            if (msg.MsgType == MessageType.Message)
                Console.WriteLine((msg as WialonIPS.MessageMessage).Text);
                //this.Messages.Received((msg as WialonIPS.MessageMessage).Text);
            if (msg.MsgType == MessageType.LoginAns && !(msg as LoginAnsMessage).Success)
                Disconnect();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\n Ошибка в JOnRecive \n\n\n");
                Console.ResetColor();
            }
        }

        static void _mc_OnSent(MessagesCommunicator comm, WialonIPS.Message msg)
        {
            try { 
            //this.Log.PostHead("<<<", msg.ToString());
            //if (msg.MsgType == MessageType.Message)
            //    this.Messages.Sent((msg as WialonIPS.MessageMessage).Text);
            if (Setemp_30.SendPingPackets)
                tmtemp_9.Change(_temp_3, _temp_3);
        }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\n\n Ошибка в OnSent \n\n\n");
                Console.ResetColor();
            }
}

        #region Connect/Disconnect/Select items/Activate controls logic and handlers
        private class DeviceNotSelected : Exception { };
        #endregion


    }



    //public class CMessages
    //{
    //    public CMessages(TextBox output, AddToTextBoxDelegate add_to_messages)
    //    {
    //        this._output = output;
    //        this._add_to_messages = add_to_messages;
    //    }

    //    public void Sent(string text)
    //    {
    //        string res = DateTime.Now.ToString("yyyy\\/MM\\/dd HH:mm:ss: [You] ") + text;
    //        this._output.Invoke(this._add_to_messages, new Object[] { res });
    //    }

    //    public void Received(string text)
    //    {
    //        string res = DateTime.Now.ToString("yyyy\\/MM\\/dd HH:mm:ss: [Server] ") + text;
    //        this._output.Invoke(this._add_to_messages, new Object[] { res });
    //    }

    //    private TextBox _output;

    //    private AddToTextBoxDelegate _add_to_messages;

    //}


    /// <summary>
    /// Server information (host+port+name);
    /// </summary>
    public class ServerInfo
    {
        [XmlAttribute]
        public string Hotemp_40 { get; set; }
        [XmlAttribute]
        public UInt16 Potemp_41 { get; set; }
        [XmlAttribute]
        public string Natemp_42 { get; set; }
        [XmlIgnore]
        public bool ReTemp_43 { get; private set; }

        public ServerInfo()
        {
            this.Hotemp_40 = "";
            this.Potemp_41 = 0;
            this.Natemp_42 = "";
            this.ReTemp_43 = false;
        }

        public ServerInfo(string host, UInt16 port, string name, bool read_only)
        {
            this.Hotemp_40 = host;
            this.Potemp_41 = port;
            this.Natemp_42 = name;
            this.ReTemp_43 = read_only;
        }

        public override string ToString()
        {
            string res = this.Hotemp_40 + ":" + this.Potemp_41;
            if (this.Natemp_42 != "")
                res = this.Natemp_42 + " (" + res + ")";
            return res;
        }
    }

    [XmlRoot("Settings")]
    public class CSettings
    {
        [XmlArray]
        public List<ServerInfo> Servers { get; set; }
        [XmlArray]
        public List<DeviceInfo> Devices { get; set; }

        public int SelectedServer { get; set; }
        public int SelectedDevice { get; set; }

        //public FormWindowState LastWindowState { get; set; }
        //public Rectangle LastWindowRect { get; set; }

        public bool AutoConnect { get; set; }
        public bool SendPingPackets { get; set; }

        #region Standard behavior
        public CSettings()
        {
            this.Servers = new List<ServerInfo>();
            this.Devices = new List<DeviceInfo>();
            this.SendPingPackets = true;
        }

        public void Save(string file_name)
        {
            XmlSerializer s = new XmlSerializer(typeof(CSettings));
            TextWriter w = new StreamWriter(file_name);
            s.Serialize(w, this);
            w.Close();
        }

        public static CSettings Load(string file_name)
        {
            CSettings obj;
            XmlSerializer s = new XmlSerializer(typeof(CSettings));
            TextReader r = new StreamReader(file_name);
            obj = (CSettings)s.Deserialize(r);
            r.Close();
            return obj;
        }
        #endregion
    }

}
