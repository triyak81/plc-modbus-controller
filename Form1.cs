using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace PLCController
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
                serialPort.Open();
                lblStatus.Text = "Durum: Bağlı";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Hata: " + ex.Message;
                MessageBox.Show("COM portu açılamadı! " + ex.Message);
            }
        }

        // Modbus RTU CRC hesapla
        private ushort CalculateCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            return crc;
        }

        // Y0'ı AÇ
        private void btnYes_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] command = new byte[8];
                command[0] = 0x01;        // Slave ID
                command[1] = 0x05;        // Function Code
                command[2] = 0x02;        // Address High Byte
                command[3] = 0x00;        // Address Low Byte
                command[4] = 0xFF;        // Value High Byte (ON)
                command[5] = 0x00;        // Value Low Byte

                ushort crc = CalculateCRC(command, 6);
                command[6] = (byte)(crc & 0xFF);
                command[7] = (byte)((crc >> 8) & 0xFF);

                serialPort.Write(command, 0, 8);
                lblY0Status.Text = "Y0: ON";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Y0'ı KAPA
        private void btnNo_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] command = new byte[8];
                command[0] = 0x01;        // Slave ID
                command[1] = 0x05;        // Function Code
                command[2] = 0x02;        // Address High Byte
                command[3] = 0x00;        // Address Low Byte
                command[4] = 0x00;        // Value High Byte (OFF)
                command[5] = 0x00;        // Value Low Byte

                ushort crc = CalculateCRC(command, 6);
                command[6] = (byte)(crc & 0xFF);
                command[7] = (byte)((crc >> 8) & 0xFF);

                serialPort.Write(command, 0, 8);
                lblY0Status.Text = "Y0: OFF";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}