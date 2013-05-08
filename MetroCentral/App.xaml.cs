using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using Kent.Boogaart.Windows.Controls;
using System.Windows.Input;

namespace MetroCentral
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
        private void PART_Grip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                (sender as FrameworkElement).CaptureMouse();
                Resizer.StartResizeCommand.Execute(sender as FrameworkElement, sender as FrameworkElement);
            }
            else if (e.ClickCount == 2)
            {
                Resizer.AutoSizeCommand.Execute(null, sender as FrameworkElement);
            }
        }

        private void PART_Grip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                Resizer.EndResizeCommand.Execute(null, sender as FrameworkElement);
                (sender as FrameworkElement).ReleaseMouseCapture();
            }
        }

        private void PART_Grip_MouseMove(object sender, MouseEventArgs e)
        {
            Resizer.UpdateSizeCommand.Execute(null, sender as FrameworkElement);
        }
	}
}