Overview
========

`AnimatedTextBoxBehavior` is a WPF behavior for `TextBox` that animates caret-based operations.
These animations are meant to simply provide a nicer experience, without getting in the way or slowing you down.

Features
========

 - Caret blink is a fade
 - Animated keyboard-based caret movement (typing, arrow keys, home/end).

Usage
========

As `AnimatedTextBoxBehavior` exists purely as a behavior, the usage is quite straightforward:

```xaml
<Window x:Class="AnimatedTextBoxBehavior.Example.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity"
        xmlns:ermau="clr-namespace:ermau;assembly=AnimatedTextBoxBehavior">
    <TextBox>
		<i:Interaction.Behaviors>
            <ermau:AnimatedTextBoxBehavior />
        </i:Interaction.Behaviors>
	</TextBox>
</Window>
```