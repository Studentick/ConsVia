using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiaCons.Properties;

namespace WiaCons
{
    class Dutyara
    {
        int id;
        int speed;
        static Random rnd = new Random();
        static public bool temptemp_1 = true;
        public static int temptemp_2;
        static public bool temptemp_3 = false;


        static SerialPort temptemp_4 = new SerialPort();
        static List<string> temptemp_5 = new List<string>();
        static string temptemp_6 = "";
        static int temp_temp_7 = 9600;
        static bool temptemp_8 = true;

        public MessageContent temptemp_9;
        private float temptemp_10;


        public int Id
        {
            get
            {
                return id;
            }
        }

        public float Corrector
        {
            get
            {
                return temptemp_10;
            }

            set
            {
                if (value >= 0)
                    temptemp_10 = value;
            }
        }

        public struct MessageContent
        {
            public string id;
            public string fuel;
            public string water;
            public string temp;
        }

        public Dutyara(int id, int speed = 9600, float corrector = 0)
        {
            this.id = id;
            this.speed = speed;
            this.temptemp_10 = corrector;
            temptemp_9 = new MessageContent();
        }


        public string FuncFunc1()
        {
            if (temptemp_8 == true)
                try
                {
                    byte[] inp;
                    int iii = 0;
                    string iiii = "";
                    string iiiii = "";


                    if (temptemp_4.PortName != temptemp_6)
                    {
                        temptemp_4.Close();
                        temptemp_4.PortName = temptemp_6;
                    }
                    if (temptemp_4.BaudRate != temp_temp_7)
                    {
                        temptemp_4.Close();
                        temptemp_4.BaudRate = temp_temp_7;
                    }
                    if (temptemp_4.Parity != System.IO.Ports.Parity.None)
                    {
                        temptemp_4.Close();
                        temptemp_4.Parity = System.IO.Ports.Parity.None;
                    }


                    if (!temptemp_4.IsOpen)
                        temptemp_4.Open();

                    if (temptemp_4.IsOpen)
                    {
                        inp = new Byte[4096];
                        iii = 0;

                        if (temptemp_4.BytesToRead > 0)    //если пришли данные
                        {
                            iiii = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00") + "." + DateTime.Now.Millisecond.ToString("000");
                            iii = temptemp_4.BytesToRead;              
                            temptemp_4.Read(inp, 0, temptemp_4.BytesToRead);  


                            if (iii > 0)
                            {
                                iiiii = "";

                                for (Int32 i = 0; i < iii; i++)
                                    iiiii += " " + FunkFunk6(inp[i]);   

                                iiiii = Encoding.ASCII.GetString(inp, 0, iii); 
                                // Console.WriteLine("ASCII: " + message);
                                return iiiii;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Dutyara.FuncFunk2();
                }
            return "";

        }



        static public void FuncFunk2()
        {

            try
            {
                string[] vvv = SerialPort.GetPortNames(); 
                int i;

                for (i = 0; i < vvv.Length; i++)
                    temptemp_5.Add(vvv[i]); 

                if (vvv.Length >= 1 )
                {
                    int ppppp = temptemp_5.IndexOf(Settings.Default.def_port);
                    if (ppppp != -1 && FunkFunk3(vvv))
                    {
                        temptemp_6 = Settings.Default.def_port;
                    }
                    else
                    {
                        Console.WriteLine("Выберите порт:");

                        for (int counter = 0; counter < vvv.Length; counter++)
                        {
                            Console.WriteLine("[" + counter.ToString() + "] " + vvv[counter].ToString());
                        }
                        int oooooo = int.Parse(Console.ReadLine());
                        temptemp_6 = vvv[oooooo];
                        Settings.Default.def_port = temptemp_6;
                        Settings.Default.Save();
                    }
                }
                else
                {
                    temptemp_6 = Settings.Default.def_port;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        static bool FunkFunk3(string[] ports)
        {
            bool mmmmmm = false;
            foreach (var item in ports)
            {
                if (item == Settings.Default.def_port)
                {
                    mmmmmm = true;
                    break;
                }
            }
            return mmmmmm;
        }

        public void FunkFunk4()
        {
            string nnnnnnnnnn = "4D ", iiiiiiiiiiiii = " 0D";
            byte[] ggg = Encoding.ASCII.GetBytes(this.id.ToString());
            string gggg = "";
            for (Int32 i = 0; i < ggg.Length; i++)
                gggg += " " + FunkFunk6(ggg[i]);
            gggg = nnnnnnnnnn + gggg + iiiiiiiiiiiii;
            byte[] qqqqqqqqqqq = FunkFunk7(gggg.Replace(" ", ""));
            int bl = qqqqqqqqqqq.Length;
            try
            {
                temptemp_4.Write(qqqqqqqqqqq, 0, bl);
            }
            catch (Exception ex)
            {
            }
        }

        public static byte[] FunkFunk5(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] qwe = new byte[hex.Length / 2];
            for (int i = 0; i < qwe.Length; i++)
            {
                qwe[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return qwe;
        }

        private static string FunkFunk6(byte b)
        {
            try
            {
                int ttttttttttttt = b / (byte)16;
                int yyy = b % (byte)16;
                string yyyyyyy = "";

                if (ttttttttttttt < 10)
                    yyyyyyy = ttttttttttttt.ToString();
                else
                {
                    if (ttttttttttttt == 10) yyyyyyy = "A";
                    if (ttttttttttttt == 11) yyyyyyy = "B";
                    if (ttttttttttttt == 12) yyyyyyy = "C";
                    if (ttttttttttttt == 13) yyyyyyy = "D";
                    if (ttttttttttttt == 14) yyyyyyy = "E";
                    if (ttttttttttttt == 15) yyyyyyy = "F";
                }

                if (yyy < 10)
                    yyyyyyy += yyy.ToString();
                else
                {
                    if (yyy == 10) yyyyyyy += "A";
                    if (yyy == 11) yyyyyyy += "B";
                    if (yyy == 12) yyyyyyy += "C";
                    if (yyy == 13) yyyyyyy += "D";
                    if (yyy == 14) yyyyyyy += "E";
                    if (yyy == 15) yyyyyyy += "F";
                }

                return yyyyyyy;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static byte[] FunkFunk7(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


    }

}
