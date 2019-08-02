using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miyop.IoT.Common
{

    public class IoTDisplay
    {
        private const string I2C_CONTROLLER_NAME = "I2C1";
        private const byte DEVICE_I2C_ADDRESS = 0x3F; //Address of IC2 device address.
        private LCDisplay _screen;
        //Setup pins
        private const byte EN = 0x02;
        private const byte RW = 0x01;
        private const byte RS = 0x00;
        private const byte D4 = 0x04;
        private const byte D5 = 0x05;
        private const byte D6 = 0x06;
        private const byte D7 = 0x07;
        private const byte BL = 0x03;

        public IoTDisplay()
        {
            _screen = new LCDisplay(DEVICE_I2C_ADDRESS, I2C_CONTROLLER_NAME, RS, RW, EN, D4, D5, D6, D7, BL);
            _screen.Init();
        }


        public void Display(string firstLine, string secondLine = "", int clearAfter=0)
        {
            _screen.ClearScreen();
            if (!string.IsNullOrEmpty(firstLine))
            {
                _screen.Print($"{firstLine}");
            }
            if (!string.IsNullOrEmpty(secondLine))
            {
                _screen.GotoSecondLine();
                _screen.Print($"{secondLine}");
            }

            if (clearAfter > 0)
            {
                Task.Delay(clearAfter);
                _screen.ClearScreen();
            }
        }

        public void Clear()
        {
            _screen.ClearScreen();
        }
    }
}
