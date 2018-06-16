using System;

namespace D3DCaptureApp {
    /// <summary>
    /// 
    /// </summary>
    public class FramingProtocol {
        // Packet data
        private byte[] buffer_payload, buffer_data;

        // Protocol details
        private int n_bytes_received;   // The amount of bytes received (The total one is buffer_data.Length)
        private int max_length_message; // Maximum amount of data allowed (To prevent specific DDoS attack)
        private bool _instadrop = false; // (Temporary) Drop packets if connection isn't fast enough (For streaming)

        // Callbacks
        public Action<byte[]> onTransferComplete { get; set; }  // Transfer is completed

        public FramingProtocol(int max_length_message) {
            buffer_payload=new byte[sizeof(int)];   // Payload is at beginning of the packet and it has integer size
            this.max_length_message=max_length_message;
        }

        public static byte[] WrapMessage(byte[] message) {
            byte[] lengthPrefix = BitConverter.GetBytes(message.Length);
            byte[] ret = new byte[lengthPrefix.Length+message.Length];
            lengthPrefix.CopyTo(ret,0);
            message.CopyTo(ret,lengthPrefix.Length);

            return ret;
        }
        public static byte[] WrapKeepaliveMessage() {
            return BitConverter.GetBytes((int)0);
        }

        public void HandleReadData(byte[] data) {
            int i = 0;
            if(data!=null) while(i!=data.Length) {
                int n_bytes_available = data.Length-i;

                // Buffer selection (Payload must be filled before filling data).
                byte[] buffer;
                if(buffer_data==null) {
                    Console.WriteLine("[PacketProtocol] Using payload buffer");
                    buffer=buffer_payload;
                } else {
                    Console.WriteLine("[PacketProtocol] Using data buffer");
                    buffer=buffer_data;
                }

                int n_bytes_requested = buffer.Length-n_bytes_received;

                // Copy the incoming bytes into the buffer
                int n_bytes_transferred = Math.Min(n_bytes_requested,n_bytes_available);
                Array.Copy(data,i,buffer,n_bytes_received,n_bytes_transferred);
                i+=n_bytes_transferred;


                Console.WriteLine("[PacketProtocol] i="+i+",d.L="+data.Length+",expected="+(buffer_data!=null ? buffer_data.Length : 4)+",nrec="+n_bytes_received+",nreq="+n_bytes_requested+",nt="+n_bytes_transferred+",payload="+BitConverter.ToInt32(buffer_payload,0));


                ReadCompleted(n_bytes_transferred); // Notify "read completion"
            }
            Console.WriteLine("[PacketProtocol] No more data");
        }
        
        private void ReadCompleted(int count) {
            n_bytes_received+=count; // Get the number of bytes read into the buffer

            if(buffer_data==null) {
                // At beginning let's handle the payload, which length will be at some time sizeof(int).
                if(n_bytes_received==sizeof(int)) {
                    // We've gotten the payload buffer.
                    int length = BitConverter.ToInt32(this.buffer_payload,0);
                    if(length<0)
                        throw new System.Net.ProtocolViolationException("Error: length of the message < 0");

                    // Another sanity check is needed here for very large packets, to prevent denial-of-service attacks
                    if(max_length_message>0&&length>max_length_message)
                        throw new System.Net.ProtocolViolationException("Message length "+length.ToString(System.Globalization.CultureInfo.InvariantCulture)+" is larger than maximum message size "+this.max_length_message.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    n_bytes_received=0;
                    if(length==0) {
                        // Zero-length packets are allowed as keepalives
                        if(onTransferComplete!=null)
                            onTransferComplete(new byte[0]);
                    } else {
                        // Create the data buffer and start reading into it
                        buffer_data=new byte[length];
                    }
                } // Else we haven't gotten all the length buffer yet: just wait for more data to arrive
            } else {
                bool reset = false;
                if(n_bytes_received==buffer_data.Length) {
                    // Packet is ready! Let's callback with the full data.
                    Console.WriteLine(DateTime.Now+": COMPLETE ("+n_bytes_received+")");
                    if(onTransferComplete!=null)
                        onTransferComplete(this.buffer_data);
                    reset=true;
                } else if(_instadrop && count!=buffer_data.Length) {
                    Console.WriteLine(DateTime.Now+": DROPPED");
                    if(onTransferComplete!=null)
                        onTransferComplete(null);
                    reset=true;
                }// Else we haven't gotten all the data buffer yet: just wait for more data to arrive

                if(reset) {
                    buffer_payload=new byte[sizeof(int)];
                    buffer_data=null;
                    n_bytes_received=0;
                }
            }
        }
    }
}
