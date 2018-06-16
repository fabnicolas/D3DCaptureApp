using Serilog.Core;
using SerilogLoggerSystem;
using System;
using System.IO;
using System.Net.Sockets;

namespace D3DCaptureApp {
    class NetClient {
        private readonly Logger logger = SerilogFactory.GetLogger();

        private TcpClient _client;
        private readonly FramingProtocol packetizer = new FramingProtocol(2000000);
        private readonly string ip;
        private readonly int port;
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
        
        /* ASYNC METHODS TO FIX
        public async Task<bool> ASYNC_send(Guid client_id, byte[] data) {
            try {
                using(NetworkStream stream = _client.GetStream()) {
                    await stream.WriteAsync(data,0,data.Length);
                    return await Task.FromResult<bool>(true);
                }
            } catch(Exception e) {
                logger.Information(e.Message);
                logger.Information(e.StackTrace);
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
                        logger.Information("Object disposed exception");
                        stop_client();
                        start_client();
                    }
                }
            } catch(Exception e) {
                logger.Information("[Client] ASYNC_on_server_response error: "+e.Message+"=TYPE="+e.GetType().Name);
                logger.Information(e.StackTrace);
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
        */

        public void OnServerMessage(Action<byte[]> callback) {
            try {
                byte[] buffer = new Byte[4573600];
                _packetizer.OnTransferCompleted=(data => {
                    if(data!=null) {
                        logger.Information("Depacking done. Data buffer length="+data.Length+". Processing data...");
                        byte[] extracted_data = ProcessData(data,data.Length);
                        if(extracted_data!=null) {
                            callback(extracted_data);
                        } else {
                            logger.Information("Notice: extracted_data is null.");
                        }
                    }
                });
                while(true) {   // Read input stream
                    try {
                        NetworkStream stream = _client.GetStream();
                        logger.Information("[Client] Waiting for server response...");
                        int bytes = stream.Read(buffer,0,buffer.Length);
                        byte[] data = new byte[bytes];
                        for(int i = 0;i<bytes;i++) {
                            data[i]=buffer[i];
                        }
                        logger.Information("[Client] Received from server "+bytes+" bytes. Unpacking...");
                        _packetizer.HandleReadData(data);
                    }catch(Exception e) {
                        if(e is System.Net.ProtocolViolationException) {
                            logger.Information("[Client] Error on frame dimension. Sending data will be skipped.");
                        }else if(e is ObjectDisposedException) {
                            logger.Information("Object disposed exception");
                            stop_client();
                            start_client();
                        } else {
                            throw;
                        }
                    }
                }
            } catch(Exception e) {
                logger.Information("Error not handled (Exception type: "+e.GetType().Name+"): "+e.Message);
                logger.Information(e.StackTrace);
            }
        }

        private byte[] ProcessData(byte[] buffer,int bytes) {
            if(bytes>0) {
                try {
                    logger.Information("It's time to process "+buffer.Length+" bytes. Trying decompressing...");
                    byte[] data = LZ4Compressor.Decompress(buffer);
                    logger.Information("LZ4 decompression done from "+buffer.Length+" bytes to: "+data.Length+" bytes.");
                    return data;
                }catch(ArgumentException) {
                    logger.Information("LZ4 decompression error, skipping frame!");
                    return null;
                }
            } else {
                return null;
            }
        }

        public static byte[] ConvertArraysIntToByte(int[] data) {
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
