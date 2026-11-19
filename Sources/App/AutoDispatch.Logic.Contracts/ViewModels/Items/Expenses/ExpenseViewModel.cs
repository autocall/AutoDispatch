using AutoCall.Common.Extensions;
using AutoCall.Common.Interfaces;
using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.ViewModels;
using System;
using System.Linq;
using System.Reflection;

namespace AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses {
    public class ExpenseViewModel : ViewModel, IAssignable, IComparable {
        private decimal? _cost;
        private string _comment;
        private bool _paid;
        private DateTime? _paidDate;

        public decimal? Cost {
            get { return _cost; }
            set {
                if (_cost != value) {
                    _cost = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Comment_Date));
                }
            }
        }

        public bool Paid {
            get { return _paid; }
            set {
                if (_paid != value) {
                    _paid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Comment_Date));
                    if (value == false) {
                        PaidDate = null;
                    } else {
                        PaidDate = DateTime.Now;
                    }
                }
            }
        }

        public DateTime? PaidDate {
            get { return _paidDate; }
            set {
                if (_paidDate != value) {
                    _paidDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Comment_Date));
                }
            }
        }

        public string Comment_Date {
            get => Paid ? $"{Comment} - {PaidDate:d}" : Comment;
            set {
                var comment = value.Replace($" - {PaidDate:d}", "");
                if (Comment != comment) {
                    Comment = comment;
                }
            }
        }

        public ExpenseType Type { get; set; }

        public bool IsCalculable {
            get {
                var type = Type.GetType();
                var field = type.GetField(Type.ToString());
                var attribute = field.GetCustomAttribute<CalculableAttribute>();
                return attribute != null;
            }
        }

        public string TypeDescription() {
            if (new ExpenseType[] { ExpenseType.Other1, ExpenseType.Other2, ExpenseType.Other3, ExpenseType.Other4 }
                .Contains(this.Type) && String.IsNullOrEmpty(this.Comment) == false) {
                return this.Comment;
            } else {
                return this.Type.GetDescription();
            }
        }

        public int CompareTo(object obj) {
            if (obj is ExpenseViewModel) {
                var vm = (ExpenseViewModel)obj;
                if (this.Cost == vm.Cost &&
                    this.Comment == vm.Comment &&
                    this.Type == vm.Type &&
                    this.Paid == vm.Paid &&
                    this.PaidDate == vm.PaidDate) {
                    return 0;
                }
            }
            return -1;
        }

        public object Clone() {
            return new ExpenseViewModel() {
                Cost = Cost,
                Comment = Comment,
                Type = Type,
                Paid = Paid,
                PaidDate = PaidDate
            };
        }

        public void Assign(object source) {
            var vm = (ExpenseViewModel)source;
            Cost = vm.Cost;
            Comment = vm.Comment;
            Type = vm.Type;
            Paid = vm.Paid;
            PaidDate = vm.PaidDate;
        }

        public override string ToString() {
            return String.Format($"expense: {((Enum)Type).GetDescriptionShort()} ${Cost:F0}");
        }
    }
}