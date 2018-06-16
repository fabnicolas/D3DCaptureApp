using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace D3DCaptureApp {
    class NetClient { 
        private TcpClient _client;
        private FramingProtocol packetizer = new FramingProtocol(2000000);
        private string ip;
        private int port;
        private FramingProtocol _packetizer;

        public NetClient(string ip, int port) {
            _client=new TcpClient();
            _client.NoDelay=true;
            _client.LingerState=new LingerOption(true,0);
            this.ip=ip;
            this.port=port;
            _packetizer=new FramingProtocol(4573600);
        }

        public async void start_client() {
            await _client.ConnectAsync(ip,port);
        }

        public void stop_client() {
            _client.GetStream().Dispose();
        }
        
        public async Task<bool> ASYNC_send(Guid client_id, byte[] data) {
            try {
                using(NetworkStream stream = _client.GetStream()) {
                    await stream.WriteAsync(data,0,data.Length);
                    return await Task.FromResult<bool>(true);
                }
            } catch(Exception e) {
                Console.WriteLine(e.Message);
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
        
        public async Task<object> ASYNC_on_server_response(Action<byte[]> callback) {
            try {
                byte[] buffer = new Byte[4573600];
                _packetizer.onTransferComplete=(data => {
                    //if(data!=null) callback(data);
                });
                while(true) {   // Read input stream
                    try {
                        NetworkStream stream = _client.GetStream();
                        int bytes = await stream.ReadAsync(buffer,0,buffer.Length);
                        byte[] data = await ASYNC_process_data(buffer,bytes);
                        Console.Write("bytes_received="+bytes+",packing...");
                        _packetizer.HandleReadData(data);
                    } catch(ObjectDisposedException) {
                        Console.WriteLine("Object disposed exception");
                        stop_client();
                        start_client();
                    }
                }
            } catch(Exception e) {
                Console.WriteLine("[Client] ASYNC_on_server_response error: "+e.Message+"=TYPE="+e.GetType().Name);
                Console.WriteLine(e.StackTrace);
            }
            return await Task.FromResult<object>(null);
        }

        private async Task<byte[]> ASYNC_process_data(byte[] buffer,int bytes) {
            if(bytes>0) {
                Console.Write("[Client] ");
                byte[] data = LZ4Compressor.Compress(buffer);
                return await Task.FromResult<byte[]>(data);
            } else {
                return await Task.FromResult<byte[]>(null);
            }
        }

        public void on_server_response(Action<byte[]> callback) {
            try {
                byte[] buffer = new Byte[4573600];
                _packetizer.onTransferComplete=(data => {
                    if(data!=null) {
                        Console.WriteLine("[Client] Depacking done. Data buffer length="+data.Length+". Decompressing...");
                        byte[] extracted_data = process_data(data,data.Length);
                        if(extracted_data!=null) callback(extracted_data);
                    }
                });
                while(true) {   // Read input stream
                    try {
                        NetworkStream stream = _client.GetStream();
                        Console.WriteLine("[Client] Waiting for server response...");
                        int bytes = stream.Read(buffer,0,buffer.Length);
                        byte[] data = new byte[bytes];
                        for(int i = 0;i<bytes;i++) {
                            data[i]=buffer[i];
                        }
                        Console.WriteLine("[Client] Received from server "+bytes+" bytes. Unpacking...");
                        _packetizer.HandleReadData(data);
                    }catch(Exception e) {
                        if(e is System.Net.ProtocolViolationException) {
                            Console.WriteLine("[Client] Error on frame dimension. Sending data will be skipped.");
                        }else if(e is ObjectDisposedException) {
                            Console.WriteLine("Object disposed exception");
                            stop_client();
                            start_client();
                        } else {
                            throw;
                        }
                    }
                }
            } catch(Exception e) {
                Console.WriteLine("[Client] on_server_response error: "+e.Message+"=TYPE="+e.GetType().Name);
                Console.WriteLine(e.StackTrace);
            }
        }

        private byte[] process_data(byte[] buffer,int bytes) {
            if(bytes>0) {
                Console.Write("[Client] ");
                try {
                    byte[] data = LZ4Compressor.Decompress(buffer);
                    return data;
                }catch(ArgumentException) {
                    Console.WriteLine("LZ4 decompression error, skipping frame!");
                    return null;
                }
            } else {
                return null;
            }
        }

        public static byte[] toByteArray(int[] data) {
            byte[] bytes;
            using(var ms = new MemoryStream())
            using(var bw = new BinaryWriter(ms)) {
                for(int i = 0;i<data.Length;i++)
                    bw.Write(data[i]);
                bytes=ms.ToArray();
            }
            return bytes;
        }
    }
}
