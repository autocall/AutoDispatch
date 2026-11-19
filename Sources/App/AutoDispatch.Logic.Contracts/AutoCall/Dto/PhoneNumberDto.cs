using System;

namespace AutoCall.Logic.Dto {
    /// <summary>
    ///     Represents phone number class to get correct format </summary>
    public class PhoneNumberDto {
        #region [ Properties ]

        /// <summary>
        ///     Gets or sets country code </summary>
        public int CountryCode { get; set; }
        /// <summary>
        ///     Gets or sets region </summary>
        public string Region { get; set; }
        /// <summary>
        ///     Gets or sets local number </summary>
        public string LocalNumber { get; set; }
        /// <summary>
        ///     Gets or sets extension number </summary>
        public string Extension { get; set; }

        /// <summary>
        ///     Gets phone number in international format </summary>
        public string InternationalPhoneNumber {
            get { return String.Format("{0}{1}{2}", CountryCode, Region, LocalNumber); }
        }
        /// <summary>
        ///     Gets phone number in international format </summary>
        public string PhoneNumber {
            get { return String.Format("{0}{1}", Region, LocalNumber); }
        }
        /// <summary>
        ///     Gets has phone number extension </summary>
        public bool HasExtension {
            get { return !String.IsNullOrEmpty(Extension); }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public PhoneNumberDto() {
            // USA phone code
            CountryCode = 1;
        }

        #endregion [ .ctor ]

        #region [ Override Methods ]

        /// <summary>
        ///     Overrided ToString method </summary>
        /// <returns>Phone number in international format</returns>
        public override string ToString() {
            if (!HasExtension)
                return InternationalPhoneNumber;

            return $"{InternationalPhoneNumber} ({Extension})";
        }

        public override bool Equals(object obj) {
            return (obj is PhoneNumberDto) &&
                ((PhoneNumberDto)obj).CountryCode == CountryCode &&
                ((PhoneNumberDto)obj).Region == Region &&
                ((PhoneNumberDto)obj).LocalNumber == LocalNumber &&
                ((PhoneNumberDto)obj).Extension == Extension;
        }

        #endregion [ Override Methods ]
    }
}