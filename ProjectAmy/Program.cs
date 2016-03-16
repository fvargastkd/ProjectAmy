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
namespace ProjectAmy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ProjectAmy - Ver. 1.0");
            var synBot = new SynBot();
            Console.Write("Enter a username: ");
            String user = Console.ReadLine();
            var botUser = new BotUser(synBot, user);
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
            } catch(Exception ex)
            {
                Console.WriteLine("Project Amy has not learned anything new yet...");
            }
            try
            {
                var fileMem = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory() + "\\package", botUser.ID, "Memorized.siml"));
                synBot.AddSiml(fileMem);
                Console.WriteLine("Loaded personal user Memory file...");
            }catch(Exception ex)
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

            //Actual Chatting Session:
            while (isChat == true)
            {
                Console.Write("Enter Message: ");
                String message = Console.ReadLine();
                if(message == "exit")
                {
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
        static void SynBot_Learning(object sender, LearningEventArgs e)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "package\\Learned.siml");
            e.Document.Save(filePath);
        }
        static void synBot_Memorizing(object sender, MemorizingEventArgs e)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\package", e.User.ID, "Memorized.siml");
            e.Document.Save(filePath);
        }
    }
}
