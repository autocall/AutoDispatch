using System;
using System.Globalization;
using System.Windows.Data;
using AutoDispatch.Dtos.Enums;

namespace AutoCall.WPF.Converters.Dispatch {
    // перестал работать на Warehouse роле. выдаёт название по DataMemberBinding
    public class DispatchBoard_BoardToCommentNameConverter : IValueConverter {
        public enum CommentType {
            Comment1,
            Comment2,
            Consolidation
        }

        public CommentType Type { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch (Type) {
                case CommentType.Comment1:
                    switch ((DispatchBoardType)value) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Comments for Dispatcher";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "External Comments";
                        default:
                            throw new Exception();
                    }
                case CommentType.Comment2:
                    switch ((DispatchBoardType)value) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Comments for Broker";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "Internal Comments";
                        default:
                            throw new Exception();
                    }
                case CommentType.Consolidation:
                    switch ((DispatchBoardType)value) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Consolidation";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "Consolidation";
                        default:
                            throw new Exception();
                    }
                default:
                    throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
