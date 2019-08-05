using System.Net;

namespace DFAssist.Core.Network
{
    public class Connection
    {
        public IPEndPoint LocalEndPoint { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var connection = obj as Connection;

            return LocalEndPoint.Equals(connection?.LocalEndPoint) && RemoteEndPoint.Equals(connection?.RemoteEndPoint);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (LocalEndPoint == null | RemoteEndPoint == null)
                return -1;

            return (LocalEndPoint.GetHashCode() + 0x0609) ^ RemoteEndPoint.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override string ToString()
        {
            return $"{LocalEndPoint} -> {RemoteEndPoint}";
        }
    }
}