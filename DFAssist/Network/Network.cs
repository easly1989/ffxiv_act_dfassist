using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace DFAssist
{
    public class Network
    {
        public const int TcpTableOwnerPidConnections = 4;

        private readonly string _exePath;
        private readonly byte[] _recvBuffer;
        private readonly object _lockAnalyse;

        private int _pid;
        private State _state;
        private Socket _socket;
        private List<Connection> _connections;
        public byte[] RcvallIplevel { get; }
        public bool IsRunning { get; private set; }

        public Network()
        {
            _state = State.IDLE;
            _connections = new List<Connection>();
            _lockAnalyse = new object();
            _recvBuffer = new byte[0x20000];

            RcvallIplevel = new byte[] { 3, 0, 0, 0 };
            _exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }

        public bool StartCapture(System.Diagnostics.Process process)
        {
            _pid = process.Id;
            try
            {
                Logger.Info("l-network-starting");

                if(IsRunning)
                {
                    Logger.Error("l-network-error-already-started");
                    return false;
                }

                UpdateGameConnections(process);

                if(_connections.Count < 2)
                {
                    Logger.Error("l-network-error-no-connection");
                    return false;
                }

                var localConnection = _connections.FirstOrDefault(x => x.LocalEndPoint.Address.ToString() != "127.0.0.1");
                if(localConnection == null)
                {
                    Logger.Error("l-network-error-no-connection");
                    return false;
                }

                var localAddress = localConnection.LocalEndPoint.Address;
                Logger.Success($"LocalEndPoint Found: {localAddress}");
                
                RegisterToFirewall();

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                _socket.Bind(new IPEndPoint(localAddress, 0));
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AcceptConnection, true);
                _socket.IOControl(IOControlCode.ReceiveAll, RcvallIplevel, null);
                _socket.ReceiveBufferSize = _recvBuffer.Length * 4;

                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, 0, OnReceive, null);
                IsRunning = true;

                Logger.Success("l-network-started");
                return true;
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-network-error-starting");
                return false;
            }
        }

        public void StopCapture()
        {
            try
            {
                if(!IsRunning)
                {
                    Logger.Error("l-network-error-already-stopped");
                    return;
                }

                _socket.Close();
                _connections.Clear();
                Logger.Info("l-network-stopping");
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-network-error-stopping");
            }
        }

        public void UpdateGameConnections(System.Diagnostics.Process process)
        {
            var update = _connections.Count < 2;
            var currentConnections = GetConnections(process);

            foreach(var connection in _connections)
            {
                if(currentConnections.Contains(connection))
                    continue;

                // Connection was lost, a new update is requested
                update = true;
                Logger.Error("l-network-detected-connection-closing");
                break;
            }

            if(!update)
                return;

            var lobbyEndPoint = GetLobbyEndPoint(process);
            _connections = currentConnections.Where(x => !x.RemoteEndPoint.Equals(lobbyEndPoint)).ToList();

            foreach(var connection in _connections)
            {
                Logger.Info("l-network-detected-connection", connection);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var length = _socket.EndReceive(ar);
                var buffer = _recvBuffer.Take(length).ToArray();
                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, 0, OnReceive, null);

                FilterAndProcessPacket(buffer);
            }
            catch(Exception ex) when(ex is ObjectDisposedException || ex is NullReferenceException)
            {
                IsRunning = false;
                _socket = null;
                Logger.Success("l-network-stopped");
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-network-error-receiving-packet");
            }
        }

        private void FilterAndProcessPacket(byte[] buffer)
        {
            try
            {
                var ipPacket = new IpPacket(buffer);
                if(!ipPacket.IsValid || ipPacket.Protocol != ProtocolType.Tcp)
                    return;

                var tcpPacket = new TcpPacket(ipPacket.Data);
                if(!tcpPacket.IsValid)
                    return;

                if(!tcpPacket.Flags.HasFlag(TcpFlags.ACK | TcpFlags.PSH))
                    return;

                var sourceEndPoint = new IPEndPoint(ipPacket.SourceIpAddress, tcpPacket.SourcePort);
                var destinationEndPoint = new IPEndPoint(ipPacket.DestinationIpAddress, tcpPacket.DestinationPort);
                var connection = new Connection { LocalEndPoint = sourceEndPoint, RemoteEndPoint = destinationEndPoint };
                var reverseConnection = new Connection { LocalEndPoint = destinationEndPoint, RemoteEndPoint = sourceEndPoint };

                if(!(_connections.Contains(connection) || _connections.Contains(reverseConnection)))
                    return;

                if(!_connections.Contains(reverseConnection))
                    return;

                lock(_lockAnalyse)
                {
                    FFXIVPacketHandler.Analyze(_pid, tcpPacket.Payload, ref _state);
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-network-error-filtering-packet");
            }
        }

        private void RegisterToFirewall()
        {
            try
            {
                var netFwMgr = GetInstance<INetFwMgr>("HNetCfg.FwMgr");
                var netAuthApps = netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications;

                var exists = false;
                foreach(var netAuthAppObject in netAuthApps)
                {
                    if(netAuthAppObject is INetFwAuthorizedApplication netAuthApp && netAuthApp.ProcessImageFileName == _exePath && netAuthApp.Enabled)
                    {
                        exists = true;
                    }
                }

                if(exists)
                    return;

                var networkApp = GetInstance<INetFwAuthorizedApplication>("HNetCfg.FwAuthorizedApplication");

                networkApp.Enabled = true;
                networkApp.Name = "FFXIV_DFAssist";
                networkApp.ProcessImageFileName = _exePath;
                networkApp.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;

                netAuthApps.Add(networkApp);

                Logger.Success("l-firewall-registered");
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-firewall-error");
            }
        }

        private IPEndPoint GetLobbyEndPoint(System.Diagnostics.Process process)
        {
            IPEndPoint ipep = null;
            string lobbyHost = null;
            var lobbyPort = 0;

            try
            {
                using(var managementObjectSearcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                {
                    foreach(var managementBaseObject in managementObjectSearcher.Get())
                    {
                        var commandline = managementBaseObject["CommandLine"].ToString();
                        var args = commandline.Split(' ');

                        foreach(var arg in args)
                        {
                            var splitted = arg.Split('=');
                            if(splitted.Length != 2)
                                continue;

                            switch(splitted[0])
                            {
                                case "DEV.LobbyHost01":
                                    lobbyHost = splitted[1];
                                    break;
                                case "DEV.LobbyPort01":
                                    lobbyPort = int.Parse(splitted[1]);
                                    break;
                            }
                        }
                    }
                }

                if(lobbyHost != null && lobbyPort > 0)
                {
                    var address = Dns.GetHostAddresses(lobbyHost)[0];
                    ipep = new IPEndPoint(address, lobbyPort);
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-network-error-finding-lobby");
            }

            return ipep;
        }

        private static List<Connection> GetConnections(System.Diagnostics.Process process)
        {
            var connections = new List<Connection>();

            var tcpTable = IntPtr.Zero;
            var tcpTableLength = 0;

            if(NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, false, AddressFamily.InterNetwork, TcpTableOwnerPidConnections, 0) == 0)
                return connections;

            try
            {
                tcpTable = Marshal.AllocHGlobal(tcpTableLength);
                if(NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, false, AddressFamily.InterNetwork, TcpTableOwnerPidConnections, 0) == 0)
                {
                    var table = (TcpTable)Marshal.PtrToStructure(tcpTable, typeof(TcpTable));
                    var rowPointer = new IntPtr(tcpTable.ToInt64() + Marshal.SizeOf(typeof(uint)));

                    for(var i = 0; i < table.length; i++)
                    {
                        var row = (TcpRow)Marshal.PtrToStructure(rowPointer, typeof(TcpRow));

                        if(row.owningPid == process.Id)
                        {
                            var local = new IPEndPoint(row.localAddr, (ushort)IPAddress.NetworkToHostOrder((short)row.localPort));
                            var remote = new IPEndPoint(row.remoteAddr, (ushort)IPAddress.NetworkToHostOrder((short)row.remotePort));

                            connections.Add(new Connection() { LocalEndPoint = local, RemoteEndPoint = remote });
                        }

                        rowPointer = new IntPtr(rowPointer.ToInt64() + Marshal.SizeOf(typeof(TcpRow)));
                    }
                }
            }
            finally
            {
                if(tcpTable != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tcpTable);
                }
            }

            return connections;
        }

        private static T GetInstance<T>(string typeName)
        {
            return (T)Activator.CreateInstance(Type.GetTypeFromProgID(typeName));
        }
    }
}
