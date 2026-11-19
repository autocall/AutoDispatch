using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Common.Collections;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Helpers;
using AutoCall.Common.Loggers;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.ViewModels.Items;
using AutoMapper;

namespace AutoDispatch.Logic.ViewModels.Pages {
    /// <summary>
    ///     Separates Selection Items by Boards </summary>
    public class DispatchBoardSelectedItemsViewModel {
        public ObservableCollection<object> SelectedPatterns { get; private set; } = new ObservableCollection<object>();
        public ObservableCollection<object> SelectedLoads { get; private set; } = new ObservableCollection<object>();


        /*
         RestoreSelection будет работать только если был вызван SaveSelection. Иначе нуллы и пропускаем RestoreSelection
         */
        private List<Guid> _savedSelectedPatternIds = null;
        private List<Guid> _savedSelectedLoadIds = null;
        private void SaveSelection() {
            _savedSelectedPatternIds = new List<Guid>();
            _savedSelectedLoadIds = new List<Guid>();
            var savedSelectedPatternIds = this.SelectedPatterns.Cast<Wrapper1LoadViewModel>().Select(e => e.Load.EntityId).ToList();
            var savedSelectedLoadIds = this.SelectedLoads.Cast<Wrapper1LoadViewModel>().Select(e => e.Load.EntityId).ToList();
            this._savedSelectedPatternIds.AddRange(savedSelectedPatternIds);
            this._savedSelectedLoadIds.AddRange(savedSelectedLoadIds);
        }

        private void RestoreSelection(IList<Wrapper1LoadViewModel> wrappers) {
            if (_savedSelectedPatternIds != null && _savedSelectedLoadIds != null) {
                var savedSelectedPatternIds = this.SelectedPatterns.Cast<Wrapper1LoadViewModel>().Select(e => e.Load.EntityId).ToList();
                var savedSelectedLoadIds = this.SelectedLoads.Cast<Wrapper1LoadViewModel>().Select(e => e.Load.EntityId).ToList();
                if (Enumerable.SequenceEqual(this._savedSelectedPatternIds, savedSelectedPatternIds) == false) {
                    var w = wrappers.Where(e => this._savedSelectedPatternIds.Contains(e.Load.EntityId)).ToList();
                    this.SelectedPatterns.Clear();
                    this.SelectedPatterns.AddRange(w);
                }
                if (Enumerable.SequenceEqual(this._savedSelectedLoadIds, savedSelectedLoadIds) == false) {
                    var w = wrappers.Where(e => this._savedSelectedLoadIds.Contains(e.Load.EntityId)).ToList();
                    this.SelectedLoads.Clear();
                    this.SelectedLoads.AddRange(w);
                }
                _savedSelectedPatternIds = null;
                _savedSelectedLoadIds = null;
            }
        }

        public void Clear() {
            this.SelectedPatterns.Clear();
            this.SelectedLoads.Clear();
        }
    }

    public partial class DispatchBoardViewModel : ViewModel {

        #region [ Singleton ]

        public static DispatchBoardViewModel Instance { get; private set; }

        #endregion [ Singleton ]

        #region [ _fields ]

        private EngineState _engineState = EngineState.Unknown;
        private bool _loading = true;

        private List<DriverViewModel> _drivers;
        private List<TruckViewModel> _trucks;
        private List<TrailerViewModel> _trailers;
        private List<WarehouseViewModel> _warehouses;
        private List<YardViewModel> _yards;
        private List<LoadViewModel> _loads;
        private List<DatCompanyViewModel> _datCompanies;
        private List<SetupPacketViewModel> _setupPackets;
        private List<DatDriverViewModel> _datDrivers;
        private List<ActualCompanyViewModel> _actualCompanies;
        private Dictionary<DateTime, int> _carrierHistoryLoads;
        private Dictionary<DateTime, int> _brokerHistoryLoads;
        private Dictionary<Guid, string> _customStatuses;
        private ObservableRangeCollection<Wrapper1LoadViewModel> _wrappers;

        #endregion [ _fields ]

        #region [ Items ]

        public List<DriverViewModel> Drivers => _drivers;
        public List<TruckViewModel> Trucks => _trucks;
        public List<TrailerViewModel> Trailers => _trailers;
        public List<WarehouseViewModel> Warehouses => _warehouses;
        public List<YardViewModel> Yards => _yards;
        public List<LoadViewModel> Loads => _loads;
        public List<DatCompanyViewModel> DatCompanies => _datCompanies;
        public List<SetupPacketViewModel> SetupPackets => _setupPackets;
        public List<DatDriverViewModel> DatDrivers => _datDrivers;
        public List<ActualCompanyViewModel> ActualCompanies => _actualCompanies;
        public Dictionary<DateTime, int> CarrierHistoryLoads => _carrierHistoryLoads;
        public Dictionary<DateTime, int> BrokerHistoryLoads => _brokerHistoryLoads;
        public Dictionary<Guid, string> CustomStatuses => _customStatuses;
        /// <summary>
        ///     Common View Model for grids </summary>
        public ObservableRangeCollection<Wrapper1LoadViewModel> Wrappers => _wrappers;
        public List<Wrapper1LoadViewModel> tmpWrappers;

        public Dictionary<DispatchFilterTableType, FiltersColumnsViewModel> FiltersColumns { get; set; } = new Dictionary<DispatchFilterTableType, FiltersColumnsViewModel>();

        public void SetFiltersColumns(DispatchFilterTableType filterType, FiltersColumnsViewModel fitlersColumnsViewModel) {
            FiltersColumns[filterType] = fitlersColumnsViewModel;
            OnPropertyChanged(nameof(FiltersColumns));
        }

        #endregion [ Items ]

        #region [ settings ]

        private GridsSettingsViewModel _gridsSettings = new GridsSettingsViewModel();

        public GridsSettingsViewModel GridsSettings => _gridsSettings;

        #endregion [ settings ]

        #region [ .ctor ]

        public DispatchBoardViewModel() {
            //SelectedLoads.CollectionChanged += (sender, e) => { Debug.WriteLine($"{DateTime.Now:HH:mm:ss} SelectedLoads: {SelectedLoads.Count}"); };

            _drivers = new List<DriverViewModel>();
            _trucks = new List<TruckViewModel>();
            _trailers = new List<TrailerViewModel>();
            _warehouses = new List<WarehouseViewModel>();
            _yards = new List<YardViewModel>();
            _loads = new List<LoadViewModel>();
            _setupPackets = new List<SetupPacketViewModel>();
            _datCompanies = new List<DatCompanyViewModel>();
            _datDrivers = new List<DatDriverViewModel>();
            _carrierHistoryLoads = new Dictionary<DateTime, int>();
            _brokerHistoryLoads = new Dictionary<DateTime, int>();
            _customStatuses = new Dictionary<Guid, string>();
            _wrappers = new ObservableRangeCollection<Wrapper1LoadViewModel>();
            _actualCompanies = new List<ActualCompanyViewModel>();

            if (Instance == null) {
                Instance = this;
            }
        }

        #endregion [ .ctor ]

        #region [ Properties ]

        public bool Loading {
            get { return _loading; }
            set {
                if (_loading != value) {
                    _loading = value;
                    OnPropertyChanged();
                }
            }
        }

        public event Action Loaded;

        public void LoadedInvoke() {
            Loaded?.Invoke();
        }

        ///// <summary>
        /////     TODO: Only uses for WPF.UI </summary>
        //public object WorkerState {
        //    get { return _workerState; }
        //    set {
        //        _workerState = value;
        //        OnPropertyChanged();
        //    }
        //}

        public EngineState EngineState {
            get { return _engineState; }
            set {
                if (_engineState != value) {
                    _engineState = value;
                    OnPropertyChanged();
                }
            }
        }

        public object Trigger { get; set; }
        public void UpdateTrigger() {
            OnPropertyChanged(nameof(Trigger));
        }

        public DateTime? MinDriverExpiration => Drivers?.Select(e => new List<DateTime?>{
            e.MVR_Expiration,
            e.MEDICAL_CARD_Expiration,
            e.CDL_Expiration,
            }.Min()).Min();

        public void UpdateMinDriverExpiration() {
            OnPropertyChanged(nameof(MinDriverExpiration));
        }

        public DateTime? MinEquipmentExpiration {
            get {
                DateTime truckExp = Trucks.Select(e => e.AnnualInspection_Expiration).Min() ?? DateTime.MaxValue;
                DateTime trailerExp1 = Trailers.Select(e => e.AnnualInspection_Expiration).Min() ?? DateTime.MaxValue;
                DateTime trailerExp2 = Trailers.Select(e => e.Maintenance_Expiration).Min() ?? DateTime.MaxValue;
                return new List<DateTime>() { truckExp, trailerExp1, trailerExp2 }.Min();
            }
        }

        public void UpdateMinEquipmentExpiration() {
            OnPropertyChanged(nameof(MinEquipmentExpiration));
        }

        /// <summary>
        ///     Gets or sets board settings </summary>
        /// TODO use Dispatch type as settings
        public /*BoardSettingsViewModel*/ object Settings { get; set; }

        #endregion [ Properties ]

        #region [ Public ]

        public void Clear() {
            SetupPackets.Clear();
            Warehouses.Clear();
            Yards.Clear();
            Trailers.Clear();
            Trucks.Clear();
            Drivers.Clear();
            DatDrivers.Clear();
            DatCompanies.Clear();
            Loads.Clear();
            //ActualCompanies.Clear(); // loading in MainWindow
            Wrappers.Clear();

            this.BrokerHistoryLoads.Clear();
            this.CarrierHistoryLoads.Clear();
            this.CustomStatuses.Clear();
            this.FiltersColumns.Clear();

            OnPropertyChanged(nameof(this.BrokerHistoryLoads));
            OnPropertyChanged(nameof(this.CarrierHistoryLoads));
            OnPropertyChanged(nameof(this.CustomStatuses));
            OnPropertyChanged(nameof(this.FiltersColumns));
        }
        /// <param name="convert">LoadConverter.ToVm (V1 — IMapper: статический конвертер лежит в AutoDispatch.Logic, Contracts на него не ссылается)</param>
        public async Task<bool> SetLoadsAsync(Func<LoadDto, LoadViewModel> convert, IList<byte[]> loadsBinary, bool history = false, bool refreshWrappers = true, bool isExpanded = false) {
            var result = false;
            Stopwatch sw0 = Stopwatch.StartNew();
            foreach (var loadsZip in loadsBinary) {
                var dtos = ZipExtension.DeserializeObject<List<LoadDto>>(loadsZip);
                var viewModels = dtos?.Select(convert).ToList() ?? new List<LoadViewModel>();
                var resultTmp = await this.SetLoadsAsync(viewModels, history, refreshWrappers: false, isExpanded: isExpanded);
                result = result || resultTmp;
            }
            Debug.WriteLine($" --> SetLoads(b): {sw0.Elapsed.TotalSeconds:F2}sec");
            if (refreshWrappers) {
                await RefreshWrappersAsync();
            }
            Debug.WriteLine($" --> RefreshWrappers: {sw0.Elapsed.TotalSeconds:F2}sec");
            return result;
        }

        /// <summary>
        ///     Adds or updates booked loads </summary>
        /// <returns>Total of loads was changed</returns>
        /// <param name="isExpanded">To expand a new group</param>
        public async Task<bool> SetLoadsAsync(IList<LoadViewModel> loads, bool history = false, bool refreshWrappers = true, bool isExpanded = false) {
            if (loads == null) return false;
            //Debug.WriteLine($"SetLoads({loads.Count}) -->");
            Stopwatch sw0 = Stopwatch.StartNew();
            var result = false;
            // NOTES: теперь не надо
            //SaveSelection();
            // вначале контейнеры. иначе приходит Child и мелькает в GridRecords
            foreach (var load in loads.OrderByDescending(e => e.ContainerType)) {
                var existsLoad = Loads.FirstOrDefault(l => l.EntityId == load.EntityId);
                if (existsLoad != null) {
                    if (history ||
                        (load.IsDeleted == false &&
                         load.Status != LoadStatus.Unpaid && load.Status != LoadStatus.Completed && load.Status != LoadStatus.Canceled2) ||
                        (existsLoad.Status == LoadStatus.Delivered ||
                         load.Status == LoadStatus.Unpaid || load.Status == LoadStatus.Completed || load.Status == LoadStatus.Canceled2)) {
                        existsLoad.Assign(load);
                    } else {
                        // Delete
                        existsLoad.Assign(load); // notes: sets deleting properties
                        Loads.Remove(existsLoad);
                        result = true;
                    }
                    //var tmpWrappers = await DeleteRelativeWrappersAsync(existsLoad.EntityId);
                    //SaveIsExpanded(tmpWrappers);
                } else if (history || (load.IsDeleted == false &&
                    load.Status != LoadStatus.Unpaid && load.Status != LoadStatus.Completed && load.Status != LoadStatus.Canceled2)) {
                    if (load.ParentId != null && load.BoardType == DispatchBoardType.Carrier) {
                        // IsExpanded для подгруза, в RefreshWrappers перенесем IsExpanded на контейнер
                        load.IsExpanded = isExpanded;
                    }
                    Loads.Add(load);
                    result = true;
                }
            }
            this.UpdateTrigger();
            RefreshCompanies(loads);
            Debug.WriteLine($"SetLoads({loads.Count}) {sw0.Elapsed.TotalSeconds:F2}sec");
            if (refreshWrappers) {
                await RefreshWrappersAsync();// slow: 6 items -> 7.5sec
                Debug.WriteLine($" --> RefreshWrappers: {sw0.Elapsed.TotalSeconds:F2}sec");
            }
            // NOTES: теперь не надо
            //RestoreSelection();
            RestoreIsExpanded();
            return result;
        }

        #region [ Save/Restore IsExpanded Wrapper ]

        private List<Guid> IsExpandedLoadIds = new List<Guid>();
        /// <summary>
        ///     Adds to List of expanded wrappers </summary>
        private void SaveIsExpanded(IEnumerable<Wrapper1LoadViewModel> wrappers) {
            foreach (var wrapper in wrappers) {
                if (wrapper.IsExpanded) {
                    IsExpandedLoadIds.Add(wrapper.Load.EntityId);
                }
            }
        }

        private void RestoreIsExpanded() {
            foreach (var wrapper in Wrappers.Where(w => IsExpandedLoadIds.Contains(w.Load.EntityId))) {
                wrapper.IsExpanded = true;
            }
            IsExpandedLoadIds.Clear();
        }

        #endregion [ Save/Restore IsExpanded Wrapper ]

        // TODO: отредактировал сомпанию в грузе. обнови эту компанию в других грузах
        // TODO: убрать Contacts, McNumber, Name из груза, Customers поместить в DatCompanies, добаивть новый тип компании CustomerType
        // TODO: вытягивать инфу из DatCompanies
        /// <summary>
        ///     Refreshes Customers and DatBrokers </summary>
        /// <param name="loads"></param>
        private void RefreshCompanies(IList<LoadViewModel> loads) {
            foreach (var newLoad in loads) {
                foreach (var existLoad in Loads) {
                    if (newLoad.EntityId != existLoad.EntityId) {
                        if (newLoad.DatBrokerId != null && newLoad.DatBrokerId == existLoad.DatBrokerId) {
                            existLoad.AssignCompany(newLoad);
                        }
                        if (newLoad.CustomerId != null && newLoad.CustomerId == existLoad.CustomerId) {
                            existLoad.AssignCompany(newLoad);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Refreshes DatBrokers </summary>
        /// <param name="loads"></param>
        private void RefreshCompanies(IList<DatCompanyViewModel> companies) {
            foreach (var company in companies) {
                foreach (var existLoad in Loads) {
                    if (existLoad.DatBrokerId != null && company.EntityId == existLoad.DatBrokerId) {
                        existLoad.AssignCompany(company);
                    }
                }
            }
        }

        /// <summary>
        ///     удаляю, чтобы потом их пересоздать. есть логика, которая отображает контейнер. CreatedDate 9/30/2022 
        ///     проблема в логике контейнеров. заасайнай все подгрузы и root будет где то не там    
        /// </summary>
        /// <returns>
        ///     возвращает только Parents. в дальнейшем эти врапперы использются для сохранения IsExpanded
        /// </returns>
        private async Task<List<Wrapper1LoadViewModel>> DeleteRelativeWrappersAsync(Guid loadId) {
            Debug.WriteLine("DeleteRelativeWrappers");
            var result = new List<Wrapper1LoadViewModel>();
            var wrapper = Wrappers.FirstOrDefault(w => w.Load.EntityId == loadId);
            if (wrapper != null) {
                using (await LockHelper.EnterAsync()) {
                    //Wrappers.Remove(wrapper); убрал мерцание одиночных грузов
                    result.Add(wrapper);
                    if (wrapper.Children != null && wrapper.Children.Any()) {
                        Wrappers.RemoveRange(wrapper.Children);
                        wrapper.Children.Clear();
                    }
                    if (wrapper.Load.Parent1 != null) {
                        Wrappers.Remove(wrapper.Load.Parent1);
                        Wrappers.RemoveRange(wrapper.Load.Parent1.Children);
                        result.Add(wrapper.Load.Parent1);
                    }
                    if (wrapper.Load.Parent2 != null) {
                        Wrappers.Remove(wrapper.Load.Parent2);
                        Wrappers.RemoveRange(wrapper.Load.Parent2.Children);
                        result.Add(wrapper.Load.Parent2);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Sets actual companies </summary>
        public void SetActualCompanies(List<ActualCompanyViewModel> companies) {
            _actualCompanies.AddRange(companies);
        }

        public async Task RefreshWrappersAsync(EventTypeDto? type = null) {
            using (await LockHelper.EnterAsync()) {
                Debug.WriteLine($"--RefreshWrappers {type}");
                Stopwatch sw0 = Stopwatch.StartNew();
                // Loads: sets, removes
                var removeRangeList = new ConcurrentBag<Wrapper1LoadViewModel>();
                DelayHelper.Restart();
                foreach (var wrapper in Wrappers) {
                    await DelayHelper.DelayAsync();
                    try {
                        var load = Loads.FirstOrDefault(l => wrapper.Load.EntityId == l.EntityId);
                        if (load != null) {
                            // check objects
                            if (load == wrapper.Load) {
                                continue;
                            } else {
                                wrapper.Load = load;
                            }
                        } else {
                            // Exception: Specified argument was out of the range of valid values. Parameter name: count
                            // when change history filter
                            removeRangeList.Add(wrapper);
                        }
                    } catch (Exception e) {
                        _webl.e("RefreshWrappers(1)", e);
                        Debug.WriteLine(e.Message);
                        throw;
                    }
                };
                // сделал так же как AddRange
                if (removeRangeList.Any()) {
                    Stopwatch sw = Stopwatch.StartNew();
                    Wrappers.RemoveRange(removeRangeList);
                    sw.Stop(); Debug.WriteLine($"RemoveRange: {sw.Elapsed.TotalSeconds:F2}sec");
                }
                Debug.WriteLine($"--RefreshWrappers(1) {sw0.Elapsed.TotalSeconds:F2}sec");

                // Loads: sets
                var addRangeList = new ConcurrentBag<Wrapper1LoadViewModel>();
                DelayHelper.Restart();
                foreach (var load in Loads) {
                    await DelayHelper.DelayAsync();
                    try {
                        var wrapper = Wrappers.FirstOrDefault(w => w.Load.EntityId == load.EntityId);
                        if (wrapper != null) {
                            // check objects
                            if (load == wrapper.Load) {
                                continue;
                            } else {
                                wrapper.Load = load;
                            }
                        } else {
                            // NOTES: добавление в ObservableCollection тормозит, ObservableRangeCollection оптимальней
                            // история на 890 грузов
                            // ObservableCollection - 52сек
                            // ObservableRangeCollection - 20сек
                            addRangeList.Add(new Wrapper1LoadViewModel() { Load = load });
                        }
                    } catch (Exception e) {
                        _webl.e("RefreshWrappers(2)", e);
                        Debug.WriteLine(e.Message);
                        throw;
                    }
                };
                Debug.WriteLine($"--RefreshWrappers(2) {sw0.Elapsed.TotalSeconds:F2}sec");
                // #slow 
                // history 2020 ~ 20sec
                // NOTES: сделал оптимизацию внутри ObservableRangeCollection. Reset = Add/Reset
                if (addRangeList.Any()) {
                    Wrappers.AddRange(addRangeList.ToList());
                    Debug.WriteLine($"--RefreshWrappers(3) {sw0.Elapsed.TotalSeconds:F2}sec");
                }

                // --RefreshWrappers(2) 1.04sec
                // --RefreshWrappers(4) 2.40sec
                DelayHelper.Restart();
                foreach (var wrapper in Wrappers.Where(w => w.Children != null)) {
                    // removes children
                    foreach (Wrapper1LoadViewModel child in wrapper.Children.ToList()) {
                        await DelayHelper.DelayAsync();
                        // child.ParentId != parent.EntityId
                        if (!(child.Load.ParentId == wrapper.Load.EntityId) && !(child.Load.ShipmentId == wrapper.Load.EntityId)) {
                            wrapper.Children.Remove(child);
                            if (child.Load.Parent1 == wrapper) child.Load.Parent1 = null;
                            if (child.Load.Parent2 == wrapper) child.Load.Parent2 = null;
                            wrapper.ChildrenOnChanged();
                        }
                        // child doesn't exists in wrappers
                        Wrapper1LoadViewModel origin = Wrappers.FirstOrDefault(w => w.Load.EntityId == child.Load.EntityId);
                        if (origin == null) {
                            wrapper.Children.Remove(child);
                            if (child.Load.Parent1 == wrapper) child.Load.Parent1 = null;
                            if (child.Load.Parent2 == wrapper) child.Load.Parent2 = null;
                            wrapper.ChildrenOnChanged();
                        }
                    }
                };

                Debug.WriteLine($"--RefreshWrappers(3.1) {sw0.Elapsed.TotalSeconds:F2}sec");

                // adds subloads
                // System.InvalidOperationException: 'Collection was modified; enumeration operation may not execute.'
                DelayHelper.Restart();
                foreach (var wrapper in Wrappers.Where(w => w.Load.ParentId != null)) {
                    await DelayHelper.DelayAsync();
                    //Parallel.ForEach(Wrappers.Where(w => w.Load.ParentId != null), (wrapper) => {
                    Wrapper1LoadViewModel parent = Wrappers.FirstOrDefault(w => w.Load.EntityId == wrapper.Load.ParentId);
                    // adds if doesn't exists
                    if (parent != null && (parent.Children == null || parent.Children.Any(w => w == wrapper) == false)) {
                        if (parent.Children == null) {
                            parent.Children = new ObservableCollection<Wrapper1LoadViewModel>();
                        }
                        //var insertIndex = parent.Children.ToList()
                        //    .Union(new List<Wrapper1LoadViewModel>() { wrapper })
                        //    .OrderBy(e => e.Load.IndexExtra).ToList().IndexOf();
                        // TODO: ПРОДЕБАЖ ВСТАВКУ ПО ИНДЕКСУ
                        //var insertIndex = parent.Children
                        //    .Select((w, i) => new { wrapper = w, index = i })
                        //    .FirstOrDefault(e => String.Compare(e.wrapper.Load.IndexExtra, wrapper.Load.IndexExtra) > 0)?.index ?? 0;
                        //parent.Children.Insert(insertIndex, wrapper);

                        var first = parent.Children.FirstOrDefault();
                        if (first != null) {
                            if (String.Compare(first.Load.IndexExtra, wrapper.Load.IndexExtra) > 0) {
                                parent.Children.Insert(0, wrapper);
                            } else {
                                parent.Children.Add(wrapper);
                            }
                        } else {
                            parent.Children.Add(wrapper);
                        }

                        wrapper.Load.Parent1 = parent;
                        // перенесём IsExpanded из Child в Parent 
                        if (wrapper.Load.IsExpanded) {
                            wrapper.Load.IsExpanded = false;
                            parent.Load.IsExpanded = true;
                        }
                        parent.ChildrenOnChanged();
                    }
                }
                //});

                // fast
                // adds shipments
                DelayHelper.Restart();
                foreach (var wrapper in Wrappers.Where(w => w.Load.ShipmentId != null)) {
                    await DelayHelper.DelayAsync();
                    //Parallel.ForEach(Wrappers.Where(w => w.Load.ShipmentId != null), (wrapper) => {
                    // gets parent
                    Wrapper1LoadViewModel parent = Wrappers.FirstOrDefault(w => w.Load.EntityId == wrapper.Load.ShipmentId);
                    // adds if doesn't exists
                    if (parent != null && (parent.Children == null || parent.Children.Any(w => w == wrapper) == false)) {
                        if (parent.Children == null) {
                            parent.Children = new ObservableCollection<Wrapper1LoadViewModel>();
                        }
                        parent.Children.Add(wrapper);
                        wrapper.Load.Parent2 = parent;
                        parent.ChildrenOnChanged();
                    }
                }
                //});

                Debug.WriteLine($"--RefreshWrappers(4) {sw0.Elapsed.TotalSeconds:F2}sec");

                DelayHelper.Restart();
                if (type == null || type == EventTypeDto.BookedLoad)
                    await AssignWrappers(Loads, l => l.ShipmentId, w => w.Shipment, (w, c) => w.Shipment = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.DatCompany)
                    await AssignWrappers(DatCompanies, l => l.DatCompanyId, w => w.DatCompany, (w, c) => w.DatCompany = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.DatDriver)
                    await AssignWrappers(DatDrivers, l => l.DatDriverId, w => w.DatDriver, (w, c) => w.DatDriver = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Driver)
                    await AssignWrappers(Drivers, l => GetDriverId(l), w => w.Driver, (w, c) => w.Driver = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Truck)
                    await AssignWrappers(Trucks, l => GetTruckId(l), w => w.Truck, (w, c) => w.Truck = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Trailer)
                    await AssignWrappers(Trailers, l => l.TrailerId, w => w.Trailer, (w, c) => w.Trailer = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Trailer)
                    await AssignWrappers(Trailers, l => l.HistoryAssignedTrailerId, w => w.HistoryAssignedTrailer, (w, c) => w.HistoryAssignedTrailer = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Yard)
                    await AssignWrappers(Yards, l => GetYardKey(l), w => w.Yard, (w, c) => w.Yard = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Warehouse)
                    await AssignWrappers(Warehouses, l => l.WarehouseId, w => w.Warehouse, (w, c) => w.Warehouse = c);
                if (type == null || type == EventTypeDto.Warehouse)
                    await AssignWrappers(Warehouses, l => l.DestinationWarehouseId, w => w.DestinationWarehouse, (w, c) => w.DestinationWarehouse = c);

                // #slow fixed!
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.SetupPacket)
                    await AssignWrappers(SetupPackets, l => GetSetupPacketKey(l), w => w.SetupPacket, (w, c) => w.SetupPacket = c);

                #region history
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                // Do not assign company if driver exists
                if (type == null || type == EventTypeDto.BookedLoad)
                    await AssignWrappers(Loads, l => l.HistoryShipmentId, w => w.HistoryShipment, (w, c) => w.HistoryShipment = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.DatDriver)
                    await AssignWrappers(DatDrivers, l => l.HistoryDatDriverId, w => w.HistoryDatDriver, (w, c) => w.HistoryDatDriver = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.DatCompany)
                    await AssignWrappers(DatCompanies, l => l.HistoryDatCompanyId, w => w.HistoryDatCompany, (w, c) => w.HistoryDatCompany = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Driver)
                    await AssignWrappers(Drivers, l => l.HistoryDriverId, w => w.HistoryDriver, (w, c) => w.HistoryDriver = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Truck)
                    await AssignWrappers(Trucks, l => l.HistoryTruckId, w => w.HistoryTruck, (w, c) => w.HistoryTruck = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Trailer)
                    await AssignWrappers(Trailers, l => l.HistoryTrailerId, w => w.HistoryTrailer, (w, c) => w.HistoryTrailer = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.Warehouse)
                    await AssignWrappers(Warehouses, l => l.HistoryWarehouseId, w => w.HistoryWarehouse, (w, c) => w.HistoryWarehouse = c);
                #endregion history
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.DatCompany)
                    await AssignWrappers(ActualCompanies, l => l.ActualCompanyId, w => w.ActualCompany, (w, c) => w.ActualCompany = c);
                await DelayHelper.DelayAsync(); DelayHelper.Restart();
                if (type == null || type == EventTypeDto.BookedLoad)
                    AssignCarrierLoad();
                await DelayHelper.DelayAsync();

                // NOTES: если упаковать в таски, через Task.WhenAll, то время не меняется

                // fast
                // TODO: update if was changed
                Parallel.ForEach(Wrappers, (wrapper) => {
                    try {
                        wrapper.IsNewOnChanged();
                        // creates empty trailer
                        FixWrapper(wrapper);
                    } catch (Exception e) {
                        _webl.e("RefreshWrappers(5)", e);
                        Debug.WriteLine(e.Message);
                        throw;
                    }
                });
                Debug.WriteLine($"--RefreshWrappers(5) {sw0.Elapsed.TotalSeconds:F2}sec");
                sw0.Stop();
            }
        }

        private void AssignCarrierLoad() {
            foreach (var wrapper in Wrappers) {
                if (wrapper.Load.ParentId != null) {
                    continue;
                }
                if (wrapper.Load.Status == LoadStatus.Canceled1 || wrapper.Load.Status == LoadStatus.Canceled2) {
                    continue;
                }
                if (String.IsNullOrEmpty(wrapper.Load.RecordIndex)) {
                    continue;
                }
                if ((wrapper.Load.BoardType == DispatchBoardType.Broker ||
                     wrapper.Load.BoardType == DispatchBoardType.BrokerAccount) &&
                    wrapper.Load.RecordIndex != null) {
                    var linkedWrappers = Wrappers
                        .Where(w => (w.Load.BoardType == DispatchBoardType.Carrier ||
                                     w.Load.BoardType == DispatchBoardType.CarrierAccount) &&
                                    w.Load.ParentId == null &&
                                    w.Load.RecordIndex == wrapper.Load.RecordIndex &&
                                    w.Load.Status != LoadStatus.Canceled1 &&
                                    w.Load.Status != LoadStatus.Canceled2).ToList();
                    wrapper.LinkedWrapperCount = linkedWrappers.Count;
                    if (linkedWrappers.Count == 1) {
                        wrapper.LinkedWrapper = linkedWrappers.First();
                        continue;
                    }
                }
            }
        }

        private void FixWrapper(Wrapper1LoadViewModel wrapper) {
            if (wrapper.Trailer == null && wrapper.Load.TrailerId != null) {
                wrapper.Trailer = new TrailerViewModel() {
                    EntityId = wrapper.Load.TrailerId.Value,
                    Number = $"HISTORY{wrapper.Load.TrailerId.Value.ToString().Truncate(4)}"
                };
            }
            if (wrapper.HistoryTrailer == null && wrapper.Load.HistoryTrailerId != null) {
                wrapper.HistoryTrailer = new TrailerViewModel() {
                    EntityId = wrapper.Load.HistoryTrailerId.Value,
                    Number = $"HISTORY{wrapper.Load.HistoryTrailerId.Value.ToString().Truncate(4)}"
                };
            }
            if (wrapper.HistoryAssignedTrailer == null && wrapper.Load.HistoryAssignedTrailerId != null) {
                wrapper.HistoryAssignedTrailer = new TrailerViewModel() {
                    EntityId = wrapper.Load.HistoryAssignedTrailerId.Value,
                    Number = $"HISTORY{wrapper.Load.HistoryAssignedTrailerId.Value.ToString().Truncate(4)}"
                };
            }
        }

        private Guid? GetTruckId(LoadViewModel load) {
            if (load.TrailerId != null) {
                var trailer = Trailers.FirstOrDefault(t => t.EntityId == load.TrailerId);
                if (trailer != null) {
                    var truck = Trucks.FirstOrDefault(t => t.TrailerId == trailer.EntityId);
                    if (truck != null) {
                        return truck.EntityId;
                    } else {
                        return null;
                    }
                } else {
                    // NOTE: Occurred when the source doesn't load
                    //_l.w($"Trailer was not found");
                    return null;
                }
            } else {
                return null;
            }
        }

        private Guid? GetSetupPacketKey(LoadViewModel load) {
            if (load.HistoryDatCompanyId != null) {
                var datCompany = DatCompanies.FirstOrDefault(t => t.EntityId == load.HistoryDatCompanyId);
                return datCompany?.SetupPacketId;
            } else {
                return null;
            }
        }

        private Guid? GetYardKey(LoadViewModel load) {
            if (load.TrailerId != null) {
                var trailer = Trailers.FirstOrDefault(t => t.EntityId == load.TrailerId);
                return trailer?.YardId;
            } else {
                return null;
            }
        }

        private Guid? GetDriverId(LoadViewModel load) {
            var truckId = GetTruckId(load);
            if (truckId != null) {
                var driver = Drivers.FirstOrDefault(t => t.TruckId == truckId);
                if (driver != null) {
                    return driver.EntityId;
                } else {
                    return null;
                }

            } else {
                return null;
            }
        }

        /// <summary>
        ///     Builds wrapper </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourse">List of BaseObjectViewModel</param>
        /// <param name="load_key">wrapper.Load.DatCompanyId</param>
        /// <param name="wrapper_ref">wrapper.DatCompany</param>
        /// <param name="setObj">wrapper.DatCompany = obj</param>
        private async Task AssignWrappers<T>(
            IList<T> sourse,
            Func<LoadViewModel, Guid?> load_key,
            Func<Wrapper1LoadViewModel, T> wrapper_ref,
            Action<Wrapper1LoadViewModel, T> setObj) where T : BaseObjectViewModel {
            // Loads: sets objects
            foreach (var wrapper in Wrappers) {
                await DelayHelper.DelayAsync();
                try {
                    // speeds up
                    var key = load_key(wrapper.Load);
                    if (key != null) {
                        var wref = wrapper_ref(wrapper);
                        // if existas
                        if (wref != null) {
                            var obj = sourse.FirstOrDefault(c => c.EntityId == key);
                            if (obj != null) {
                                if (wref == obj) {
                                    continue;
                                } else {
                                    setObj(wrapper, obj);
                                }
                            } else {
                                // NOTE: Occurred when the source doesn't load
                                //_l.w($"{typeof(T)} was not found");
                            }
                        }
                        // if new
                        else {
                            var obj = sourse.FirstOrDefault(c => c.EntityId == key);
                            if (obj != null) {
                                setObj(wrapper, obj);
                            } else {
                                // NOTE: Occurred when the source doesn't load
                                //_l.w($"{typeof(T)} was not found");
                            }
                        }
                    } else {
                        // wrapper.DatCompany = null;
                        setObj(wrapper, null);
                    }
                } catch (Exception e) {
                    _webl.e("AssignWrappers", e);
                    Debug.WriteLine(e.Message);
                    throw;
                }
            }
        }

        /// <summary>
        ///     Updates properties in load </summary>
        /// <param name="entityId">Load ID</param>
        /// <param name="model">Load View Model which contains new properties</param>
        public void UpdateLoad(Guid entityId, LoadViewModel model) {
            var load = Loads.FirstOrDefault(l => l.EntityId == entityId);
            if (load != null) {
                load.Assign(model);
            }
        }

        /// <summary>
        ///     Sets status for root and children. Or set status for single load if it doesn't have parent </summary>
        /// <remarks>Sets completed status if all loads was paid</remarks>
        /// <returns>Changed loads</returns>
        public List<LoadViewModel> UpdateDependencyLoads(LoadViewModel child) {
            if (child.ParentId != null) {
                var root = Loads.FirstOrDefault(l => l.EntityId == child.ParentId);
                var loads = Loads.Where(l => l.ParentId == child.ParentId).ToList();
                if (root != null) {
                    if (loads.Any()) {
                        // if all loads was paid
                        if (loads.All(l => l.Status == LoadStatus.Completed)) {
                            // gets all uncompleted loads
                            var changes = loads.Where(l => l.Status != LoadStatus.Completed).ToList();
                            // addes root if it uncompleted
                            if (root.Status != LoadStatus.Completed) {
                                changes.Add(root);
                            }
                            // sets status completed
                            changes.ForEach(l => l.Status = LoadStatus.Completed);
                            return changes;
                        }
                    }
                } else {
                    _webl.e($"Root '{child.ParentId}' was not found");
                }
            }
            return null;
        }

        public LoadViewModel GetLoadById(Guid loadId) {
            return Loads.FirstOrDefault(l => l.EntityId == loadId);
        }
        public ActualCompanyViewModel GetActualCompanyById(Guid? companyId) {
            return ActualCompanies.FirstOrDefault(l => l.EntityId == companyId);
        }

        #endregion [ Public ]
    }
}