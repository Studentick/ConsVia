﻿using System;
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

namespace WiaCons
{
    class Program
    {
        static private int _ping_interval = 240000; // 4 minutes
        static private MessagesCommunicator _mc = null;
        static private bool _closed = false, _ready = false;
        static ServerInfo selected_server;
        static DeviceInfo selected_device;
        static private string _settings_file_name, _log_file_name;
        //private CMessages Messages;
        static private System.Threading.Timer tmrPing;
                
        static public CSettings Settings { get; private set; }

        /// =========================================================================================================
        ///Переменные для работы с дутами
        ///
        // Делегат для типа Timer
        static Stopwatch sw_timeout = new Stopwatch(); // Для проверки потраченного времени на опрос ДУТа
        static Stopwatch sw_request = new Stopwatch(); // Для проверки необходимости повторного опроса ДУТов
        static TimerCallback timeCB = new TimerCallback(tmrDutControl_Tick);
        static Timer tmrDutControl = new Timer(timeCB, null, 0, 30000);
        static bool need_request = true;
        static int dut_selected = 0;
        static string dut_data = "";
        public static int? message_status = null;
        static int request_time = 15000;
        static int time_to_dut_read = 5000;
        public const int MSG_SUCCESS = 1, MSG_FAIL = 0, MSG_DROP = -1, PORT_DROP = -2;
        const string FAIL_VALUE = "65536" /*Не верный формат данных*/, DROP_VALUE = "65533" /*Часть данных была потеряна*/,
            PORT_VALUE = "65530" /*Ошибка COM-порта*/;
        private static bool stopped_request = false;

        static List<Dutyara> dut_list = new List<Dutyara>();

        /// =========================================================================================================



        //// =========================================================================================================
        // Переменные для работы сервером виалоновстким 

        public static List<string> black_box = new List<string>();
        //public static System.Windows.Forms.Timer timer_stopper;

        //// =========================================================================================================



        static void Main(string[] args)
        {
            //Dutyara.GetPorts();
            dut_list.Add(new Dutyara(33722, 9600));
            Dutyara.GetPorts();
            //tmrDutControl.Change(time,);
            tmrPing = new System.Threading.Timer(new TimerCallback(PingTimerCallback), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            var base_path = Environment.GetEnvironmentVariable("USERPROFILE");
            if (base_path == null)
                base_path = Environment.GetEnvironmentVariable("HOME");
            if (base_path == null)
                base_path = Path.GetDirectoryName(Environment.CurrentDirectory);
            _settings_file_name = Path.Combine(base_path, ".wialon_ips_emulator.settings");


            try
            {
                Settings = CSettings.Load(_settings_file_name);
                Console.WriteLine("Emulator" + "Settings loaded successfully");
            }
            catch (Exception)
            {
                Console.WriteLine("Emulator" + "Cannot load settings");
                Settings = new CSettings();
            }


            ConnectClick();
            Console.ReadKey();
        }



        private static void tmrDutControl_Tick(object state)
        {
            need_request = true;
            while (need_request)
            {
                // Console.WriteLine(sw.ElapsedMilliseconds);
                if (Dutyara.opened)
                {
                    sw_timeout.Restart();
                    Dutyara.opened = false;
                    dut_list[dut_selected].GetData();
                    dut_list[dut_selected].SendMsg();
                    //GetAnsver();
                }
                else

                //if (!Dutyara.opened)
                {
                    //dut_data = "44N0=+210=01345.27=00632.55=094";
                    if (dut_data != "")
                    {
                        CheckData(dut_data);
                    }
                    // Так должно быть лучше, но нужно проверит, а на это нет времени:
                    else
                    if (sw_timeout.ElapsedMilliseconds > time_to_dut_read)
                    {
                        message_status = MSG_FAIL;
                    }

                    switch (message_status)
                    {
                        case MSG_FAIL:
                            dut_list[dut_selected].msg_cont.id = dut_list[dut_selected].Id.ToString();
                            dut_list[dut_selected].msg_cont.water = FAIL_VALUE;
                            dut_list[dut_selected].msg_cont.fuel = FAIL_VALUE;
                            dut_list[dut_selected].msg_cont.temp = FAIL_VALUE;
                            //dut_data = "44N0=65536=65536=65536=094";
                            break;
                        case MSG_DROP:
                            dut_list[dut_selected].msg_cont.id = dut_list[dut_selected].Id.ToString();
                            dut_list[dut_selected].msg_cont.water = DROP_VALUE;
                            dut_list[dut_selected].msg_cont.fuel = DROP_VALUE;
                            dut_list[dut_selected].msg_cont.temp = DROP_VALUE;
                            break;
                        case PORT_DROP:
                            dut_list[dut_selected].msg_cont.id = dut_list[dut_selected].Id.ToString();
                            dut_list[dut_selected].msg_cont.water = PORT_VALUE;
                            dut_list[dut_selected].msg_cont.fuel = PORT_VALUE;
                            dut_list[dut_selected].msg_cont.temp = PORT_VALUE;
                            break;
                        default:
                            break;
                    }

                    if (message_status != null)
                    {
                        //if (message_status != MSG_SUCCESS)
                        dut_data = dut_list[dut_selected].msg_cont.id + "N0=" + dut_list[dut_selected].msg_cont.temp + "=" +
                                        dut_list[dut_selected].msg_cont.fuel + "=" + dut_list[dut_selected].msg_cont.water + "=094"; //"44N0=65536=65536=65536=094";
                        Console.WriteLine(dut_data);

                        GoToNextDut(ref _mc);
                    }
                    else dut_data = dut_list[dut_selected].GetData();
                    Thread.Sleep(100);
                    //var rr = timer_stopper.Enabled;
                }

            }
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
                message_status = MSG_DROP;
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
                    message_status = MSG_DROP; return;
                }
                if (dut_data_arr[1][0] != '+' && dut_data_arr[1][0] != '-')
                {
                    message_status = MSG_DROP; return;
                }
                bb = float.TryParse(dut_data_arr[2].Replace(".", ","), out i);
                if (!bb)
                {
                    message_status = MSG_DROP; return;
                }
                bb = float.TryParse(dut_data_arr[3].Replace(".", ","), out i);
                if (!bb)
                {
                    message_status = MSG_DROP; return;
                }
                // Если айдишник отличается от запрашиваемого - данные считаются битыми, т.к. пришли от другого ДУТа
                var idish = dut_list[dut_selected].Id.ToString();
                if (dut_id != idish)
                {
                    message_status = MSG_DROP; Console.WriteLine("Err!"); return;
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

                message_status = MSG_SUCCESS;
                dut_list[dut_selected].msg_cont.id = dut_id;
                dut_list[dut_selected].msg_cont.fuel = ViaDataFormater.CorrectoinNull(dut_data_arr[2], dut_list[dut_selected].Corrector);
                dut_list[dut_selected].msg_cont.water = ViaDataFormater.CorrectoinNull(dut_data_arr[3], dut_list[dut_selected].Corrector);
                dut_list[dut_selected].msg_cont.temp = dut_data_arr[1];

            }
            return;
        }

        static void GoToNextDut(ref MessagesCommunicator _mmc)
        {
            Dutyara.need_a_stop = true;
            Dutyara.opened = true;
            dut_data = "";
            dut_selected = (dut_selected + 1);
            if (dut_selected >= dut_list.Count)
            {
                // need_request = false;
                dut_selected %= dut_list.Count;
                SendToVialon(ref _mmc);
                need_request = false;
            }
            message_status = null;
        }

        private static void SendToVialon(ref MessagesCommunicator _mmc)
        {
            string params_string = String.Empty;
            int dl_len = dut_list.Count;
            Dutyara counter;
            for (int iterator = 0; iterator < dl_len; iterator++)
            {
                counter = dut_list[iterator];
                params_string += ViaDataFormater.GenerateString(counter, iterator);
            }
            params_string = params_string.Remove(params_string.Length - 1);
            //MessageBox.Show(params_string);
            //SendDutData(params_string, _mmc);
            //Thread.Sleep(1000);
            //SendDutData(params_string, _mmc);
            //Thread.Sleep(1000);
            //SendDutData(params_string, _mmc);
            //Thread.Sleep(1000);
            bool conn = false;

            if (_mmc != null)
            {
                conn = _mmc.IsConnected;
            }
            if (conn)
            {
                //timer_stopper.Enabled = false;
                //var gg = timer_stopper.Interval;
                black_box.Add(params_string);
                if (black_box.Count > 0)
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
                    foreach (var item in black_box)
                    {
                        Console.WriteLine(item);
                        SendDutData(item, _mmc);
                        Thread.Sleep(1000); // A nado?
                    }
                    black_box.Clear();
                    //timer_stopper.Start();
                }
            }
            else
            {
                black_box.Add(params_string);
                Console.WriteLine("Add");
            }

        }


        async static void SendDutData(string ips_params, MessagesCommunicator _mmc)
        {
            await Task.Run(() =>
            {
                bool gg = true;
                // Тут работа кипит
                var l_dt = MyParseDateTime(DateTime.Now.ToUniversalTime());
                //string t_msg = "#D#"+ l_dt[0] + ";"+ l_dt[1] + ";;;;;;;;;;;;;;";
                string t_msg = "#D#" + DateTime.Now.ToUniversalTime().ToString("ddMMyy;HHmmss") + ";;;;;;;;;;;;;;";
                t_msg += ips_params;
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
            );
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
            _mc.Send(new WialonIPS.PingMessage());
        }


        static void ConnectClick()
        {
            try
            {
                selected_server = new ServerInfo("77.123.137.100", 20332, "Wialon Hosting", true);
                selected_device = new DeviceInfo();
                selected_device.ID = "1000005";

                //if (di == null)
                //    throw new DeviceNotSelected();

                if (_mc != null)
                    _mc.Dispose();

                _mc = new MessagesCommunicator(selected_server.Host, selected_server.Port);
                _mc.OnConnect += new MessageConnectorOperation(_mc_OnConnect);
                _mc.OnDisconnect += new MessageConnectorOperation(_mc_OnDisconnect);
                _mc.OnReceive += new MessageOperationDelegate(_mc_OnReceive);
                _mc.OnSent += new MessageOperationDelegate(_mc_OnSent);
                _mc.Connect();
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
                if (_mc != null)
                {
                    if (_mc.IsConnected)
                        _mc.Disconnect();
                }
                // this.ActivateConnectControls(true);
            }
            catch (Exception exc)
            {
                //this.ActivateConnectControls(true);
                Console.WriteLine("(Log) Exception Disconnect: " + exc);
                Console.WriteLine(exc.Message);
            }
        }

        static void _mc_OnConnect(MessagesCommunicator comm)
        {
            //this.ActivateConnectControls(false);
            Console.WriteLine("Emulator: " + "Connected to " + selected_server);
            _mc.Send(new LoginMessage(selected_device.ID, selected_device.Password));
            if (Settings.SendPingPackets)
                tmrPing.Change(_ping_interval, _ping_interval);
        }

        static void _mc_OnDisconnect(MessagesCommunicator comm)
        {
            Console.WriteLine("Emulator: " + "Disconnected from " + selected_server);
            tmrPing.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }


        static void _mc_OnReceive(MessagesCommunicator comm, WialonIPS.Message msg)
        {
            Console.WriteLine(">>>" + msg.ToString());
            if (msg.MsgType == MessageType.Message)
                Console.WriteLine((msg as WialonIPS.MessageMessage).Text);
                //this.Messages.Received((msg as WialonIPS.MessageMessage).Text);
            if (msg.MsgType == MessageType.LoginAns && !(msg as LoginAnsMessage).Success)
                Disconnect();
        }

        static void _mc_OnSent(MessagesCommunicator comm, WialonIPS.Message msg)
        {
            //this.Log.PostHead("<<<", msg.ToString());
            //if (msg.MsgType == MessageType.Message)
            //    this.Messages.Sent((msg as WialonIPS.MessageMessage).Text);
            if (Settings.SendPingPackets)
                tmrPing.Change(_ping_interval, _ping_interval);
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
        public string Host { get; set; }
        [XmlAttribute]
        public UInt16 Port { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlIgnore]
        public bool ReadOnly { get; private set; }

        public ServerInfo()
        {
            this.Host = "";
            this.Port = 0;
            this.Name = "";
            this.ReadOnly = false;
        }

        public ServerInfo(string host, UInt16 port, string name, bool read_only)
        {
            this.Host = host;
            this.Port = port;
            this.Name = name;
            this.ReadOnly = read_only;
        }

        public override string ToString()
        {
            string res = this.Host + ":" + this.Port;
            if (this.Name != "")
                res = this.Name + " (" + res + ")";
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