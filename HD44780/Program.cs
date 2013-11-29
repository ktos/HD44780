using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaspberryPiDotNet;
using RaspberryPiDotNet.MicroLiquidCrystal;
using System.Threading;

namespace Ktos.Hd44780
{
    class Program
    {

        static void Main(string[] args)
        {
            var lcdProvider = new RaspPiGPIOMemLcdTransferProvider(GPIOPins.GPIO_07, GPIOPins.GPIO_08, GPIOPins.GPIO_25, GPIOPins.GPIO_24, GPIOPins.GPIO_23, GPIOPins.GPIO_18);
            var lcd = new Lcd(lcdProvider);

            GPIOMem backlit = new GPIOMem(GPIOPins.GPIO_15, GPIODirection.Out);
            backlit.Write(false);

            lcd.Begin(16, 2);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);

            if (args.Length == 2)
            {
                if (args[0] == "--light" || args[0] == "-l")
                    backlit.Write(true);

                lcd.Write(args[1]);
            }
            else if (args.Length == 1)
            {
                if (args[0] == "--light" || args[0] == "-l")
                {
                    backlit.Write(true);

                    var text = Console.ReadLine();
                    lcd.Write(text);
                }
                else
                    lcd.Write(args[0]);
            }
            else
            {
                var text = Console.ReadLine();
                lcd.Write(text);
            }
                

                    
        }        
    }
}
