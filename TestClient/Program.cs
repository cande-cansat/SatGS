using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    internal class Program
    {
        const int width = 640;
        const int height = 480;
        const int BufferSize = width * height + 1;
        static Random rand = new Random();

        static float[] gyros = new float[]
        {
            0.27f, -0.14f, 1.29f,
            0.67f, -0.05f, 1.48f,
            -96.95f, 1.59f, 3.02f,
            -1.88f, -8.35f, 1.5f,
            0.49f, 0.43f, 2.05f,
            0.43f, 0.01f, 1.46f,
            0.92f, 0.91f, 1.21f,
            171.2f, 1.53f, -3.68f,
            -2.26f, -0.17f, 1.41f,
            -31.3f, 3.99f, 15.7f,
            -1.18f, 0.55f, 1.17f,
            0.56f, -91.74f, -0.91f,
            0.63f, 4.69f, 1.13f,
            -3.14f, 100.35f, 5.78f,
            1.21f, 3.97f, 2.84f,
            18.48f, 85.05f, -8f,
            0.3f, -3.28f, 1.3f,
            -0.75f, -61.49f, -2.16f,
            0.46f, -0.24f, 1.27f,
            0.17f, -0.12f, 0.12f,
            4.32f, -1.84f, -1.54f,
            4.49f, 0.41f, 0.86f,
            9.61f, -12.31f, -3.07f,
            0.44f, -0.11f, 1.30f,
            -5.32f, 1.37f, -72.59f,
            0.35f, -0.26f, 1.24f,
            0.06f, 0.02f, -0.87f,
            4.13f, -2.49f, 43.05f,
            5.23f, -0.56f, 69.15f,
            0.56f, -0.20f, 1.91f,
            0.47f, 0.08f, 1.33f,
            -5.05f, 1.79f, -79.71f,
            0.65f, -0.41f, 1.26f,
            0.43f, -0.25f, 1.41f,
            0.69f, -0.24f, 1.23f
        };

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 6060);

            var image = MakeRandomImage();

            client.Client.Send(image, 0, BufferSize, SocketFlags.None);


            for (int i = 0; i < gyros.Length; i += 3)
            {
                var buffer = new byte[49];
                var rotation = new byte[][]
                {
                    BitConverter.GetBytes(gyros[i]),
                    BitConverter.GetBytes(gyros[i + 1]),
                    BitConverter.GetBytes(gyros[i + 2]),
                };

                int offset = 17;

                for(int y = 0; y < 3; ++y)
                {
                    for(int x = 0; x < 4; ++x)  
                    {
                        buffer[offset++] = rotation[y][x];
                    }
                }

                client.Client.Send(buffer, 0, 49, SocketFlags.None);

                Console.WriteLine("Sent");

                Thread.Sleep(500);
            }


            while (true) ;
        }

        static byte[] MakeRandomImage()
        {
            byte[] buffer = new byte[BufferSize];

            buffer[0] = 1;
            for (int i = 1; i < width * height; ++i)
            {
                buffer[i] = (byte)rand.Next(0, 255);
            }

            return buffer;
        }   
    }
}
