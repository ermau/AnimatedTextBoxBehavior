using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnimatedTextBoxBehavior.Example
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();
		}

		private void OnToggleAnimations (object sender, RoutedEventArgs e)
		{
			BehaviorCollection behaviors = Interaction.GetBehaviors (this.text);
			if (behaviors.Count > 0)
				behaviors.RemoveAt (0);
			else
				behaviors.Add (new ermau.AnimatedTextBoxBehavior());
		}

		private void OnToggleColor (object sender, RoutedEventArgs e)
		{
			this.text.CaretBrush = (this.text.CaretBrush == Brushes.Red) ? Brushes.Blue : Brushes.Red;
		}

		private void OnToggleVisibility (object sender, RoutedEventArgs e)
		{
			this.text.Focus();
			this.text.Visibility = (this.text.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
		}
	}
}
