using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoDispatch.Logic.Contracts.ViewModels.Pages {
    public class FiltersColumnsViewModel : ViewModel, ICloneable {

        private FilterColumnsViewModel _selectedFilter;

        public DispatchFilterTableType Table { get; set; }
        public ObservableCollection<FilterColumnsViewModel> Filters { get; set; } = new ObservableCollection<FilterColumnsViewModel>();
        public FilterColumnsViewModel SelectedFilter {
            get => _selectedFilter;
            set {
                _selectedFilter = value;
                OnPropertyChanged();

                foreach(var filter in this.Filters) {
                    filter.Selected = filter == value;
                }
            }
        }

        public object Clone() {
            var clone = new FiltersColumnsViewModel() {
                Table = this.Table,
                Filters = new ObservableCollection<FilterColumnsViewModel>(Filters.Select(e => (FilterColumnsViewModel)e.Clone()))
            };
            return clone;
        }
    }

    public class FilterColumnsViewModel : ViewModel, ICloneable {
        private FilterColumnEnum? _newKey;
        private string _newValue;
        private bool _selected;

        public FilterColumnEnum? NewKey {
            get => _newKey;
            set {
                _newKey = value;
                OnPropertyChanged();
            }
        }

        public string NewValue {
            get => _newValue;
            set {
                _newValue = value;
                OnPropertyChanged();
            }
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public ObservableCollection<FilterColumnViewModel> Columns { set; get; } = new ObservableCollection<FilterColumnViewModel>();


        public object Clone() {
            var clone = new FilterColumnsViewModel() {
                Name = this.Name,
                Columns = new ObservableCollection<FilterColumnViewModel>(Columns.Select(e => (FilterColumnViewModel)e.Clone()))
            };
            return clone;
        }
    }

    public class FilterColumnViewModel : ViewModel, ICloneable {

        public FilterColumnViewModel(FilterColumnEnum column, string value) {
            this.Column = column;
            this.Value = value;
        }

        public FilterColumnEnum Column { get; set; }
        public string Value { get; set; }

        public object Clone() {
            return new FilterColumnViewModel(this.Column, this.Value);
        }
    }

}
