using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wpfApplication
{
    public class ZoomMoveImage
    {
        ScrollViewer scrollViewer;
        ScaleTransform scaleTransform;
        Slider slider;

        public ZoomMoveImage(ScrollViewer scroll, ScaleTransform scale, Slider slide)
        {
            scrollViewer = scroll;
            scaleTransform = scale;
            slider = slide;
        }
        bool WheelScale = false;
        public void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            WheelScale = true;
            if (e.Delta > 0)
            {
                slider.Value += 0.1;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 0.1;
            }
            Scale();
            WheelScale = false;
        }
        void Scale()
        {
            // Применяем новый масштаб
            scaleTransform.ScaleX = slider.Value;
            scaleTransform.ScaleY = slider.Value;
            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                     scrollViewer.ViewportHeight / 2);
            scrollViewer.ScrollToHome();
        }

        public void OnSliderValueChanged(object sender,
             RoutedPropertyChangedEventArgs<double> e)
        {
            if (!WheelScale)
                Scale();
        }
    }
}