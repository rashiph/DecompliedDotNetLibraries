namespace Microsoft.VisualBasic.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO.Ports;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Ports
    {
        public SerialPort OpenSerialPort(string portName)
        {
            SerialPort port2 = new SerialPort(portName);
            port2.Open();
            return port2;
        }

        public SerialPort OpenSerialPort(string portName, int baudRate)
        {
            SerialPort port2 = new SerialPort(portName, baudRate);
            port2.Open();
            return port2;
        }

        public SerialPort OpenSerialPort(string portName, int baudRate, Parity parity)
        {
            SerialPort port2 = new SerialPort(portName, baudRate, parity);
            port2.Open();
            return port2;
        }

        public SerialPort OpenSerialPort(string portName, int baudRate, Parity parity, int dataBits)
        {
            SerialPort port2 = new SerialPort(portName, baudRate, parity, dataBits);
            port2.Open();
            return port2;
        }

        public SerialPort OpenSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            SerialPort port2 = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            port2.Open();
            return port2;
        }

        public ReadOnlyCollection<string> SerialPortNames
        {
            get
            {
                string[] portNames = SerialPort.GetPortNames();
                List<string> list = new List<string>();
                foreach (string str in portNames)
                {
                    list.Add(str);
                }
                return new ReadOnlyCollection<string>(list);
            }
        }
    }
}

