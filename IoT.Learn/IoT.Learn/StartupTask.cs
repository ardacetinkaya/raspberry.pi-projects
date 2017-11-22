using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using Sensors.Dht;
using Windows.UI.Xaml;
using Windows.System.Threading;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System.Threading;
using Newtonsoft.Json;

namespace IoT.Learn
{



    public sealed class StartupTask : IBackgroundTask
    {

        private const string I2C_CONTROLLER_NAME = "I2C1";
        private const byte DEVICE_I2C_ADDRESS = 0x3F; //Address of IC2 device address.

        //Setup pins
        private const byte EN = 0x02;
        private const byte RW = 0x01;
        private const byte RS = 0x00;
        private const byte D4 = 0x04;
        private const byte D5 = 0x05;
        private const byte D6 = 0x06;
        private const byte D7 = 0x07;
        private const byte BL = 0x03;

        private GpioPin _pin;
        private displayI2C.LCDisplay _screen;


        private BackgroundTaskDeferral _deferral;
        volatile bool _cancelRequested = false;
        private ThreadPoolTimer _periodicTimer = null;

        private DeviceClient _deviceClient = null;

        private readonly string _deviceConnectionString = "";

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            //Attach to cancel event to cancel if it is needed;
            taskInstance.Canceled += TaskInstance_Canceled;
            _deferral = taskInstance.GetDeferral();

            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString);

            //Thread timer to check sensor data
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromSeconds(60));

            //Init PINs and LCD 
            _pin = GpioController.GetDefault().OpenPin(4, GpioSharingMode.Exclusive);
            _screen = new displayI2C.LCDisplay(DEVICE_I2C_ADDRESS, I2C_CONTROLLER_NAME, RS, RW, EN, D4, D5, D6, D7, BL);
            _screen.Init();

        }




        private void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            if ((_cancelRequested == false))
            {
                ReadTemprature(_pin);
            }
            else
            {
                _periodicTimer.Cancel();
                _deferral.Complete();
            }
        }

        public async void ReadTemprature(GpioPin pin)
        {
            Dht11 dht11 = new Dht11(pin, GpioPinDriveMode.Input);
            var result = await dht11.GetReadingAsync();

            if (result.IsValid)
            {
                var temp = Convert.ToSingle(result.Temperature);
                var humidity = Convert.ToSingle(result.Humidity);
                _screen.ClearScreen();
                _screen.Print($"Temp : {temp}");
                _screen.GotoSecondLine();
                _screen.Print($"Humidity : {humidity}");

                var telemeteryData = new
                {
                    date = DateTime.Now,
                    temperature = temp,
                    humidity = humidity
                };

                var messageString = JsonConvert.SerializeObject(telemeteryData);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("lowTemp", (temp < 20) ? "true" : "false");

                await _deviceClient.SendEventAsync(message);
                Debug.WriteLine($"{DateTime.Now} > Sending message to IoT Hub {messageString}");

                await Task.Delay(1000);

            }

        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;

        }
    }
}
