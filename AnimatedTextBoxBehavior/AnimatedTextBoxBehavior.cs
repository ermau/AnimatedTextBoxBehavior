//
// AnimatedTextBoxBehavior.cs
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace ermau
{
	public sealed class AnimatedTextBoxBehavior
		: Behavior<TextBox>
	{
		protected override void OnAttached()
		{
			this.originalCaretBrush = AssociatedObject.CaretBrush;
			AssociatedObject.CaretBrush = Brushes.Transparent;

			var descriptor = DependencyPropertyDescriptor.FromProperty (TextBoxBase.CaretBrushProperty, typeof (TextBox));
			descriptor.AddValueChanged (AssociatedObject, OnCaretBrushChanged);

			if (!AssociatedObject.IsLoaded) {
				AssociatedObject.Loaded += OnAssociatedObjectLoaded;
			} else
				SetupAdorner();
		}

		protected override void OnDetaching()
		{
			var descriptor = DependencyPropertyDescriptor.FromProperty (TextBoxBase.CaretBrushProperty, typeof (TextBox));
			descriptor.RemoveValueChanged (AssociatedObject, OnCaretBrushChanged);

			AssociatedObject.CaretBrush = this.originalCaretBrush;
			this.layer.Remove (this.caretAdorner);
			this.caretAdorner.Dispose();
			this.caretAdorner = null;
		}

		private Brush originalCaretBrush;
		private AdornerLayer layer;
		private CaretAdorner caretAdorner;

		private void OnCaretBrushChanged (object sender, EventArgs eventArgs)
		{
			Brush brush = AssociatedObject.CaretBrush;
			this.caretAdorner.CaretBrush = brush;
			this.originalCaretBrush = brush;
		}

		private void OnAssociatedObjectLoaded (object sender, RoutedEventArgs e)
		{
			SetupAdorner();
			AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
		}

		private void SetupAdorner()
		{
			this.layer = AdornerLayer.GetAdornerLayer (AssociatedObject);

			this.caretAdorner = new CaretAdorner (AssociatedObject) {
				CaretBrush = this.originalCaretBrush
			};
			this.layer.Add (this.caretAdorner);
		}
	}
}
