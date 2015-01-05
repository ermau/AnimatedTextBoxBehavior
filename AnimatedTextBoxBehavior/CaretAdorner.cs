//
// CaretAdorner.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014-2015 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TextBox = System.Windows.Controls.TextBox;

namespace ermau
{
	internal sealed class CaretAdorner
		: Adorner, IDisposable
	{
		public CaretAdorner (TextBox adornedElement)
			: base (adornedElement)
		{
			var fontRect = adornedElement.GetRectFromCharacterIndex (0);
			this.caret = new Rectangle {
				Width = 1,
				Height = fontRect.Height,
				Fill = Brushes.Black,
				RenderTransform = new TranslateTransform (fontRect.Left, fontRect.Top),
				Opacity = 0,
				SnapsToDevicePixels = true
			};

			adornedElement.SelectionChanged += OnSelectionChanged;
			adornedElement.PreviewMouseDown += OnPreviewMouseDown;
			adornedElement.GotKeyboardFocus += OnGotFocus;
			adornedElement.LostKeyboardFocus += OnLostFocus;

			this.children = new VisualCollection (this);
			this.children.Add (this.caret);

			double transitionTime = SystemInformation.CaretBlinkTime;
			TimeSpan fadeTime = TimeSpan.FromMilliseconds (transitionTime * (1d / 3d));
			TimeSpan holdTime = TimeSpan.FromMilliseconds (transitionTime * (2d / 3d));

			var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

			var fade = new DoubleAnimationUsingKeyFrames {
				KeyFrames = {
					new EasingDoubleKeyFrame (0, KeyTime.FromTimeSpan (fadeTime)),
					new DiscreteDoubleKeyFrame (0, KeyTime.FromTimeSpan (fadeTime + holdTime)),
					new EasingDoubleKeyFrame (1, KeyTime.FromTimeSpan (fadeTime + holdTime + fadeTime)),
					new DiscreteDoubleKeyFrame (1, KeyTime.FromTimeSpan (fadeTime + holdTime + fadeTime + holdTime))
				},

				Duration = new Duration (TimeSpan.FromMilliseconds (transitionTime * 2))
			};
			Storyboard.SetTargetProperty (fade, new PropertyPath ("Opacity"));
			fade.Freeze();

			storyboard.Children.Add (fade);
			this.blinkStoryboard = storyboard;
		}

		public Brush CaretBrush
		{
			get { return this.caret.Fill; }
			set
			{
				if (value == null)
					value = Brushes.Black;

				this.caret.Fill = value;
			}
		}

		public void Dispose()
		{
			if (TextBox == null)
				return;

			TextBox.SelectionChanged -= OnSelectionChanged;
			TextBox.PreviewMouseDown -= OnPreviewMouseDown;
			TextBox.GotKeyboardFocus -= OnGotFocus;
			TextBox.LostKeyboardFocus -= OnLostFocus;
		}

		protected override int VisualChildrenCount
		{
			get { return this.children.Count; }
		}

		protected override Visual GetVisualChild (int index)
		{
			return this.children[index];
		}

		protected override Size ArrangeOverride (Size finalSize)
		{
			this.caret.Arrange (new Rect (0, 0, finalSize.Width, finalSize.Height));
			return this.caret.RenderSize;
		}

		protected override Size MeasureOverride (Size constraint)
		{
			this.caret.Measure (constraint);
			return this.caret.DesiredSize;
		}

		private Storyboard blinkStoryboard;
		private bool mouseInput;
		private CancellationTokenSource cancelSource;
		private readonly Rectangle caret;
		private readonly VisualCollection children;
		private readonly Duration shiftDuration = new Duration (TimeSpan.FromMilliseconds (50));

		private TextBox TextBox
		{
			get { return (TextBox) AdornedElement; }
		}

		private async void UpdateCaretPosition()
		{
			if (this.cancelSource != null)
				this.cancelSource.Cancel();

			this.cancelSource = new CancellationTokenSource();

			bool fromMouse = this.mouseInput;
			this.mouseInput = false;

			bool visible = (TextBox.SelectedText == String.Empty);
			this.caret.Visibility = (visible) ? Visibility.Visible : Visibility.Collapsed;
			
			Rect position = TextBox.GetRectFromCharacterIndex (TextBox.CaretIndex);
			double left = Math.Round (position.Left);
			double top = Math.Round (position.Top);

			this.blinkStoryboard.Stop (this.caret);
			this.caret.Opacity = 1;

			if (fromMouse || !visible)
				this.caret.RenderTransform = new TranslateTransform (left, top);
			else {
				if (left != ((TranslateTransform) this.caret.RenderTransform).X)
					this.caret.RenderTransform.BeginAnimation (TranslateTransform.XProperty, new DoubleAnimation (left, this.shiftDuration), HandoffBehavior.Compose);
				if (top != ((TranslateTransform) this.caret.RenderTransform).Y)
					this.caret.RenderTransform.BeginAnimation (TranslateTransform.YProperty, new DoubleAnimation (top, this.shiftDuration), HandoffBehavior.Compose);
			}

			if (visible) {
				try {
					await Task.Delay (SystemInformation.CaretBlinkTime, this.cancelSource.Token);
					this.blinkStoryboard.Begin (this.caret, isControllable: true);
					this.cancelSource = null;
				} catch (OperationCanceledException) {
				}
			}
		}

		private void OnLostFocus (object sender, RoutedEventArgs routedEventArgs)
		{
			this.blinkStoryboard.Stop (this.caret);
			this.caret.Opacity = 0;
		}

		private void OnGotFocus (object sender, RoutedEventArgs routedEventArgs)
		{
			this.caret.Opacity = 1;
			this.blinkStoryboard.Begin (this.caret, isControllable: true);
		}

		private void OnPreviewMouseDown (object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Right)
				return;

			this.mouseInput = true;
		}

		private void OnSelectionChanged (object sender, RoutedEventArgs e)
		{
			UpdateCaretPosition();
		}
	}
}
