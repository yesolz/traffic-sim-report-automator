using Simulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    public class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine($"입력 인수: {arg}");
            }
            Simulate(args).GetAwaiter().GetResult();
            
        }

        static async Task Simulate(string[] args)
        {
            if (args[0] == "test")
            {
                SimulatorViewModel simulator = new SimulatorViewModel();
                if (args.Length<2)
                    simulator.jsonconvert("", "", "C:\\Users\\EugeneLee\\Desktop\\result.json");
                else if(args.Length == 4)
                {
                    simulator.jsonconvert(args[1], args[2], args[3]);
                }
                else
                {
                    Console.WriteLine("Not enough args");
                }
            }
            else
            {
                SelectViewModel selection = new SelectViewModel();
                SimulatorViewModel simulator = new SimulatorViewModel();
                if (args.Length > 1)
                {
                    selection.SearchScenarioFile(args[0]);//xodr 경로 바꾸기
                }
                else
                {
                    Console.WriteLine("test");
                    selection.SearchScenarioFile("sn_gen_7635.xosc");//시나리오 파일 확인해봐야 알듯?
                }

                await simulator.ActivateSimulation(args[1]);
                simulator.jsonconvert(args[2], args[3], args[4]);
            }
            
        }
    }
}
