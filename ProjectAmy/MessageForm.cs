using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syn.Bot;
using Syn.Bot.Events;
namespace ProjectAmy
{
    public partial class MessageForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        public string serverName;
        public String frmId { get; set; }
        String jidFrom;
        public string remoteUID = null;
        public SynBot synBot;
        public BotUser botUser;

        public MessageForm(string jid, string domain)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            jidFrom = jid;
            serverName = domain;
            
            this.Text = "Chat with: " + jid;
            synBot = new SynBot();
            botUser = new BotUser(synBot, jid);
            synBot.Learning += SynBot_Learning;
            synBot.Memorizing += synBot_Memorizing;
            synBot.Configuration.StoreVocabulary = true;
            synBot.Configuration.StoreExamples = true;
            synBot.Learning += SynBot_Learning;
            synBot.Memorizing += synBot_Memorizing;
            synBot.Configuration.StoreVocabulary = true;
            synBot.Configuration.StoreExamples = true;
            //Load brain files(siml) here...
            Program.xclient.SendMessage(jidFrom + "@" + serverName, "Loading Brain please wait...");
            var simlPackage = File.ReadAllText("package\\projectamy.simlpk");
            synBot.PackageManager.LoadFromString(simlPackage);
            try
            {
                var fileLearn = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "package\\Learned.siml"));
                synBot.AddSiml(fileLearn);
                Console.WriteLine("Loaded learning file...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Project Amy has not learned anything new yet...");
            }
            try
            {
                //var fileMem = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory() + "\\package", botUser.ID, "Memorized.siml"));
                //synBot.AddSiml(fileMem);
                var memPackage = File.ReadAllText("package\\" + jid + "\\memory.simlpk");
                synBot.PackageManager.LoadFromString(simlPackage);
                Console.WriteLine("Loaded personal user Memory file...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("No memory file for user " + botUser.ID);
            }
            Program.xclient.SendMessage(jidFrom + "@" + serverName, "Loaded! You may now send me messages!");
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {
            
        }
        public void _msgText(String jid, String Message)
        {
            appendMsg(jid, Message);
            var chatReq = new ChatRequest(Message, botUser);
            var chatResult = synBot.Chat(chatReq);
            Program.xclient.SendMessage(jidFrom + "@" + serverName, chatResult.BotMessage);
            appendMsg("Amy", chatResult.BotMessage);
        }
        public void appendMsg(String jid, String Message)
        {
            String timeNow = DateTime.Now.ToString("hh:mm:ss tt");
            if (jid == "Amy")
            {
                richTextBox1.SelectionColor = Color.Cyan;
                richTextBox1.AppendText("[" + timeNow + "]" + jid + ": ");
                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(Message + Environment.NewLine);
            }
            else
            {
                richTextBox1.SelectionColor = Color.LightGreen;
                richTextBox1.AppendText("[" + timeNow + "]" + jid + ": ");
                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(Message + Environment.NewLine);

            }
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();

        }
        static void SynBot_Learning(object sender, LearningEventArgs e)
        {
            Console.WriteLine("--------------------------------");
            try
            {
                Console.WriteLine("Writing to learn file...");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "package\\Learned.siml");
                e.Document.Save(filePath);
                Console.WriteLine("Saved learn file!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Write Failed: " + ex.Message);
            }
            Console.WriteLine("--------------------------------");
        }
        static void synBot_Memorizing(object sender, MemorizingEventArgs e)
        {
            Random random = new Random();
            string randNum = "";
            for (int i = 0; i < 10; i++)
            {
                int randomNumber = random.Next(0, 100);
                randNum += randomNumber.ToString();
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Writing to memory file for user: " + e.User.ID);
            try
            {
                if (Directory.Exists(Directory.GetCurrentDirectory() + "\\package\\" + e.User.ID))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\package", e.User.ID, "Memorized-" + randNum + ".siml");
                    e.Document.Save(filePath);
                }
                else
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\package\\" + e.User.ID);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\package", e.User.ID, "Memorized" + randNum + ".siml");
                    e.Document.Save(filePath);
                }
                Console.WriteLine("Write success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Write failed: " + ex.Message);
            }

            Console.WriteLine("--------------------------------");
        }


    }
}
