using BrickPi3;
using BrickPi3.Models;
using BrickPi3.Movement;
using BrickPi3.Sensors;

namespace Miyop.IoT.Common
{
    public class LEGODeviceContainer
    {
        private Brick _brickPi;
        private NXTTouchSensor _touch;
        private Motor _motor;

        public LEGODeviceContainer()
        {
            _brickPi = new Brick();
            _brickPi.InitSPI();
            _motor = new Motor(_brickPi, BrickPortMotor.PORT_B);

            _touch = new NXTTouchSensor(_brickPi, BrickPortSensor.PORT_S1, 500);
        }

        public string GetVoltageInfo()
        {
            var voltage = _brickPi.BrickPi3Voltage;
            return $"3V:{voltage.Voltage3V3} 5V:{ voltage.Voltage5V}\r\n9V: {voltage.Voltage9V} B: {voltage.VoltageBattery}";
        }

        public bool IsTouchPressed()
        {
            var result= _touch?.IsPressed();

            if (!result.HasValue) return false;

            return result.Value;
        }

        public void StartMotor(int speed)
        {
            if (speed > 100)
            {
                speed = 100;
            } else if (speed < -100)
            {
                speed = -100;
            }
            _motor?.SetSpeed(speed);
            _motor?.Start();
        }

        public void StopMotor()
        {
            _motor?.Stop();

        }

        public void Reset()
        {
            _brickPi?.reset_all();
        }




    }
}
