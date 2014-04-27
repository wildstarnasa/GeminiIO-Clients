using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace GeminiScanner
{
    class ClipboardParser : IDisposable
    {
        private const string TOKEN = "GeminiIO";
        private string[] SEPERATOR = {":::"};
        private bool disposed = false;

        Dictionary<String, StreamWriter> writers;
        public ClipboardParser()
        {
            writers = new Dictionary<String, StreamWriter>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (StreamWriter writer in writers.Values)
                    {
                        writer.Dispose();
                    }
                }
            }
            disposed = true;
        }

        /// <summary>
        /// Occurs when a message is parsed.
        /// </summary>
        public event EventHandler<ParsedMessageArgs> ParsedMessage;

        /// <summary>
        /// Raises the <see cref="ParsedMessage"/> event.
        /// </summary>
        /// <param name="e">Event arguments for the event.</param>
        protected virtual void OnParsedMessage(ParsedMessageArgs e)
        {
            EventHandler<ParsedMessageArgs> handler = ParsedMessage;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        
        public void parseMessage(String incomingData)
        {
            if (incomingData == null || !incomingData.StartsWith(TOKEN))
            {
                // No logging/error this is just a regular copy we don't care about.
                return;
            }
            string[] segments = incomingData.Split(SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 3 && segments[1] != "Init")
            {
                Debug.WriteLine("Failed to parse tokenized clipboard content: " + incomingData);
                return;
            }
            switch (segments[1])
            {
                case "Init":
                    initGeminiScanner();
                    break;
                case "OpenFile":
                    openOutputFile(segments[2], false);
                    break;
                case "OpenFileAppend":
                    openOutputFile(segments[2], true);
                    break;
                case "CloseFile":
                    closeOutputFile(segments[2]);
                    break;
                case "WriteToFile":
                    if (segments.Length < 4) return;
                    writeContent(segments[2], segments[3]);
                    break;
                default:
                    Debug.WriteLine("Unknown MessageType Segment: " + segments[1]);
                    return;
            }
            ParsedMessageArgs e = new ParsedMessageArgs();
            e.action = segments[1];
            if (segments.Length >= 3)
            {
                e.path = segments[2];
            }
            else
            {
                e.path = "";
            }
            if (segments.Length >= 4)
            {
                e.message = segments[3];
            }
            else
            {
                e.message = "";
            }
            OnParsedMessage(e);
        }

        private StreamWriter openOutputFile(String strPath, Boolean bAppend)
        {
            StreamWriter writer = null;
            if (writers.ContainsKey(strPath))
            {
                return writers[strPath];
            }

            if (File.Exists(strPath) && !bAppend)
            {
                return null;
            }
            
            try
            {
                FileInfo pathInfo = new FileInfo(strPath);
                if (!Directory.Exists(pathInfo.Directory.FullName))
                {
                    Directory.CreateDirectory(pathInfo.Directory.FullName);
                }
                writer = new StreamWriter(strPath, bAppend);
                writers.Add(strPath, writer);
            }
            catch (IOException ex)
            {
            }

            return writer;
        }

        private void closeOutputFile(String strPath)
        {
            StreamWriter writer;

            if (!writers.ContainsKey(strPath))
            {
                return;
            }

            writer = writers[strPath];
            try
            {
                writer.Close();
            }
            catch (IOException ex)
            {
                
            }
            writers.Remove(strPath);
        }

        public void CloseFiles()
        {
            initGeminiScanner();
        }

        /// <summary>
        /// Upon GeminiIO init we need to close any open file handles.
        /// </summary>
        private void initGeminiScanner()
        {
            foreach (StreamWriter writer in writers.Values)
            {
                try
                {
                    writer.Close();
                }
                catch (IOException ex)
                {
                }
            }
            writers.Clear();
        }

        /// <summary>
        /// This function outputs some text to the specified file
        /// </summary>
        /// <param name="strPath">Path to the file being written to</param>
        /// <param name="strContent">String to write to the file</param>
        private void writeContent(String strPath, String strContent)
        {
            StreamWriter writer;

            if (!writers.ContainsKey(strPath))
            {
                writer = openOutputFile(strPath, true);
            }
            else
                writer = writers[strPath];

            try
            {
                writer.Write(strContent);
                writer.Flush();
            }
            catch (IOException ex)
            {

            }
        }
    }
}
