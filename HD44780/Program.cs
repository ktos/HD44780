#region License
/*
 * Hd44780
 *
 * Copyright (C) Marcin Badurowicz 2013
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. 
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaspberryPiDotNet;
using RaspberryPiDotNet.MicroLiquidCrystal;
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

            var lcdProvider = new RaspPiGPIOMemLcdTransferProvider(GPIOPins.GPIO_07, GPIOPins.GPIO_08, GPIOPins.GPIO_25, GPIOPins.GPIO_24, GPIOPins.GPIO_23, GPIOPins.GPIO_18);
            var lcd = new Lcd(lcdProvider);

            GPIOMem backlit = new GPIOMem(GPIOPins.GPIO_15, GPIODirection.Out);
            backlit.Write(false);

            lcd.Begin(16, 2);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);

            if (pp.SwitchExists("--light"))
            {
                backlit.Write(true);                
            }

            if (pp.SwitchExists("--no-light"))
            {
                backlit.Write(false);                
            }
            
            string text;
            try
            {
                var isKey = System.Console.KeyAvailable;
                text = Console.ReadLine();
            }
            catch (InvalidOperationException)
            {
                // when we're in piped output, InvalidOperationException is thrown for KeyAvaliable
                // and we're using it here to check... dirty, dirty hack!
                text = Console.In.ReadToEnd();
            }                

            lcd.Write(text);
        }


    }
}
