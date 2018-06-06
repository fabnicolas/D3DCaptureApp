using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace D3DCaptureApp {
    class NetServer {
        private TcpListener _server;
        private ConcurrentDictionary<Guid,TcpClient> _clients;
        private FramingProtocol _packetizer;

        public NetServer(string ip, int port) {
            _clients = new ConcurrentDictionary<Guid,TcpClient>();
            _server = new TcpListener(IPAddress.Parse(ip),port);
            _packetizer = new FramingProtocol(4573600);
        }

        public async void start_server() {
            _server.Start();
            while(true) {
                accept_client(await _server.AcceptTcpClientAsync());
            }
        }

        private Guid accept_client(TcpClient client) {
            Guid new_client_id = Guid.NewGuid();
            try {
                _clients.TryAdd(new_client_id,client);
                return new_client_id;
            } catch(ArgumentNullException ane) {
                Console.WriteLine("Argument is null");
                Console.WriteLine(ane.Message);
                Console.WriteLine(ane.StackTrace);
                return Guid.Empty;
            } catch(OverflowException ofe) {
                Console.WriteLine("Maximum number of possible clients reached");
                Console.WriteLine(ofe.Message);
                Console.WriteLine(ofe.StackTrace);
                return Guid.Empty;
            }
        }

        public bool acquire_connected_client(Guid client_id,out TcpClient client) {
            return _clients.TryGetValue(client_id,out client);
        }
        
        public async Task<bool> ASYNC_send(Guid client_id, byte[] data) {
            try {
                TcpClient client;
                if(acquire_connected_client(client_id, out client)) {
                    Console.WriteLine("Client acquired. Data length="+data.Length);
                    data=FramingProtocol.WrapMessage(data);
                    await client.GetStream().WriteAsync(data,0,data.Length);
                    return await Task.FromResult<bool>(true);
                } else {
                    return await Task.FromResult<bool>(false);
                }
            } catch(Exception e) {
                Console.WriteLine("[Server] ASYNC_send error: "+e.Message);
                Console.WriteLine(e.StackTrace);
                return await Task.FromResult<bool>(false);
            }
        }

        public async Task<bool> ASYNC_send_bytes(Guid client_id, byte[] data) {
            return await ASYNC_send(client_id, data);
        }

        public async Task<bool> ASYNC_send_integers(Guid client_id, int[] data) {
            byte[] bytes_data = toByteArray(data);
            return await ASYNC_send(client_id, bytes_data);
        }
        
        public async Task<bool> ASYNC_sendToAll_bytes(byte[] data) {
            bool status = true;
            foreach(KeyValuePair<Guid,TcpClient> client in _clients) {
                if(!await ASYNC_send_bytes(client.Key,data))
                    status=false;
            }
            return await Task.FromResult<bool>(status);
        }

        public static byte[] toByteArray(int[] data) {
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
