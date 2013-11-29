using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using RaspberryPiDotNet;
//using RaspberryPiDotNet.MicroLiquidCrystal;
using System.Threading;
using Ktos.Common;

namespace Ktos.Hd44780
{
    class Program
    {
        static void Main(string[] args)
        {
            ArgumentParser pp = new ArgumentParser(args);
            pp.AddExpanded("-l", "--light");
            pp.AddExpanded("-nl", "--no-light");
            pp.Parse();

            /*var lcdProvider = new RaspPiGPIOMemLcdTransferProvider(GPIOPins.GPIO_07, GPIOPins.GPIO_08, GPIOPins.GPIO_25, GPIOPins.GPIO_24, GPIOPins.GPIO_23, GPIOPins.GPIO_18);
            var lcd = new Lcd(lcdProvider);

            GPIOMem backlit = new GPIOMem(GPIOPins.GPIO_15, GPIODirection.Out);
            backlit.Write(false);

            lcd.Begin(16, 2);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);*/

            if (pp.SwitchExists("--light"))
            {
                //backlit.Write(true);
                Console.WriteLine("light");
            }

            if (pp.SwitchExists("--no-light"))
            {
                //backlit.Write(true);
                Console.WriteLine("nolight");
            }
            
            string text;
            try
            {
                var isKey = System.Console.KeyAvailable;
                text = Console.ReadLine();
            }
            catch (Exception e)
            {
                text = Console.In.ReadToEnd();
            }                

            //lcd.Write(text);            
        }        
    }
}
