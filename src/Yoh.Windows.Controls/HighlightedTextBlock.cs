﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Yoh.Windows.Controls
{
    public class HighlightedTextBlock : TextBlock
    {
        private sealed class TextPropertyMetadata : FrameworkPropertyMetadata
        {
            protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
            {
                BaseTextChanged = baseMetadata.PropertyChangedCallback;

                base.Merge(baseMetadata, dp);
                PropertyChangedCallback = (d, e) => ((HighlightedTextBlock)d).OnTextChanged(e);
            }
        }

        private static readonly Binding HighlightBackgroundBinding;
        private static readonly Binding HighlightForegroundBinding;
        private static PropertyChangedCallback BaseTextChanged;
        private bool _textChanging;

        static HighlightedTextBlock()
        {
            HighlightBackgroundBinding = new Binding { Path = new PropertyPath(HighlightBackgroundProperty), RelativeSource = RelativeSource.Self };
            HighlightForegroundBinding = new Binding { Path = new PropertyPath(HighlightForegroundProperty), RelativeSource = RelativeSource.Self };
            TextProperty.OverrideMetadata(typeof(HighlightedTextBlock), new TextPropertyMetadata());
        }

        public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached(
            nameof(HighlightBackground),
            typeof(Brush),
            typeof(HighlightedTextBlock),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                FrameworkPropertyMetadataOptions.Inherits));

        public Brush HighlightBackground
        {
            get => (Brush)GetValue(HighlightBackgroundProperty);
            set => SetValue(HighlightBackgroundProperty, value);
        }

        public static readonly DependencyProperty HighlightForegroundProperty = DependencyProperty.RegisterAttached(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(HighlightedTextBlock),
            new FrameworkPropertyMetadata(
                Brushes.Black,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                FrameworkPropertyMetadataOptions.Inherits));

        public Brush HighlightForeground
        {
            get => (Brush)GetValue(HighlightForegroundProperty);
            set => SetValue(HighlightForegroundProperty, value);
        }

        public static readonly DependencyProperty HighlightedTextProperty = DependencyProperty.Register(
            nameof(HighlightedText),
            typeof(string),
            typeof(HighlightedTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((HighlightedTextBlock)d).OnTextChanged(e),
                (d, v) => v ?? string.Empty));

        public string HighlightedText
        {
            get => (string)GetValue(HighlightedTextProperty);
            set => SetValue(HighlightedTextProperty, value);
        }

        private void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_textChanging)
                return;

            try
            {
                _textChanging = true;

                var text = Text;
                var highlighted = HighlightedText;

                if (highlighted.Length == 0)
                {
                    BaseTextChanged(this, e.Property == TextProperty
                        ? e : new DependencyPropertyChangedEventArgs(TextProperty, text, text));
                }
                else
                {
                    Inlines.Clear();
                    Inlines.AddRange(GetInlines());
                }

                IEnumerable<Run> GetInlines()
                {
                    for (int start, end = 0; end < text.Length;)
                    {
                        start = text.IndexOf(highlighted, end);

                        if (start < 0)
                        {
                            yield return new Run(text.Substring(end));
                            yield break;
                        }

                        if (start > end)
                            yield return new Run(text.Substring(end, start - end));

                        var highlight = new Run();

                        highlight.SetBinding(TextElement.BackgroundProperty, HighlightBackgroundBinding);
                        highlight.SetBinding(TextElement.ForegroundProperty, HighlightForegroundBinding);
                        highlight.Text = text.Substring(start, highlighted.Length);

                        yield return highlight;
                        end = start + highlighted.Length;

                    }
                }
            }
            finally
            {
                _textChanging = false;
            }
        }
    }
}
