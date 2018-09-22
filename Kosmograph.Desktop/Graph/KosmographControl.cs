﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kosmograph.Desktop.Graph"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kosmograph.Desktop.Graph;assembly=Kosmograph.Desktop.Graph"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:KosmographControl/>
    ///
    /// </summary>
    public partial class KosmographControl : Canvas
    {
        private readonly ClickCounter clickCounter;
        private readonly KosmographViewer msaglGraphViewer;

        static KosmographControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KosmographControl), new FrameworkPropertyMetadata(typeof(KosmographControl)));
        }

        public KosmographControl()
        {
            this.clickCounter = new ClickCounter(GetMousePosition);
            this.msaglGraphViewer = new KosmographViewer(this, this.clickCounter);
            this.Loaded += this.KosmographControl_Loaded;
        }

        private Point GetMousePosition() => Mouse.GetPosition((IInputElement)this.Parent);

        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    this.clickCounter.AddMouseDown(_objectUnderMouseCursor);
        //    this.MouseDown?.Invoke(this, this.CreateMouseEventArgs(e));

        //    if (e.Handled)
        //        return;

        //    this.msaglGraphViewr.SetMousePosition(e.GetPosition(this));

        //    base.OnMouseLeftButtonDown(e);
        //}
    }
}