using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     DTO of State </summary>
    /// <remarks> Represent database object </remarks>
    [DataContract]
    public class SetupPacketStateDto {
        /// <summary>
        ///     Entity ID </summary>
        [DataMember]public byte EntityId { get; set; }
        /// <summary>
        ///     Code of State </summary>
        /// <example>"AG", "CH", "CU"</example>
        [DataMember]public string Code { get; set; }
        /// <summary>
        ///     Name of State </summary>
        [DataMember]public string Name { get; set; }
        /// <summary>
        ///     Zone ID </summary>
        [DataMember]public byte ZoneId { get; set; }
    }
}