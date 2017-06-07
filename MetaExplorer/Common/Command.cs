using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MetaExplorer.Common
{
    /// <summary>
    /// This class can be used to run external commands and capture the stdout and stderr.
    /// </summary>
    public class Command
    {
        #region Private Members

        private StringBuilder myStdout = new StringBuilder();
        private StringBuilder myStderr = new StringBuilder();
        private StringBuilder myStdoutAndStderr = new StringBuilder();

        private string myLastCommand;
        private StreamWriter streamWriter = null;
        private string fileRedirect = string.Empty;
        private Process aProcess = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Keep stdout and stderr in one buffer for correct sequence
        /// </summary>
        public string StdoutAndStderr
        {
            get { return myStdoutAndStderr.ToString(); }
        }

        /// <summary>
        /// The exit code of the last executed command
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// The error output of the last executed comand
        /// </summary>
        public string LastError
        {
            get { return myStderr.ToString(); }
        }

        /// <summary>
        /// If this is set, the stdout and stderr of the process will be copied and redirected to the specified 
        /// file.
        /// </summary>
        public string FileRedirect
        {
            set
            {
                this.fileRedirect = value;
                this.streamWriter = new StreamWriter(this.fileRedirect);
            }

            get
            {
                return this.fileRedirect;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The actually buffered stddout and stderr are returned and cleared from internal buffer.
        /// </summary>
        /// <returns>The actually buffered stdout and stderr</returns>
        public string FlushStdoutAndErr()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(myStdoutAndStderr);
            myStdoutAndStderr.Clear();

            return buf.ToString();
        }

        ///<apiflag>yes</apiflag>
        /// <summary>
        /// API:YES Starts a command asynchronously. It is not waited for the command to exit.
        /// </summary>
        /// <param name="commandName">command name (including the path, if necessary)</param>
        /// <param name="args">arguments of the command</param>
        public void StartAsync(string commandName, string args)
        {
            //clean up
            myLastCommand = commandName + " " + args;
            myStderr.Clear();
            myStdout.Clear();
            myStdoutAndStderr.Clear();

            ProcessStartInfo pInfo = null;

            //build up pinfo based on where the command need to executed.
            pInfo = new ProcessStartInfo(commandName, args);

            pInfo.UseShellExecute = false;
            pInfo.CreateNoWindow = true;
            pInfo.RedirectStandardOutput = true;
            pInfo.RedirectStandardError = true;

            // using async readin as synchronous readin can cause deadlocks
            aProcess = new Process();
            aProcess.StartInfo = pInfo;
            aProcess.OutputDataReceived += new DataReceivedEventHandler(OnOutputDataReceived);
            aProcess.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);

            aProcess.Start();

            aProcess.BeginOutputReadLine();
            aProcess.BeginErrorReadLine();
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="commandName">command name (including the path, if necessary)</param>
        /// <param name="args">arguments of the command</param>
        /// <param name="timeout">Timeout for command execution</param>
        /// <exception cref="ArgumentException">Is thrown if command times out or errorlevel set.</exception>
        /// <returns>The stdout of the command</returns>
        public string Cmd(string commandName, string args, TimeSpan timeout)
        {
            this.StartAsync(commandName, args);

            bool success = aProcess.WaitForExit((int)(timeout.TotalMilliseconds));

            //flush buffered content of the streamwriter to the stream
            if (this.streamWriter != null)
            {
                this.streamWriter.Flush();
                this.streamWriter.Dispose();
            }

            aProcess.OutputDataReceived -= new DataReceivedEventHandler(OnOutputDataReceived);
            aProcess.ErrorDataReceived -= new DataReceivedEventHandler(OnErrorDataReceived);
            if (!success)
            {
                try
                {
                    aProcess.Kill();
                    aProcess.Dispose();
                }
                catch
                {
                    //nothing to do
                }

                throw new ArgumentException(String.Format("process {0} {1} timed out after {2} sec", commandName, args, (timeout.TotalSeconds)));
            }

            ExitCode = aProcess.ExitCode;
            aProcess.Dispose();
            return myStdout.ToString();
        }

        /// <summary>
        /// Stops a currently running process (the process must have been started using the StartAsync(...) method.
        /// </summary>
        /// <param name="timeout">Timeout for stopping the process.</param>
        public void StopAsync(TimeSpan timeout)
        {
            if (aProcess == null || aProcess.HasExited)
            {
                return;
            }

            //flush buffered content of the streamwriter to the stream
            if (this.streamWriter != null)
            {
                this.streamWriter.Flush();
                this.streamWriter.Dispose();
            }

            //release handler
            aProcess.OutputDataReceived -= new DataReceivedEventHandler(OnOutputDataReceived);
            aProcess.ErrorDataReceived -= new DataReceivedEventHandler(OnErrorDataReceived);

            //kill the process 
            aProcess.Kill();
            bool success = aProcess.WaitForExit((int)timeout.TotalMilliseconds);
            if (!success)
            {
                throw new Exception(String.Format("process {0} {1} could not be killed after {2} sec", aProcess.StartInfo.FileName, aProcess.StartInfo.Arguments, timeout.TotalSeconds));
            }
            else
            {
                ExitCode = aProcess.ExitCode;
                aProcess.Dispose();
            }
        }

        ///<apiflag>yes</apiflag>
        /// <summary>
        /// API:YES Checks if the previously executed command has written to stderr.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the previously executed command has written to stderr.</exception>
        public void CheckErrorThrow()
        {
            if (myStderr.Length > 0)
            {
                throw new InvalidOperationException(string.Format("There was an unexpected error: \r\n{0}\r\nThe original command was: {1}", myStderr, myLastCommand));
            }
        }

        #endregion

        #region Private Methods

        ///<apiflag>internal</apiflag>
        /// <summary>
        /// API:INTERNAL Handler for data received on stderr.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Received event args</param>
        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.EndsWith("\\n"))
                {
                    myStderr.Append(e.Data);
                }
                else
                {
                    myStderr.AppendLine(e.Data);
                }
                lock (this)
                {
                    myStdoutAndStderr.AppendLine(e.Data);

                    if (this.streamWriter != null)
                    {
                        this.streamWriter.WriteLine(e.Data);
                    }
                }
            }
        }

        ///<apiflag>internal</apiflag>
        /// <summary>
        /// API:INTERNAL Handler for data received on stdout.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Received event args</param>
        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.EndsWith("\\n"))
                {
                    myStdout.Append(e.Data);
                }
                else
                {
                    myStdout.AppendLine(e.Data);
                }
                lock (this)
                {
                    myStdoutAndStderr.AppendLine(e.Data);

                    if (this.streamWriter != null)
                    {
                        this.streamWriter.WriteLine(e.Data);
                    }
                }
            }
        }

        #endregion
    }
}


