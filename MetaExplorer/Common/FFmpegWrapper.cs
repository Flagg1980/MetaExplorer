using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetaExplorer.Common
{
    public class FFmpegWrapper
    {
        private readonly TimeSpan TIMEOUT_EXEC = TimeSpan.FromSeconds(40);

        private readonly List<string> errorMessages = new List<string>
        {
            //e.g. if -ss parameter is bigger that file duration
            "Output file is empty",
        };

        private string _execLocation = null;

        public FFmpegWrapper(string execLocation)
        {
            if (!File.Exists(execLocation))
            {
                throw new Exception(String.Format("Could not find FFmpeg at expected location <{0}>.", execLocation));
            }
            else
            {
                _execLocation = execLocation;
            }
        }

        public TimeSpan GetDuration(string inputFileLocation)
        {
            Command c = new Command();

            //construct parameters
            string parameters = "";
            parameters += @"-i " + '"' + inputFileLocation + '"';               //input file

            //execute command 
            c.Cmd(_execLocation, parameters, TIMEOUT_EXEC);

            //get the output
            string output = c.FlushStdoutAndErr();

            //grep duration
            try
            {
                Match m = Regex.Match(output, @"Duration: (\d{1,3}):(\d\d):(\d\d)");

                TimeSpan result = new TimeSpan(Int32.Parse(m.Groups[1].Value), Int32.Parse(m.Groups[2].Value), Int32.Parse(m.Groups[3].Value));
                return result;
            }
            catch (Exception e)
            {
                string errorMsg = String.Format("Could not get time duration from video file <{0}> with command <{1}> and error message <{2}>.", inputFileLocation, _execLocation + " " + parameters, e.Message);
                Trace.TraceError(errorMsg);
                throw new Exception(errorMsg);
            }
        }

        public void CreateJpegThumbnail(string inputFile, TimeSpan position, string outputFile, bool overrideOutput = false)
        {
            if (!overrideOutput && File.Exists(outputFile))
            {
                string errorMsg = String.Format("Output file <{0}> already exists while override flag was set to <{1}>.", outputFile, overrideOutput);
                Trace.TraceError(errorMsg);
                throw new Exception(errorMsg);
            }

            //dirty hack: put .png at the end of output file and rename it back afterwards
            string outputFileNew = outputFile + ".jpeg";

            Command c = new Command();

            //construct parameters
            string parameters = "";
            parameters += "-ss " + position.ToString(@"hh\:mm\:ss");                //position (order of the arguments is extremely important for speed of thumbnail generation!!!!
            parameters += " ";
            parameters += @"-i " + '"' + inputFile + '"';                                       //input file
            parameters += " ";
            parameters += "-vframes 1";                                             //number of frames to capture
            parameters += " ";

            if (overrideOutput)
            {
                parameters += "-y";                                                 //override existing file
                parameters += " ";
            }

            parameters += '"' + outputFileNew + '"';                                               //output file

            c.Cmd(_execLocation, parameters, TIMEOUT_EXEC);

            //validation
            int exitCode = c.ExitCode;
            string output = c.FlushStdoutAndErr();
            bool outputIsBad = errorMessages.Any(x => output.Contains(x));
            bool fileExists = File.Exists(outputFileNew);

            if (exitCode != 0 || outputIsBad || !fileExists)
            {
                string errorMsg = String.Format("Something went wrong while creating the output. Exitcode of the command is <{0}>. Parameters were: <{1}>. Output of the command is <{2}>. File was created: <{3}>.", exitCode, parameters, output, fileExists);
                Trace.TraceError(errorMsg);
                throw new Exception(errorMsg);
            }

            //rename back
            File.Move(outputFileNew, outputFile);
        }
    }
}
