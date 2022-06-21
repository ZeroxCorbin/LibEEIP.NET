using System;
using System.Linq;
using Sres.Net.EEIP.CIP.ObjectLibrary;

namespace Sres.Net.EEIP.CIP
{
    public class GeneralException :
        Exception
    {
        protected GeneralException(string message, byte status, GeneralException innerException = null) :
            base(message, innerException)
            => Status = status;

        public GeneralException(byte code, GeneralException innerException = null) :
            this(GetMessage(code, GetMessage(code)), code, innerException)
        { }

        public static GeneralException From(MessageRouterResponse response, Func<ushort, string> getStatusMessage = null)
        {
            if (response is null)
                throw new ArgumentNullException(nameof(response));
            return response.Status == Success ?
                null :
                response.ExtendedStatuses.Any() ?
                    new ExtendedException(
                        response.Status,
                        response.ExtendedStatuses,
                        getStatusMessage ??
                        (response.Status == ConnectionFailure ?
                            ConnectionManager.GetExtendedStatus :
                            null)) :
                    new GeneralException(response.Status);
        }

        protected internal static string GetMessage<T>(T status, string message = null) => message is null ?
            status?.ToString() :
            $"{status}: {message}";

        /// <summary>
        /// General status
        /// </summary>
        public byte Status { get; }

        public const byte Success = 0x00;
        public const byte ConnectionFailure = 0x01;

        /// <summary>
        /// Table B-1.1 CIP General Status Codes
        /// </summary>
        protected static string GetMessage(byte status)
        {
            switch (status)
            {
                case Success: return "Success";
                case ConnectionFailure: return "Connection failure";
                case 0x02: return "Resource unavailable";
                case 0x03: return "Invalid Parameter value";
                case 0x04: return "Path segment error";
                case 0x05: return "Path destination unknown";
                case 0x06: return "Partial transfer";
                case 0x07: return "Connection lost";
                case 0x08: return "Service not supported";
                case 0x09: return "Invalid attribute value";
                case 0x0A: return "Attribute List error";
                case 0x0B: return "Already in requested mode/state";
                case 0x0C: return "Object state conflict";
                case 0x0D: return "Object already exists";
                case 0x0E: return "Attribute not settable";
                case 0x0F: return "Privilege violation";
                case 0x10: return "Device state conflict";
                case 0x11: return "Reply data too large";
                case 0x12: return "Fragmentation of a primitive value";
                case 0x13: return "Not enough data";
                case 0x14: return "Attribute not supported";
                case 0x15: return "Too much data";
                case 0x16: return "Object does not exist";
                case 0x17: return "Service fragmentation sequence not in progress";
                case 0x18: return "No stored attribute data";
                case 0x19: return "Store operation failure";
                case 0x1A: return "Routing failure, request packet too large";
                case 0x1B: return "Routing failure, response packet too large";
                case 0x1C: return "Missing attribute list entry data";
                case 0x1D: return "Invalid attribute value list";
                case 0x1E: return "Embedded service error";
                case 0x1F: return "Vendor specific error";
                case 0x20: return "Invalid parameter";
                case 0x21: return "Write-once value or medium atready written";
                case 0x22: return "Invalid Reply Received";
                case 0x23: return "Buffer overflow";
                case 0x24: return "Message format error";
                case 0x25: return "Key failure path";
                case 0x26: return "Path size invalid";
                case 0x27: return "Unecpected attribute list";
                case 0x28: return "Invalid Member ID";
                case 0x29: return "Member not settable";
                case 0x2A: return "Group 2 only Server failure";
                case 0x2B: return "Unknown Modbus Error";
                default: return "unknown";
            }
        }
    }
}
