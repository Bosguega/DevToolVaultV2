// DevToolVaultV2_Refatorado/Converters/IconConverter.cs
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
            // value é o 'Name' do FileSystemItem (string)
            if (value is string name)
            {
                // Determina a extensão
                string extension = Path.GetExtension(name).ToLowerInvariant();

                // Se não tem extensão, assume que é uma pasta
                if (string.IsNullOrEmpty(extension))
                {
                    return new PackIcon { Kind = PackIconKind.Folder, Width = 16, Height = 16 };
                }

                // Tenta obter o ícone baseado na extensão
                if (ExtensionIconMap.TryGetValue(extension, out PackIconKind kind))
                {
                    return new PackIcon { Kind = kind, Width = 16, Height = 16 };
                }
                else
                {
                    // Ícone padrão para arquivos desconhecidos
                    return new PackIcon { Kind = PackIconKind.FileDocumentOutline, Width = 16, Height = 16 };
                }
            }

            // Retorna um ícone padrão se o valor não for uma string
            return new PackIcon { Kind = PackIconKind.FileDocument, Width = 16, Height = 16 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}