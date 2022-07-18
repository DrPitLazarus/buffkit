using System.Threading;
using System.Collections.Generic;
using WebSocketSharp;

namespace BuffKit.Broadcast
{


    public class Broadcaster
    {
        private static void Broadcast(MessageQueue messageQueue)
        {
            // Try to connect
            var socket = new WebSocket("ws://localhost:8080");
            try
            {
                socket.Connect();
                socket.Send("broadcast");
                while (Instance._doRun)
                {
                    if (messageQueue.TryDequeue(out var item))
                    {
                        socket.Send(item);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                socket.Close();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                MuseLog.Error("Got SocketException:\n" + e.Message);
            }
            catch (System.InvalidOperationException e)
            {
                MuseLog.Error("Got InvalidOperationException:\n" + e.Message);
            }
        }

        private MessageQueue _messageQueue;
        private volatile bool _doRun = true;
        private float _prevTime = 0;

        public void SendMatchData(MatchData data)
        {
            float currTime = UnityEngine.Time.realtimeSinceStartup;
            float timeChange = currTime - _prevTime;
            _prevTime = currTime;
            MuseLog.Info("Data per second: " + (1 / timeChange));

            string dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            MuseLog.Info(dataStr);
        }

        public void StopBroadcasting()
        {
            _doRun = false;
        }

        private Broadcaster()
        {
            _messageQueue = new MessageQueue();
            var th = new Thread(() => Broadcast(_messageQueue));
            th.Start();
        }
        public static Broadcaster Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance == null)
                Instance = new Broadcaster();
        }

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
