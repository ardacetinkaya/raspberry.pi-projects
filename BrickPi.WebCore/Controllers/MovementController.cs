namespace BrickPi.WebCore.Controllers
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using BrickPi.WebCore.Models;
    using Microsoft.Extensions.FileProviders;

    [Route("api/[controller]")]
    public class MovementController : Controller
    {
        private static bool isCaptureStarted = false;

        [HttpGet]
        public IActionResult GetCaptureStatus()
        {
            return Content(isCaptureStarted.ToString());
        }

        [HttpPost]
        public IActionResult Move([FromBody] DirectionModel direction)
        {

            IFileProvider provider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            IDirectoryContents contents = provider.GetDirectoryContents("");
            IFileInfo fileInfo;
            if (direction.Direction == "capturestart")
            {
                isCaptureStarted = true;
                fileInfo = provider.GetFileInfo("wwwroot/py/MiyopFindFace.py");
            }
            else if (direction.Direction == "capturestop")
            {
                isCaptureStarted = false;
                fileInfo = provider.GetFileInfo("wwwroot/py/MiyopFindFace.py");
            }
            else
            {
                fileInfo = provider.GetFileInfo("wwwroot/py/MiyopRobotMove.py");
            }
            var pythonRunner = new RunPython();
            var result = pythonRunner.Run(fileInfo.PhysicalPath, direction.Direction);
            return Content(direction.Direction);
        }
    }
}