using MenuApp.Models;
using MenuApp.Services;
using MenuApp.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MenuApp.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private string _searchTerm = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _searchByAllergen;
        private bool _containsFilter = true;
        private ObservableCollection<Dish> _searchResults = new ObservableCollection<Dish>();
        private int _selectedAllergenId;


        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool SearchByAllergen
        {
            get => _searchByAllergen;
            set
            {
                _searchByAllergen = value;
                OnPropertyChanged(nameof(SearchByAllergen));
                OnPropertyChanged(nameof(SearchByName));
            }
        }

        public bool SearchByName
        {
            get => !_searchByAllergen;
            set
            {
                _searchByAllergen = !value;
                OnPropertyChanged(nameof(SearchByName));
                OnPropertyChanged(nameof(SearchByAllergen));
            }
        }

        public bool ContainsFilter
        {
            get => _containsFilter;
            set
            {
                _containsFilter = value;
                OnPropertyChanged(nameof(ContainsFilter));
                OnPropertyChanged(nameof(DoesNotContainFilter));
            }
        }

        public bool DoesNotContainFilter
        {
            get => !_containsFilter;
            set
            {
                _containsFilter = !value;
                OnPropertyChanged(nameof(ContainsFilter));
                OnPropertyChanged(nameof(DoesNotContainFilter));
            }
        }

        public ObservableCollection<Dish> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        public int SelectedAllergenId
        {
            get => _selectedAllergenId;
            set
            {
                _selectedAllergenId = value;
                OnPropertyChanged(nameof(SelectedAllergenId));
            }
        }

        public ICommand SearchCommand { get; }

        public SearchViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            SearchCommand = new RelayCommand(ExecuteSearch);
        }

        private async void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                StatusMessage = "Introduceți un termen de căutare.";
                return;
            }

            IsLoading = true;
            StatusMessage = "Se caută...";
            SearchResults.Clear();

            try
            {
                if (SearchByAllergen)
                {
                    if (int.TryParse(SearchTerm, out int allergenId))
                    {
                        SelectedAllergenId = allergenId;
                        var results = await _databaseService.SearchDishesByAllergenAsync(allergenId, ContainsFilter);

                        foreach (var dish in results)
                        {
                            SearchResults.Add(dish);
                        }

                        StatusMessage = ContainsFilter
                            ? $"Preparate care conțin alergenul \"{SearchTerm}\""
                            : $"Preparate care NU conțin alergenul \"{SearchTerm}\"";
                    }
                    else
                    {
                        StatusMessage = "ID-ul alergenului trebuie să fie un număr!";
                    }
                }
                else
                {
                    var results = await _databaseService.SearchDishesByNameAsync(SearchTerm);

                    foreach (var dish in results)
                    {
                        SearchResults.Add(dish);
                    }

                    StatusMessage = $"Rezultate pentru \"{SearchTerm}\"";
                }

                if (SearchResults.Count == 0)
                {
                    StatusMessage = "Nu s-au găsit rezultate.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la căutare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}