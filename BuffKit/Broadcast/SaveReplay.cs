using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BuffKit.Broadcast
{
    public class SaveReplay
    {
        private static void Begin(MessageQueue messageQueue, string matchID)
        {

            MuseLog.Info("Opening save file for match: " + matchID);

            string path = Directory.GetCurrentDirectory();
            string replayDirectory = "Replays";
            var target = Path.Combine(path, replayDirectory);
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            string txtFileName = matchID + ".txt";
            var txtTargetFile = Path.Combine(target, txtFileName);
            var gzTargetFile = Path.Combine(target, txtFileName + ".gz");

            using (StreamWriter sw = File.CreateText(txtTargetFile)) { 
                sw.Write("{\n\"data\":[\n");

                bool firstItem = true;

                while (Instance._doRun)
                {
                    if (messageQueue.TryDequeue(out var item))
                    {
                        if (!firstItem)
                            sw.Write(",\n");
                        sw.Write(item);
                        firstItem = false;
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }

                sw.WriteLine("\n]\n}");
            }

            MuseLog.Info("Closed save file for match: " + matchID);
            MuseLog.Info("Compressing save file for match: " + matchID);

            using (FileStream ofs = File.Open(Path.Combine(replayDirectory, txtTargetFile), FileMode.Open))
            {
                using (FileStream cfs = File.Create(Path.Combine(replayDirectory, gzTargetFile)))
                {
                    using (var compressor = new GZipStream(cfs, CompressionMode.Compress))
                    {
                        // TODO: make this nicer - may not actually close?
                        while (true)
                        {
                            var b = ofs.ReadByte();
                            if (b == -1) break;
                            compressor.WriteByte((byte)b);
                        }
                    }
                }
            }

            MuseLog.Info("Compressed save file for match: " + matchID + ", deleting original");
        }

        private MessageQueue _messageQueue;
        private volatile bool _doRun = false;
        private float _prevTime = 0;

        public void SaveMatchData(MatchData data)
        {
            float currTime = UnityEngine.Time.realtimeSinceStartup;
            float timeChange = currTime - _prevTime;
            _prevTime = currTime;
            //MuseLog.Info("Data per second: " + (1 / timeChange));

            string dataStr = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            //MuseLog.Info(dataStr);
            _messageQueue.Enqueue(dataStr);
        }

        public void BeginMatch(string matchID)
        {
            if (_doRun) { 
                MuseLog.Info("Attempted to begin recording match while match is already running, ignored");
                return;
            }
            _doRun = true;
            _messageQueue = new MessageQueue();
            var th = new Thread(() => Begin(_messageQueue, matchID));
            th.Start();
        }

        public void EndMatch() {
            _doRun = false;
        }

        public static SaveReplay Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance == null)
                Instance = new SaveReplay();
            MatchBlockerView.Done += delegate
            {
                Instance.BeginMatch(MatchLobbyView.Instance.MatchId);
            };
        }

        private SaveReplay() { }

        private class MessageQueue
        {
            private Queue<string> _queue;
            private readonly object _lock = new object();
            public void Enqueue(string msg)
            {
                lock (_lock)
                {
                    _queue.Enqueue(msg);
                }
            }
            public bool TryDequeue(out string msg)
            {
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        msg = _queue.Dequeue();
                        return true;
                    }
                    else
                    {
                        msg = null;
                        return false;
                    }
                }
            }
            public MessageQueue()
            {
                _queue = new Queue<string>();
            }
        }
    }
}
