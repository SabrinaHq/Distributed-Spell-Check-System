
/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                                                       STUDENT NAME:SABRINA HAQUE
                                                           UTA ID:1001843912
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;



/* Client-Server data transfer was done following a youtube tutorial:
  https://www.youtube.com/watch?v=651yVDINPBY&list=PLAC179D21AF94D28F&index=5
 *
 ******************************************************************/

namespace ClientGUI
{
    public partial class Form1 : Form
    {
        Socket sck;

        public Form1()
        {
            InitializeComponent();
            sck = socket();
        }

        Socket socket()
        {

            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //sck.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3));
            try
            {
                sck.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3));
            }
            catch
            {
                sck.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1003));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            try
            {

                new Thread(() =>
                {
                    read();
                }).Start();

            }

            catch
            {
                MessageBox.Show("Connection failed!");
                Application.Exit();
            }



        }
        int formatByte;

        // Sends username and waits for data from server
        void read()
        {
            
            string usernamestring = username.Text;
            try
            {
                byte[] userdata = Encoding.Default.GetBytes(username.Text);
                sck.Send(userdata, 0, userdata.Length, 0);

                while (true)
                {

                    NetworkStream ns = new NetworkStream(sck);
                    int formatByte = ns.ReadByte();
                    if (formatByte == (int)'*')
                    {
                        string w = new string("");
                        MessageBox.Show(username.Text, "Polled");

                        if (lexiconarray.Count > 0)
                        {

                            Invoke((MethodInvoker)delegate
                            {
                                for (int i = 0; i < wordlist.Items.Count; i++)
                                {
                                    foreach (string s in lexiconarray)
                                    {
                                        if (((string)wordlist.Items[i]).Contains(s))
                                        {
                                            wordlist.Items.RemoveAt(i);
                                        }
                                        addedword.Items.Add(s);
                                    }
                                }

                            });
                            lexiconarray.Clear();

                        }

                    }


                    else
                    {

                        byte[] buffer = new byte[1024];
                        int rec = sck.Receive(buffer, 0, buffer.Length, 0);

                        if (rec <= 0)
                        {
                            throw new SocketException();
                        }

                        Array.Resize(ref buffer, rec);

                        Invoke((MethodInvoker)delegate
                        {

                            StreamWriter swclient = new StreamWriter(textBox2.Text, false);
                            swclient.WriteLine(Encoding.Default.GetString(buffer));
                            swclient.Close();
                            MessageBox.Show("Spelling check complete,file received by " + username.Text, "Client", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                        });
                    }



                }


            }

            catch
            {

                handledisconnection();
                

            }
        
             


        }

        void handledisconnection()
        {
            if (clientgone == true)
            {
                
            }
              
            else
            {
                MessageBox.Show("Primary Server Unavailable!");
                sck.Shutdown(SocketShutdown.Both);

                sck.Disconnect(true);
                sck.Close();
                sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sck.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1003));



                new Thread(() =>
                {
                    read();
                }).Start();

            }

        }
        /*
         * Opening file dialog to select the text file:
        https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog?view=net-5.0
         *
         **************************************************/


        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            textBox2.Text = openFileDialog.FileName;
        }

        bool clientgone = false;

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            //sck.Close();
            MessageBox.Show(username.Text + " closed the connection", null, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //sck.Dispose();
            clientgone = true;
            sck.Close();
            Application.Exit();
        }

        /*Send file:
         * 
         * https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendfile?view=net-5.0
         ************************************************************************/

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.WriteByte((int)'b');
                byte[] formatdata = memoryStream.ToArray();
                sck.Send(formatdata);

                byte[] data = Encoding.Default.GetBytes(textBox2.Text);

                sck.SendFile(textBox2.Text);
            }
            catch
            {
                MessageBox.Show("Server not responding!");
            }

        }
        /*client side word queue*/
        Queue<string> addlexicon = new Queue<string>();
        List<string> lexiconarray = new List<string>();
       
        //Adds words to the client queue
        private void Enter_Click(object sender, EventArgs e)
        {

            addlexicon.Enqueue(LexiconTextBox.Text);
            wordlist.Items.Add(LexiconTextBox.Text);



            using (MemoryStream memoryStream = new MemoryStream())
                while (addlexicon.Count > 0)
                {
                    string newWord = addlexicon.Dequeue();
                    lexiconarray.Add(newWord);


                    {
                        // Ref: https://stackoverflow.com/questions/7906300/sending-multiple-type-of-data-from-a-single-network-stream-in-c-sharp

                        memoryStream.WriteByte((int)'a');// Store data type header for type 'a' to differentiate word from file
                        var bf = new BinaryFormatter();
                        bf.Serialize(memoryStream, newWord);

                        byte[] writeData = memoryStream.ToArray();

                        sck.Send(writeData, 0, writeData.Length, 0);
                    }
                }






        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
