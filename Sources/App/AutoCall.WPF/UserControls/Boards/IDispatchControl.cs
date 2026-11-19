using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using AutoCall.Logic.Contracts.Enums;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.UserControls.Boards {
    public interface IDispatchControl {
        BoardType BoardType { get; }
        DispatchBoardType DispatchBoardType { get; }
        Dispatcher Dispatcher { get; }
        ActualCompanyViewModel SelectedBrokerCompany { get; }
        ActualCompanyViewModel SelectedCarrierCompany { get; }

        void BusyFinish();
        void BusyStart(string message = null);
        Task SaveCompleted<T, F>(ViewModelWrapper<T> wrapper1, ViewModelWrapper<F> wrapper2);
        Task SaveCompleted<T>(ViewModelWrapper<T> wrapper);
        Task SetLoads(List<LoadViewModel> loads);
    }
}