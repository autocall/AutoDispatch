using System;
using System.Collections.Generic;
using System.Linq;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Helpers {
    public static class DispatchHelper {

        /// <summary>
        ///     Текущий wrapper может не иметь трейлер, но все дети имеют. Тогда возвращаем первый 
        ///     Используется, если кликаешь на Container, дети которго на одном трейлере </summary>
        public static Wrapper1LoadViewModel GetTrailerWrapper(Wrapper1LoadViewModel wrapper) {
            if (wrapper == null) return null;
            if (wrapper.Trailer != null) return wrapper;
            if (wrapper.Children == null) return wrapper;
            var children = wrapper.Children.Where(e => e.Trailer != null);
            if (children.Select(e => e.Trailer).Distinct().Count() == 1) {
                return children.First();
            }
            return wrapper;
        }
        /// <summary>
        ///     похож на Wrapper1LoadViewModel.DatCompanyView </summary>
        public static Wrapper1LoadViewModel GetDatCompanyWrapper(Wrapper1LoadViewModel wrapper) {
            if (wrapper.DatCompany != null) return wrapper;
            var children = wrapper.Children.Where(e => e.DatCompany != null);
            if (children.Select(e => e.DatCompany).Distinct().Count() == 1) {
                return children.First();
            }
            return wrapper;
        }

        public static Guid? GetAssignedId(Wrapper1LoadViewModel wrapper) {
            return (wrapper.Driver ??
                wrapper.Truck ??
                wrapper.Trailer ??
                wrapper.Warehouse ??
                wrapper.Yard ??
                wrapper.Shipment ??
                (BaseObjectViewModel)wrapper.DatCompany ??
                wrapper.DatDriver)?.EntityId;
        }

        public static DivideType GetDivideType(LoadViewModel load) {
            if (load.Pallets != null) {
                return DivideType.Pallets;
            } else if (load.Length != null) {
                return DivideType.Length;
            } else {
                return DivideType.None;
            }
        }

        public static int? GetValueByDivideType(DivideType divideType, LoadViewModel load) {
            switch (divideType) {
                case DivideType.Pallets:
                    return load.Pallets;
                case DivideType.Length:
                    return load.Length;
                case DivideType.Weight:
                    return load.Weight;
                default:
                    throw new Exception("Split Type must be setted");
            }
        }

        public static void FindAndRemovePalletFromTrailer(TrailerViewModel trailer, LoadPalletViewModel pallet) {
            foreach (TrailerCellViewModel cell in trailer.Layout) {
                var p = cell.Pallets.FirstOrDefault(e => e.Index == pallet.Index && e.Level == pallet.Level);
                if (p != null) {
                    cell.Pallets.Remove(p);
                    RecalculateLevels(cell);
                    break;
                }
            }
        }

        public static void RecalculateLevels(TrailerCellViewModel cell) {
            for (byte i = 0; i < cell.Pallets.Count; i++) {
                cell.Pallets[i].Level = i;
            }
        }

        public static int LengthToPallets(int length) {
            return length * 30 / 53;
        }

        public static void CopyContainerProperties(LoadViewModel source, LoadViewModel destination) {
            destination.DivideType = source.DivideType;
            destination.Foreground = source.Foreground;
            destination.Background = source.Background;
            destination.Equipment = source.Equipment;
            destination.Temperature = source.Temperature;
            destination.BookedDate = source.BookedDate;
            destination.RecordId = source.RecordId;
            destination.BOLNumber = source.BOLNumber;
            destination.BOLNumber = source.BOLNumber;
            destination.OldOrderId = source.OldOrderId;
            destination.LoadType = LoadType.Full;
            destination.Company = source.Company;
            destination.McNumber = source.McNumber;
            destination.Contacts = source.Contacts;
        }

        private static float _ratio(LoadViewModel numerator, LoadViewModel denominator) {
            if (numerator.Pallets != null && denominator.Pallets != null && denominator.Pallets != 0) {
                return (float)denominator.Pallets / (float)numerator.Pallets;
            }
            if (numerator.Length != null && denominator.Length != null && denominator.Length != 0) {
                return (float)denominator.Length / (float)numerator.Length;
            }
            if (numerator.Weight != null && denominator.Weight != null && denominator.Weight != 0) {
                return (float)denominator.Weight / (float)numerator.Weight;
            }
            return 1f;
        }

        public static List<ExpenseViewModel> Ratio(LoadViewModel numerator, LoadViewModel denominator, ICollection<ExpenseViewModel> expenses) {
            var ratio = _ratio(numerator, denominator);

            var result = new List<ExpenseViewModel>();
            foreach (ExpenseViewModel expense in expenses) {
                var clone = (ExpenseViewModel)expense.Clone();
                if (clone.Cost != null) {
                    clone.Cost = (decimal)Math.Round((double)clone.Cost * ratio);
                }
                result.Add(clone);
            }
            return result;
        }

        public static List<GainViewModel> Ratio(LoadViewModel numerator, LoadViewModel denominator, ICollection<GainViewModel> gains) {
            var ratio = _ratio(numerator, denominator);

            var result = new List<GainViewModel>();
            foreach (GainViewModel gain in gains) {
                var clone = (GainViewModel)gain.Clone();
                if (clone.Cost != null) {
                    clone.Cost = (decimal)Math.Round((double)clone.Cost * ratio);
                }
                result.Add(clone);
            }
            return result;
        }

        public static void SetRatio(LoadViewModel root, IEnumerable<LoadViewModel> children) {
            if (children.Any()) {
                foreach (var child in children) {
                    var ratio = _ratio(root, child);
                    if (ratio != 1f) {
                        if (root.LineHaul != null) {
                            child.LineHaul = (decimal)((float)root.LineHaul * ratio);
                        }
                        if (root.Pallets != null) {
                            child.Pallets = (short)(root.Pallets * ratio);
                        }
                        if (root.Length != null) {
                            child.Length = (int)(root.Length * ratio);
                        }
                        if (root.Weight != null) {
                            child.Weight = (int)(root.Weight * ratio);
                        }
                    }
                }
            }
        }

        public static string GetMessage(StopType stopType, LoadViewModel load, ActualCompanyViewModel company) {
            if (company == null) return "empty";
            switch (company.BoardType) {
                case DispatchBoardType.Broker:
                case DispatchBoardType.BrokerAccount:
                    switch (stopType) {
                        case StopType.Pickup:
                            return $@"NO-REPLY!
Make sure the Seal and Trailer Numbers match the Bill Of Lading!!!
Check In As Driver From {company.LegalName}
If they give you the seal don't put it on if they seal you don't brake the seal
Ask For The Load To: {load.DestinationCity}
Pickup: {load.Pickup}
{load.ShipperCompany.ToUpper()}
{load.OriginAddress.ToUpper()}
{load.OriginCity.ToUpper()}
PU # {load.PickupNumber}
".Trim();
                        case StopType.Delivery:
                            return $@"NO-REPLY!
Make sure the Seal and Trailer Numbers match the Bill Of Lading!!!
Check In As Driver From {company.LegalName}
If they give you the seal don't put it on if they seal you don't brake the seal
Origin: {load.OriginCity}
Delivery: {load.Delivery}
{load.ReceiverCompany.ToUpper()}
{load.DestinationAddress.ToUpper()}
{load.DestinationCity.ToUpper()}
Del # {load.DeliveryNumber}
".Trim();
                        default:
                            throw new Exception();
                    }
                case DispatchBoardType.Carrier:
                case DispatchBoardType.CarrierAccount:
                    switch (stopType) {
                        case StopType.Pickup:
                            return $@"NO-REPLY!
Make sure the Seal and Trailer Numbers match the Bill Of Lading!!!
Check In As Driver From {company.LegalName}
Ask For The Load To: {load.DestinationCity}
Pickup: {load.Pickup}
{load.ShipperCompany.ToUpper()}
{load.OriginAddress.ToUpper()}
{load.OriginCity.ToUpper()}
PU # {load.PickupNumber}
".Trim();
                        case StopType.Delivery:
                            return $@"NO-REPLY!

Check In As Driver From {company.LegalName}
Origin: {load.OriginCity}
Delivery: {load.Delivery}
{load.ReceiverCompany.ToUpper()}
{load.DestinationAddress.ToUpper()}
{load.DestinationCity.ToUpper()}
Del # {load.DeliveryNumber}
".Trim();
                        default:
                            throw new Exception();
                    }
                default:
                    throw new Exception();
            }
        }
    }
}
