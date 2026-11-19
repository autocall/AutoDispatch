using AutoCall.Common;
using AutoCall.Common.Attributes;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoDispatch.Logic.Attributes {
    public enum ValidationEnum {
        Normal = 0,
        Warning = 1,
        Critical = 2,
    }

    public abstract class CustomValidationAttribute : Attribute {
        public abstract string ErrorMessage { get; }
        public abstract ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName);

        protected bool CheckCanceled(Wrapper1LoadViewModel wrapper) {
            return wrapper == null || wrapper.Load == null ||
                wrapper.Load.Status == LoadStatus.Canceled1 || wrapper.Load.Status == LoadStatus.Canceled2 ||
                (wrapper.Load.Status == LoadStatus.None &&
                (wrapper.Load.BoardType == DispatchBoardType.CarrierAccount || wrapper.Load.BoardType == DispatchBoardType.BrokerAccount));
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotNullValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "Required";

        public bool OnlyAssigned { get; set; }
        public bool NullParent { get; set; }
        public bool OnlyAccounting { get; set; }
        public bool OnlyDispatch { get; set; }
        public bool NotSplit { get; set; }

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            if (NotSplit == true && (wrapper.Load.ContainerType == ContainerType.Quantity || wrapper.Load.ContainerType == ContainerType.Distance)) return ValidationEnum.Normal;
            if (OnlyAssigned == true && wrapper.Load.Assigned == false) return ValidationEnum.Normal;
            if (NullParent == true && wrapper.Load.ParentId != null) return ValidationEnum.Normal;
            if (OnlyAccounting == true &&
                wrapper.Load.BoardType != DispatchBoardType.CarrierAccount &&
                wrapper.Load.BoardType != DispatchBoardType.BrokerAccount) return ValidationEnum.Normal;
            if (OnlyDispatch == true &&
                wrapper.Load.BoardType != DispatchBoardType.Carrier &&
                wrapper.Load.BoardType != DispatchBoardType.Broker) return ValidationEnum.Normal;
            return ReflectionHelper.GetPropValue(wrapper, propName) != null ? ValidationEnum.Normal : ValidationEnum.Critical;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SumValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "Wrong amount";

        public bool OnlyDispatch { get; set; }

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            if (OnlyDispatch == true &&
               wrapper.Load.BoardType != DispatchBoardType.Carrier &&
               wrapper.Load.BoardType != DispatchBoardType.Broker) return ValidationEnum.Normal;
            if (wrapper.Load.Parent1 != null) {
                return this.Validate(wrapper.Load.Parent1, propName);
            }
            if (wrapper.Load.Parent2 != null) {
                return this.Validate(wrapper.Load.Parent2, propName);
            }
            var rootCount = (dynamic)ReflectionHelper.GetPropValue(wrapper, propName);
            if (rootCount == null) return ValidationEnum.Normal;
            //  || wrapper.Load.ContainerType == ContainerType.Shipment для Rate не подходит
            if (wrapper.Load.ContainerType == ContainerType.Quantity) {
                foreach (var child in wrapper.Children) {
                    rootCount = rootCount - (dynamic)ReflectionHelper.GetPropValue(child, propName);
                }
                return rootCount == 0 ? ValidationEnum.Normal : ValidationEnum.Critical;
            }
            if (wrapper.Load.ContainerType == ContainerType.Distance) {
                return wrapper.Children.All(w => rootCount == (dynamic)ReflectionHelper.GetPropValue(w, propName)) ? ValidationEnum.Normal : ValidationEnum.Critical;
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DatRateValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "Rate is lower than DAT Rate";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (load.LineHaul != null && load.DatRate != null) {
                if (load.BoardType == DispatchBoardType.Broker && load.LineHaul > load.DatRate) {
                    return ValidationEnum.Critical;
                }
                if (load.BoardType == DispatchBoardType.Carrier && load.LineHaul < load.DatRate) {
                    return ValidationEnum.Critical;
                }
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PickupValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "late";
        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (DisplayAttribute.GetOrder(load.Status) < DisplayAttribute.GetOrder(LoadStatus.PickedUp)) {
                if ((load.PickupDate.Date + load.PickupTo).AddSeconds(-load.CurrentOriginUtcOffset) < DateTime.UtcNow) {
                    return ValidationEnum.Critical;
                } else {
                    if (load.Status == LoadStatus.Waiting1) {
                        // прошло 2 часа и сейчас статус сменился waiting
                        return ValidationEnum.Warning;
                    }
                }
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DeliveryValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "late";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (DisplayAttribute.GetOrder(load.Status) < DisplayAttribute.GetOrder(LoadStatus.Delivered)) {
                if ((load.DeliveryDate.Date + load.DeliveryTo).AddSeconds(-load.CurrentDestinationUtcOffset) < DateTime.UtcNow) {
                    // просрочен
                    return ValidationEnum.Critical;
                } else {
                    if (load.Status == LoadStatus.Waiting2) {
                        // прошло 2 часа и сейчас статус сменился waiting
                        return ValidationEnum.Warning;
                    }
                }
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CompareAnyValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "CompareAnyValidation";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            return wrapper.Children.Any() == false ||
                   wrapper.Children.Any(w => Object.Equals(ReflectionHelper.GetPropValue(w, propName), ReflectionHelper.GetPropValue(wrapper, propName))) ?
            ValidationEnum.Normal : ValidationEnum.Critical;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "Required";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (load.ParentId != null) {
                return ValidationEnum.Normal;
            } else if (load.BoardType == DispatchBoardType.BrokerAccount) {
                if (load.CheckDate != null) {
                    return ValidationEnum.Normal;
                } else
                if (load.Age.Days >= 31) {
                    return ValidationEnum.Critical;
                } else
                if (load.Age.Days >= 14) {
                    return ValidationEnum.Warning;
                }
            } else if (load.BoardType == DispatchBoardType.CarrierAccount) {
                if (load.CheckDate != null) {
                    return ValidationEnum.Normal;
                } else {
                    if (load.EmailInvoiceSentDate != null) {
                        if (load.Age.Days >= 23) {
                            return ValidationEnum.Critical;
                        } else {
                            return ValidationEnum.Warning;
                        }
                    }
                }
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InvoiceValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "Required";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (wrapper == null) return ValidationEnum.Normal;
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (load.ParentId != null) {
                return ValidationEnum.Normal;
            } else if (load.InvoiceDate != null) {
                return ValidationEnum.Normal;
            } else if (load.Age.Days >= 31) {
                return ValidationEnum.Critical;
            } else if (load.Age.Days >= 14) {
                return ValidationEnum.Warning;
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class QuikPayValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "QuikPayValidation";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            if (load.QuikPay == null && load.Status != LoadStatus.Completed) {
                return ValidationEnum.Normal;
            } else if (DateTime.Now - load.QuikPay >= TimeSpan.FromDays(2)) {
                return ValidationEnum.Critical;
            } else if (DateTime.Now - load.QuikPay >= TimeSpan.FromDays(1)) {
                return ValidationEnum.Warning;
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ETA_DerampValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => "ETA_DerampValidation";

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            var load = wrapper.Load;
            var trailer = wrapper.Trailer;
            if (trailer != null && String.IsNullOrEmpty(trailer.BnsfETA_Deramp) == false) {
                // "07-05 02:31 Fri"
                var arr = trailer.BnsfETA_Deramp.Split(new[] { ' ', '-', ':' });
                var delivery = load.DeliveryDate;
                // проверка если delivery в декабре а доставка в январе, то янватю пристоить следующий год. и наоборот
                if (Int32.TryParse(arr[0], out var etaMonth) &&
                    Int32.TryParse(arr[1], out var etaday) &&
                    Int32.TryParse(arr[2], out var etaHour) &&
                    Int32.TryParse(arr[3], out var etaMunite)) {
                    var etaYear = delivery.Year;
                    var etaDt = new DateTime(etaYear, etaMonth, etaday, etaHour, etaMunite, 0);
                    if (etaDt.DayOfWeek.ToString().StartsWith(arr[4] /*week*/)) {
                        // год совпал с week
                    } else
                    if (etaDt.AddYears(1).DayOfWeek.ToString().StartsWith(arr[4] /*week*/)) {
                        etaDt = etaDt.AddYears(1);
                    } else
                    if (etaDt.AddYears(-1).DayOfWeek.ToString().StartsWith(arr[4] /*week*/)) {
                        etaDt = etaDt.AddYears(-1);
                    } else {
                        _webl.e("Undefined Week of Year: " + trailer.BnsfETA_Deramp);
                        return ValidationEnum.Normal;
                    }

                    if (delivery < etaDt) {
                        return ValidationEnum.Critical;
                    }
                } else {
                    _webl.e("Undefined Week of ETA Deramp");
                }
            }
            return ValidationEnum.Normal;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LateValidationAttribute : CustomValidationAttribute {
        public override string ErrorMessage => _errorMessage;

        private string _errorMessage;

        public override ValidationEnum Validate(Wrapper1LoadViewModel wrapper, string propName) {
            if (base.CheckCanceled(wrapper)) return ValidationEnum.Normal;
            _errorMessage = $"Delivery: {wrapper.Load.DeliveryDate:d}, Delivered: {(wrapper.Load.DeliveredDate != null ? wrapper.Load.DeliveredDate.Value.ToString("d") : null)}";
            if (wrapper.Load.Late) {
                return ValidationEnum.Critical;
            } else {
                return ValidationEnum.Normal;
            }
        }
    }
}
