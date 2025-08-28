﻿// DevToolVaultV2/Converters/IconConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using MaterialDesignThemes.Wpf; // Necessário para PackIconKind e PackIcon

namespace DevToolVaultV2.Converters
{
    // Corrigido: Nome da classe
    public class IconConverter : IValueConverter
    {
        // Mapeamento de extensões para PackIconKind
        private static readonly Dictionary<string, PackIconKind> ExtensionIconMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".cs", PackIconKind.LanguageCsharp },
            { ".dart", PackIconKind.FileDocument },
            { ".json", PackIconKind.FileDocument },
            { ".xml", PackIconKind.Xml },
            { ".yml", PackIconKind.FileDocument },
            { ".yaml", PackIconKind.FileDocument },
            { ".md", PackIconKind.LanguageMarkdown },
            { ".txt", PackIconKind.TextBox },
            { ".html", PackIconKind.LanguageHtml5 },
            { ".css", PackIconKind.LanguageCss3 },
            { ".js", PackIconKind.LanguageJavascript },
            { ".ts", PackIconKind.LanguageTypescript },
            { ".xaml", PackIconKind.Xml },
            { ".config", PackIconKind.Settings },
            { ".ini", PackIconKind.Settings },
            { ".sln", PackIconKind.MicrosoftVisualStudio },
            { ".csproj", PackIconKind.MicrosoftVisualStudio },
            // Adicione mais conforme necessário
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value is IsDirectory (bool)
            if (value is bool isDirectory)
            {
                return isDirectory ? "📁" : "📄"; // Folder or File emoji
            }

            // Default to file icon
            return "📄";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}