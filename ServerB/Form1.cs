
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                                                        STUDENT NAME:SABRINA HAQUE
                                                            UTA ID:1001843912
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.SqlClient;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using Timer = System.Windows.Forms.Timer;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServerB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private ArrayList nSockets;

        Socket sck, serversck;
        Socket acc, serveracc;
        int count = 0;
        bool isconnected = false;
        int serverdown;
        Socket socket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            nSockets = new ArrayList();
            isconnected = true;
            Thread userthread = new Thread(new ThreadStart(() => SpellingCheck()));
            userthread.Start();
            serverdown = 0;


        }

        public void SpellingCheck()
        {

            sck = socket();
            serversck = socket();
            sck.Bind(new IPEndPoint(0, 1003));
            serversck.Bind(new IPEndPoint(0, 1005));
            sck.Listen(100);
            serversck.Listen(100);
            MessageBox.Show("Backup Server is listening.....");
            serveracc = serversck.Accept();
            MessageBox.Show("Backup server connected");
            ThreadStart thdstHandler = new ThreadStart(lexiconCreate);

            Thread thdHandler = new Thread(thdstHandler);
            thdHandler.Start();
            // lexiconCreate();

            while (true)
            {

                try
                {
                    acc = sck.Accept();


                    if (serverdown == 0)
                    {
                        MessageBox.Show("Primary server down", " Backup server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                        Invoke((MethodInvoker)delegate
                        {
                            InitTimer();

                        });


                    }
                    serverdown = 1;
                    //MessageBox.Show("Client connected to backup server");
                    Control.CheckForIllegalCrossThreadCalls = false;

                    usernameCheck();


                }
                catch
                {

                }


            }



        }

        public void usernameCheck()
        {

            if (count < 3 && isconnected == true)
            {


                byte[] userinfo = new byte[255];
                try
                {
                    int rec_info = acc.Receive(userinfo, 0, userinfo.Length, 0);
                    string usernameinput = Encoding.Default.GetString(userinfo);


                    SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Star\Documents\backup.mdf;Integrated Security=True;Connect Timeout=30;");
                    connection.Open();
                    string filename = usernameinput + ".txt";

                    SqlCommand command = new SqlCommand("select * from Backuplogintable where username='" + usernameinput + "'", connection);
                    SqlDataReader dr = command.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        MessageBox.Show("Username already exists please try another ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        usernameCheck();


                    }
                    else
                    {
                        dr.Close();
                        command = new SqlCommand("insert into Backuplogintable values(@username,@ipaddress)", connection);
                        command.Parameters.AddWithValue("username", usernameinput);
                        command.Parameters.AddWithValue("ipaddress", acc.RemoteEndPoint.ToString());
                        command.ExecuteNonQuery();
                        MessageBox.Show(usernameinput, " CONNECTED to backup server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                        Invoke((MethodInvoker)delegate
                        {
                            ConnectedClient.Items.Add(acc.RemoteEndPoint.ToString() + "->" + usernameinput);
                        }
                            );
                        lock (this)
                        {
                            nSockets.Add(acc);

                        }


                        ThreadStart thdstHandler = new ThreadStart(handlerThread);

                        Thread thdHandler = new Thread(thdstHandler);
                        thdHandler.Start();

                        //************************

                    }


                }
                catch
                {
                    //  MessageBox.Show("Client unexpectedly closed connection!");
                    MessageBox.Show("Client unexpectedly closed connection!", "Backup server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    acc.Close();
                    acc.Dispose();
                }



            }
            else if (count < 3 && isconnected == false)
            {
                MessageBox.Show("Server is disconnected");
                acc.Close();
            }

            else
            {
                MessageBox.Show("Client Limit exceeded");
                acc.Close();

            }

        }

        private Timer timer1;

        /* Used to poll client every 60 seconds:
         * https://stackoverflow.com/questions/6169288/execute-specified-function-every-x-seconds
         * 
         * ******************************************************************************************/

        public void InitTimer()
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 60000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            addtoFile();

        }

        public void addtoFile()
        {
            for (int i = 0; i < nSockets.Count; i++)
            {
                Socket acc = (Socket)nSockets[i];
                try
                {
                    MemoryStream memoryStream = new MemoryStream();
                    memoryStream.WriteByte((int)'*');
                    acc.Send(memoryStream.ToArray());
                }
                catch
                {

                }
            }
            // string filename = "newlexicon.txt";

            string newfile = "D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData\\newlexicon.txt";

            while (serverQueue.Count > 0)
            {
                string lexicon = File.ReadAllText(newfile);

                StreamWriter sw1 = File.AppendText(newfile);
                string s = serverQueue.Dequeue().ToString();



                if (lexicon.Contains(s) == false)
                {
                    byte[] userdata = Encoding.Default.GetBytes(s);
                    // backupsck.Send(userdata, 0, userdata.Length, 0);

                    sw1.WriteLine(s);
                    receivedword.Items.Add(s);
                }

                sw1.Close();
            }


        }

        public void handlerThread()
        {
            Socket acc = (Socket)nSockets[nSockets.Count - 1];
            string socketip = acc.RemoteEndPoint.ToString();

            while (acc != null)
            {

                try
                {

                    {
                        NetworkStream ns = new NetworkStream(acc);
                        int formatByte = ns.ReadByte(); //To differentiate between file and word to add in lexicon
                        byte[] formatbuffer = BitConverter.GetBytes(formatByte);
                        int arrsize = formatbuffer.Length;
                        Array.Resize(ref formatbuffer, arrsize);
                        if (formatByte == (int)'a')
                            lexiconAdd(acc);
                        else
                        {

                            //*************************************************************
                            byte[] buffer = new byte[1024];
                            int rec = acc.Receive(buffer, 0, buffer.Length, 0);

                            if (rec <= 0)
                            {
                                throw new SocketException();
                            }

                            Array.Resize(ref buffer, rec);

                            Invoke((MethodInvoker)delegate
                            {

                                string filename = ((IPEndPoint)acc.RemoteEndPoint).Port.ToString() + ".txt";


                                string newfile = Path.Combine("D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData", filename);
                                StreamWriter sw1 = new StreamWriter(newfile, false);
                                sw1.Write(Encoding.Default.GetString(buffer));
                                sw1.Close();
                                MessageBox.Show("File received by server", "Server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);


                                /****************************Spelling Check*************************************************
                                 * 
                                 *          https://stackoverflow.com/questions/38416265/c-sharp-checking-if-a-word-is-in-an-english-dictionary
                                 *          
                                 ********************************************************************************************/
                                //string inputfilename = "newlexicon.txt";
                                string newinputfile = "D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData\\newlexicon.txt";

                                string lexicon = File.ReadAllText(newinputfile);
                                string text = File.ReadAllText(newfile);

                                string[] inputwords = text.Split();
                                inputwords = inputwords.Distinct().ToArray();

                                string[] lexiconwords = lexicon.Split();

                                StreamWriter sw = new StreamWriter("D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData\\new.txt", false);


                                foreach (string s in inputwords)
                                {
                                    {
                                        if (lexiconwords.Contains(s))
                                        {
                                            string s1 = "[" + s + "]";
                                            if (s.Length != 0)
                                            {
                                                string find = string.Format(@"\b{0}\b", s);
                                                text = Regex.Replace(text, find, s1);
                                            }


                                            s1 = null;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                sw.WriteLine(text);
                                sw.Close();

                                File.Replace("D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData\\new.txt", newfile, null);



                                //*******************************************Send Back to Client****************************************
                                MemoryStream memoryStream = new MemoryStream();
                                memoryStream.WriteByte((int)'f');
                                byte[] formatdata = memoryStream.ToArray();
                                acc.Send(formatdata);

                                byte[] correctedfile = Encoding.Default.GetBytes(newfile);

                                acc.SendFile(newfile);


                            });
                        }

                    }



                }

                catch
                {
                    Invoke((MethodInvoker)delegate
                    {
                        for (int i = ConnectedClient.Items.Count - 1; i > -1; i--)
                        {
                            {
                                if (((string)ConnectedClient.Items[i]).Contains(socketip))
                                {
                                    ConnectedClient.Items.RemoveAt(i);
                                }
                            }
                        }

                        count--;


                        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Star\Documents\user.mdf;Integrated Security=True;Connect Timeout=30;");
                        con.Open();

                        SqlCommand cmd = new SqlCommand("delete from login where ipaddress='" + socketip + "'", con);
                        SqlDataReader dr = cmd.ExecuteReader();
                        acc = null;
                    }
                        );



                }
            }


        }
        Queue<string> serverQueue = new Queue<string>();

        private void ConnectedClient_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < nSockets.Count; i++)
            {
                Socket acc = (Socket)nSockets[i];
                try
                {

                    acc.Shutdown(SocketShutdown.Both);

                    acc.Disconnect(true);
                    acc.Close();
                }
                catch
                {

                }
            }

            sck.Dispose();

            MessageBox.Show("Backup Server Disconnected!\n", null, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            Application.Exit();
        }

        public void lexiconAdd(Socket acc)
        {


            try
            {
                byte[] lexicon = new byte[255];

                int rec_info = acc.Receive(lexicon, 0, lexicon.Length, 0);

                using (var memoryStream = new MemoryStream())
                {

                    memoryStream.Write(lexicon, 0, lexicon.Length);
                    memoryStream.Position = 0;

                    BinaryFormatter bf = new BinaryFormatter();
                    string newLexicon = (string)bf.Deserialize(memoryStream);


                    serverQueue.Enqueue(newLexicon);


                }

            }
            catch
            {
            }



        }

        void lexiconCreate()
        {
            byte[] buffer = new byte[1024];
            int rec = serveracc.Receive(buffer, 0, buffer.Length, 0);

            if (rec <= 0)
            {
                throw new SocketException();
            }

            Array.Resize(ref buffer, rec);
            string newfile = "D:\\UTA(Spring'2021)\\CSE 5306\\Data\\ServerData\\newlexicon.txt";
            Invoke((MethodInvoker)delegate
            {

                StreamWriter sw1 = new StreamWriter(newfile, false);
                sw1.Write(Encoding.Default.GetString(buffer));
                sw1.Close();
                MessageBox.Show("File received by Backup server", "Backup Server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            });


            while (serveracc != null)
            {
                byte[] newWord = new byte[255];
                try
                {
                    int rec_info = serveracc.Receive(newWord, 0, newWord.Length, 0);
                    string addLexicon = Encoding.Default.GetString(newWord);
                    StreamWriter sw1 = File.AppendText(newfile);
                    sw1.WriteLine(addLexicon);
                    sw1.Close();
                   // MessageBox.Show("Backup lexicon changed");
                }
                catch
                {

                }
            }
        }
    }


}
