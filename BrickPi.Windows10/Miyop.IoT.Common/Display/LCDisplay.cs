/**
 *  Character-LCD-over-I2C 
 *  ===================
 *  Connect HD44780 LCD character display to Windows 10 IoT devices via I2C and PCF8574
 *
 *  Author: Jaroslav Zivny
 *  Version: 1.1
 *  Keywords: Windows IoT, LCD, HD44780, PCF8574, I2C bus, Raspberry Pi 2
 *  Git: https://github.com/DzeryCZ/Character-LCD-over-I2C
**/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Gpio;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using System.Diagnostics;

namespace Miyop.IoT.Common
{
    internal class LCDisplay
    {

        private const byte LCD_WRITE = 0x07;

        private byte _D4;
        private byte _D5;
        private byte _D6;
        private byte _D7;
        private byte _En;
        private byte _Rw;
        private byte _Rs;
        private byte _Bl;

        private byte[] _lineAddress = new byte[] { 0x00, 0x40 };

        private byte _backLight = 0x01;

        private I2cDevice _i2cPortExpander;


        public LCDisplay(byte deviceAddress, string controllerName, byte Rs, byte Rw, byte En, byte D4, byte D5, byte D6, byte D7, byte Bl, byte[] LineAddress) : this(deviceAddress, controllerName, Rs, Rw, En, D4, D5, D6, D7, Bl)
        {
            this._lineAddress = LineAddress;
        }


        public LCDisplay(byte deviceAddress, string controllerName, byte Rs, byte Rw, byte En, byte D4, byte D5, byte D6, byte D7, byte Bl)
        {
            // Configure pins
            this._Rs = Rs;
            this._Rw = Rw;
            this._En = En;
            this._D4 = D4;
            this._D5 = D5;
            this._D6 = D6;
            this._D7 = D7;
            this._Bl = Bl;

            // It's async method, so we have to wait
            Task.Run(() => this.startI2C(deviceAddress, controllerName)).Wait();
        }

        public async void startI2C(byte deviceAddress, string controllerName)
        {
            try
            {
                var i2cSettings = new I2cConnectionSettings(deviceAddress);
                i2cSettings.BusSpeed = I2cBusSpeed.StandardMode;
                string deviceSelector = I2cDevice.GetDeviceSelector(controllerName);
                var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
                this._i2cPortExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
                if (_i2cPortExpander == null)
                    throw new Exception("Could not find any device");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                return;
            }
        }

        public void Init(bool turnOnDisplay = true, bool turnOnCursor = false, bool blinkCursor = false, bool cursorDirection = true, bool textShift = false)
        {


            /* Init sequence */
            Task.Delay(100).Wait();
            PulseEnable(Convert.ToByte((1 << this._D5) | (1 << this._D4)));
            Task.Delay(5).Wait();
            PulseEnable(Convert.ToByte((1 << this._D5) | (1 << this._D4)));
            Task.Delay(5).Wait();
            PulseEnable(Convert.ToByte((1 << this._D5) | (1 << this._D4)));

            /*  Init 4-bit mode */
            PulseEnable(Convert.ToByte((1 << this._D5)));

            /* Init 4-bit mode + 2 line */
            PulseEnable(Convert.ToByte((1 << this._D5)));
            PulseEnable(Convert.ToByte((1 << this._D7)));

            /* Turn on display, cursor */
            PulseEnable(0);
            PulseEnable(Convert.ToByte((1 << this._D7) | (Convert.ToByte(turnOnDisplay) << this._D6) | (Convert.ToByte(turnOnCursor) << this._D5) | (Convert.ToByte(blinkCursor) << this._D4)));

            this.ClearScreen();

            PulseEnable(0);
            PulseEnable(Convert.ToByte((1 << this._D6) | (Convert.ToByte(cursorDirection) << this._D5) | (Convert.ToByte(textShift) << this._D4)));
        }

        public void TurnOnBacklight()
        {
            this._backLight = 0x01;
            this.SendCommand(0x00);
        }

        public void TurnOffBacklight()
        {
            this._backLight = 0x00;
            this.SendCommand(0x00);
        }


        public void Print(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                this.PrintChar(text[i]);
            }

        }

        private void PrintChar(char letter)
        {
            try
            {
                this.Write(Convert.ToByte(letter), 1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Unable to print char. Details:{0}", e.Message));
            }
        }

        public void GotoSecondLine()
        {
            this.SendCommand(0xc0);
        }

        public void GotoXY(byte x, byte y)
        {
            this.SendCommand(Convert.ToByte(x | _lineAddress[y] | (1 << LCD_WRITE)));
        }

        private void SendData(byte data)
        {
            this.Write(data, 1);
        }



        public void SendCommand(byte data)
        {
            this.Write(data, 0);
        }

        public void ClearScreen()
        {
            PulseEnable(0);
            PulseEnable(Convert.ToByte((1 << this._D4)));
            Task.Delay(5).Wait();
        }


        private void Write(byte data, byte Rs)
        {
            PulseEnable(Convert.ToByte((data & 0xf0) | (Rs << this._Rs)));
            PulseEnable(Convert.ToByte((data & 0x0f) << 4 | (Rs << this._Rs)));
            //Task.Delay(5).Wait(); //In case of problem with displaying wrong characters uncomment this part
        }

        private void PulseEnable(byte data)
        {
            this._i2cPortExpander.Write(new byte[] { Convert.ToByte(data | (1 << this._En) | (this._backLight << this._Bl)) }); // Enable bit HIGH
            this._i2cPortExpander.Write(new byte[] { Convert.ToByte(data | (this._backLight << this._Bl)) }); // Enable bit LOW
            //Task.Delay(2).Wait(); //In case of problem with displaying wrong characters uncomment this part
        }

        public void CreateSymbol(byte[] data, byte address)
        {
            this.SendCommand(Convert.ToByte(0x40 | (address << 3)));
            for (var i = 0; i < data.Length; i++)
            {
                this.SendData(data[i]);
            }
            this.ClearScreen();
        }

        public void PrintSymbol(byte address)
        {
            this.SendData(address);
        }


    }
}