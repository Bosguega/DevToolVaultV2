﻿// DevToolVaultV2/Core/Models/FileSystemItem.cs
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevToolVaultV2.Core.Models
{
    // Corrigido: INotifyPropertyChanged
    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool? _isChecked;
        private bool _isExpanded;
        private string _name;
        private string _fullPath;
        // Corrigido: _isDirectory
        private bool _isDirectory;
        // Corrigido: List<FileSystemItem>
        private List<FileSystemItem> _children;
        private FileSystemItem _parent;
        private bool _isUpdatingState; // Flag to prevent recursive updates

        // --- Propriedades adicionadas ---
        private string _fullName; // Novo campo de apoio
        private string _relativePath; // Novo campo de apoio

        // Propriedade FullName (pode ser um alias para FullPath ou definida separadamente)
        public string FullName
        {
            get => _fullName ?? _fullPath; // Padrão para FullPath se _fullName não estiver definido
            set => SetProperty(ref _fullName, value);
        }

        // Propriedade RelativePath
        public string RelativePath
        {
            get => _relativePath;
            set => SetProperty(ref _relativePath, value);
        }
        // --- Fim das propriedades adicionadas ---

        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value) && !_isUpdatingState)
                {
                    _isUpdatingState = true;
                    try
                    {
                        // Propagar mudança para filhos quando definido explicitamente
                        if (value.HasValue)
                        {
                            UpdateChildrenState(this, value.Value);
                        }
                        // Notificar pai sobre mudança
                        _parent?.UpdateParentState();
                    }
                    finally
                    {
                        _isUpdatingState = false;
                    }
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set => SetProperty(ref _isDirectory, value);
        }

        public List<FileSystemItem> Children
        {
            get => _children;
            set
            {
                if (SetProperty(ref _children, value))
                {
                    // Set parent reference for all children
                    if (_children != null)
                    {
                        foreach (var child in _children)
                        {
                            child._parent = this;
                        }
                    }
                }
            }
        }

        public FileSystemItem Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Corrigido: OnPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Corrigido: private
        private void UpdateChildrenState(FileSystemItem item, bool isChecked)
        {
            if (item.Children == null) return;

            foreach (var child in item.Children)
            {
                child._isUpdatingState = true;
                try
                {
                    child._isChecked = isChecked;
                    child.OnPropertyChanged(nameof(IsChecked));
                    child.UpdateChildrenState(child, isChecked);
                }
                finally
                {
                    child._isUpdatingState = false;
                }
            }
        }

        // Corrigido: private
        private void UpdateParentState()
        {
            if (Parent == null || Parent._isUpdatingState) return;

            bool allChecked = true;
            bool allUnchecked = true;
            bool hasIndeterminate = false;

            foreach (var child in Parent.Children)
            {
                if (child.IsChecked == true)
                {
                    allUnchecked = false;
                }
                else if (child.IsChecked == false)
                {
                    allChecked = false;
                }
                else // null (indeterminate)
                {
                    allChecked = false;
                    allUnchecked = false;
                    hasIndeterminate = true;
                }
            }

            Parent._isUpdatingState = true;
            try
            {
                if (allChecked)
                {
                    Parent._isChecked = true;
                }
                else if (allUnchecked && !hasIndeterminate)
                {
                    Parent._isChecked = false;
                }
                else
                {
                    Parent._isChecked = null; // Indeterminado
                }
                Parent.OnPropertyChanged(nameof(IsChecked));
                Parent.Parent?.UpdateParentState();
            }
            finally
            {
                Parent._isUpdatingState = false;
            }
        }
    }
}