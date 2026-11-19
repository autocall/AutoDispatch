using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    public class TwoColumnPanel : Panel {
        protected override Size MeasureOverride(Size availableSize) {
            double columnWidth = availableSize.Width / 2;
            double leftHeight = 0, rightHeight = 0;

            int count = InternalChildren.Count;
            for (int i = 0; i < count; i++) {
                var child = InternalChildren[i];
                child.Measure(new Size(columnWidth, availableSize.Height));

                if (i < (count + 1) / 2)
                    leftHeight += child.DesiredSize.Height;
                else
                    rightHeight += child.DesiredSize.Height;
            }

            return new Size(availableSize.Width, Math.Max(leftHeight, rightHeight));
        }

        protected override Size ArrangeOverride(Size finalSize) {
            double columnWidth = finalSize.Width / 2;
            double leftY = 0, rightY = 0;

            int count = InternalChildren.Count;
            for (int i = 0; i < count; i++) {
                var child = InternalChildren[i];
                Rect rect;
                if (i < (count + 1) / 2) {
                    rect = new Rect(0, leftY, columnWidth, child.DesiredSize.Height);
                    leftY += child.DesiredSize.Height;
                } else {
                    rect = new Rect(columnWidth, rightY, columnWidth, child.DesiredSize.Height);
                    rightY += child.DesiredSize.Height;
                }
                child.Arrange(rect);
            }

            return finalSize;
        }
    }

}
