using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syn.Bot;
using System.Reflection;
using System.IO;
using Syn.Bot.Events;
using SimpleTCP;
using System.Xml.Linq;
using System.Collections;
using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Core;
using S22.Xmpp.Im;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace ProjectAmy
{
    class Program
    {

        public static XmppClient xclient = new XmppClient("urgero.org", 5222, true);
        public static bool firstRun = false;
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frm = new Form1();
            Thread t = new Thread(ConsoleInputThreadProc);
            t.Start(frm);
            Application.Run(frm);
            //---------------------------------------------------//

        }
        static void ConsoleInputThreadProc(object state)
        {
            xclient.Message += OnNewMessage;
            Console.WriteLine("ProjectAmy - Ver. 1.0");
            var synBot = new SynBot();
            Console.Write("Enter a username: ");
            String user = Console.ReadLine();
            var botUser = new BotUser(synBot, user);
            Console.WriteLine();
            //NOT A RELIABLE WAY TO GET USERS PASSWORD, JUST A QUICK AND DIRTY DEMO!!!!!
            Console.Write("Enter A Password: ");
            String pass = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("ProjectAmy - Ver. 1.0");
            Console.WriteLine();
            Console.WriteLine("Please wait, contacting XMPP Server...");
            try
            {
                xclient.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to server, you can still chat in console though." + Environment.NewLine + ex.ToString());
            }
            if (xclient.Connected == true)
            {
                try
                {
                    xclient.Authenticate(user, pass);
                }
                catch (System.Security.Authentication.AuthenticationException ex)
                {
                    Console.WriteLine("Authentication failure!." + Environment.NewLine + ex.ToString());
                }

                if (xclient.Authenticated == true)
                {
                    Console.WriteLine("Logged into urgero.net:5222 xmpp protocol.");

                    //No error handlers yet, lets focus on OnNewMessage first...
                    //xclient.Error += OnError;


                }


            }
            Console.WriteLine();
            Console.WriteLine("Loading brain files, please wait...");
            Console.WriteLine();
            Console.WriteLine("-----------------------------------");
            synBot.Learning += SynBot_Learning;
            synBot.Memorizing += synBot_Memorizing;
            synBot.Configuration.StoreVocabulary = true;
            synBot.Configuration.StoreExamples = true;
            //Load brain files(siml) here...
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
                var memPackage = File.ReadAllText("package\\" + user + "\\memory.simlpk");
                synBot.PackageManager.LoadFromString(simlPackage);
                Console.WriteLine("Loaded personal user Memory file...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("No memory file for user " + botUser.ID);
            }

            bool isChat = true;

            var interactions = synBot.Stats.Interactions;
            var idleTime = synBot.Stats.IdleTime;
            var loadTime = synBot.Stats.LoadTime;
            var mappingTime = synBot.Stats.MappingTime;
            var modelCount = synBot.Stats.ModelCount;
            var vocabCount = synBot.Stats.Vocabulary.Count();
            Console.WriteLine("Interactions: " + interactions);
            Console.WriteLine("LoadTime: " + loadTime);
            Console.WriteLine("Mapping Time: " + mappingTime);
            Console.WriteLine("Vocabulary Count: " + vocabCount);
            Console.WriteLine("Model count: " + modelCount);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();
            Console.WriteLine("Brain Loaded.");
            //Actual Chatting Session:
            if (xclient.Connected == false)
            {
                while (isChat == true)
                {
                    Console.Write("Enter Message: ");
                    String message = Console.ReadLine();
                    if (message == "exit")
                    {
                        if (xclient.Connected == true)
                        {
                            xclient.Dispose();
                        }
                        //var settings = synBot.Settings.GetDocument();
                        //settings.Save("package\\BotSettings.siml");
                        Console.WriteLine("--------------------");
                        Console.WriteLine("Writing memory to package for later...");

                        //For some reason the following does not work.
                        try
                        {
                            var elementList = new List<XDocument>();
                            foreach (var simlFile in Directory.GetFiles(@"package\\" + user, "*.siml"))
                            {
                                var simlElement = XElement.Load(simlFile);
                                elementList.Add(simlElement.Document);
                            }
                            var xdoc = new XDocument(elementList);
                            var packageString = synBot.PackageManager.ConvertToPackage(elementList);
                            File.WriteAllText(@"package\\" + user + "\\memory.simlpk", packageString);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ERROR: " + ex.ToString());
                            Console.ReadLine();
                        }

                        Console.WriteLine("--------------------");
                        Environment.Exit(0);
                    }
                    var chatReq = new ChatRequest(message, botUser);
                    var chatResult = synBot.Chat(chatReq);
                    if (chatResult.Success)
                    {
                        Console.WriteLine("Amy: " + chatResult.BotMessage);
                    }
                }
            }

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
            for(int i = 0; i < 10; i++)
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
                    var filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\package", e.User.ID, "Memorized"+ randNum + ".siml");
                    e.Document.Save(filePath);
                }
                Console.WriteLine("Write success!");
            } catch(Exception ex)
            {
                Console.WriteLine("Write failed: " + ex.Message);
            }

            Console.WriteLine("--------------------------------");
        }
        static void OnNewMessage(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            String resID = e.Jid.Resource;
            String domain = e.Jid.Domain;
            String jid = e.Jid.ToString().Replace(resID, "");
            jid = jid.Replace(domain, "");
            jid = jid.Replace("@/", "");
            String mes = e.Message.Body;
            if (CheckIfFormIsOpen(jid, mes) == true)
            {

            }
            else
            {
                var invokingForm = Application.OpenForms[0]; // or whatever Form you can access
                if (invokingForm.InvokeRequired)
                {
                    invokingForm.BeginInvoke(new EventHandler<S22.Xmpp.Im.MessageEventArgs>(OnNewMessage), sender, e);
                    return; // important!!!
                }
                MessageForm tempMsg = new MessageForm(jid, domain);

                tempMsg._msgText(jid, mes);
                tempMsg.frmId = jid;
                tempMsg.Show();
            }


        }
        static bool CheckIfFormIsOpen(string id, string message)
        {

            FormCollection fc = Application.OpenForms;
            foreach (MessageForm frm in fc.OfType<MessageForm>())
            {
                if (frm.frmId == id)
                {
                    frm._msgText(id, message);
                    return true;
                }
            }
            return false;
        }
    }
}
