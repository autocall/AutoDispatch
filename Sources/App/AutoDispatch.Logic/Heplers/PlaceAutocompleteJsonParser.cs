using System;
using AutoDispatch.Dtos.Dtos;
using Newtonsoft.Json.Linq;

namespace AutoDispatch.Logic.Heplers {
    public static class PlaceAutocompleteJsonParser {

        public static string GetMainMatch(JObject obj) {
            try {
                var strucured = ((dynamic)obj).structured_formatting;
                string mainMatch = ((string)strucured.main_text.Value).Substring((int)strucured.main_text_matched_substrings[0].offset.Value, (int)strucured.main_text_matched_substrings[0].length.Value);
                return mainMatch;
            } catch (Exception) {
                return null;
            }
        }

        public static string GetMainText(JObject obj) {
            try {
                var strucured = ((dynamic)obj).structured_formatting;
                string mainText = (string)strucured.main_text.Value;
                return mainText;
            } catch (Exception) {
                return null;
            }
        }

        public static string GetSecondaryText(JObject obj) {
            try {
                var strucured = ((dynamic)obj).structured_formatting;
                if (((JObject)strucured).Property("secondary_text") != null) {
                    string secondaryText = strucured.secondary_text.Value;
                    return secondaryText;
                } else {
                    return null;
                }
            } catch (Exception) {
                return null;
            }
        }

        public static string GetPlaceId(JObject obj) {
            try {
                return ((dynamic) obj).place_id.Value;
            } catch (Exception) {
                return null;
            }
        }

        /// <summary>
        ///     Likes to SelectLocation.ts -> getAddrComponentsByType(addrs, type) </summary>
        private static JObject getAddrComponentsByType(dynamic addrs, string type) {
            for (var i = 0; i < ((JContainer)addrs).Count; i++) {
                if (addrs[i].types[0] == type) {
                    return addrs[i];
                }
            }
            return null;
        }

        /// <summary>
        ///     Likes to SelectLocation.ts -> getCityAndAddress(_this: SelectLocation, result: google.maps.GeocoderResult) </summary>
        public static QueryLoadDto GetCityAndAddress(dynamic result) {
            try {
                //"long_name" : "Winnetka",
                //"short_name" : "Winnetka",
                var neighborhood = getAddrComponentsByType(result.address_components, "neighborhood");
                //"long_name" : "1600",
                //"short_name" : "1600",
                var street_number = getAddrComponentsByType(result.address_components, "street_number");
                //"long_name" : "Amphitheatre Pkwy",
                //"short_name" : "Amphitheatre Pkwy",
                var route = getAddrComponentsByType(result.address_components, "route");
                //"long_name" : "Mountain View",
                //"short_name" : "Mountain View",
                var locality = getAddrComponentsByType(result.address_components, "locality");

                var administrative_area_level_5 = getAddrComponentsByType(result.address_components, "administrative_area_level_5");
                var administrative_area_level_4 = getAddrComponentsByType(result.address_components, "administrative_area_level_4");
                var administrative_area_level_3 = getAddrComponentsByType(result.address_components, "administrative_area_level_3");
                //"long_name" : "Santa Clara County",
                //"short_name" : "Santa Clara County",
                var administrative_area_level_2 = getAddrComponentsByType(result.address_components, "administrative_area_level_2");
                //"long_name" : "California",
                //"short_name" : "CA",
                var administrative_area_level_1 = getAddrComponentsByType(result.address_components, "administrative_area_level_1");
                //"long_name" : "United States",
                //"short_name" : "US",
                var country = getAddrComponentsByType(result.address_components, "country");
                //"long_name" : "94043",
                //"short_name" : "94043",
                var postal = getAddrComponentsByType(result.address_components, "postal_code");

                var description = "";

                string address = "", city = "", formattedAddress = "";
                float lat, lng;

                if (route != null) {
                    address = route.long_name;
                    if (street_number != null) {
                        address = street_number.long_name + " " + address;
                    }
                } else if (neighborhood != null) {
                    address = neighborhood.long_name;
                }
                if (administrative_area_level_1 != null &&
                    locality != null) {
                    city = locality.long_name + ", " + administrative_area_level_1.short_name;
                    if (postal != null) {
                        city += " " + postal.long_name;
                    }
                }
                formattedAddress = result.formatted_address;
                lat = result.geometry.location.lat;
                lng = result.geometry.location.lng;

                return new QueryLoadDto() {
                    City = city,
                    Address = address,
                    Location = new LocationDto() {
                        FormattedAddress = formattedAddress,
                        Latitude = lat,
                        Longitude = lng
                    }
                };
            } catch (Exception e) {
                return null;
            }
        }
    }
}
