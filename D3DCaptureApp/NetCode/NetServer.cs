using Serilog.Core;
using SerilogLoggerSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace D3DCaptureApp {
    class NetServer {
        private readonly Logger logger = SerilogFactory.GetLogger();

        private TcpListener _server;
        private ConcurrentDictionary<Guid,TcpClient> _clients;
        private FramingProtocol _packetizer;

        public NetServer(string ip, int port) {
            _clients = new ConcurrentDictionary<Guid,TcpClient>();
            _server = new TcpListener(IPAddress.Parse(ip),port);
            _packetizer = new FramingProtocol(4573600);
        }

        public async void StartServer() {
            _server.Start();
            while(true) {
                AcceptClient(await _server.AcceptTcpClientAsync());
            }
        }

        private Guid AcceptClient(TcpClient client) {
            Guid new_client_id = Guid.NewGuid();
            try {
                _clients.TryAdd(new_client_id,client);
                return new_client_id;
            } catch(ArgumentNullException ane) {
                logger.Information("Argument is null.");
                logger.Information(ane.Message);
                logger.Information(ane.StackTrace);
                return Guid.Empty;
            } catch(OverflowException ofe) {
                logger.Information("Maximum number of possible clients reached.");
                logger.Information(ofe.Message);
                logger.Information(ofe.StackTrace);
                return Guid.Empty;
            }
        }

        public bool AcquireConnectedClient(Guid client_id,out TcpClient client) {
            return _clients.TryGetValue(client_id,out client);
        }

        public bool DisconnectClient(Guid client_id) {
            TcpClient client_discarded;
            bool try_remove=_clients.TryRemove(client_id,out client_discarded);
            client_discarded.Dispose();
            return try_remove;
        }
        
        /* ASYNC METHODS TO FIX
        public async Task<bool> ASYNC_send(Guid client_id, byte[] data) {
            try {
                TcpClient client;
                if(AcquireConnectedClient(client_id, out client)) {
                    logger.Information("Invoked...");
                    data=LZ4Compressor.Compress(data);
                    data=FramingProtocol.WrapMessage(data);
                    await client.GetStream().WriteAsync(data,0,data.Length).ConfigureAwait(false);
                    logger.Information("... returned");
                    return true;//return await Task.FromResult<bool>(true);
                } else {
                    return false;//await Task.FromResult<bool>(false);
                }
            } catch(Exception e) {
                logger.Information("Exception occurred while sending data as byte[] stream: "+e.Message);
                logger.Information(e.StackTrace);
                return false;// await Task.FromResult<bool>(false);
            }
        }
        public async Task<bool> ASYNC_send_bytes(Guid client_id, byte[] data) {
            return await ASYNC_send(client_id, data);
        }
        public async Task<bool> ASYNC_send_integers(Guid client_id, int[] data) {
            byte[] bytes_data = ConvertArraysIntToByte(data);
            return await ASYNC_send(client_id, bytes_data);
        }   
        public async Task<bool[]> ASYNC_sendToAll_bytes(byte[] data) {
            bool status = true;
            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach(KeyValuePair<Guid,TcpClient> client in _clients) {
                Task<bool> task_sendclient = ASYNC_send_bytes(client.Key,data);
                status=(!(await task_sendclient) ? false : status);
            }
            return await Task.WhenAll(tasks.ToArray());
        }
        */

        public bool Send(Guid client_id,byte[] data) {
            try {
                TcpClient client;
                if(AcquireConnectedClient(client_id,out client)) {
                    logger.Information("Data length to compress: "+data.Length+" bytes; compressing and wrapping...");
                    data=LZ4Compressor.Compress(data);
                    data=FramingProtocol.WrapMessage(data);
                    logger.Information("Data length after processing: "+data.Length+" bytes. Writing on stream.");
                    client.GetStream().Write(data,0,data.Length);
                    logger.Information(data.Length+" bytes wrote on stream!");
                    return true;
                } else {
                    return false;
                }
            } catch(Exception e) {
                if(e is IOException || e is InvalidOperationException || e is ObjectDisposedException) {
                    logger.Information("Socket connection lost with client with guid: "+client_id+". Client detached from server.");
                    DisconnectClient(client_id);
                } else {
                    logger.Information("An error has occurred while sending data: "+e.Message);
                    logger.Information(e.StackTrace);
                }
                return false;
            }
        }

        public bool SendBytes(Guid client_id,byte[] data) {
            return Send(client_id,data);
        }

        public bool SendIntegers(Guid client_id,int[] data) {
            byte[] bytes_data = ConvertArraysIntToByte(data);
            return Send(client_id,bytes_data);
        }

        public bool sendToAll_bytes(byte[] data) {
            bool status = true;
            foreach(KeyValuePair<Guid,TcpClient> client in _clients) {
                status=(!(SendBytes(client.Key,data)))==false ? false : status;
            }
            return status;
        }

        public static byte[] ConvertArraysIntToByte(int[] data) {
            byte[] bytes;
            using(var ms = new MemoryStream())
            using(var bw = new BinaryWriter(ms)) {
                for(int i = 0;i<3;i++)
                    bw.Write(data[i]);
                bytes=ms.ToArray();
            }
            return bytes;
        }
    }
}
