using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     DTO of Country </summary>
    /// <remarks> Represent database object </remarks>
    [DataContract]
    public class SetupPacketCountryDto {
        /// <summary>
        ///     Entity ID </summary>
        [DataMember]public byte EntityId { get; set; }
        /// <summary>
        ///     country ID </summary>
        [DataMember]public string Name { get; set; }
        /// <summary>
        ///     Code of Country </summary>
        /// <example>US, CA, MX</example>
        [DataMember]public string Code { get; set; }
    }
}