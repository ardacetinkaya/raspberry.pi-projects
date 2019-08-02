using Microsoft.Azure.Devices.Client;
using Miyop.IoT.Common;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;

namespace Miyop.IoT.Job
{
    public sealed class StartupTask : IBackgroundTask
    {

        private BackgroundTaskDeferral _deferral;
        private ThreadPoolTimer _periodicTimer = null;
        private IBackgroundTaskInstance _taskInstance = null;

        private volatile bool _cancelRequested = false;
        private volatile bool _isStarted = false;
        private volatile bool _isPressed = false;

        private IoTCam _camera;
        private IoTDisplay _screen;
        private LEGODeviceContainer _legoContainer;

        private DeviceClient _client = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                await Logging.WriteInfoLog("Miyop.IoT is starting...");
                _taskInstance = taskInstance;

                this.InitDevice();
                this.InitConnection();

                var lines =_legoContainer.GetVoltageInfo().Split(new string[] { Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
                _screen.Display(lines[0], lines[1]);
                _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromSeconds(10));

            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"{ex.Message}");
            }
        }

        private async void InitConnection()
        {
            try
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile connectionStringFile = await storageFolder.GetFileAsync("connection.string.iothub");

                var deviceConnectionString = await FileIO.ReadTextAsync(connectionStringFile);

                _client = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp_WebSocket_Only);
                await _client.SetMethodHandlerAsync("command", OnCommandReceived, null);

                await _client.OpenAsync();
         
                await Logging.WriteInfoLog("IoT device initialized...");

            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"Init device connection. Detail:{ex.Message}");
            }

        }

        private async void InitDevice()
        {
            try
            {

                _deferral = _taskInstance.GetDeferral();
                _taskInstance.Canceled += TaskInstance_Canceled;
                _screen = new IoTDisplay();
                _camera = new IoTCam();

                _camera.Init(true);
                _camera.FaceDetected += CameraDetectedFace;

                _legoContainer = new LEGODeviceContainer();

                
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"Init. hardware. Detail:{ex.Message}");
                _deferral.Complete();
            }
        }
        private static async Task<MethodResponse> OnCommandReceived(MethodRequest methodRequest, object userContext)
        {
            try
            {

                var data = methodRequest.DataAsJson;

                await Logging.WriteInfoLog($"Command received...{data}");
                var command = JsonConvert.DeserializeObject<CommandContainer>(data);
                
                return new MethodResponse(200);
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"On command receive. Detail:{ex.Message}");
                return new MethodResponse(500);
            }

        }

        private void CameraDetectedFace(object sender, int e)
        {
            _screen?.Display("Face", "Detected", 400);

            var message = new CapturedFace
            {
                Count = e,
                Date = DateTime.Now
            };

            SendMessage(message);
        }


        async void SendMessage(object messageData)
        {
            try
            {
                var messageString = JsonConvert.SerializeObject(messageData);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                await _client.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"Sending message: { ex.Message}");
            }
        }

        private async void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            try
            {
                while (!_cancelRequested)
                {

                   
                    if (_legoContainer.IsTouchPressed() && !_isPressed)
                    {
                        
                        await Logging.WriteInfoLog($"Touch is pressed.");
                        if (!_isStarted)
                        {
                            _camera.StartToTakeVideo();
                            _isStarted = true;
                            await Logging.WriteInfoLog($"Video is started.");

                        }
                        else
                        {
                            _camera.StopToTakeVideo();
                            _isStarted = false;
                            await Logging.WriteInfoLog($"Video is stopped.");
                        }
                        _isPressed = false;
                        await Task.Delay(1000);
                        
                    }
                    
                }
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"Timer - { ex.Message }");
            }
        }

        private async void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {

            switch (reason)
            {
                case BackgroundTaskCancellationReason.Abort:
                case BackgroundTaskCancellationReason.Terminating:
                case BackgroundTaskCancellationReason.LoggingOff:
                case BackgroundTaskCancellationReason.ServicingUpdate:
                case BackgroundTaskCancellationReason.IdleTask:
                case BackgroundTaskCancellationReason.Uninstall:
                case BackgroundTaskCancellationReason.ConditionLoss:
                case BackgroundTaskCancellationReason.SystemPolicy:
                case BackgroundTaskCancellationReason.ExecutionTimeExceeded:
                case BackgroundTaskCancellationReason.ResourceRevocation:
                case BackgroundTaskCancellationReason.EnergySaver:
                default:
                    await Logging.WriteInfoLog($"Task is cancelled...{reason.ToString() }");
                    _cancelRequested = true;
                    
                    _screen.Clear();
                    _legoContainer.Reset();
                    _deferral.Complete();
                    break;
            }


        }
    }
}
