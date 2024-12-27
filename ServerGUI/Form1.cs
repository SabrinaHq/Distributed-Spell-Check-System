
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

namespace ServerGUI
{
    public partial class Form1 : Form
    {
        Socket sck,backupsck;
        Socket acc;
        int count = 0;
        bool isconnected = false;
        public Form1()
        {
            InitializeComponent();
        }

        private ArrayList nSockets;

        Socket socket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /* Client-Server data transfer was done following tutorial:
         https://technotif.com/creating-simple-tcpip-server-client-transfer-data-using-c-vb-net/
         *
         ******************************************************/

        private void Form1_Load(object sender, EventArgs e)
        {
            nSockets = new ArrayList();
            isconnected = true;
            InitTimer();
            Thread userthread = new Thread(new ThreadStart(() => SpellingCheck()));
            userthread.Start();
            backupsck = socket();
            backupsck.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1005));


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
            timer1.Interval = 30000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            addtoFile();

        }

        public void SpellingCheck()
        {

            sck = socket();
            sck.Bind(new IPEndPoint(0, 3));
            sck.Listen(100);
            MessageBox.Show("Server is listening.....");



            while (true)
            {
                try
                {
                    acc = sck.Accept();
                    Control.CheckForIllegalCrossThreadCalls = false;
                    usernameCheck();
                }
                catch
                {

                }
               


            }


        }

        // Checks is the username is valid
        public void usernameCheck()
        {
            if (count < 3 && isconnected == true)
            {


                byte[] userinfo = new byte[255];
                try
                {
                    int rec_info = acc.Receive(userinfo, 0, userinfo.Length, 0);
                    string usernameinput = Encoding.Default.GetString(userinfo);
                    string filename = usernameinput + ".txt";

                    SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Star\Documents\user.mdf;Integrated Security=True;Connect Timeout=30;");
                    con.Open();

                    SqlCommand cmd = new SqlCommand("select * from login where username='" + usernameinput + "'", con);
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        MessageBox.Show("Username already exists please try another ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        usernameCheck();


                    }
                    else
                    {
                        dr.Close();
                        cmd = new SqlCommand("insert into login values(@username,@ipaddress)", con);
                        cmd.Parameters.AddWithValue("username", usernameinput);
                        cmd.Parameters.AddWithValue("ipaddress", acc.RemoteEndPoint.ToString());
                        cmd.ExecuteNonQuery();
                        MessageBox.Show(usernameinput, " CONNECTED", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        count++;

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
                    MessageBox.Show("Client unexpectedly closed connection!");
                    acc.Close();
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
        Queue<string> serverQueue = new Queue<string>();

        // Add new words to the lexicon

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
            string filename = "lexicon.txt";
            
            string newfile = Path.Combine(foldername, filename);

            while (serverQueue.Count > 0)
            {
                string lexicon = File.ReadAllText(newfile);

                StreamWriter sw1 = File.AppendText(newfile);
                string s = serverQueue.Dequeue().ToString();

               

                if (lexicon.Contains(s) == false)
                {
                    byte[] userdata = Encoding.Default.GetBytes(s);
                    backupsck.Send(userdata, 0, userdata.Length, 0);
                    sw1.WriteLine(s);
                    receivedword.Items.Add(s);
                }

                sw1.Close();
            }


        }

        /* Used to add words to server side queue*/

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
            catch { }



        }


        //Does the spelling check

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
                                string newfile = Path.Combine(foldername, filename);
                                StreamWriter sw1 = new StreamWriter(newfile, false);
                                sw1.Write(Encoding.Default.GetString(buffer));
                                sw1.Close();
                                MessageBox.Show("File received by server", "Server", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);


                                /****************************Spelling Check*************************************************
                                 * 
                                 *          https://stackoverflow.com/questions/38416265/c-sharp-checking-if-a-word-is-in-an-english-dictionary
                                 *          
                                 ********************************************************************************************/
                                string inputfilename = "lexicon.txt";
                                string newinputfile = Path.Combine(foldername, inputfilename);

                                string lexicon = File.ReadAllText(newinputfile);
                                string text = File.ReadAllText(newfile);

                                string[] inputwords = text.Split();
                                inputwords = inputwords.Distinct().ToArray();
                                string[] lexiconwords = lexicon.Split();

                                StreamWriter sw = new StreamWriter(Path.Combine(foldername, "new.txt"), false);


                                foreach (string s in inputwords)
                                {
                                    {
                                        if(lexiconwords.Contains(s))
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
                              /*foreach(string s in inputwords)
                                {
                                    string pattern=@"\b" + Regex.Escape(s) + @"\b";
                                    foreach (string lex in lexiconwords)
                                    {
                                        
                                        if (Regex.Match(lex, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                                        {
                                            string s1 = "[" + s + "]";
                                            if (s.Length != 0)
                                            {
                                                text = text.Replace(s, s1);
                                            }


                                            s1 = null;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }*/
                                    


                                
                                sw.WriteLine(text);
                                sw.Close();
                                

                                File.Replace(Path.Combine(foldername, "new.txt"), newfile, null);



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

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < nSockets.Count; i++)
            {
                Socket acc = (Socket)nSockets[i];
                try
                {
                    /*MemoryStream memoryStream = new MemoryStream();
                    memoryStream.WriteByte((int)'@');
                    acc.Send(memoryStream.ToArray());*/
                    acc.Shutdown(SocketShutdown.Both);

                    acc.Disconnect(true);
                    acc.Close();
                }
                catch
                {

                }
            }
            //sck.Shutdown(SocketShutdown.Both);
            //sck.Disconnect(true);
            //sck.Close();
            sck.Dispose();

            MessageBox.Show("Server Disconnected!", null, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            Application.Exit();
        }

        /*
         * Opening folder dialog to select a folder in server:
       https://www.google.com/search?q=open+folder+dialog+c%23&oq=op&aqs=chrome.2.69i59l3j69i57j0j69i61j69i60l2.2871j1j4&sourceid=chrome&ie=UTF-8
         *
         ****************************************************************/

        string foldername;
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                foldername = @folderBrowserDialog.SelectedPath;
                textBox1.Text = foldername;
            }

            string inputfilename = "lexicon.txt";
            string newinputfile = Path.Combine(foldername, inputfilename);
            byte[] lexiconfile = Encoding.Default.GetBytes(newinputfile);

            backupsck.SendFile(newinputfile);

        }
    }

}
