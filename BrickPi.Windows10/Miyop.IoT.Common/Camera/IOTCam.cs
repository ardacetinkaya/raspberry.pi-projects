using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace Miyop.IoT.Common
{
    public class CapturedFace
    {
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class IoTCam
    {
        private MediaCapture _mediaCapture;
        private StorageLibrary _photoStorage;
        private StorageLibrary _videoStorage;

        private IAsyncAction _videoStopped;

        private FaceDetectionEffect _faceDetectionEffect;

        public bool IsInitialized { get; private set; } = false;

        public event EventHandler<int> FaceDetected;

        public MediaCapture MediaCapture { get => _mediaCapture; set => _mediaCapture = value; }

        public IoTCam()
        {

        }

        public async void Init(bool enableFaceDetection = false)
        {
            try
            {
                _photoStorage = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                _videoStorage = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += _mediaCapture_Failed;
                _mediaCapture.RecordLimitationExceeded += _mediaCapture_RecordLimitationExceeded;
                await _mediaCapture.InitializeAsync();

                if (enableFaceDetection)
                    await CreateFaceDetectionEffectAsync();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"IoTCam initialize is failed. Detail:{ex.Message}");
            }
        }

        private async void _mediaCapture_RecordLimitationExceeded(MediaCapture sender)
        {
            this.StopToTakeVideo();
            await Logging.WriteInfoLog("Record limitation exceeded.");

        }

        private async void _mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            await Logging.WriteInfoLog($"Media capture device is failed. Error:{errorEventArgs.Message}");
        }

        public async Task<bool> TakePhoto()
        {
            bool isTaken = false;
            try
            {
                if (_mediaCapture == null)
                {
                    await Logging.WriteInfoLog("Media device is not initialized.");
                    return isTaken;
                }

                var fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_photo.jpg";
                StorageFile file = await _photoStorage.SaveFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                await _mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                isTaken = true;
            }
            catch (Exception ex)
            {
                await Logging.WriteErrorLog($"Take photo have some error.{ex.Message}");
            }

            return isTaken;
        }

        public async void StartToTakeVideo()
        {
            StorageFile file = null;
            try
            {

                        var fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_video.mp4";
                        file = await _videoStorage.SaveFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                        await _mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p), file);
            }
            catch (Exception ex)
            {
                if (file != null) await file.DeleteAsync();
                await Logging.WriteErrorLog($"Can not start to take video. Detail:{ex.Message}");
            }


        }

        public async void StopToTakeVideo()
        {
            try
            {
                if (_mediaCapture != null)
                {
                    _videoStopped = _mediaCapture.StopRecordAsync();
                    
                }

            }
            catch (Exception ex)
            {

                await Logging.WriteErrorLog($"Can not stop taking video. Detail:{ex.Message}");
            }

        }

        private async Task CreateFaceDetectionEffectAsync()
        {
            try
            {
                var definition = new FaceDetectionEffectDefinition();

                definition.SynchronousDetectionEnabled = false;
                definition.DetectionMode = FaceDetectionMode.HighPerformance;

                _faceDetectionEffect = (FaceDetectionEffect)await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoRecord);
                _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;
                _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(33);
                _faceDetectionEffect.Enabled = true;

            }
            catch (Exception ex)
            {

                await Logging.WriteErrorLog($"Can not create Face Detection Effect. Detail: {ex.Message}");
            }
            

        }
        private async Task DeleteFaceDetectionEffectAsync()
        {
            _faceDetectionEffect.Enabled = false;
            _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;

            await _mediaCapture.RemoveEffectAsync(_faceDetectionEffect);

            _faceDetectionEffect = null;

        }
        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            if (args.ResultFrame.DetectedFaces.Count >= 1)
            {
                
                FaceDetected(this, args.ResultFrame.DetectedFaces.Count);
                await Logging.WriteInfoLog($"Face(s) detected.{args.ResultFrame.DetectedFaces.Count.ToString()}");
            }
        }
    }
}
