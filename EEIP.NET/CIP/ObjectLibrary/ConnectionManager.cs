namespace Sres.Net.EEIP.CIP.ObjectLibrary
{
    /// <summary>
    /// Connection Manager Object - Class ID 6
    /// </summary>
    public class ConnectionManager :
        ObjectBase
    {
        public ConnectionManager(EIPClient client) :
            base(client, ClassId)
        { }

        public const uint ClassId = 6;

        /// <summary>
        /// Returns the Explanation of a given statuscode (Table 3-5-29) Page 3-75 Vol 1
        /// </summary>
        /// <param name="status">Extended Status Code</param> 
        public static string GetExtendedStatus(ushort status)
        {
            return status switch
            {
                0x0100 => "Connection in use or duplicate forward open",
                0x0103 => "Transport class and trigger combination not supported",
                0x0106 => "Ownership conflict",
                0x0107 => "Target connection not found",
                0x0108 => "Invalid network connection parameter",
                0x0109 => "Invalid connection size",
                0x0110 => "Target for connection not configured",
                0x0111 => "RPI not supported",
                0x0113 => "Out of connections",
                0x0114 => "Vendor ID or product code missmatch",
                0x0115 => "Product type missmatch",
                0x0116 => "Revision mismatch",
                0x0117 => "Invalid produced or consumed application path",
                0x0118 => "Invalid or inconsistent configuration application path",
                0x0119 => "non-listen only connection not opened",
                0x011A => "Target Object out of connections",
                0x011B => "RPI is smaller than the production inhibit time",
                0x0203 => "Connection timed out",
                0x0204 => "Unconnected request timed out",
                0x0205 => "Parameter Error in unconnected request service",
                0x0206 => "Message too large for unconnected_send service",
                0x0207 => "Unconnected acknowledge without reply",
                0x0301 => "No Buffer memory available",
                0x0302 => "Network Bandwidth not available for data",
                0x0303 => "No consumed connection ID Filter available",
                0x0304 => "Not configured to send Scheduled priority data",
                0x0305 => "Schedule signature missmatch",
                0x0306 => "Schedule signature validation not possible",
                0x0311 => "Port not available",
                0x0312 => "Link address not valid",
                0x0315 => "Invalid segment in connection path",
                0x0316 => "Error in forward close service connection path",
                0x0317 => "Scheduling not specified",
                0x0318 => "Link address to self invalid",
                0x0319 => "Secondary resources unavailable",
                0x031A => "Rack connation already established",
                0x031B => "Module connection already established",
                0x031C => "Miscellaneous",
                0x031D => "Redundant connection Mismatch",
                0x031E => "No more user configurable link consumer resources available in the producing module",
                0x0800 => "Network link in path module is offline",
                0x0810 => "No target application data available",
                0x0811 => "No originator application data available",
                0x0812 => "Node address has changed since the network was scheduled",
                0x0813 => "Not configured for off-Subnet Multicast",
                _ => "unknown",
            };
        }
    }
}
