using System;

namespace AutoDispatch.Logic.ViewModels {
    public abstract class BaseViewModel : ViewModel {
        private bool _isDeleted;
        private DateTime _createdDate;
        private Guid _createdById;
        private DateTime _lastModifiedDate;
        private Guid _lastModifiedById;

        /// <summary>
        ///     Entity ID </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        ///     First time creation date </summary>
        public DateTime CreatedDate {
            get => _createdDate;
            set {
                _createdDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Created));
                OnPropertyChanged(nameof(CreatedByName));
            }
        }
        /// <summary>
        ///     Fist time created by person id </summary>
        public Guid CreatedById {
            get => _createdById;
            set {
                _createdById = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Created));
                OnPropertyChanged(nameof(CreatedByName));
            }
        }
        /// <summary>
        ///     Fist time created by person </summary>
        public string CreatedByName { get; set; }
        /// <summary>
        ///     Last modified date </summary>
        public DateTime LastModifiedDate {
            get => _lastModifiedDate;
            set {
                _lastModifiedDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastModified));
                OnPropertyChanged(nameof(LastModifiedByName));
            }
        }
        /// <summary>
        ///     Last modified by person id </summary>
        public Guid LastModifiedById {
            get => _lastModifiedById;
            set {
                _lastModifiedById = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastModified));
                OnPropertyChanged(nameof(LastModifiedByName));
            }
        }
        /// <summary>
        ///     Last modified person </summary>
        public string LastModifiedByName { get; set; }

        public virtual string Created => $"{CreatedByName}, {CreatedDate:M/d/yyyy h:mmtt}";
        public virtual string LastModified => $"{LastModifiedByName}, {LastModifiedDate:M/d/yyyy h:mmtt}";

        /// <summary>
        ///     Marked as deleted </summary>
        public bool IsDeleted {
            get { return _isDeleted; }
            set {
                if (value != _isDeleted) {
                    _isDeleted = value;
                    OnPropertyChanged();
                }
            }
        }

        protected BaseViewModel() {
            EntityId = Guid.NewGuid();
        }
    }
}