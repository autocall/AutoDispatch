using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Behaviors {

    public class RadGridViewMultiSelectBehavior : Behavior<RadGridView> {

        private RadGridView Grid {
            get {
                return AssociatedObject as RadGridView;
            }
        }

        public INotifyCollectionChanged SelectedItems {
            get { return (INotifyCollectionChanged)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItemsProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(INotifyCollectionChanged), typeof(RadGridViewMultiSelectBehavior), new PropertyMetadata(OnSelectedItemsPropertyChanged));

        private static void OnSelectedItemsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args) {
            var collection = args.NewValue as INotifyCollectionChanged;
            if (collection != null) {
                collection.CollectionChanged += ((RadGridViewMultiSelectBehavior)target).ContextSelectedItems_CollectionChanged;
            }
        }

        protected override void OnAttached() {
            base.OnAttached();
            Grid.SelectedItems.CollectionChanged += GridSelectedItems_CollectionChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            Grid.SelectedItems.CollectionChanged -= GridSelectedItems_CollectionChanged;
        }

        private void ContextSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // если контекст изменился, то пропускай subloads
            if (String.IsNullOrEmpty(Grid.Name) && Grid.SelectedItems.Count == 0) {
                return;
            }
            UnsubscribeFromEvents();

            //if (e.Action == NotifyCollectionChangedAction.Remove) {
            //    this.AssociatedObject.SelectedItem = null;
            //}

            Transfer(SelectedItems as IList, Grid.SelectedItems);

            SubscribeToEvents();
#if DEBUG
            //Debug.WriteLine($" - Context: {e.Action} grid:{String.Join(", ", this.Grid.SelectedItems.Cast<Wrapper1LoadViewModel>().Select(i=>i.Load))}  context:{String.Join(", ", (SelectedItems as IList).Cast<Wrapper1LoadViewModel>().Select(i => i.Load))}");
#endif
        }

        private void GridSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UnsubscribeFromEvents();

            //if (e.Action == NotifyCollectionChangedAction.Remove) {
            //    this.AssociatedObject.SelectedItem = null;
            //}

            Transfer(Grid.SelectedItems, SelectedItems as IList);

            SubscribeToEvents();
#if DEBUG
            //Debug.WriteLine($" - GridView: {e.Action} {e.Action} grid:{String.Join(", ", this.Grid.SelectedItems.Cast<Wrapper1LoadViewModel>().Select(i => i.Load))}  context:{String.Join(", ", (SelectedItems as IList).Cast<Wrapper1LoadViewModel>().Select(i => i.Load))}");
#endif
        }

        private void SubscribeToEvents() {
            Grid.SelectedItems.CollectionChanged += GridSelectedItems_CollectionChanged;

            if (SelectedItems != null) {
                SelectedItems.CollectionChanged += ContextSelectedItems_CollectionChanged;
            }
        }

        private void UnsubscribeFromEvents() {
            Grid.SelectedItems.CollectionChanged -= GridSelectedItems_CollectionChanged;

            if (SelectedItems != null) {
                SelectedItems.CollectionChanged -= ContextSelectedItems_CollectionChanged;
            }
        }

        // NOTES: с флагом subload selection не сбрасывался
        //static bool flag = false;
        public void Transfer(IList source, IList target) {
            if (source == null || target == null)
                return;
            Debug.WriteLine($"{DateTime.Now:HH:mm:ss} source: {source.Count}, target: {target.Count} name: {Grid.Name}");
            //if (flag == true) {
            //    return;
            //}
            //flag = true;

            // Root Grid
            for (int i = 0; i < source.Count; i++) {
                try {
                    if (target.Cast<object>().Any(e => e == source[i]) == false) {
                        target.Add(source[i]);
                    }
                } catch (Exception) {
                }
            }

            for (int i = target.Count - 1; i >= 0; i--) {
                try {
                    if (source.Cast<object>().Any(e => e == target[i]) == false) {
                        target.Remove(target[i]);
                    }
                } catch (Exception) {
                }
            }

            //flag = false;
        }
    }
}
